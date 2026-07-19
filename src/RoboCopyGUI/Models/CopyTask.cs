namespace RoboCopyGUI.Models;

public sealed class CopyTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string[] ExtraFiles { get; set; } = [];
    public RoboCopyOptions Options { get; set; } = new();
    public CopyTaskStatus Status { get; set; } = CopyTaskStatus.Pending;
    public double ProgressPercent { get; set; }
    public string CurrentFile { get; set; } = string.Empty;
    public long FilesCopied { get; set; }
    public long TotalBytes { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public RoboCopyResult? Result { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string PresetName { get; set; } = string.Empty;

    public CopyTask Clone()
    {
        return new CopyTask
        {
            Source = Source,
            Destination = Destination,
            ExtraFiles = (string[])ExtraFiles.Clone(),
            Options = CloneOptions(Options),
            PresetName = PresetName
        };
    }

    private static RoboCopyOptions CloneOptions(RoboCopyOptions src) => new()
    {
        Source = src.Source,
        Destination = src.Destination,
        ExtraFiles = (string[])src.ExtraFiles.Clone(),
        RetryCount = src.RetryCount,
        RetryWaitSeconds = src.RetryWaitSeconds,
        Mirror = src.Mirror,
        MoveFiles = src.MoveFiles,
        CopySubdirectories = src.CopySubdirectories,
        CopyEmptySubdirectories = src.CopyEmptySubdirectories,
        IncludeAttributes = src.IncludeAttributes,
        ExcludeAttributes = src.ExcludeAttributes,
        RestartMode = src.RestartMode,
        BackupMode = src.BackupMode,
        UnbufferedIO = src.UnbufferedIO,
        EfsRawMode = src.EfsRawMode,
        FixFileSecurity = src.FixFileSecurity,
        FixFileTimes = src.FixFileTimes,
        RemoveFileAfterCopy = src.RemoveFileAfterCopy,
        PurgeDestination = src.PurgeDestination,
        FatFileTimes = src.FatFileTimes,
        DisableDirectoryTimestamps = src.DisableDirectoryTimestamps,
        ExcludeJunctions = src.ExcludeJunctions,
        UseUncompressedNetworkIO = src.UseUncompressedNetworkIO,
        LogFile = src.LogFile,
        AppendLogFile = src.AppendLogFile,
        NoDirectoryList = src.NoDirectoryList,
        NoFileClasses = src.NoFileClasses,
        NoFileSizes = src.NoFileSizes,
        NoFileList = src.NoFileList,
        NoProgress = src.NoProgress,
        ShowEstimatedTimeOfArrival = src.ShowEstimatedTimeOfArrival,
        VerboseOutput = src.VerboseOutput,
        IncludeSourceTimestamps = src.IncludeSourceTimestamps,
        ProduceUnicodeLog = src.ProduceUnicodeLog,
        PrintFullPaths = src.PrintFullPaths,
        DryRun = src.DryRun,
        IncludePatterns = (string[])src.IncludePatterns.Clone(),
        ExcludePatterns = (string[])src.ExcludePatterns.Clone(),
        ExcludeDirectories = (string[])src.ExcludeDirectories.Clone(),
        MaxDepth = src.MaxDepth,
        MinFileSize = src.MinFileSize,
        MaxFileSize = src.MaxFileSize,
        MinLastAccessDate = src.MinLastAccessDate,
        MaxLastAccessDate = src.MaxLastAccessDate,
        MultiThreadCount = src.MultiThreadCount,
        IpPort = src.IpPort,
        LogPath = src.LogPath
    };
}
