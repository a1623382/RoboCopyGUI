namespace RoboCopyGUI.Models;

public sealed class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? TaskId { get; init; }
}

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Success
}
