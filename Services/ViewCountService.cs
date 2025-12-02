using System.Collections.Concurrent;
using System.Text.Json;

namespace Portfolio.Services;

public class ViewCountService : IDisposable
{
    private readonly ILogger<ViewCountService> _logger;
    private readonly string _dataFile;
    private readonly ConcurrentDictionary<string, int> _viewCounts = new();
    private readonly Timer _timer;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _hasChanges;
    private bool _isSaving;

    public ViewCountService(IWebHostEnvironment env, ILogger<ViewCountService> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _dataFile = Path.Combine(env.ContentRootPath, "viewcounts.json");
        LoadViewCounts();

        // Save counts every 5 minutes (with exception handling)
        _timer = new Timer(async _ =>
        {
            try
            {
                await SaveViewCountsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in timer-triggered save. File: {DataFile}", _dataFile);
            }
        }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        // Ensure counts are saved on shutdown
        appLifetime.ApplicationStopping.Register(() =>
        {
            try
            {
                SaveViewCountsOnExitAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save view counts during shutdown. File: {DataFile}", _dataFile);
            }
        });
    }

    public int GetViewCount(string slug)
    {
        return _viewCounts.GetValueOrDefault(slug, 0);
    }

    public int IncrementViewCount(string slug)
    {
        var newCount = _viewCounts.AddOrUpdate(slug, 1, (key, oldValue) => oldValue + 1);
        _hasChanges = true;
        return newCount;
    }

    private void LoadViewCounts()
    {
        try
        {
            if (File.Exists(_dataFile))
            {
                var json = File.ReadAllText(_dataFile);
                var data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        _viewCounts[kvp.Key] = kvp.Value;
                    }
                    _logger.LogInformation("View counts loaded successfully from {DataFile}", _dataFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load view counts from {DataFile}. Starting with empty counts.", _dataFile);
        }
    }

    private async Task SaveViewCountsAsync()
    {
        // Don't save if there are no changes or if a save is already in progress
        if (!_hasChanges || _isSaving)
        {
            return;
        }

        await _saveLock.WaitAsync();
        try
        {
            // Double-check inside the lock
            if (!_hasChanges || _isSaving)
            {
                return;
            }

            _isSaving = true;
        }
        finally
        {
            _saveLock.Release();
        }

        try
        {
            _logger.LogInformation("Saving view counts to {DataFile}...", _dataFile);
            var data = _viewCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            // Retry logic with exponential backoff
            const int maxRetries = 3;
            bool success = false;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await File.WriteAllTextAsync(_dataFile, json);
                    _hasChanges = false;
                    _logger.LogInformation("View counts saved successfully to {DataFile}.", _dataFile);
                    success = true;
                    break;
                }
                catch (IOException ex) when (attempt < maxRetries)
                {
                    var delayMs = 100 * (int)Math.Pow(2, attempt - 1); // Exponential backoff: 100ms, 200ms, 400ms
                    _logger.LogWarning(ex,
                        "Failed to save view counts (attempt {Attempt}/{MaxRetries}). Retrying in {DelayMs}ms. File: {DataFile}, Error: {ErrorType}",
                        attempt, maxRetries, delayMs, _dataFile, ex.GetType().Name);
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to save view counts (attempt {Attempt}/{MaxRetries}). File: {DataFile}, Error: {ErrorType}, Message: {ErrorMessage}",
                        attempt, maxRetries, _dataFile, ex.GetType().Name, ex.Message);
                    
                    // Don't throw on final retry to avoid crashing the timer/shutdown
                    if (attempt == maxRetries)
                    {
                        break;
                    }
                }
            }
            
            if (!success)
            {
                _logger.LogWarning("View counts could not be saved after {MaxRetries} attempts. Data may be lost.", maxRetries);
            }
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task SaveViewCountsOnExitAsync()
    {
        _logger.LogInformation("Application is shutting down. Performing final save of view counts.");
        // Stop the timer to prevent it from interfering
        _timer?.Change(Timeout.Infinite, 0);
        // Perform one final async save
        await SaveViewCountsAsync();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _saveLock?.Dispose();
    }
}
