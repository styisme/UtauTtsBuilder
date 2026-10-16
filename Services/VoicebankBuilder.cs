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
        string? enginePackage, IProgress<string> progress,
        VoicebankLanguage language = VoicebankLanguage.Japanese,
        string? iconPath = null, string? portraitPath = null, string? author = null)
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
            _progress.Report(Loc.F("prog_init_engine_fmt", langCode));
            await tts.InitializeAsync(langCode);

            Directory.CreateDirectory(outputDir);
            var rawDir = Path.Combine(outputDir, "_raw");
            Directory.CreateDirectory(rawDir);

            var reclist = ReclistProvider.GetReclist(_language);
            var otoEntries = new List<OtoEntry>();
            var ttsCache = new Dictionary<string, string>();
            int total = reclist.Count, current = 0, failed = 0, shared = 0;

            foreach (var entry in reclist)
            {
                current++;
                _progress.Report(Loc.F("prog_synth_fmt", current, total, entry.Alias));

                var rawPath = Path.Combine(rawDir, entry.FileName + ".wav");
                var finalPath = Path.Combine(outputDir, entry.FileName + ".wav");

                try
                {
                    if (ttsCache.TryGetValue(entry.TtsText, out var srcPath)
                        && File.Exists(srcPath))
                    {
                        File.Copy(srcPath, finalPath, true);
                        shared++;
                    }
                    else
                    {
                        await tts.SynthesizeToFileAsync(entry.TtsText, rawPath);
                        if (!File.Exists(rawPath) || new FileInfo(rawPath).Length <= 44)
                        {
                            failed++;
                            _progress.Report(Loc.F("prog_no_output_fmt", current, total, entry.Alias));
                            continue;
                        }
                        WavProcessor.ConvertToUtauFormat(rawPath, finalPath);
                        ttsCache[entry.TtsText] = finalPath;
                    }

                    otoEntries.Add(OtoGenerator.GenerateEntry(
                        finalPath, entry.Alias, entry.IsTransition));
                }
                catch (Exception ex)
                {
                    failed++;
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

            try { Directory.Delete(rawDir, true); } catch { }

            _progress.Report(Loc.F("prog_done_fmt",
                otoEntries.Count, shared, failed));
        }
        finally { tts.Dispose(); }
    }
}
