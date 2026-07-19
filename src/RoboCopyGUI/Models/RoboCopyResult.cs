namespace RoboCopyGUI.Models;

public sealed class RoboCopyResult
{
    public bool Success { get; init; }
    public int ExitCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public long TotalFiles { get; init; }
    public long TotalBytes { get; init; }
    public TimeSpan Duration { get; init; }
    public IReadOnlyList<string> LogLines { get; init; } = [];

    public static string InterpretExitCode(int code) => code switch
    {
        0 => "No files were copied. No failure was met.",
        1 => "One or more files were copied successfully.",
        2 => "Some Extra files or directories were detected.",
        4 => "Some Mismatched files or directories were detected.",
        8 => "Some files or directories could not be copied.",
        16 => "Serious error. Robocopy did not copy any files.",
        _ => $"Unknown exit code: {code}"
    };
}
