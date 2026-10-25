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
    private readonly CrossLingualMode _crossLingual;

    public VoicebankBuilder(
        string? enginePackage, IProgress<string> progress,
        VoicebankLanguage language = VoicebankLanguage.Japanese,
        string? iconPath = null, string? portraitPath = null, string? author = null,
        float speechRate = 0.85f, float pitch = 1.0f, bool skipExisting = true,
        CrossLingualMode crossLingual = CrossLingualMode.Native)
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
        _crossLingual = crossLingual;
    }

    public async Task BuildAsync(string outputDir, string characterName)
    {
        bool isCloud = CloudConfigService.IsCloudEngine(_enginePackage);
        bool isLocalAi = _enginePackage == "__local_ai";
        
        var cloudProvider = CloudConfigService.GetProvider(_enginePackage);

        TtsSynthesizer? localTts = null;
        CloudTtsEngine? cloudTts = null;
        LocalAiTtsEngine? aiTts = null;

        var log = new StringBuilder();
        log.AppendLine("UTAU Voicebank Builder - Build Log");
        log.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        log.AppendLine("Engine: " + (_enginePackage ?? "(default)"));
        log.AppendLine("IsCloud: " + isCloud);
        log.AppendLine("IsLocalAI: " + isLocalAi);
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
            else if (isLocalAi)
            {
                aiTts = new LocalAiTtsEngine();
                await aiTts.InitializeAsync(langCode);
            }
            else
            {
                localTts = new TtsSynthesizer(_enginePackage);
                await localTts.InitializeAsync(langCode, _speechRate, _pitch, AppSettings.ForceBuild);
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
                var finalTtsText = CrossLingualService.Transform(entry.TtsText, langCode, _crossLingual);
                _progress.Report(Loc.F("prog_synth_fmt", current, total, entry.Alias));

                var rawPath = Path.Combine(rawDir, entry.FileName + ".wav");
                var finalPath = Path.Combine(outputDir, entry.FileName + ".wav");

                try
                {
                    if (_skipExisting
                        && File.Exists(finalPath)
                        && new FileInfo(finalPath).Length > 44)
                    {
                        skipped++;
                        log.AppendLine($"[{current}/{total}] SKIP  {entry.Alias}");
                        otoEntries.Add(OtoGenerator.GenerateEntry(finalPath, entry.Alias, entry.IsTransition));
                        continue;
                    }

                    if (ttsCache.TryGetValue(finalTtsText, out var srcPath) && File.Exists(srcPath))
                    {
                        File.Copy(srcPath, finalPath, true);
                        shared++;
                        log.AppendLine($"[{current}/{total}] SHARE {entry.Alias} (TTS: {finalTtsText})");
                    }
                    else
                    {
                        sw.Restart();

                        if (isCloud && cloudTts != null)
                            await cloudTts.SynthesizeToFileAsync(finalTtsText, rawPath);
                        else if (isLocalAi && aiTts != null)
                            await aiTts.SynthesizeToFileAsync(finalTtsText, rawPath);
                        else if (localTts != null)
                            await localTts.SynthesizeToFileAsync(finalTtsText, rawPath);
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
                        ttsCache[finalTtsText] = finalPath;
                        log.AppendLine($"[{current}/{total}] OK    {entry.Alias} ({sw.ElapsedMilliseconds}ms) (TTS: {finalTtsText})");
                    }

                    otoEntries.Add(OtoGenerator.GenerateEntry(finalPath, entry.Alias, entry.IsTransition));
                }
                catch (Exception ex)
                {
                    failed++;
                    log.AppendLine($"[{current}/{total}] FAIL  {entry.Alias}: {ex.Message}");
                    _progress.Report(Loc.F("prog_failed_fmt", current, total, entry.Alias, ex.Message));
                }
            }

            _progress.Report(Loc.T("prog_write_oto"));
            await OtoGenerator.WriteOtoIniAsync(otoEntries, Path.Combine(outputDir, "oto.ini"));

            string? iconFileName = null, portraitFileName = null;
            if (!string.IsNullOrEmpty(_iconPath) && File.Exists(_iconPath))
            {
                iconFileName = "icon" + Path.GetExtension(_iconPath);
                try { File.Copy(_iconPath, Path.Combine(outputDir, iconFileName), true); } catch { }
            }
            if (!string.IsNullOrEmpty(_portraitPath) && File.Exists(_portraitPath))
            {
                portraitFileName = "portrait" + Path.GetExtension(_portraitPath);
                try { File.Copy(_portraitPath, Path.Combine(outputDir, portraitFileName), true); } catch { }
            }

            await OtoGenerator.WriteCharacterTxtAsync(outputDir, characterName, iconFileName, _author);
            await OtoGenerator.WriteCharacterYamlAsync(outputDir, characterName, iconFileName, portraitFileName, _author);

            log.AppendLine(new string('-', 60));
            log.AppendLine($"Total: {total}, OK: {total - failed - skipped - shared}, Shared: {shared}, Skipped: {skipped}, Failed: {failed}");
            await File.WriteAllTextAsync(Path.Combine(outputDir, "_log.txt"), log.ToString(), new UTF8Encoding(false));

            try { Directory.Delete(rawDir, true); } catch { }
            _progress.Report(Loc.F("prog_done_fmt", otoEntries.Count, shared + skipped, failed));
        }
        finally
        {
            localTts?.Dispose();
            aiTts?.Dispose();
        }
    }
}
