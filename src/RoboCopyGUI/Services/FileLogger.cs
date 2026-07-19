using System.Text;

namespace RoboCopyGUI.Services;

public sealed class FileLogger : IDisposable
{
    private readonly string _logDirectory;
    private readonly object _lock = new();
    private StreamWriter? _writer;
    private string _currentLogFile = string.Empty;

    public FileLogger()
    {
        _logDirectory = Path.Combine(AppContext.BaseDirectory, "log");
        Directory.CreateDirectory(_logDirectory);
        RotateLogFile();
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);
    public void Error(string message, Exception ex) => Write("ERROR", $"{message}: {ex}");
    public void Debug(string message) => Write("DEBUG", message);

    public void Write(string level, string message)
    {
        lock (_lock)
        {
            try
            {
                EnsureWriter();
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                _writer!.WriteLine(line);
                _writer.Flush();
            }
            catch
            {
                // Silently fail - don't crash the app over logging
            }
        }
    }

    public string LogDirectory => _logDirectory;

    public IReadOnlyList<string> GetRecentLogFiles()
    {
        return Directory.GetFiles(_logDirectory, "*.log")
            .OrderByDescending(f => f)
            .Take(10)
            .Select(Path.GetFileName)
            .ToList()
            .AsReadOnly();
    }

    private void EnsureWriter()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var expectedFile = Path.Combine(_logDirectory, $"robocopygui-{today}.log");

        if (_currentLogFile != expectedFile)
        {
            _writer?.Dispose();
            _currentLogFile = expectedFile;
            _writer = new StreamWriter(expectedFile, append: true, encoding: Encoding.UTF8)
            {
                AutoFlush = true
            };
        }
    }

    private void RotateLogFile()
    {
        var files = Directory.GetFiles(_logDirectory, "*.log")
            .OrderByDescending(f => f)
            .ToList();

        while (files.Count > 30)
        {
            try { File.Delete(files.Last()); } catch { }
            files.RemoveAt(files.Count - 1);
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _writer = null;
    }
}
