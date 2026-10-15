using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder.Services;

public class VoicebankBuilder
{
    private readonly string? _enginePackage;
    private readonly IProgress<string> _progress;

    public VoicebankBuilder(string? enginePackage, IProgress<string> progress)
    {
        _enginePackage = enginePackage;
        _progress = progress;
    }

    public async Task BuildAsync(string outputDir, string characterName)
    {
        var tts = new TtsSynthesizer(_enginePackage);
        try
        {
            _progress.Report("正在初始化 TTS 引擎...");
            await tts.InitializeAsync();

            Directory.CreateDirectory(outputDir);
            var rawDir = Path.Combine(outputDir, "_raw");
            Directory.CreateDirectory(rawDir);

            var otoEntries = new List<OtoEntry>();
            int total = JapaneseCvReclist.CvList.Count;
            int current = 0;
            int failed = 0;

            foreach (var kv in JapaneseCvReclist.CvList)
            {
                current++;
                var kana = kv.Key;
                var romaji = kv.Value;

                _progress.Report("[" + current + "/" + total + "] 合成: "
                    + kana + " (" + romaji + ")");

                var rawPath = Path.Combine(rawDir, romaji + ".wav");
                var finalPath = Path.Combine(outputDir, romaji + ".wav");

                try
                {
                    await tts.SynthesizeToFileAsync(kana, rawPath);

                    if (File.Exists(rawPath) && new FileInfo(rawPath).Length > 44)
                    {
                        WavProcessor.ConvertToUtauFormat(rawPath, finalPath);
                        var entry = OtoGenerator.GenerateEntry(finalPath, kana);
                        otoEntries.Add(entry);
                    }
                    else
                    {
                        failed++;
                        _progress.Report("[" + current + "/" + total + "] "
                            + kana + ": 无输出，跳过");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    _progress.Report("[" + current + "/" + total + "] "
                        + kana + " 失败: " + ex.Message);
                }
            }

            _progress.Report("正在写入 oto.ini...");
            await OtoGenerator.WriteOtoIniAsync(
                otoEntries, Path.Combine(outputDir, "oto.ini"));

            _progress.Report("正在写入 character.txt...");
            await OtoGenerator.WriteCharacterTxtAsync(outputDir, characterName);

            try { Directory.Delete(rawDir, true); } catch { }

            _progress.Report("完成！共生成 " + otoEntries.Count
                + " 个音节，失败 " + failed + " 个");
        }
        finally
        {
            tts.Dispose();
        }
    }
}
