using System.Diagnostics;
using System.Text.RegularExpressions;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Services;

public sealed partial class RoboCopyService
{
    private static readonly string RoboCopyPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System),
        "robocopy.exe");

    private Process? _currentProcess;

    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*%")]
    private static partial Regex PercentPattern();

    [GeneratedRegex(@"([\d,\.]+)\s*([\w/]*)\s+([\d,\.]+)\s*([\w/]*)")]
    private static partial Regex FileSizePattern();

    [GeneratedRegex(@"^\s*([\d\.]+)%\s")]
    private static partial Regex LinePercentPattern();

    [GeneratedRegex(@"^\s*New File\s+[\w\.]+\s+([\d\.]+)\s+(.+)$")]
    private static partial Regex NewFilePattern();

    [GeneratedRegex(@"^\s*Newer\s+[\w\.]+\s+([\d\.]+)\s+(.+)$")]
    private static partial Regex NewerFilePattern();

    [GeneratedRegex(@"^\s*Older\s+[\w\.]+\s+([\d\.]+)\s+(.+)$")]
    private static partial Regex OlderFilePattern();

    [GeneratedRegex(@"^\s*EXTRA File\s+[\w\.]+\s+([\d\.]+)\s+(.+)$")]
    private static partial Regex ExtraFilePattern();

    public event EventHandler<string>? RawOutputReceived;

    public async Task<RoboCopyResult> ExecuteAsync(
        RoboCopyOptions options,
        IProgress<RoboCopyProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(RoboCopyPath))
            return new RoboCopyResult
            {
                Success = false,
                ExitCode = -1,
                Message = "robocopy.exe not found at expected location."
            };

        var source = options.Source.TrimEnd('\\');
        var dest = options.Destination.TrimEnd('\\');

        if (Directory.Exists(source))
        {
            var folderName = Path.GetFileName(source);
            if (!string.Equals(Path.GetFileName(dest), folderName, StringComparison.OrdinalIgnoreCase))
                dest = Path.Combine(dest, folderName);
        }

        var extraFiles = options.ExtraFiles.Length > 0
            ? " " + string.Join(" ", options.ExtraFiles.Select(f => $"\"{f}\""))
            : string.Empty;

        var arguments = $"\"{source}\" \"{dest}\"{extraFiles} {options.BuildArguments()}";

        var startInfo = new ProcessStartInfo
        {
            FileName = RoboCopyPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        };

        var logLines = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            _currentProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            var outputBuffer = new List<char>();
            long lastParsedBytes = 0;
            long lastParsedTotalBytes = 0;
            long filesCopied = 0;
            long totalFiles = 0;
            double lastPercent = 0;
            string currentFile = string.Empty;
            string speedInfo = string.Empty;

            _currentProcess.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                ParseOutputLine(e.Data, outputBuffer, logLines, progress,
                    ref lastParsedBytes, ref lastParsedTotalBytes,
                    ref filesCopied, ref totalFiles,
                    ref lastPercent, ref currentFile, ref speedInfo);
            };

            _currentProcess.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                logLines.Add($"[ERROR] {e.Data}");
            };

            _currentProcess.Start();
            _currentProcess.BeginOutputReadLine();
            _currentProcess.BeginErrorReadLine();

            await using (cancellationToken.Register(() =>
            {
                try
                {
                    if (!_currentProcess.HasExited)
                        _currentProcess.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) { }
            }))
            {
                await _currentProcess.WaitForExitAsync(cts.Token);
            }

            stopwatch.Stop();

            int exitCode = _currentProcess.ExitCode;
            _currentProcess.Dispose();
            _currentProcess = null;

            return new RoboCopyResult
            {
                Success = exitCode < 8,
                ExitCode = exitCode,
                Message = RoboCopyResult.InterpretExitCode(exitCode),
                TotalFiles = filesCopied,
                TotalBytes = lastParsedBytes,
                Duration = stopwatch.Elapsed,
                LogLines = logLines.AsReadOnly()
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new RoboCopyResult
            {
                Success = false,
                ExitCode = -1,
                Message = "Operation was cancelled by user.",
                Duration = stopwatch.Elapsed,
                LogLines = logLines.AsReadOnly()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new RoboCopyResult
            {
                Success = false,
                ExitCode = -1,
                Message = $"Unexpected error: {ex.Message}",
                Duration = stopwatch.Elapsed,
                LogLines = logLines.AsReadOnly()
            };
        }
        finally
        {
            _currentProcess?.Dispose();
            _currentProcess = null;
        }
    }

    private void ParseOutputLine(
        string line,
        List<char> outputBuffer,
        List<string> logLines,
        IProgress<RoboCopyProgress>? progress,
        ref long bytesCopied,
        ref long totalBytes,
        ref long filesCopied,
        ref long totalFiles,
        ref double lastPercent,
        ref string currentFile,
        ref string speedInfo)
    {
        RawOutputReceived?.Invoke(this, line);

        if (line.Contains('\r'))
        {
            var segments = line.Split('\r', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                ProcessSegment(segment.Trim(), outputBuffer, logLines, progress,
                    ref bytesCopied, ref totalBytes,
                    ref filesCopied, ref totalFiles,
                    ref lastPercent, ref currentFile, ref speedInfo);
            }
        }
        else
        {
            ProcessSegment(line, outputBuffer, logLines, progress,
                ref bytesCopied, ref totalBytes,
                ref filesCopied, ref totalFiles,
                ref lastPercent, ref currentFile, ref speedInfo);
        }
    }

    private void ProcessSegment(
        string segment,
        List<char> outputBuffer,
        List<string> logLines,
        IProgress<RoboCopyProgress>? progress,
        ref long bytesCopied,
        ref long totalBytes,
        ref long filesCopied,
        ref long totalFiles,
        ref double lastPercent,
        ref string currentFile,
        ref string speedInfo)
    {
        if (string.IsNullOrWhiteSpace(segment)) return;

        logLines.Add(segment);

        var percentMatch = LinePercentPattern().Match(segment);
        if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out var pct))
        {
            lastPercent = pct;
        }

        var newFileMatch = NewFilePattern().Match(segment);
        if (newFileMatch.Success)
        {
            currentFile = newFileMatch.Groups[2].Value.Trim();
            filesCopied++;
        }

        var newerMatch = NewerFilePattern().Match(segment);
        if (newerMatch.Success)
        {
            currentFile = newerMatch.Groups[2].Value.Trim();
            filesCopied++;
        }

        var olderMatch = OlderFilePattern().Match(segment);
        if (olderMatch.Success)
        {
            currentFile = olderMatch.Groups[2].Value.Trim();
        }

        var extraMatch = ExtraFilePattern().Match(segment);
        if (extraMatch.Success)
        {
            currentFile = extraMatch.Groups[2].Value.Trim();
        }

        if (segment.Contains("Bytes :", StringComparison.OrdinalIgnoreCase) ||
            segment.Contains("Bytes:", StringComparison.OrdinalIgnoreCase))
        {
            ParseSummaryLine(segment, ref totalFiles, ref totalBytes);
        }

        progress?.Report(new RoboCopyProgress
        {
            Percent = lastPercent,
            CurrentFile = currentFile,
            BytesCopied = bytesCopied,
            TotalBytes = totalBytes,
            FilesCopied = filesCopied,
            TotalFiles = totalFiles,
            SpeedInfo = speedInfo,
            RawLine = segment
        });
    }

    private static void ParseSummaryLine(string line, ref long totalFiles, ref long totalBytes)
    {
        var match = Regex.Match(line, @"Bytes\s*:\s*([\d,]+)");
        if (match.Success && long.TryParse(match.Groups[1].Value.Replace(",", ""), out var bytes))
            totalBytes = bytes;

        match = Regex.Match(line, @"Files\s*:\s*([\d,]+)");
        if (match.Success && long.TryParse(match.Groups[1].Value.Replace(",", ""), out var files))
            totalFiles = files;
    }

    public void Cancel()
    {
        try
        {
            if (_currentProcess is { HasExited: false })
                _currentProcess.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) { }
    }

    public static bool IsRoboCopyAvailable() => File.Exists(RoboCopyPath);
}
