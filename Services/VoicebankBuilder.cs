using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder.Services;

public class VoicebankBuilder
{
    private readonly string? _enginePackage;
    private readonly IProgress<string> _progress;
    private readonly VoicebankLanguage _language;
    private readonly string? _iconPath;
    private readonly string? _portraitPath;
    private readonly string? _author;

    public VoicebankBuilder(
        string? enginePackage,
        IProgress<string> progress,
        VoicebankLanguage language = VoicebankLanguage.Japanese,
        string? iconPath = null,
        string? portraitPath = null,
        string? author = null)
    {
        _enginePackage = enginePackage;
        _progress = progress;
        _language = language;
        _iconPath = iconPath;
        _portraitPath = portraitPath;
        _author = author;
    }

    public async Task BuildAsync(string outputDir, string characterName)
    {
        var tts = new TtsSynthesizer(_enginePackage);
        try
        {
            var langCode = ReclistProvider.GetLanguageCode(_language);
            _progress.Report("正在初始化 TTS 引擎（" + langCode + "）...");
            await tts.InitializeAsync(langCode);

            Directory.CreateDirectory(outputDir);
            var rawDir = Path.Combine(outputDir, "_raw");
            Directory.CreateDirectory(rawDir);

            var reclist = ReclistProvider.GetReclist(_language);
            var otoEntries = new List<OtoEntry>();
            int total = reclist.Count;
            int current = 0;
            int failed = 0;

            foreach (var kv in reclist)
            {
                current++;
                var text = kv.Key;
                var romaji = kv.Value;

                _progress.Report("[" + current + "/" + total + "] 合成: "
                    + text + " (" + romaji + ")");

                var rawPath = Path.Combine(rawDir, romaji + ".wav");
                var finalPath = Path.Combine(outputDir, romaji + ".wav");

                try
                {
                    await tts.SynthesizeToFileAsync(text, rawPath);

                    if (File.Exists(rawPath) && new FileInfo(rawPath).Length > 44)
                    {
                        WavProcessor.ConvertToUtauFormat(rawPath, finalPath);
                        var entry = OtoGenerator.GenerateEntry(finalPath, text);
                        otoEntries.Add(entry);
                    }
                    else
                    {
                        failed++;
                        _progress.Report("[" + current + "/" + total + "] "
                            + text + ": 无输出，跳过");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    _progress.Report("[" + current + "/" + total + "] "
                        + text + " 失败: " + ex.Message);
                }
            }

            _progress.Report("正在写入 oto.ini...");
            await OtoGenerator.WriteOtoIniAsync(
                otoEntries, Path.Combine(outputDir, "oto.ini"));

            string? iconFileName = null;
            string? portraitFileName = null;

            if (!string.IsNullOrEmpty(_iconPath) && File.Exists(_iconPath))
            {
                iconFileName = "icon" + Path.GetExtension(_iconPath);
                try
                {
                    File.Copy(_iconPath,
                        Path.Combine(outputDir, iconFileName), true);
                    _progress.Report("已复制图标: " + iconFileName);
                }
                catch (Exception ex)
                {
                    _progress.Report("图标复制失败: " + ex.Message);
                    iconFileName = null;
                }
            }

            if (!string.IsNullOrEmpty(_portraitPath) && File.Exists(_portraitPath))
            {
                portraitFileName = "portrait" + Path.GetExtension(_portraitPath);
                try
                {
                    File.Copy(_portraitPath,
                        Path.Combine(outputDir, portraitFileName), true);
                    _progress.Report("已复制立绘: " + portraitFileName);
                }
                catch (Exception ex)
                {
                    _progress.Report("立绘复制失败: " + ex.Message);
                    portraitFileName = null;
                }
            }

            _progress.Report("正在写入 character.txt（UTAU）...");
            await OtoGenerator.WriteCharacterTxtAsync(
                outputDir, characterName, iconFileName, _author);

            _progress.Report("正在写入 character.yaml（OpenUtau）...");
            await OtoGenerator.WriteCharacterYamlAsync(
                outputDir, characterName, iconFileName, portraitFileName, _author);

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
