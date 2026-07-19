namespace RoboCopyGUI.Models;

public sealed class RoboCopyOptions
{
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string[] ExtraFiles { get; set; } = [];

    public int RetryCount { get; set; } = 3;
    public int RetryWaitSeconds { get; set; } = 5;

    public bool Mirror { get; set; }
    public bool MoveFiles { get; set; }
    public bool CopySubdirectories { get; set; }
    public bool CopyEmptySubdirectories { get; set; }
    public bool IncludeAttributes { get; set; }
    public bool ExcludeAttributes { get; set; }
    public bool RestartMode { get; set; }
    public bool BackupMode { get; set; }
    public bool UnbufferedIO { get; set; }
    public bool EfsRawMode { get; set; }
    public bool FixFileSecurity { get; set; }
    public bool FixFileTimes { get; set; }
    public bool RemoveFileAfterCopy { get; set; }
    public bool PurgeDestination { get; set; }
    public bool FatFileTimes { get; set; }
    public bool DisableDirectoryTimestamps { get; set; }
    public bool ExcludeJunctions { get; set; }
    public bool UseUncompressedNetworkIO { get; set; }
    public bool LogFile { get; set; }
    public bool AppendLogFile { get; set; }
    public bool NoDirectoryList { get; set; }
    public bool NoFileClasses { get; set; }
    public bool NoFileSizes { get; set; }
    public bool NoFileList { get; set; }
    public bool NoProgress { get; set; }
    public bool ShowEstimatedTimeOfArrival { get; set; }
    public bool VerboseOutput { get; set; }
    public bool IncludeSourceTimestamps { get; set; }
    public bool ProduceUnicodeLog { get; set; }
    public bool PrintFullPaths { get; set; }

    public bool DryRun { get; set; }

    public string[] IncludePatterns { get; set; } = [];
    public string[] ExcludePatterns { get; set; } = [];
    public string[] ExcludeDirectories { get; set; } = [];
    public int MaxDepth { get; set; }
    public long MinFileSize { get; set; }
    public long MaxFileSize { get; set; }
    public DateTime? MinLastAccessDate { get; set; }
    public DateTime? MaxLastAccessDate { get; set; }

    public int MultiThreadCount { get; set; }
    public string IpPort { get; set; } = string.Empty;

    public string LogPath { get; set; } = string.Empty;

    public string BuildArguments()
    {
        var args = new List<string>();

        if (Mirror) args.Add("/MIR");
        else
        {
            if (CopySubdirectories)
                args.Add(CopyEmptySubdirectories ? "/E" : "/S");
            if (PurgeDestination) args.Add("/PURGE");
        }

        if (MoveFiles) args.Add("/MOVE");
        if (RestartMode) args.Add("/Z");
        if (BackupMode) args.Add("/B");
        if (UnbufferedIO) args.Add("/J");
        if (EfsRawMode) args.Add("/EFSRAW");
        if (FixFileSecurity) args.Add("/SEC");
        if (FixFileTimes) args.Add("/TIMFIX");
        if (RemoveFileAfterCopy) args.Add("/MOV");
        if (FatFileTimes) args.Add("/FFT");
        if (DisableDirectoryTimestamps) args.Add("/DST");
        if (ExcludeJunctions) args.Add("/XJ");
        if (UseUncompressedNetworkIO) args.Add("/NOOFFLOAD");

        if (RetryCount != 3) args.Add($"/R:{RetryCount}");
        if (RetryWaitSeconds != 5) args.Add($"/W:{RetryWaitSeconds}");

        if (MultiThreadCount > 0) args.Add($"/MT:{MultiThreadCount}");
        if (!string.IsNullOrWhiteSpace(IpPort)) args.Add($"/IPG:{IpPort}");

        if (IncludePatterns.Length > 0)
            args.Add($"/IF {string.Join(" ", IncludePatterns.Select(p => $"\"{p}\""))}");
        if (ExcludePatterns.Length > 0)
            args.Add($"/XF {string.Join(" ", ExcludePatterns.Select(p => $"\"{p}\""))}");
        if (ExcludeDirectories.Length > 0)
            args.Add($"/XD {string.Join(" ", ExcludeDirectories.Select(d => $"\"{d}\""))}");

        if (MaxDepth > 0) args.Add($"/LEV:{MaxDepth}");
        if (MinFileSize > 0) args.Add($"/MIN:{MinFileSize}");
        if (MaxFileSize > 0) args.Add($"/MAX:{MaxFileSize}");

        if (MinLastAccessDate.HasValue)
            args.Add($"/MINLAD:{MinLastAccessDate.Value:yyyyMMdd}");
        if (MaxLastAccessDate.HasValue)
            args.Add($"/MAXLAD:{MaxLastAccessDate.Value:yyyyMMdd}");

        if (IncludeAttributes) args.Add("/IA:RASHCNETO");
        if (ExcludeAttributes) args.Add("/XA:SH");

        if (NoDirectoryList) args.Add("/NODL");
        if (NoFileClasses) args.Add("/NJH");
        if (NoFileSizes) args.Add("/NJS");
        if (NoFileList) args.Add("/NFL");
        if (NoProgress) args.Add("/NP");
        if (ShowEstimatedTimeOfArrival) args.Add("/ETA");
        if (VerboseOutput) args.Add("/V");
        if (IncludeSourceTimestamps) args.Add("/TS");
        if (ProduceUnicodeLog) args.Add("/UNILOG+:-");
        if (PrintFullPaths) args.Add("/FP");

        if (DryRun) args.Add("/L");

        if (LogFile)
        {
            if (!string.IsNullOrWhiteSpace(LogPath))
                args.Add($"/LOG:\"{LogPath}\"");
            else
                args.Add("/LOG+:-");
        }
        else if (AppendLogFile && !string.IsNullOrWhiteSpace(LogPath))
        {
            args.Add($"/LOG+:{LogPath}");
        }

        return string.Join(" ", args);
    }
}
