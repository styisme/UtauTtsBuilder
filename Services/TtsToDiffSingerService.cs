using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder.Services;

public class DsCorpusRequest
{
    public string Text { get; set; } = "";
    public string LanguageCode { get; set; } = "ja-JP";
}

public static class TtsToDiffSingerService
{
    public static async Task<string> GenerateMixedCorpusAsync(
        List<DsCorpusRequest> requests,
        string enginePackage,
        float speechRate,
        float pitch,
        bool doAugmentation,
        string outputZipPath,
        IProgress<string> progress)
    {
        var tmpDir = Path.Combine(FileSystem.CacheDirectory, "corpus_" + Guid.NewGuid().ToString("N"));
        var rawDir = Path.Combine(tmpDir, "raw");
        Directory.CreateDirectory(rawDir);

        var durBuilder = new StringBuilder();
        var f0Builder = new StringBuilder();
        var energyBuilder = new StringBuilder();
        var transcriptions = new StringBuilder();

        bool isCloud = CloudConfigService.IsCloudEngine(enginePackage);
        TtsSynthesizer? localTts = null;
        CloudTtsEngine? cloudTts = null;

        if (isCloud)
        {
            cloudTts = new CloudTtsEngine(CloudConfigService.GetProvider(enginePackage));
        }
        else
        {
            localTts = new TtsSynthesizer(enginePackage);
        }

        int globalIndex = 0;

        try
        {
            foreach (var req in requests)
            {
                if (isCloud) await cloudTts!.InitializeAsync(req.LanguageCode);
                else await localTts!.InitializeAsync(req.LanguageCode, speechRate, pitch, AppSettings.ForceBuild);

                // 1. 按标点切分长文本
                var sentences = Regex.Split(req.Text, @"(?<=[。！？.!?\n])")
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                // 2. 二次切分：强制不超过 20 个字，确保所有手机 TTS 都能处理
                var chunks = new List<string>();
                foreach (var s in sentences)
                {
                    var sub = Regex.Split(s, @"(?<=[，、；,;])").Where(x => !string.IsNullOrWhiteSpace(x));
                    foreach (var part in sub)
                    {
                        var p = part.Trim();
                        if (p.Length > 20)
                        {
                            for (int k = 0; k < p.Length; k += 20)
                                chunks.Add(p.Substring(k, Math.Min(20, p.Length - k)));
                        }
                        else
                        {
                            chunks.Add(p);
                        }
                    }
                }

                if (chunks.Count == 0) continue;

                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    var baseName = $"corpus_{req.LanguageCode.Replace("-", "")}_{globalIndex:D4}";
                    globalIndex++;

                    progress.Report($"[{baseName}] 合成: {chunk.Substring(0, Math.Min(10, chunk.Length))}...");

                    // ★ 关键修复：临时文件加 _temp 后缀，防止被误删
                    var tempPath = Path.Combine(rawDir, baseName + "_temp.wav");
                    var finalPath = Path.Combine(rawDir, baseName + ".wav");

                    try
                    {
                        if (isCloud) await cloudTts!.SynthesizeToFileAsync(chunk, tempPath);
                        else await localTts!.SynthesizeToFileAsync(chunk, tempPath);
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"[跳过] TTS 不支持或出错: {ex.Message}");
                        continue;
                    }

                    if (!File.Exists(tempPath) || new FileInfo(tempPath).Length <= 44)
                    {
                        progress.Report($"[跳过] TTS 没有生成音频文件");
                        continue;
                    }

                    try
                    {
                        // 转换格式并保存到 finalPath
                        WavProcessor.ConvertToUtauFormat(tempPath, finalPath);
                        
                        // 这时候删除 tempPath 就不会影响 finalPath 了
                        if (File.Exists(tempPath)) File.Delete(tempPath);

                        var info = WavProcessor.ReadWav(finalPath);
                        var samples = WavProcessor.DecodeToFloat(info.RawData, info.BitsPerSample, info.Channels);
                        var durMs = WavProcessor.GetDurationMs(finalPath);

                        var phonemes = ConvertTextToPhonemes(chunk);
                        transcriptions.AppendLine($"raw/{baseName}.wav|{phonemes}");
                        durBuilder.AppendLine($"{baseName}.wav|{durMs:F0}");

                        var f0 = ExtractF0(samples, info.SampleRate, 10);
                        f0Builder.AppendLine($"{baseName}.wav={string.Join(",", f0)}");

                        var energy = ExtractEnergy(samples, info.SampleRate, 10);
                        energyBuilder.AppendLine($"{baseName}.wav={string.Join(",", energy)}");

                        if (doAugmentation)
                        {
                            var augName = baseName + "_aug";
                            var augPath = Path.Combine(rawDir, augName + ".wav");
                            var augSamples = ResampleForPitch(samples, 1.122f);
                            WavProcessor.WriteWav16(augPath, augSamples, info.SampleRate, 1);
                            
                            transcriptions.AppendLine($"raw/{augName}.wav|{phonemes}");
                            durBuilder.AppendLine($"{augName}.wav|{durMs:F0}");
                            f0Builder.AppendLine($"{augName}.wav={string.Join(",", ExtractF0(augSamples, info.SampleRate, 10))}");
                            energyBuilder.AppendLine($"{augName}.wav={string.Join(",", ExtractEnergy(augSamples, info.SampleRate, 10))}");
                        }
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"[处理失败] {ex.Message}");
                    }
                }
            }
        }
        finally
        {
            localTts?.Dispose();
        }

        progress.Report("写入文本与特征文件...");
        File.WriteAllText(Path.Combine(tmpDir, "transcriptions.txt"), transcriptions.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(tmpDir, "ph_durs.txt"), durBuilder.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(tmpDir, "f0.txt"), f0Builder.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(tmpDir, "energy.txt"), energyBuilder.ToString(), Encoding.UTF8);

        var config = $@"# DiffSinger Config - Corpus Mode
