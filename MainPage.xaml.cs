using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _localEngines = new();
    private CrossLingualMode _selectedCrossLang = CrossLingualMode.Native;
    private List<string> _engineLabels = new();
    private List<string> _enginePackages = new();

    private string? _iconPath;
    private string? _portraitPath;
    private List<VoicebankLanguage> _languageList = new();
    private int _selectedLanguageIndex = 0;
    private int _selectedEngineIndex = 0;
    private bool _updateChecked = false;

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
        LoadEngines();
        Logger.Info("MainPage", "Appeared");

        if (!_updateChecked)
        {
            _updateChecked = true;
            _ = CheckUpdateAsync();
        }
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

        InitLanguageList();
        CrossLangValueLabel.Text = CrossLingualService.GetDisplayName(_selectedCrossLang);
    }

    private void InitLanguageList()
    {
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

        if (_selectedLanguageIndex >= _languageList.Count)
            _selectedLanguageIndex = 0;

        LanguageValueLabel.Text = ReclistProvider.GetDisplayName(
            _languageList[_selectedLanguageIndex]);
    }

    // ===== 自定义选择页 =====
    private async Task<int> ShowSelectionAsync(string title, List<string> items, int current)
    {
        var page = new SelectionPage(title, items, current);
        var task = page.WaitResultAsync();
        await Navigation.PushAsync(page);
        return await task;
    }

    private async Task<string?> ShowActionSheetAsync(string title, List<string> actions)
    {
        var page = new ActionSheetPage(title, actions, Loc.T("action_cancel"));
        var task = page.WaitResultAsync();
        await Navigation.PushAsync(page);
        return await task;
    }

    // ===== 顶部「⋯」菜单 =====
    private async void OnMoreClicked(object sender, EventArgs e)
    {
        Logger.Info("MainPage", "More menu opened");
        var actions = new List<string>();
        var keys = new Dictionary<string, string>();

        var sList = Loc.T("settings_voicebank_list");
        actions.Add(sList); keys[sList] = "list";

        var sBatch = Loc.T("batch_title");
        actions.Add(sBatch); keys[sBatch] = "batch";

        var sCompare = Loc.T("compare_title");
        actions.Add(sCompare); keys[sCompare] = "compare";

        var sUpdate = Loc.T("update_check_now");
        actions.Add(sUpdate); keys[sUpdate] = "update";

        var sLog = Loc.T("log_viewer_title");
        actions.Add(sLog); keys[sLog] = "view_log";

        var sClear = Loc.T("settings_clear_cache");
        actions.Add(sClear); keys[sClear] = "clear_cache";

        var choice = await ShowActionSheetAsync(Loc.T("more_menu_title"), actions);
        if (choice == null) return;
        if (!keys.TryGetValue(choice, out var action)) return;

        switch (action)
        {
            case "list":
                Logger.Info("MainPage", "→ Voicebank list");
                try { await Navigation.PushAsync(new VoicebankListPage()); }
                catch (Exception ex) { Logger.Error("MainPage", ex); await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }

                break;

            case "batch":
                Logger.Info("MainPage", "→ Batch build");
                try { await RunBatchAsync(); }
                catch (Exception ex) { Logger.Error("MainPage", ex); await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }

                break;

            case "compare":
                Logger.Info("MainPage", "→ Compare");
                try { await RunCompareAsync(); }
                catch (Exception ex) { Logger.Error("MainPage", ex); await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }

                break;

            case "update":
                Logger.Info("MainPage", "→ Manual update check");
                try { await ManualCheckUpdateAsync(); }
                catch (Exception ex) { Logger.Error("MainPage", ex); await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }

                break;

            case "view_log":
                Logger.Info("MainPage", "→ Log viewer");
                try { await Navigation.PushAsync(new LogViewerPage()); }
                catch (Exception ex) { Logger.Error("MainPage", ex); await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }

                break;

            case "clear_cache":
                Logger.Info("MainPage", "→ Clear cache");
                try { await ClearCacheAsync(); }
                catch (Exception ex) { Logger.Error("MainPage", ex); await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }

                break;
        }
    }

    // ===== 声库列表 =====
    private async Task OpenListAsync()
    {
        await Navigation.PushAsync(new VoicebankListPage());
    }

    // ===== 批量生成 =====
    private async Task RunBatchAsync()
    {
        if (_enginePackages.Count == 0)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_tip"), Loc.T("btn_refresh_engines"), Loc.T("btn_ok"));
            return;
        }

        var baseName = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(baseName)) baseName = "voicebank";

        bool ok = await DialogService.ShowConfirmAsync(this,
            Loc.T("batch_title"),
            Loc.F("batch_desc_fmt", _enginePackages.Count),
            Loc.T("batch_start"),
            Loc.T("action_cancel"));
        if (!ok) return;

        var baseDir = Path.Combine(GetExternalRoot(), "Download", "UtauTtsBuilder");
        var reqs = new List<BatchBuildRequest>();

        for (int i = 0; i < _enginePackages.Count; i++)
        {
            reqs.Add(new BatchBuildRequest
            {
                Name = baseName + "_" + SanitizeFileName(_engineLabels[i]),
                EnginePackage = _enginePackages[i],
                EngineLabel = _engineLabels[i],
                Language = GetSelectedLanguage(),
                SpeechRate = (float)SpeedSlider.Value,
                Pitch = (float)PitchSlider.Value,
                IconPath = _iconPath,
                PortraitPath = _portraitPath,
                Author = AuthorEntry.Text?.Trim(),
                BaseOutputDir = baseDir
            });
        }

        ProgressBar.IsVisible = true;
        BuildButton.IsEnabled = false;

        try
        {
            var progress = new Progress<string>(m => StatusLabel.Text = m);
            var results = await BatchBuildService.RunAsync(reqs, progress);

            int success = results.Count(r => r.Contains("✅"));
            int failed = results.Count - success;
            var msg = Loc.F("batch_result_fmt", success, failed)
                + "\n\n" + string.Join("\n", results);
            await DialogService.ShowAlertAsync(this,
                Loc.T("batch_title"), msg, Loc.T("btn_ok"));
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
        finally
        {
            ProgressBar.IsVisible = false;
            BuildButton.IsEnabled = true;
        }
    }

    // ===== 音质对比 =====
        private async Task RunCompareAsync()
    {
        if (_enginePackages.Count == 0)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_tip"), Loc.T("btn_refresh_engines"), Loc.T("btn_ok"));
            return;
        }

        var engines = new List<(string Package, string Label)>();
        for (int i = 0; i < _enginePackages.Count; i++)
            engines.Add((_enginePackages[i], _engineLabels[i]));

        var language = GetSelectedLanguage();
        var langCode = ReclistProvider.GetLanguageCode(language);
        float speechRate = (float)SpeedSlider.Value;
        float pitch = (float)PitchSlider.Value;

        ProgressBar.IsVisible = true;
        BuildButton.IsEnabled = false;

        try
        {
            var progress = new Progress<string>(m => StatusLabel.Text = m);
            var results = await QuickCompareService.RunAsync(
                engines, "\u3042", langCode, speechRate, pitch, progress);

            var ok = results.Where(r => r.WavPath != null).ToList();
            if (ok.Count == 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("All engines failed:");
                foreach (var r in results)
                {
                    sb.Append('\n').Append(r.EngineLabel).Append(": ").Append(r.Error);
                }
                await DialogService.ShowAlertAsync(this,
                    Loc.T("compare_title"), sb.ToString(), Loc.T("btn_ok"));
                return;
            }

            var actions = ok.Select(r => r.EngineLabel).ToList();
            var choice = await ShowActionSheetAsync(
                Loc.T("compare_result_title"), actions);
            if (choice == null) return;

            var idx = actions.IndexOf(choice);
            if (idx < 0) return;

            await AudioPreviewer.PlayAsync(ok[idx].WavPath!, choice);
        }
        catch (Exception ex)
        {
            Logger.Error("MainPage", ex);
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
        finally
        {
            ProgressBar.IsVisible = false;
            BuildButton.IsEnabled = true;
            StatusLabel.Text = "";
        }
    }


    // ===== 检查更新 =====
    private async Task CheckUpdateAsync()
    {
        var info = await UpdateService.CheckAsync();
        if (info == null || !info.HasUpdate) return;

        bool go = await DialogService.ShowConfirmAsync(this,
            Loc.T("update_title"),
            Loc.F("update_message_fmt", info.CurrentVersion, info.LatestVersion),
            Loc.T("btn_update_now"),
            Loc.T("btn_update_later"));

        if (go)
        {
            try { await Launcher.Default.OpenAsync(info.ReleaseUrl); }
            catch { }
        }
    }

    private async Task ManualCheckUpdateAsync()
    {
        StatusLabel.Text = Loc.T("update_checking");
        var info = await UpdateService.CheckAsync();
        StatusLabel.Text = "";

        if (info == null)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("update_title"), Loc.T("update_check_failed"), Loc.T("btn_ok"));
            return;
        }

        if (!info.HasUpdate)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("update_title"),
                Loc.F("update_latest_fmt", info.CurrentVersion),
                Loc.T("btn_ok"));
            return;
        }

        bool go = await DialogService.ShowConfirmAsync(this,
            Loc.T("update_title"),
            Loc.F("update_message_fmt", info.CurrentVersion, info.LatestVersion),
            Loc.T("btn_update_now"),
            Loc.T("btn_update_later"));

        if (go)
        {
            try { await Launcher.Default.OpenAsync(info.ReleaseUrl); }
            catch { }
        }
    }

    // ===== 清缓存 =====
    private async Task ClearCacheAsync()
    {
        bool ok = await DialogService.ShowConfirmAsync(this,
            Loc.T("settings_clear_cache"),
            Loc.T("settings_clear_cache_confirm"),
            Loc.T("btn_clear_action"),
            Loc.T("action_cancel"));
        if (!ok) return;

        try
        {
            var cacheDir = FileSystem.CacheDirectory;
            foreach (var dir in Directory.GetDirectories(cacheDir))
            {
                try { Directory.Delete(dir, true); } catch { }
            }
            await DialogService.ShowAlertAsync(this,
                Loc.T("settings_clear_cache"),
                Loc.T("settings_clear_cache_done"), Loc.T("btn_ok"));
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
    }

    // ===== 语言选择 =====
    private async void OnLanguageTapped(object sender, EventArgs e)
    {
        var titles = _languageList
            .Select(l => ReclistProvider.GetDisplayName(l))
            .ToList();

        var idx = await ShowSelectionAsync(
            Loc.T("sec_language"), titles, _selectedLanguageIndex);

        if (idx >= 0)
        {
            _selectedLanguageIndex = idx;
            LanguageValueLabel.Text = titles[idx];
        }
    }

    // ===== 引擎选择 =====
    private async void OnEngineTapped(object sender, EventArgs e)
    {
        if (_engineLabels.Count == 0)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_tip"), Loc.T("btn_refresh_engines"), Loc.T("btn_ok"));
            return;
        }

        var idx = await ShowSelectionAsync(
            Loc.T("sec_engine"), _engineLabels, _selectedEngineIndex);

        if (idx >= 0)
        {
            _selectedEngineIndex = idx;
            EngineValueLabel.Text = _engineLabels[idx];
            AutoFillName(_engineLabels[idx]);
        }
    }

    private void AutoFillName(string label)
    {
        if (!string.IsNullOrWhiteSpace(NameEntry.Text)) return;

        var name = label.TrimStart('★', '☁', ' ').Trim();

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

    private VoicebankLanguage GetSelectedLanguage()
    {
        if (_selectedLanguageIndex >= 0 && _selectedLanguageIndex < _languageList.Count)
            return _languageList[_selectedLanguageIndex];
        return VoicebankLanguage.Japanese;
    }

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

            if (_engineLabels.Count == 0)
            {
                EngineValueLabel.Text = "—";
                StatusLabel.Text = Loc.F("engines_found_fmt", 0);
                return;
            }

            var defaultIdx = _localEngines.FindIndex(e => e.IsDefault);
            _selectedEngineIndex = defaultIdx >= 0 ? defaultIdx : 0;
            EngineValueLabel.Text = _engineLabels[_selectedEngineIndex];

            StatusLabel.Text = Loc.F("engines_found_fmt", _engineLabels.Count);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = Loc.F("engines_load_failed_fmt", ex.Message);
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e) => LoadEngines();

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }

    private void OnSpeedChanged(object sender, ValueChangedEventArgs e) => UpdateSpeedLabel();
    private void OnPitchChanged(object sender, ValueChangedEventArgs e) => UpdatePitchLabel();

    private void UpdateSpeedLabel()
        => SpeedValue.Text = SpeedSlider.Value.ToString("F2") + "×";

    private void UpdatePitchLabel()
        => PitchValue.Text = PitchSlider.Value.ToString("F2") + "×";

    // ===== 导入自定义音素表 =====
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
                await DialogService.ShowAlertAsync(this,
                    Loc.T("title_parse_failed"),
                    Loc.T("msg_parse_failed"), Loc.T("btn_ok"));
                return;
            }

            string ttsLang = langFromFile;
            if (string.IsNullOrEmpty(ttsLang))
            {
                var sJa = Loc.T("lang_ja_jp");
                var sZh = Loc.T("lang_zh_cn");
                var sEn = Loc.T("lang_en_us");
                var choice = await ShowActionSheetAsync(
                    Loc.T("title_choose_tts_lang"),
                    new List<string> { sJa, sZh, sEn });

                if (choice == null) return;
                if (choice == sZh) ttsLang = "zh-CN";
                else if (choice == sEn) ttsLang = "en-US";
                else ttsLang = "ja-JP";
            }

            string name = Path.GetFileNameWithoutExtension(result.FileName);
            if (string.IsNullOrEmpty(name)) name = Loc.T("custom_default_name");

            // 预览
            var pv = new ReclistPreviewPage(result.FileName, reclist, ttsLang);
            var pvTask = pv.WaitResultAsync();
            await Navigation.PushAsync(pv);
            if (!await pvTask) return;

            ReclistProvider.SaveCustomReclist(name, ttsLang, reclist);
            ApplyLocalization();
            _selectedLanguageIndex = _languageList.Count - 1;
            LanguageValueLabel.Text = ReclistProvider.GetDisplayName(
                _languageList[_selectedLanguageIndex]);

            await DialogService.ShowAlertAsync(this,
                Loc.T("title_import_success"),
                Loc.F("msg_import_success_fmt", name, reclist.Count, ttsLang),
                Loc.T("btn_ok"));
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_import_failed"), ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnClearReclistClicked(object sender, EventArgs e)
    {
        bool ok = await DialogService.ShowConfirmAsync(this,
            Loc.T("title_clear_reclist"),
            Loc.T("msg_clear_reclist"),
            Loc.T("btn_clear_action"),
            Loc.T("action_cancel"));
        if (!ok) return;
        ReclistProvider.ClearCustomReclist();
        _selectedLanguageIndex = 0;
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

    // ===== 图标 =====
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

    // ===== 立绘 =====
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
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_pick_image_failed"), ex.Message, Loc.T("btn_ok"));
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

    // ===== 生成 =====
    private async void OnBuildClicked(object sender, EventArgs e)
    {
        Logger.Info("MainPage", "Build clicked");
        var name = NameEntry.Text?.Trim();
        var customPath = OutputDirEntry.Text?.Trim();
        var author = AuthorEntry.Text?.Trim();

        if (_selectedEngineIndex < 0 || _selectedEngineIndex >= _enginePackages.Count
            || string.IsNullOrEmpty(name))
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_tip"),
                Loc.T("msg_select_engine_and_name"), Loc.T("btn_ok"));
            return;
        }

        var language = GetSelectedLanguage();
        var enginePackage = _enginePackages[_selectedEngineIndex];
        float speechRate = (float)SpeedSlider.Value;
        float pitch = (float)PitchSlider.Value;
        bool skipExisting = SkipExistingCheckBox.IsChecked;

        // 预估时间弹窗
        var est = EstimateService.Estimate(language, enginePackage);
        var reclist = ReclistProvider.GetReclist(language);
        bool go = await DialogService.ShowConfirmAsync(this,
            Loc.T("btn_build"),
            Loc.F("estimate_confirm_fmt", name, reclist.Count,
                EstimateService.Format(est)),
            Loc.T("btn_ok"), Loc.T("action_cancel"));
        if (!go) return;

        string outputDir = ResolveOutputDir(name, customPath);
        try { Directory.CreateDirectory(outputDir); }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_cannot_create_dir"), ex.Message, Loc.T("btn_ok"));
            return;
        }

        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(
            enginePackage, progress, language, _iconPath, _portraitPath, author,
            speechRate, pitch, skipExisting, _selectedCrossLang);

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

            // 生成预览图
            try
            {
                var wavs = Directory.GetFiles(outputDir, "*.wav").Length;
                await PreviewImageGenerator.GenerateAsync(outputDir, name, wavs, _iconPath);
            }
            catch { }

            await ShowCompletionMenuAsync(outputDir, name, zipPath);
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
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

        var sPreview = Loc.T("action_view_preview");
        actions.Add(sPreview); keyMap[sPreview] = "preview";

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

        var choice = await ShowActionSheetAsync(Loc.T("title_done"), actions);
        if (choice == null) return;
        if (!keyMap.TryGetValue(choice, out var action)) return;

        switch (action)
        {
            case "preview":
                await Navigation.PushAsync(new PreviewImagePage(outputDir, name));
                break;

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
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;

            case "gen_ust":
                try
                {
                    var ustPath = await UstTemplateGenerator.GenerateAsync(outputDir, name);
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_done"),
                        Loc.F("ust_generated_fmt", ustPath), Loc.T("btn_ok"));
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
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
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;
        }
    }
}
