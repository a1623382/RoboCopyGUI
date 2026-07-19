namespace RoboCopyGUI.Models;

public sealed class RoboCopyProgress
{
    public double Percent { get; init; }
    public string CurrentFile { get; init; } = string.Empty;
    public long BytesCopied { get; init; }
    public long TotalBytes { get; init; }
    public long FilesCopied { get; init; }
    public long TotalFiles { get; init; }
    public string SpeedInfo { get; init; } = string.Empty;
    public string RawLine { get; init; } = string.Empty;
}