raw_data_dir: raw
binary_data_dir: binary
work_dir: checkpoints/corpus

sample_rate: 44100
hop_size: 512
fft_size: 2048
win_size: 2048
";
        File.WriteAllText(Path.Combine(tmpDir, "config.yaml"), config, Encoding.UTF8);

        progress.Report("打包压缩...");
        if (File.Exists(outputZipPath)) File.Delete(outputZipPath);
        ZipFile.CreateFromDirectory(tmpDir, outputZipPath);
        
        try { Directory.Delete(tmpDir, true); } catch { }
        return outputZipPath;
    }

    private static string ConvertTextToPhonemes(string text)
    {
        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (char.IsPunctuation(c) || char.IsWhiteSpace(c)) continue;
            sb.Append(c);
        }
        return sb.Length > 0 ? sb.ToString() : "sp";
    }

    private static float[] ResampleForPitch(float[] samples, float ratio)
    {
        int newLength = (int)(samples.Length / ratio);
        var result = new float[newLength];
        for (int i = 0; i < newLength; i++)
        {
            float srcPos = i * ratio;
            int i0 = (int)srcPos;
            int i1 = Math.Min(i0 + 1, samples.Length - 1);
            float frac = srcPos - i0;
            result[i] = samples[i0] * (1 - frac) + samples[i1] * frac;
        }
        return result;
    }

    private static List<string> ExtractEnergy(float[] samples, int sr, int hopMs)
    {
        int hop = (int)(sr * (hopMs / 1000.0));
        int window = (int)(sr * 0.04);
        var list = new List<string>();
        for (int i = 0; i <= samples.Length - window; i += hop)
        {
            float sumSq = 0;
            for (int j = 0; j < window; j++) sumSq += samples[i + j] * samples[i + j];
            list.Add(Math.Sqrt(sumSq / window).ToString("F4"));
        }
        return list;
    }

    private static List<string> ExtractF0(float[] samples, int sr, int hopMs)
    {
        int hop = (int)(sr * (hopMs / 1000.0));
        int window = (int)(sr * 0.04);
        int minLag = sr / 800;
        int maxLag = sr / 80;
        var f0List = new List<string>();

        for (int i = 0; i <= samples.Length - window; i += hop)
        {
            float bestCorr = 0; int bestLag = 0;
            for (int lag = minLag; lag <= maxLag && lag < window; lag++)
            {
                float sum = 0;
                for (int j = 0; j < window - lag; j++) sum += samples[i + j] * samples[i + j + lag];
                if (sum > bestCorr) { bestCorr = sum; bestLag = lag; }
            }
            if (bestLag > 0 && bestCorr > 0.01f) f0List.Add($"{(float)sr / bestLag:F1}");
            else f0List.Add("0");
        }
        for (int i = 1; i < f0List.Count - 1; i++)
        {
            if (f0List[i] != "0" && f0List[i - 1] != "0" && f0List[i + 1] != "0")
            {
                float p = float.Parse(f0List[i - 1]), c = float.Parse(f0List[i]), n = float.Parse(f0List[i + 1]);
                if (Math.Abs(c - p) > 50 && Math.Abs(c - n) > 50) f0List[i] = ((p + n) / 2).ToString("F1");
            }
        }
        return f0List;
    }
}
