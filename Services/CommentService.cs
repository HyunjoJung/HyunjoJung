using Portfolio.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Portfolio.Services;

public class CommentService : IDisposable
{
    private readonly ILogger<CommentService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly string _dataFile;
    private readonly ConcurrentDictionary<string, List<Comment>> _commentsByPost = new();
    private readonly Timer _timer;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _hasChanges;
    private bool _isSaving;

    public event Action? OnCommentsChanged;

    public CommentService(IWebHostEnvironment env, ILogger<CommentService> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _env = env;
        _dataFile = Path.Combine(env.ContentRootPath, "comments.json");
        LoadComments();

        // Save comments every 2 minutes
        _timer = new Timer(async _ =>
        {
            try
            {
                await SaveCommentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in timer-triggered save. File: {DataFile}", _dataFile);
            }
        }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));

        // Ensure comments are saved on shutdown
        appLifetime.ApplicationStopping.Register(() =>
        {
            try
            {
                SaveCommentsOnExitAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save comments during shutdown. File: {DataFile}", _dataFile);
            }
        });
    }

    public List<Comment> GetComments(string postSlug)
    {
        _commentsByPost.TryGetValue(postSlug.ToLower(), out var comments);
        return comments?.OrderBy(c => c.CreatedAt).ToList() ?? new List<Comment>();
    }

    public async Task<Comment> AddCommentAsync(string postSlug, string username, string email, string githubUsername, string content)
    {
        var comment = new Comment
        {
            PostSlug = postSlug.ToLower(),
            Username = username,
            Email = email,
            GitHubUsername = githubUsername,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsAuthor = githubUsername.Equals("HyunjoJung", StringComparison.OrdinalIgnoreCase) // Replace with your GitHub username
        };

        _commentsByPost.AddOrUpdate(
            postSlug.ToLower(),
            new List<Comment> { comment },
            (key, existing) =>
            {
                existing.Add(comment);
                return existing;
            });

        _hasChanges = true;
        _logger.LogInformation("New comment added by {Username} on post {PostSlug}", username, postSlug);

        // Trigger immediate save for comments
        await SaveCommentsAsync();

        // Notify subscribers
        OnCommentsChanged?.Invoke();

        return comment;
    }

    public int GetCommentCount(string postSlug)
    {
        return GetComments(postSlug).Count;
    }

    private void LoadComments()
    {
        try
        {
            if (File.Exists(_dataFile))
            {
                var json = File.ReadAllText(_dataFile);
                var data = JsonSerializer.Deserialize<Dictionary<string, List<Comment>>>(json);

                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        _commentsByPost[kvp.Key] = kvp.Value;
                    }
                    _logger.LogInformation("Comments loaded successfully from {DataFile}", _dataFile);
                }
            }
            else
            {
                _logger.LogInformation("No existing comments file found. Starting fresh.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load comments from {DataFile}. Starting with empty comments.", _dataFile);
        }
    }

    private async Task SaveCommentsAsync()
    {
        if (!_hasChanges || _isSaving)
        {
            return;
        }

        await _saveLock.WaitAsync();
        try
        {
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
            _logger.LogInformation("Saving comments to {DataFile}...", _dataFile);
            var data = _commentsByPost.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            const int maxRetries = 3;
            bool success = false;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await File.WriteAllTextAsync(_dataFile, json);
                    _hasChanges = false;
                    _logger.LogInformation("Comments saved successfully to {DataFile}.", _dataFile);
                    success = true;
                    break;
                }
                catch (IOException ex) when (attempt < maxRetries)
                {
                    var delayMs = 100 * (int)Math.Pow(2, attempt - 1);
                    _logger.LogWarning(ex,
                        "Failed to save comments (attempt {Attempt}/{MaxRetries}). Retrying in {DelayMs}ms. File: {DataFile}",
                        attempt, maxRetries, delayMs, _dataFile);
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to save comments (attempt {Attempt}/{MaxRetries}). File: {DataFile}",
                        attempt, maxRetries, _dataFile);

                    if (attempt == maxRetries)
                    {
                        break;
                    }
                }
            }

            if (!success)
            {
                _logger.LogWarning("Comments could not be saved after {MaxRetries} attempts. Data may be lost.", maxRetries);
            }
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task SaveCommentsOnExitAsync()
    {
        _logger.LogInformation("Application is shutting down. Performing final save of comments.");
        _timer?.Change(Timeout.Infinite, 0);
        await SaveCommentsAsync();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _saveLock?.Dispose();
    }
}
