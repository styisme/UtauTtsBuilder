using System.Diagnostics;
using System.Text;
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
    private readonly float _speechRate;
    private readonly float _pitch;
    private readonly bool _skipExisting;

    public VoicebankBuilder(
        string? enginePackage, IProgress<string> progress,
        VoicebankLanguage language = VoicebankLanguage.Japanese,
        string? iconPath = null, string? portraitPath = null, string? author = null,
        float speechRate = 0.85f, float pitch = 1.0f, bool skipExisting = true)
    {
        _enginePackage = enginePackage;
        _progress = progress;
        _language = language;
        _iconPath = iconPath;
        _portraitPath = portraitPath;
        _author = author;
        _speechRate = speechRate;
        _pitch = pitch;
        _skipExisting = skipExisting;
    }

    public async Task BuildAsync(string outputDir, string characterName)
    {
        bool isCloud = CloudConfigService.IsCloudEngine(_enginePackage);
        var cloudProvider = CloudConfigService.GetProvider(_enginePackage);

        TtsSynthesizer? localTts = null;
        CloudTtsEngine? cloudTts = null;

        var log = new StringBuilder();
        log.AppendLine("UTAU Voicebank Builder - Build Log");
        log.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        log.AppendLine("Engine: " + (_enginePackage ?? "(default)"));
        log.AppendLine("IsCloud: " + isCloud);
        log.AppendLine("Language: " + _language);
        log.AppendLine("SpeechRate: " + _speechRate.ToString("F2"));
        log.AppendLine("Pitch: " + _pitch.ToString("F2"));
        log.AppendLine("SkipExisting: " + _skipExisting);
        log.AppendLine(new string('-', 60));

        try
        {
            var langCode = ReclistProvider.GetLanguageCode(_language);
            _progress.Report(Loc.F("prog_init_engine_fmt", langCode));

            if (isCloud)
            {
                cloudTts = new CloudTtsEngine(cloudProvider);
                await cloudTts.InitializeAsync(langCode);
            }
            else
            {
                localTts = new TtsSynthesizer(_enginePackage);
                await localTts.InitializeAsync(langCode, _speechRate, _pitch);
            }

            Directory.CreateDirectory(outputDir);
            var rawDir = Path.Combine(outputDir, "_raw");
            Directory.CreateDirectory(rawDir);

            var reclist = ReclistProvider.GetReclist(_language);
            var otoEntries = new List<OtoEntry>();
            var ttsCache = new Dictionary<string, string>();
            int total = reclist.Count, current = 0;
            int failed = 0, shared = 0, skipped = 0;
            var sw = new Stopwatch();

            foreach (var entry in reclist)
            {
                current++;
                _progress.Report(Loc.F("prog_synth_fmt", current, total, entry.Alias));

                var rawPath = Path.Combine(rawDir, entry.FileName + ".wav");
                var finalPath = Path.Combine(outputDir, entry.FileName + ".wav");

                try
                {
                    // 断点续传
                    if (_skipExisting
                        && File.Exists(finalPath)
                        && new FileInfo(finalPath).Length > 44)
                    {
                        skipped++;
                        log.AppendLine($"[{current}/{total}] SKIP  {entry.Alias} -> {entry.FileName}.wav");
                        otoEntries.Add(OtoGenerator.GenerateEntry(
                            finalPath, entry.Alias, entry.IsTransition));
                        continue;
                    }

                    if (ttsCache.TryGetValue(entry.TtsText, out var srcPath)
                        && File.Exists(srcPath))
                    {
                        File.Copy(srcPath, finalPath, true);
                        shared++;
                        log.AppendLine($"[{current}/{total}] SHARE {entry.Alias} -> {entry.FileName}.wav");
                    }
                    else
                    {
                        sw.Restart();

                        // ★ 核心：本地 / 云端分发
                        if (isCloud && cloudTts != null)
                            await cloudTts.SynthesizeToFileAsync(entry.TtsText, rawPath);
                        else if (localTts != null)
                            await localTts.SynthesizeToFileAsync(entry.TtsText, rawPath);
                        else
                            throw new Exception("无可用 TTS 引擎");

                        sw.Stop();

                        if (!File.Exists(rawPath) || new FileInfo(rawPath).Length <= 44)
                        {
                            failed++;
                            log.AppendLine($"[{current}/{total}] EMPTY {entry.Alias}");
                            _progress.Report(Loc.F("prog_no_output_fmt", current, total, entry.Alias));
                            continue;
                        }

                        WavProcessor.ConvertToUtauFormat(rawPath, finalPath);
                        ttsCache[entry.TtsText] = finalPath;
                        log.AppendLine($"[{current}/{total}] OK    {entry.Alias} ({sw.ElapsedMilliseconds}ms)");
                    }

                    otoEntries.Add(OtoGenerator.GenerateEntry(
                        finalPath, entry.Alias, entry.IsTransition));
                }
                catch (Exception ex)
                {
                    failed++;
                    log.AppendLine($"[{current}/{total}] FAIL  {entry.Alias}: {ex.Message}");
                    _progress.Report(Loc.F("prog_failed_fmt", current, total, entry.Alias, ex.Message));
                }
            }

            _progress.Report(Loc.T("prog_write_oto"));
            await OtoGenerator.WriteOtoIniAsync(
                otoEntries, Path.Combine(outputDir, "oto.ini"));

            string? iconFileName = null, portraitFileName = null;

            if (!string.IsNullOrEmpty(_iconPath) && File.Exists(_iconPath))
            {
                iconFileName = "icon" + Path.GetExtension(_iconPath);
                try
                {
                    File.Copy(_iconPath, Path.Combine(outputDir, iconFileName), true);
                    _progress.Report(Loc.F("prog_copy_icon_fmt", iconFileName));
                    log.AppendLine("Icon: " + iconFileName);
                }
                catch (Exception ex)
                {
                    _progress.Report(Loc.F("prog_icon_copy_failed_fmt", ex.Message));
                    iconFileName = null;
                }
            }

            if (!string.IsNullOrEmpty(_portraitPath) && File.Exists(_portraitPath))
            {
                portraitFileName = "portrait" + Path.GetExtension(_portraitPath);
                try
                {
                    File.Copy(_portraitPath, Path.Combine(outputDir, portraitFileName), true);
                    _progress.Report(Loc.F("prog_copy_portrait_fmt", portraitFileName));
                    log.AppendLine("Portrait: " + portraitFileName);
                }
                catch (Exception ex)
                {
                    _progress.Report(Loc.F("prog_portrait_copy_failed_fmt", ex.Message));
                    portraitFileName = null;
                }
            }

            _progress.Report(Loc.T("prog_write_chartxt"));
            await OtoGenerator.WriteCharacterTxtAsync(
                outputDir, characterName, iconFileName, _author);

            _progress.Report(Loc.T("prog_write_charyaml"));
            await OtoGenerator.WriteCharacterYamlAsync(
                outputDir, characterName, iconFileName, portraitFileName, _author);

            // 日志
            log.AppendLine(new string('-', 60));
            log.AppendLine($"Total: {total}, OK: {total - failed - skipped - shared}, "
                + $"Shared: {shared}, Skipped: {skipped}, Failed: {failed}");
            log.AppendLine("End: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await File.WriteAllTextAsync(
                Path.Combine(outputDir, "_log.txt"), log.ToString(),
                new UTF8Encoding(false));

            try { Directory.Delete(rawDir, true); } catch { }

            _progress.Report(Loc.F("prog_done_fmt",
                otoEntries.Count, shared + skipped, failed));
        }
        finally
        {
            localTts?.Dispose();
        }
    }
}
