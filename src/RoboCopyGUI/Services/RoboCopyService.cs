using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Services;

public sealed partial class RoboCopyService
{
    private static readonly string RoboCopyPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System),
        "robocopy.exe");

    private Process? _currentProcess;

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

        if (!options.DryRun)
            FileLogger.Instance?.Info($"RoboCopy cmd: robocopy {arguments}");
        else
            FileLogger.Instance?.Info($"RoboCopy DRY RUN: robocopy {arguments}");

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

            long filesCopied = 0;
            long totalFiles = 0;
            double lastPercent = 0;
            string currentFile = string.Empty;

            _currentProcess.Start();

            await using var cancelReg = cancellationToken.Register(() =>
            {
                try { if (!_currentProcess.HasExited) _currentProcess.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { }
            });

            var stdoutTask = Task.Run(async () =>
            {
                var reader = _currentProcess.StandardOutput;
                var buffer = new char[4096];
                var lineBuffer = new StringBuilder();

                while (true)
                {
                    int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    for (int i = 0; i < bytesRead; i++)
                    {
                        char c = buffer[i];
                        if (c is '\r' or '\n')
                        {
                            if (lineBuffer.Length > 0)
                            {
                                var line = lineBuffer.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    ParseProgressLine(line, logLines, progress,
                                        ref filesCopied, ref totalFiles,
                                        ref lastPercent, ref currentFile);
                                }
                                lineBuffer.Clear();
                            }
                        }
                        else
                        {
                            lineBuffer.Append(c);
                        }
                    }
                }

                if (lineBuffer.Length > 0)
                {
                    var line = lineBuffer.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                        ParseProgressLine(line, logLines, progress,
                            ref filesCopied, ref totalFiles,
                            ref lastPercent, ref currentFile);
                }
            }, cts.Token);

            _currentProcess.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                logLines.Add($"[ERROR] {e.Data}");
            };
            _currentProcess.BeginErrorReadLine();

            await stdoutTask;
            await _currentProcess.WaitForExitAsync(cts.Token);

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
                TotalBytes = lastPercent > 0 ? (long)(lastPercent * 100) : 0,
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

    private void ParseProgressLine(
        string line,
        List<string> logLines,
        IProgress<RoboCopyProgress>? progress,
        ref long filesCopied,
        ref long totalFiles,
        ref double lastPercent,
        ref string currentFile)
    {
        logLines.Add(line);

        if (logLines.Count <= 50)
            FileLogger.Instance?.Debug($"RC: {line}");

        var pctMatch = Regex.Match(line, @"(\d+(?:\.\d+)?)\s*%");
        if (pctMatch.Success && double.TryParse(pctMatch.Groups[1].Value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var pct))
        {
            lastPercent = pct;
            FileLogger.Instance?.Debug($"Progress: {pct}%");
        }

        var fileMatch = Regex.Match(line, @"(?:New File|Newer)\s+\S+\s+(\S+)", RegexOptions.IgnoreCase);
        if (fileMatch.Success)
        {
            currentFile = fileMatch.Groups[1].Value.Trim();
            filesCopied++;
        }

        var filesMatch = Regex.Match(line, @"Files\s*:\s*([\d,]+)");
        if (filesMatch.Success && long.TryParse(filesMatch.Groups[1].Value.Replace(",", ""), out var files))
            totalFiles = files;

        progress?.Report(new RoboCopyProgress
        {
            Percent = lastPercent,
            CurrentFile = currentFile,
            FilesCopied = filesCopied,
            TotalFiles = totalFiles,
            RawLine = line
        });
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
