using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _engines = new();
    private string? _iconPath;
    private string? _portraitPath;
    private List<VoicebankLanguage> _languageList = new();

    public MainPage()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ThemeService.Apply();
        ApplyLocalization();
        if (_engines.Count == 0) LoadEngines();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = Loc.T("app_title");
        SubtitleLabel.Text = Loc.T("app_subtitle");
        SecLangLabel.Text = Loc.T("sec_language");
        ImportReclistBtn.Text = Loc.T("btn_import_reclist");
        ClearReclistBtn.Text = Loc.T("btn_clear");
        SecEngineLabel.Text = Loc.T("sec_engine");
        RefreshBtn.Text = Loc.T("btn_refresh_engines");
        SecNameLabel.Text = Loc.T("sec_name");
        NameEntry.Placeholder = Loc.T("ph_name");
        SecIconLabel.Text = Loc.T("sec_icon");
        HintIconLabel.Text = Loc.T("hint_icon");
        PickIconBtn.Text = Loc.T("btn_pick_icon");
        ClearIconBtn.Text = Loc.T("btn_clear");
        SecPortraitLabel.Text = Loc.T("sec_portrait");
        HintPortraitLabel.Text = Loc.T("hint_portrait");
        PickPortraitBtn.Text = Loc.T("btn_pick_portrait");
        ClearPortraitBtn.Text = Loc.T("btn_clear");
        SecAuthorLabel.Text = Loc.T("sec_author");
        AuthorEntry.Placeholder = Loc.T("ph_author");
        SecOutputLabel.Text = Loc.T("sec_output");
        HintOutputLabel.Text = Loc.T("hint_output");
        OutputDirEntry.Placeholder = Loc.T("ph_output");
        ZipLabel.Text = Loc.T("zip_checkbox");
        BuildButton.Text = Loc.T("btn_build");

        if (string.IsNullOrEmpty(_iconPath)) IconLabel.Text = Loc.T("not_selected");
        else IconLabel.Text = Loc.F("selected_fmt", Path.GetFileName(_iconPath));

        if (string.IsNullOrEmpty(_portraitPath)) PortraitLabel.Text = Loc.T("not_selected");
        else PortraitLabel.Text = Loc.F("selected_fmt", Path.GetFileName(_portraitPath));

        if (_engines.Count > 0)
            StatusLabel.Text = Loc.F("engines_found_fmt", _engines.Count);

        InitLanguagePicker();
    }

    private void InitLanguagePicker()
    {
        VoicebankLanguage current = VoicebankLanguage.Japanese;
        if (_languageList.Count > 0 && LanguagePicker.SelectedIndex >= 0
            && LanguagePicker.SelectedIndex < _languageList.Count)
        {
            current = _languageList[LanguagePicker.SelectedIndex];
        }

        _languageList = new List<VoicebankLanguage>
        {
            VoicebankLanguage.Japanese,
            VoicebankLanguage.JapaneseCvvc,
            VoicebankLanguage.JapaneseVcv,
            VoicebankLanguage.Chinese,
            VoicebankLanguage.ChineseCvvc,
            VoicebankLanguage.ChineseVcv,
            VoicebankLanguage.English,
        };

        if (ReclistProvider.HasCustomReclist)
        {
            _languageList.Add(VoicebankLanguage.Custom);
            ClearReclistBtn.IsVisible = true;
        }
        else ClearReclistBtn.IsVisible = false;

        LanguagePicker.ItemsSource = _languageList
            .Select(l => ReclistProvider.GetDisplayName(l))
            .ToList();

        int idx = _languageList.IndexOf(current);
        LanguagePicker.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private VoicebankLanguage GetSelectedLanguage()
    {
        int idx = LanguagePicker.SelectedIndex;
        if (idx >= 0 && idx < _languageList.Count) return _languageList[idx];
        return VoicebankLanguage.Japanese;
    }

    private void LoadEngines()
    {
        try
        {
            _engines = TtsEngineHelper.GetAvailableEngines();
            EnginePicker.ItemsSource = _engines
                .Select(e => e.IsDefault ? "★ " + e.Label : e.Label)
                .ToList();
            var defaultIdx = _engines.FindIndex(e => e.IsDefault);
            if (defaultIdx >= 0) EnginePicker.SelectedIndex = defaultIdx;
            StatusLabel.Text = Loc.F("engines_found_fmt", _engines.Count);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = Loc.F("engines_load_failed_fmt", ex.Message);
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        _engines.Clear();
        LoadEngines();
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }

    private async void OnImportReclistClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = Loc.T("btn_import_reclist")
            });
            if (result == null) return;

            string content;
            using (var stream = await result.OpenReadAsync())
            using (var reader = new StreamReader(stream))
                content = await reader.ReadToEndAsync();

            var reclist = ParseReclist(content, out var langFromFile);
            if (reclist.Count == 0)
            {
                await DisplayAlertAsync(Loc.T("title_parse_failed"),
                    Loc.T("msg_parse_failed"), Loc.T("btn_ok"));
                return;
            }

            string ttsLang = langFromFile;
            if (string.IsNullOrEmpty(ttsLang))
            {
                var choice = await DisplayActionSheet(
                    Loc.T("title_choose_tts_lang"),
                    Loc.T("action_cancel"), null,
                    Loc.T("lang_ja_jp"), Loc.T("lang_zh_cn"), Loc.T("lang_en_us"));
                if (choice == Loc.T("action_cancel") || string.IsNullOrEmpty(choice)) return;
                if (choice == Loc.T("lang_zh_cn")) ttsLang = "zh-CN";
                else if (choice == Loc.T("lang_en_us")) ttsLang = "en-US";
                else ttsLang = "ja-JP";
            }

            string name = Path.GetFileNameWithoutExtension(result.FileName);
            if (string.IsNullOrEmpty(name)) name = Loc.T("custom_default_name");

            ReclistProvider.SaveCustomReclist(name, ttsLang, reclist);
            ApplyLocalization();
            LanguagePicker.SelectedIndex = _languageList.Count - 1;

            await DisplayAlertAsync(Loc.T("title_import_success"),
                Loc.F("msg_import_success_fmt", name, reclist.Count, ttsLang),
                Loc.T("btn_ok"));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Loc.T("title_import_failed"), ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnClearReclistClicked(object sender, EventArgs e)
    {
        bool ok = await DisplayAlertAsync(
            Loc.T("title_clear_reclist"),
            Loc.T("msg_clear_reclist"),
            Loc.T("btn_clear_action"),
            Loc.T("action_cancel"));
        if (!ok) return;
        ReclistProvider.ClearCustomReclist();
        ApplyLocalization();
    }

    private static Dictionary<string, string> ParseReclist(string content, out string languageCode)
    {
        var result = new Dictionary<string, string>();
        languageCode = "";
        var lines = content.Replace("\r\n", "\n").Split('\n');
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("#") || line.StartsWith("//"))
            {
                var lower = line.ToLowerInvariant();
                int li = lower.IndexOf("language:");
                if (li >= 0)
                {
                    var lang = line.Substring(li + 9).Trim();
                    if (!string.IsNullOrEmpty(lang)) languageCode = lang;
                }
                continue;
            }
            int sep = -1;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '=' || c == ',' || c == '\t' || c == ' ') { sep = i; break; }
            }
            if (sep <= 0) continue;
            var alias = line.Substring(0, sep).Trim();
            var ttsText = line.Substring(sep + 1).Trim();
            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(ttsText)) continue;
            result[alias] = ttsText;
        }
        return result;
    }

    private void OnEngineChanged(object sender, EventArgs e)
    {
        if (EnginePicker.SelectedIndex < 0 || EnginePicker.SelectedIndex >= _engines.Count) return;
        if (!string.IsNullOrWhiteSpace(NameEntry.Text)) return;

        var label = _engines[EnginePicker.SelectedIndex].Label;
        if (string.IsNullOrWhiteSpace(label)) return;

        int idx = -1;
        for (int i = 2; i < label.Length; i++)
            if (char.IsDigit(label[i])) { idx = i; break; }

        var name = idx > 0 ? label.Substring(0, idx).TrimEnd('.', ' ', '-') : label;
        if (!string.IsNullOrWhiteSpace(name)) NameEntry.Text = name;
    }

    private async void OnPickIconClicked(object sender, EventArgs e)
    {
        var p = await PickImageAsync(Loc.T("btn_pick_icon"));
        if (p != null) { _iconPath = p; IconLabel.Text = Loc.F("selected_fmt", Path.GetFileName(p)); }
    }

    private void OnClearIconClicked(object sender, EventArgs e)
    {
        _iconPath = null; IconLabel.Text = Loc.T("not_selected");
    }

    private async void OnPickPortraitClicked(object sender, EventArgs e)
    {
        var p = await PickImageAsync(Loc.T("btn_pick_portrait"));
        if (p != null) { _portraitPath = p; PortraitLabel.Text = Loc.F("selected_fmt", Path.GetFileName(p)); }
    }

    private void OnClearPortraitClicked(object sender, EventArgs e)
    {
        _portraitPath = null; PortraitLabel.Text = Loc.T("not_selected");
    }

    private async Task<string?> PickImageAsync(string title)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = title,
                FileTypes = FilePickerFileType.Images
            });
            if (result == null) return null;

            var ext = Path.GetExtension(result.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".png";

            var local = Path.Combine(FileSystem.CacheDirectory,
                "pick_" + Guid.NewGuid().ToString("N") + ext);

            using (var src = await result.OpenReadAsync())
            using (var dst = File.Create(local))
                await src.CopyToAsync(dst);
            return local;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Loc.T("title_pick_image_failed"), ex.Message, Loc.T("btn_ok"));
            return null;
        }
    }

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

    private string ResolveOutputDir(string name, string? customRelative)
    {
        var root = GetExternalRoot();
        var safeName = SanitizeFileName(name);
        if (!string.IsNullOrEmpty(customRelative))
        {
            customRelative = customRelative.Trim().TrimStart('/', (char)92);
            return Path.Combine(root, customRelative, safeName);
        }
        return Path.Combine(root, "Download", "UtauTtsBuilder", safeName);
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }

    private async void OnBuildClicked(object sender, EventArgs e)
    {
        int engineIdx = EnginePicker.SelectedIndex;
        var name = NameEntry.Text?.Trim();
        var customPath = OutputDirEntry.Text?.Trim();
        var author = AuthorEntry.Text?.Trim();

        if (engineIdx < 0 || string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync(Loc.T("title_tip"),
                Loc.T("msg_select_engine_and_name"), Loc.T("btn_ok"));
            return;
        }

        string outputDir = ResolveOutputDir(name, customPath);
        try { Directory.CreateDirectory(outputDir); }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Loc.T("title_cannot_create_dir"),
                ex.Message, Loc.T("btn_ok"));
            return;
        }

        var language = GetSelectedLanguage();
        var engine = _engines[engineIdx];

        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(
            engine.Name, progress, language, _iconPath, _portraitPath, author);

        ProgressBar.IsVisible = true;
        BuildButton.IsEnabled = false;
        RefreshBtn.IsEnabled = false;

        string? zipPath = null;
        try
        {
            await builder.BuildAsync(outputDir, name);

            if (ZipCheckBox.IsChecked)
            {
                StatusLabel.Text = Loc.T("prog_compressing");
                try
                {
                    zipPath = outputDir.TrimEnd('/', (char)92) + ".zip";
                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    ZipFile.CreateFromDirectory(outputDir, zipPath);
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = Loc.F("prog_compress_failed_fmt", ex.Message);
                }
            }

            string msg = Loc.F("msg_done_fmt", outputDir, zipPath ?? "-");

            if (zipPath != null && File.Exists(zipPath))
            {
                bool share = await DisplayAlertAsync(Loc.T("title_done"), msg,
                    Loc.T("btn_share_zip"), Loc.T("btn_close"));
                if (share)
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = Loc.T("share_zip_title"),
                        File = new ShareFile(zipPath)
                    });
                }
            }
            else
            {
                await DisplayAlertAsync(Loc.T("title_done"), msg, Loc.T("btn_ok"));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
        finally
        {
            ProgressBar.IsVisible = false;
            BuildButton.IsEnabled = true;
            RefreshBtn.IsEnabled = true;
        }
    }
}
