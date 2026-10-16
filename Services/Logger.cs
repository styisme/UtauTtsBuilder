using System.Text;

namespace UtauTtsBuilder.Services;

public static class Logger
{
    private static readonly object _lock = new();
    private static string? _logDir;
    private static string? _currentFile;
    private static bool _initialized;

    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            _logDir = Path.Combine(FileSystem.AppDataDirectory, "logs");
            Directory.CreateDirectory(_logDir);
            _currentFile = Path.Combine(_logDir,
                $"app_{DateTime.Now:yyyyMMdd}.log");
            CleanupOldLogs();
            Info("Logger", "===== Session started =====");
            Info("Logger", "App version: " + (AppInfo.Current.VersionString ?? "?"));
            Info("Logger", "Log file: " + _currentFile);
        }
        catch { }
    }

    public static void Info(string tag, string msg) => Write("INFO ", tag, msg);
    public static void Warn(string tag, string msg) => Write("WARN ", tag, msg);
    public static void Error(string tag, string msg) => Write("ERROR", tag, msg);

    public static void Error(string tag, Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append(ex.GetType().Name).Append(": ").AppendLine(ex.Message);
        if (!string.IsNullOrEmpty(ex.StackTrace)) sb.AppendLine(ex.StackTrace);
        if (ex.InnerException != null)
        {
            sb.Append("--> Inner: ")
              .Append(ex.InnerException.GetType().Name)
              .Append(": ")
              .AppendLine(ex.InnerException.Message);
        }
        Write("ERROR", tag, sb.ToString());
    }

    private static void Write(string level, string tag, string msg)
    {
        if (!_initialized) Init();
        if (string.IsNullOrEmpty(_logDir)) return;

        try
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(_currentFile))
                    _currentFile = Path.Combine(_logDir,
                        $"app_{DateTime.Now:yyyyMMdd}.log");

                var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] [{tag}] {msg}{Environment.NewLine}";
                File.AppendAllText(_currentFile, line, Encoding.UTF8);
            }
        }
        catch { }
    }

    public static string GetCurrentFilePath() => _currentFile ?? "";

    public static string GetLogDir() => _logDir ?? "";

    public static string ReadRecent(int maxLines = 800)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentFile) || !File.Exists(_currentFile))
                return "(no log yet)";
            var lines = File.ReadAllLines(_currentFile, Encoding.UTF8);
            if (lines.Length <= maxLines) return string.Join("\n", lines);
            return string.Join("\n",
                lines.Skip(lines.Length - maxLines));
        }
        catch (Exception ex)
        {
            return "(failed to read log: " + ex.Message + ")";
        }
    }

    public static void Clear()
    {
        try
        {
            if (!string.IsNullOrEmpty(_currentFile) && File.Exists(_currentFile))
                File.Delete(_currentFile);
            Info("Logger", "Log cleared");
        }
        catch { }
    }

    private static void CleanupOldLogs()
    {
        try
        {
            if (string.IsNullOrEmpty(_logDir)) return;
            var cutoff = DateTime.Now.AddDays(-7);
            foreach (var f in Directory.GetFiles(_logDir, "*.log"))
            {
                try
                {
                    if (File.GetCreationTime(f) < cutoff)
                        File.Delete(f);
                }
                catch { }
            }
        }
        catch { }
    }
}
