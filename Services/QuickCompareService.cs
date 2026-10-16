using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder.Services;

/// <summary>
/// 对多个引擎快速合成同一个音节，用于对比音质。
/// 只合成一个音节，不生成整个声库。
/// </summary>
public static class QuickCompareService
{
    public class CompareResult
    {
        public string EngineLabel { get; set; } = "";
        public string? WavPath { get; set; }
        public string? Error { get; set; }
    }

    public static async Task<List<CompareResult>> RunAsync(
        List<(string Package, string Label)> engines,
        string text,
        string languageCode,
        float speechRate,
        float pitch,
        IProgress<string> progress)
    {
        var results = new List<CompareResult>();
        var tmpDir = Path.Combine(FileSystem.CacheDirectory,
            "cmp_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);

        for (int i = 0; i < engines.Count; i++)
        {
            var (package, label) = engines[i];
            progress.Report($"[{i + 1}/{engines.Count}] {label}");

            var result = new CompareResult { EngineLabel = label };
            var outPath = Path.Combine(tmpDir, $"cmp_{i}.wav");
            var rawPath = Path.Combine(tmpDir, $"cmp_{i}_raw.wav");

            try
            {
                bool isCloud = CloudConfigService.IsCloudEngine(package);

                if (isCloud)
                {
                    var provider = CloudConfigService.GetProvider(package);
                    var cloud = new CloudTtsEngine(provider);
                    await cloud.InitializeAsync(languageCode);
                    await cloud.SynthesizeToFileAsync(text, rawPath);
                }
                else
                {
                    var local = new TtsSynthesizer(package);
                    try
                    {
                        await local.InitializeAsync(languageCode, speechRate, pitch);
                        await local.SynthesizeToFileAsync(text, rawPath);
                    }
                    finally
                    {
                        local.Dispose();
                    }
                }

                if (File.Exists(rawPath) && new FileInfo(rawPath).Length > 44)
                {
                    WavProcessor.ConvertToUtauFormat(rawPath, outPath);
                    result.WavPath = outPath;
                }
                else
                {
                    result.Error = "no output";
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                Logger.Error("QuickCompare", ex);
            }

            results.Add(result);
        }

        return results;
    }
}
