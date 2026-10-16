namespace UtauTtsBuilder.Services;

public class VoicebankInfo
{
    public string Name { get; set; } = "";
    public string DirectoryPath { get; set; } = "";
    public string CharacterTxtPath { get; set; } = "";
    public int WavCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime CreatedTime { get; set; }
    public string? ZipPath { get; set; }

    public string SizeDisplay
    {
        get
        {
            double mb = TotalSizeBytes / 1024.0 / 1024.0;
            if (mb < 1) return (TotalSizeBytes / 1024.0).ToString("F0") + " KB";
            if (mb < 1024) return mb.ToString("F1") + " MB";
            return (mb / 1024.0).ToString("F2") + " GB";
        }
    }
}

public static class VoicebankScanner
{
    private static string GetExternalRoot()
    {
#if ANDROID
        return global::Android.OS.Environment
            .ExternalStorageDirectory?.AbsolutePath
            ?? FileSystem.AppDataDirectory;
#else
        return FileSystem.AppDataDirectory;
#endif
    }

    public static List<VoicebankInfo> ScanAll()
    {
        var result = new List<VoicebankInfo>();
        var root = GetExternalRoot();

        var defaultDir = Path.Combine(root, "Download", "UtauTtsBuilder");
        ScanDir(defaultDir, result);

        return result.OrderByDescending(v => v.CreatedTime).ToList();
    }

    private static void ScanDir(string baseDir, List<VoicebankInfo> list)
    {
        if (!Directory.Exists(baseDir)) return;

        foreach (var dir in Directory.GetDirectories(baseDir))
        {
            var hasOto = File.Exists(Path.Combine(dir, "oto.ini"));
            var hasChar = File.Exists(Path.Combine(dir, "character.txt"));
            if (!hasOto && !hasChar) continue;

            try
            {
                var info = new VoicebankInfo
                {
                    Name = Path.GetFileName(dir),
                    DirectoryPath = dir,
                    CharacterTxtPath = Path.Combine(dir, "character.txt"),
                    CreatedTime = Directory.GetCreationTime(dir)
                };

                long totalSize = 0;
                int wavCount = 0;
                foreach (var f in Directory.GetFiles(dir, "*.wav"))
                {
                    wavCount++;
                    try { totalSize += new FileInfo(f).Length; } catch { }
                }
                info.WavCount = wavCount;
                info.TotalSizeBytes = totalSize;

                var zipPath = dir.TrimEnd('/', (char)92) + ".zip";
                if (File.Exists(zipPath)) info.ZipPath = zipPath;

                list.Add(info);
            }
            catch { }
        }
    }
}
