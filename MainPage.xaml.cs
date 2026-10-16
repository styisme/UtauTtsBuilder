using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _localEngines = new();
    private List<string> _engineLabels = new();      // 显示名（含本地 + 云端）
    private List<string> _enginePackages = new();    // 对应的引擎包名（含云端虚包名）

    private string? _iconPath;
    private string? _portraitPath;
    private List<VoicebankLanguage> _languageList = new();

    public MainPage()
    {
        InitializeComponent();
        ApplyLocalization();
        UpdateSpeedLabel();
        UpdatePitchLabel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ThemeService.Apply();
        ApplyLocalization();
        // 每次回来都刷新引擎列表（云端配置可能变了）
        LoadEngines();
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
        SecTuneLabel.Text = Loc.T("sec_tune");
        SpeedLabel.Text = Loc.T("label_speed");
        PitchLabel.Text = Loc.T("label_pitch");
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
        SkipExistingLabel.Text = Loc.T("skip_existing_checkbox");
        BuildButton.Text = Loc.T("btn_build");

        if (string.IsNullOrEmpty(_iconPath)) IconLabel.Text = Loc.T("not_selected");
        else IconLabel.Text = Loc.F("selected_fmt", Path.GetFileName(_iconPath));

        if (string.IsNullOrEmpty(_portraitPath)) PortraitLabel.Text = Loc.T("not_selected");
        else PortraitLabel.Text = Loc.F("selected_fmt", Path.GetFileName(_portraitPath));

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

    // ============================================================
    // 引擎列表：本地 + 云端混合
    // ============================================================
    private void LoadEngines()
    {
        try
        {
            _localEngines = TtsEngineHelper.GetAvailableEngines();
            _engineLabels.Clear();
            _enginePackages.Clear();

            foreach (var e in _localEngines)
            {
                _engineLabels.Add(e.IsDefault ? "★ " + e.Label : e.Label);
                _enginePackages.Add(e.Name);
            }

            // 追加云端引擎
            if (CloudConfigService.AzureConfigured)
            {
                var voice = CloudConfigService.AzureVoice;
                _engineLabels.Add("☁ " + Loc.F("engine_azure_fmt", voice));
                _enginePackages.Add(CloudConfigService.AzureEnginePackage);
            }
            if (CloudConfigService.GoogleConfigured)
            {
                _engineLabels.Add("☁ " + Loc.F("engine_google_fmt", "Text-to-Speech"));
                _enginePackages.Add(CloudConfigService.GoogleEnginePackage);
            }

            EnginePicker.ItemsSource = _engineLabels;

            // 默认选第一个本地引擎
            var defaultIdx = _localEngines.FindIndex(e => e.IsDefault);
            if (defaultIdx >= 0) EnginePicker.SelectedIndex = defaultIdx;
            else if (_engineLabels.Count > 0) EnginePicker.SelectedIndex = 0;

            var total = _engineLabels.Count;
            StatusLabel.Text = Loc.F("engines_found_fmt", total);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = Loc.F("engines_load_failed_fmt", ex.Message);
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadEngines();
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }

    // ===== 语速 / 音高 =====
    private void OnSpeedChanged(object sender, ValueChangedEventArgs e) => UpdateSpeedLabel();
    private void OnPitchChanged(object sender, ValueChangedEventArgs e) => UpdatePitchLabel();

    private void UpdateSpeedLabel()
    {
        SpeedValue.Text = SpeedSlider.Value.ToString("F2") + "×";
    }

    private void UpdatePitchLabel()
    {
        PitchValue.Text = PitchSlider.Value.ToString("F2") + "×";
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

    // ===== 自动填写声库名 =====
    private void OnEngineChanged(object sender, EventArgs e)
    {
        if (EnginePicker.SelectedIndex < 0
            || EnginePicker.SelectedIndex >= _engineLabels.Count) return;
        if (!string.IsNullOrWhiteSpace(NameEntry.Text)) return;

        var label = _engineLabels[EnginePicker.SelectedIndex];
        if (string.IsNullOrWhiteSpace(label)) return;

        // 去掉星号、云图标、版本号
        var name = label
            .TrimStart('★', '☁', ' ')
            .Trim();

        // 遇到数字就截断（去掉版本号）
        int idx = -1;
        for (int i = 2; i < name.Length; i++)
        {
            if (char.IsDigit(name[i])) { idx = i; break; }
        }
        if (idx > 0) name = name.Substring(0, idx);

        name = name.TrimEnd('.', ' ', '-', '(', ')');

        if (!string.IsNullOrWhiteSpace(name))
            NameEntry.Text = name;
    }

    private async void OnPickIconClicked(object sender, EventArgs e)
    {
        var p = await PickImageAsync(Loc.T("btn_pick_icon"));
        if (p != null)
        {
            _iconPath = p;
            IconLabel.Text = Loc.F("selected_fmt", Path.GetFileName(p));
            try { IconPreview.Source = ImageSource.FromFile(p); } catch { }
        }
    }

    private void OnClearIconClicked(object sender, EventArgs e)
    {
        _iconPath = null;
        IconLabel.Text = Loc.T("not_selected");
        IconPreview.Source = null;
    }

    private async void OnPickPortraitClicked(object sender, EventArgs e)
    {
        var p = await PickImageAsync(Loc.T("btn_pick_portrait"));
        if (p != null)
        {
            _portraitPath = p;
            PortraitLabel.Text = Loc.F("selected_fmt", Path.GetFileName(p));
            try { PortraitPreview.Source = ImageSource.FromFile(p); } catch { }
        }
    }

    private void OnClearPortraitClicked(object sender, EventArgs e)
    {
        _portraitPath = null;
        PortraitLabel.Text = Loc.T("not_selected");
        PortraitPreview.Source = null;
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

    // ============================================================
    // 生成
    // ============================================================
    private async void OnBuildClicked(object sender, EventArgs e)
    {
        int engineIdx = EnginePicker.SelectedIndex;
        var name = NameEntry.Text?.Trim();
        var customPath = OutputDirEntry.Text?.Trim();
        var author = AuthorEntry.Text?.Trim();

        if (engineIdx < 0 || engineIdx >= _enginePackages.Count
            || string.IsNullOrEmpty(name))
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
        var enginePackage = _enginePackages[engineIdx];
        float speechRate = (float)SpeedSlider.Value;
        float pitch = (float)PitchSlider.Value;
        bool skipExisting = SkipExistingCheckBox.IsChecked;

        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(
            enginePackage, progress, language, _iconPath, _portraitPath, author,
            speechRate, pitch, skipExisting);

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

            // ===== 完成后的操作菜单 =====
            await ShowCompletionMenuAsync(outputDir, name, zipPath);
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

    private async Task ShowCompletionMenuAsync(string outputDir, string name, string? zipPath)
    {
        var otoPath = Path.Combine(outputDir, "oto.ini");
        var hasOto = File.Exists(otoPath);
        var hasZip = zipPath != null && File.Exists(zipPath);

        var actions = new List<string>();
        var keyMap = new Dictionary<string, string>();

        if (hasOto)
        {
            var s = Loc.T("action_edit_oto");
            actions.Add(s); keyMap[s] = "edit_oto";
        }
        var sUst = Loc.T("action_gen_ust");
        actions.Add(sUst); keyMap[sUst] = "gen_ust";

        if (hasZip)
        {
            var s = Loc.T("btn_share_zip");
            actions.Add(s); keyMap[s] = "share_zip";
        }
        var sClose = Loc.T("btn_close");
        actions.Add(sClose); keyMap[sClose] = "close";

        var choice = await DisplayActionSheet(
            Loc.T("title_done"),
            Loc.T("action_cancel"),
            null,
            actions.ToArray());

        if (choice == null || choice == Loc.T("action_cancel")) return;
        if (!keyMap.TryGetValue(choice, out var action)) return;

        switch (action)
        {
            case "edit_oto":
                try
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = Loc.T("action_edit_oto"),
                        File = new ShareFile(otoPath)
                    });
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;

            case "gen_ust":
                try
                {
                    var ustPath = await UstTemplateGenerator.GenerateAsync(outputDir, name);
                    await DisplayAlertAsync(Loc.T("title_done"),
                        Loc.F("ust_generated_fmt", ustPath), Loc.T("btn_ok"));
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;

            case "share_zip":
                try
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = Loc.T("share_zip_title"),
                        File = new ShareFile(zipPath!)
                    });
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;
        }
    }
}
