namespace Portfolio.Models;

public class CommandResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string? NavigateTo { get; set; }
    public bool ClearScreen { get; set; }
}

public class CommandHistoryEntry
{
    public string Command { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
