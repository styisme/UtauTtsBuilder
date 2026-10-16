using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder.Services;

public static class QuickCompareService
{
    public class CompareResult
    {
        public string EngineLabel { get; set; } = "";
        public string? WavPath { get; set; }
        public string? Error { get; set; }
    }

    // 探测顺序：日语 → 中文 → 英语
    private static readonly string[] FallbackLanguages = { "ja-JP", "zh-CN", "en-US" };

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
                    // 本地引擎：逐个尝试候选语言
                    var candidateLangs = BuildCandidates(languageCode);
                    bool success = false;
                    string? lastError = null;

                    foreach (var lang in candidateLangs)
                    {
                        var local = new TtsSynthesizer(package);
                        try
                        {
                            await local.InitializeAsync(lang, speechRate, pitch);
                            await local.SynthesizeToFileAsync(text, rawPath);
                            success = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            lastError = ex.Message;
                        }
                        finally
                        {
                            local.Dispose();
                        }
                    }

                    if (!success)
                    {
                        result.Error = lastError ?? "unsupported";
                        results.Add(result);
                        continue;
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

    private static List<string> BuildCandidates(string preferred)
    {
        var list = new List<string>();
        if (!string.IsNullOrEmpty(preferred)) list.Add(preferred);
        foreach (var lang in FallbackLanguages)
        {
            if (!list.Contains(lang)) list.Add(lang);
        }
        return list;
    }
}
