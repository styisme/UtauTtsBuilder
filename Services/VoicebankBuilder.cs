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

            // TTS 文本 → 已生成音频的完整路径（用于共享）
            var ttsCache = new Dictionary<string, string>();

            int total = reclist.Count;
            int current = 0;
            int failed = 0;
            int shared = 0;

            foreach (var entry in reclist)
            {
                current++;
                _progress.Report("[" + current + "/" + total + "] "
                    + entry.Alias + " ← " + entry.TtsText);

                var rawPath = Path.Combine(rawDir, entry.FileName + ".wav");
                var finalPath = Path.Combine(outputDir, entry.FileName + ".wav");

                try
                {
                    if (ttsCache.TryGetValue(entry.TtsText, out var srcPath)
                        && File.Exists(srcPath))
                    {
                        // 复用已有音频
                        File.Copy(srcPath, finalPath, true);
                        shared++;
                    }
                    else
                    {
                        await tts.SynthesizeToFileAsync(entry.TtsText, rawPath);

                        if (!File.Exists(rawPath) || new FileInfo(rawPath).Length <= 44)
                        {
                            failed++;
                            _progress.Report("[" + current + "/" + total + "] "
                                + entry.Alias + ": 无输出，跳过");
                            continue;
                        }

                        WavProcessor.ConvertToUtauFormat(rawPath, finalPath);
                        ttsCache[entry.TtsText] = finalPath;
                    }

                    var otoEntry = OtoGenerator.GenerateEntry(
                        finalPath, entry.Alias, entry.IsTransition);
                    otoEntries.Add(otoEntry);
                }
                catch (Exception ex)
                {
                    failed++;
                    _progress.Report("[" + current + "/" + total + "] "
                        + entry.Alias + " 失败: " + ex.Message);
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

            _progress.Report("完成！共 " + otoEntries.Count + " 个音节，"
                + "共享音频 " + shared + " 个，失败 " + failed + " 个");
        }
        finally
        {
            tts.Dispose();
        }
    }
}
