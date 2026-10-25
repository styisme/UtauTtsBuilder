using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _localEngines = new();
    private List<string> _engineLabels = new();
    private List<string> _enginePackages = new();

    private string? _iconPath;
    private string? _portraitPath;
    private List<VoicebankLanguage> _languageList = new();
    private int _selectedLanguageIndex = 0;
    private int _selectedEngineIndex = 0;
    private bool _updateChecked = false;
    private CrossLingualMode _selectedCrossLang = CrossLingualMode.Native;

    // DiffSinger 内置语料
    private const string CorpusZh = 
        "我冒了严寒，回到相隔二千余里，别了二十余年的故乡去。时候既然是深冬；渐近故乡时，天气又阴晦了，冷风吹进船舱中，呜呜的响，从蓬隙向外一望，苍黄的天底下，远近横着几个萧索的荒村，没有一些活气。我的心禁不住悲凉起来了。" +
        "深蓝的天空中挂着一轮金黄的圆月，下面是海边的沙地，都种着一望无际的碧绿的西瓜，其间有一个十一二岁的少年，项带银圈，手捏一柄钢叉，向一匹猹尽力的刺去，那猹却将身一扭，反从他的胯下逃走了。这少年便是闰土。" +
        "八百标兵奔北坡，炮兵并排北边跑，炮兵怕把标兵碰，标兵怕碰炮兵炮。四是四，十是十，十四是十四，四十是四十，谁能说准四十、十四、四十四，谁来试一试。黑化肥发灰会挥发，灰化肥挥发会发黑。" +
        "大弦嘈嘈如急雨，小弦切切如私语。嘈嘈切切错杂弹，大珠小珠落玉盘。间关莺语花底滑，幽咽泉流冰下难。冰泉冷涩弦凝绝，凝绝不通声暂歇。别有幽愁暗恨生，此时无声胜有声。" +
        "中华人民共和国，简称中国，位于亚洲大陆的东部、太平洋西岸。陆地面积约960万平方公里。中国领土北起漠河以北的黑龙江江心，南到南沙群岛南端的曾母暗沙。中国是一个统一的多民族国家。" +
        "萌娘百科对OpenUTAU的介绍：OpenUTAU是由杉田朗主导、由UTAU社区为UTAU社区打造的免费开源歌声合成编辑器，以实现UTAU框架与引擎的现代化而开发。" +
        "关于DiffSinger：DiffSinger是一个基于扩散概率模型的声学模型，用于歌声合成（SVS）。它可以将噪声迭代地转换为梅尔频谱图。";
    
    private const string CorpusJa = 
        "吾輩は猫である。名前はまだ無い。どこで生れたかとんと見当がつかぬ。何でも薄暗いじめじめした所でニャーニャー泣いていた事だけは記憶している。吾輩はここで始めて人間というものを見た。" +
        "国境の長いトンネルを抜けると雪国であった。夜の底が白くなった。信号所に汽車が止まった。向側の座席から娘が立ってきて、男の前のガラス窓を落とした。雪の冷気が流れこんだ。" +
        "重音テト（かさねてと、Kasane Teto）は、2008年4月1日のエイプリルフール前夕に、匿名掲示板2chで「存在しないボーカロイド」として企画されたバーチャルシンガーです。" +
        "足立レイ（あだち れい、Adachi Rei）は、missile 39（みさいる）によって制作されたオリジナルキャラクターです。正弦波の合成によって作られた声源を持ち、UTAU音源ライブラリを保有しています。" +
        "日本国は、アジア大陸の東、太平洋の西岸に位置する島国です。北海道、本州、四国、九州の4つの大きな島と、6800以上の小さな島々からなります。";

    private const string CorpusEn = 
        "The United States of America, commonly known as the United States or America, is a country primarily located in North America. It consists of 50 states, a federal district, five major unincorporated territories, nine Minor Outlying Islands, and 326 Indian reservations." +
        "The quick brown fox jumps over the lazy dog. Pack my box with five dozen liquor jugs. How vexingly quick daft zebras jump! She sells seashells by the seashore." +
        "It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness, it was the epoch of belief, it was the epoch of incredulity." +
        "OpenUTAU is a free, open-source editor made for the UTAU community. It is developed by a group of volunteers and is designed to modernize the UTAU framework and engine." +
        "DiffSinger is an acoustic model for Singing Voice Synthesis (SVS) based on the diffusion probabilistic model.";

    public MainPage()
    {
        InitializeComponent();
        ApplyLocalization();
        UpdateSpeedLabel();
        UpdatePitchLabel();
        CrossLangValueLabel.Text = CrossLingualService.GetDisplayName(_selectedCrossLang);
        UpdateCharCounts();
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
        TabUtauBtn.Text = "UTAU 生成";
        TabDsBtn.Text = "DiffSinger 语料";
        
        // UTAU 面板翻译
        SecLangLabel.Text = Loc.T("sec_language");
        SecCrossLangLabel.Text = Loc.T("sec_cross_lang");
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

        // DS 面板翻译
        DsSectionLabel.Text = Loc.T("ds_corpus_section_builtin");
        DsHintLabel.Text = Loc.T("ds_corpus_hint");
        LblZh.Text = Loc.T("ds_corpus_zh");
        LblJa.Text = Loc.T("ds_corpus_ja");
        LblEn.Text = Loc.T("ds_corpus_en");
        DsCustomLabel.Text = Loc.T("ds_corpus_section_custom");
        TextEditor.Placeholder = Loc.T("ds_corpus_custom_ph");
        AugLabel.Text = Loc.T("ds_corpus_aug");
        DsStartBtn.Text = Loc.T("ds_corpus_btn_start");

        InitLanguageList();
    }

    private void UpdateCharCounts()
    {
        LblZhCount.Text = $"({CorpusZh.Length})";
        LblJaCount.Text = $"({CorpusJa.Length})";
        LblEnCount.Text = $"({CorpusEn.Length})";
    }

    // ===== Tab 切换 =====
    private void OnTabUtauClicked(object sender, EventArgs e)
    {
        UtauPanel.IsVisible = true;
        DsPanel.IsVisible = false;
        TabUtauBtn.Style = (Style)Application.Current!.Resources["Md3TonalButton"];
        TabDsBtn.Style = (Style)Application.Current!.Resources["Md3OutlinedButton"];
    }

    private void OnTabDsClicked(object sender, EventArgs e)
    {
        UtauPanel.IsVisible = false;
        DsPanel.IsVisible = true;
        TabDsBtn.Style = (Style)Application.Current!.Resources["Md3TonalButton"];
        TabUtauBtn.Style = (Style)Application.Current!.Resources["Md3OutlinedButton"];
    }

    // ===== DiffSinger 生成 =====
    private async void OnDsStartClicked(object sender, EventArgs e)
    {
        var requests = new List<DsCorpusRequest>();
        if (ChkZh.IsChecked) requests.Add(new DsCorpusRequest { Text = CorpusZh, LanguageCode = "zh-CN" });
        if (ChkJa.IsChecked) requests.Add(new DsCorpusRequest { Text = CorpusJa, LanguageCode = "ja-JP" });
        if (ChkEn.IsChecked) requests.Add(new DsCorpusRequest { Text = CorpusEn, LanguageCode = "en-US" });

        var customText = TextEditor.Text?.Trim();
        if (!string.IsNullOrEmpty(customText))
        {
            var sysLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            string code = sysLang == "zh" ? "zh-CN" : sysLang == "ja" ? "ja-JP" : "en-US";
            requests.Add(new DsCorpusRequest { Text = customText, LanguageCode = code });
        }

        if (requests.Count == 0)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_tip"), Loc.T("ds_corpus_no_text"), Loc.T("btn_ok"));
            return;
        }

        var engines = TtsEngineHelper.GetAvailableEngines();
        if (engines.Count == 0)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), Loc.T("engines_found_fmt"), Loc.T("btn_ok"));
            return;
        }
        var engine = engines.FirstOrDefault(en => en.IsDefault) ?? engines.First();

        DsStartBtn.IsEnabled = false;
        DsProgressBar.IsVisible = true;
        DsProgressBar.Progress = 0;

        var progress = new Progress<string>(msg => DsStatusLabel.Text = msg);
        var zipPath = Path.Combine(FileSystem.CacheDirectory, "DiffSinger_Corpus.zip");

        try
        {
            await TtsToDiffSingerService.GenerateMixedCorpusAsync(
                requests, engine.Name, 0.85f, 1.0f, ChkAug.IsChecked, zipPath, progress);

            DsStatusLabel.Text = Loc.T("ds_corpus_done");
            DsProgressBar.Progress = 1;

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = Loc.T("ds_corpus_title"),
                File = new ShareFile(zipPath)
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
            DsStatusLabel.Text = Loc.T("ds_corpus_failed");
        }
        finally
        {
            DsStartBtn.IsEnabled = true;
            DsProgressBar.IsVisible = false;
        }
    }

    // ===== 以下为原有的 UTAU 逻辑（完全保留） =====
    private void InitLanguageList()
    {
        _languageList = new List<VoicebankLanguage>
        {
            VoicebankLanguage.Japanese, VoicebankLanguage.JapaneseCvvc, VoicebankLanguage.JapaneseVcv,
            VoicebankLanguage.Chinese, VoicebankLanguage.ChineseCvvc, VoicebankLanguage.ChineseVcv,
            VoicebankLanguage.English,
        };

        if (ReclistProvider.HasCustomReclist)
        {
            _languageList.Add(VoicebankLanguage.Custom);
            ClearReclistBtn.IsVisible = true;
        }
        else ClearReclistBtn.IsVisible = false;

        if (_selectedLanguageIndex >= _languageList.Count) _selectedLanguageIndex = 0;
        LanguageValueLabel.Text = ReclistProvider.GetDisplayName(_languageList[_selectedLanguageIndex]);
    }

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

    private async void OnMoreClicked(object sender, EventArgs e)
    {
        Logger.Info("MainPage", "More menu opened");
        var actions = new List<string>();
        var keys = new Dictionary<string, string>();

        void AddAction(string key, string label) { actions.Add(label); keys[label] = key; }

        AddAction("list", Loc.T("settings_voicebank_list"));
        AddAction("batch", Loc.T("batch_title"));
        AddAction("compare", Loc.T("compare_title"));
        AddAction("update", Loc.T("update_check_now"));
        AddAction("view_log", Loc.T("log_viewer_title"));
        AddAction("clear_cache", Loc.T("settings_clear_cache"));

        var choice = await ShowActionSheetAsync(Loc.T("more_menu_title"), actions);
        if (choice == null) return;
        if (!keys.TryGetValue(choice, out var action)) return;

        try
        {
            switch (action)
            {
                case "list": await Navigation.PushAsync(new VoicebankListPage()); break;
                case "batch": await RunBatchAsync(); break;
                case "compare": await RunCompareAsync(); break;
                case "update": await ManualCheckUpdateAsync(); break;
                case "view_log": await Navigation.PushAsync(new LogViewerPage()); break;
                case "clear_cache": await ClearCacheAsync(); break;
            }
        }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }
    }

    private async void OnCrossLangTapped(object sender, EventArgs e)
    {
        var modes = new List<CrossLingualMode> { CrossLingualMode.Native, CrossLingualMode.ZhToJa, CrossLingualMode.JaToZh, CrossLingualMode.EnToJa, CrossLingualMode.EnToZh, CrossLingualMode.JaToEn, CrossLingualMode.ZhToEn, CrossLingualMode.Smart };
        var titles = modes.Select(CrossLingualService.GetDisplayName).ToList();
        var idx = await ShowSelectionAsync(Loc.T("sec_cross_lang"), titles, modes.IndexOf(_selectedCrossLang));
        if (idx >= 0) { _selectedCrossLang = modes[idx]; CrossLangValueLabel.Text = titles[idx]; }
    }

    private async void OnLanguageTapped(object sender, EventArgs e)
    {
        var titles = _languageList.Select(l => ReclistProvider.GetDisplayName(l)).ToList();
        var idx = await ShowSelectionAsync(Loc.T("sec_language"), titles, _selectedLanguageIndex);
        if (idx >= 0) { _selectedLanguageIndex = idx; LanguageValueLabel.Text = titles[idx]; }
    }

    private async void OnEngineTapped(object sender, EventArgs e)
    {
        if (_engineLabels.Count == 0) { await DialogService.ShowAlertAsync(this, Loc.T("title_tip"), Loc.T("btn_refresh_engines"), Loc.T("btn_ok")); return; }
        var idx = await ShowSelectionAsync(Loc.T("sec_engine"), _engineLabels, _selectedEngineIndex);
        if (idx >= 0) { _selectedEngineIndex = idx; EngineValueLabel.Text = _engineLabels[idx]; AutoFillName(_engineLabels[idx]); }
    }

    private void AutoFillName(string label)
    {
        if (!string.IsNullOrWhiteSpace(NameEntry.Text)) return;
        var name = label.Replace("★", "").Replace("☁", "").Replace("🧠", "").Trim();
        int idx = -1;
        for (int i = 2; i < name.Length; i++) if (char.IsDigit(name[i])) { idx = i; break; }
        if (idx > 0) name = name.Substring(0, idx);
        name = name.TrimEnd('.', ' ', '-', '(', ')');
        if (!string.IsNullOrWhiteSpace(name)) NameEntry.Text = name;
    }

    private void LoadEngines()
    {
        try
        {
            _localEngines = TtsEngineHelper.GetAvailableEngines();
            _engineLabels.Clear(); _enginePackages.Clear();
            foreach (var e in _localEngines) { _engineLabels.Add(e.IsDefault ? "★ " + e.Label : e.Label); _enginePackages.Add(e.Name); }
            if (CloudConfigService.AzureConfigured) { _engineLabels.Add("☁ " + Loc.F("engine_azure_fmt", CloudConfigService.AzureVoice)); _enginePackages.Add(CloudConfigService.AzureEnginePackage); }
            if (CloudConfigService.GoogleConfigured) { _engineLabels.Add("☁ " + Loc.F("engine_google_fmt", "TTS")); _enginePackages.Add(CloudConfigService.GoogleEnginePackage); }
            _engineLabels.Add("🧠 本地 AI (Kokoro)"); _enginePackages.Add("__local_ai");

            var defaultIdx = _localEngines.FindIndex(e => e.IsDefault);
            _selectedEngineIndex = defaultIdx >= 0 ? defaultIdx : 0;
            EngineValueLabel.Text = _engineLabels[_selectedEngineIndex];
            StatusLabel.Text = Loc.F("engines_found_fmt", _engineLabels.Count);
        }
        catch (Exception ex) { StatusLabel.Text = Loc.F("engines_load_failed_fmt", ex.Message); }
    }

    private void OnRefreshClicked(object sender, EventArgs e) => LoadEngines();
    private async void OnSettingsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SettingsPage());
    private void OnSpeedChanged(object sender, ValueChangedEventArgs e) => UpdateSpeedLabel();
    private void OnPitchChanged(object sender, ValueChangedEventArgs e) => UpdatePitchLabel();
    private void UpdateSpeedLabel() => SpeedValue.Text = SpeedSlider.Value.ToString("F2") + "×";
    private void UpdatePitchLabel() => PitchValue.Text = PitchSlider.Value.ToString("F2") + "×";

    private VoicebankLanguage GetSelectedLanguage() => (_selectedLanguageIndex >= 0 && _selectedLanguageIndex < _languageList.Count) ? _languageList[_selectedLanguageIndex] : VoicebankLanguage.Japanese;

    private async void OnImportReclistClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = Loc.T("btn_import_reclist") });
            if (result == null) return;
            string content;
            using (var stream = await result.OpenReadAsync())
            using (var reader = new StreamReader(stream)) content = await reader.ReadToEndAsync();
            var reclist = ParseReclist(content, out var langFromFile);
            if (reclist.Count == 0) { await DialogService.ShowAlertAsync(this, Loc.T("title_parse_failed"), Loc.T("msg_parse_failed"), Loc.T("btn_ok")); return; }

            string ttsLang = langFromFile;
            if (string.IsNullOrEmpty(ttsLang))
            {
                var sJa = Loc.T("lang_ja_jp"); var sZh = Loc.T("lang_zh_cn"); var sEn = Loc.T("lang_en_us");
                var choice = await ShowActionSheetAsync(Loc.T("title_choose_tts_lang"), new List<string> { sJa, sZh, sEn });
                if (choice == null) return;
                if (choice == sZh) ttsLang = "zh-CN"; else if (choice == sEn) ttsLang = "en-US"; else ttsLang = "ja-JP";
            }
            string name = Path.GetFileNameWithoutExtension(result.FileName);
            if (string.IsNullOrEmpty(name)) name = Loc.T("custom_default_name");

            var pv = new ReclistPreviewPage(result.FileName, reclist, ttsLang);
            var pvTask = pv.WaitResultAsync(); await Navigation.PushAsync(pv);
            if (!await pvTask) return;

            ReclistProvider.SaveCustomReclist(name, ttsLang, reclist);
            ApplyLocalization();
            _selectedLanguageIndex = _languageList.Count - 1;
            LanguageValueLabel.Text = ReclistProvider.GetDisplayName(_languageList[_selectedLanguageIndex]);
            await DialogService.ShowAlertAsync(this, Loc.T("title_import_success"), Loc.F("msg_import_success_fmt", name, reclist.Count, ttsLang), Loc.T("btn_ok"));
        }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, Loc.T("title_import_failed"), ex.Message, Loc.T("btn_ok")); }
    }

    private async void OnClearReclistClicked(object sender, EventArgs e)
    {
        bool ok = await DialogService.ShowConfirmAsync(this, Loc.T("title_clear_reclist"), Loc.T("msg_clear_reclist"), Loc.T("btn_clear_action"), Loc.T("action_cancel"));
        if (!ok) return;
        ReclistProvider.ClearCustomReclist(); _selectedLanguageIndex = 0; ApplyLocalization();
    }

    private static Dictionary<string, string> ParseReclist(string content, out string languageCode)
    {
        var result = new Dictionary<string, string>(); languageCode = "";
        foreach (var line in content.Replace("\r\n", "\n").Split('\n'))
        {
            var trimmed = line.Trim(); if (string.IsNullOrEmpty(trimmed)) continue;
            if (trimmed.StartsWith("#") || trimmed.StartsWith("//")) { var lower = trimmed.ToLowerInvariant(); int li = lower.IndexOf("language:"); if (li >= 0) { var lang = trimmed.Substring(li + 9).Trim(); if (!string.IsNullOrEmpty(lang)) languageCode = lang; } continue; }
            int sep = -1; for (int i = 0; i < trimmed.Length; i++) { char c = trimmed[i]; if (c == '=' || c == ',' || c == '\t' || c == ' ') { sep = i; break; } }
            if (sep <= 0) continue;
            var alias = trimmed.Substring(0, sep).Trim(); var ttsText = trimmed.Substring(sep + 1).Trim(); if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(ttsText)) continue; result[alias] = ttsText;
        }
        return result;
    }

    private async void OnPickIconClicked(object sender, EventArgs e) { var p = await PickImageAsync(Loc.T("btn_pick_icon")); if (p != null) { _iconPath = p; IconLabel.Text = Loc.F("selected_fmt", Path.GetFileName(p)); try { IconPreview.Source = ImageSource.FromFile(p); } catch { } } }
    private void OnClearIconClicked(object sender, EventArgs e) { _iconPath = null; IconLabel.Text = Loc.T("not_selected"); IconPreview.Source = null; }
    private async void OnPickPortraitClicked(object sender, EventArgs e) { var p = await PickImageAsync(Loc.T("btn_pick_portrait")); if (p != null) { _portraitPath = p; PortraitLabel.Text = Loc.F("selected_fmt", Path.GetFileName(p)); try { PortraitPreview.Source = ImageSource.FromFile(p); } catch { } } }
    private void OnClearPortraitClicked(object sender, EventArgs e) { _portraitPath = null; PortraitLabel.Text = Loc.T("not_selected"); PortraitPreview.Source = null; }

    private async Task<string?> PickImageAsync(string title)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = title, FileTypes = FilePickerFileType.Images });
            if (result == null) return null;
            var ext = Path.GetExtension(result.FileName); if (string.IsNullOrEmpty(ext)) ext = ".png";
            var local = Path.Combine(FileSystem.CacheDirectory, "pick_" + Guid.NewGuid().ToString("N") + ext);
            using (var src = await result.OpenReadAsync()) using (var dst = File.Create(local)) await src.CopyToAsync(dst);
            return local;
        }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, Loc.T("title_pick_image_failed"), ex.Message, Loc.T("btn_ok")); return null; }
    }

    private static string GetExternalRoot()
    {
#if ANDROID
        return global::Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath ?? FileSystem.AppDataDirectory;
#else
        return FileSystem.AppDataDirectory;
#endif
    }

    private string ResolveOutputDir(string name, string? customRelative)
    {
        var root = GetExternalRoot(); var safeName = SanitizeFileName(name);
        if (!string.IsNullOrEmpty(customRelative)) { customRelative = customRelative.Trim().TrimStart('/', (char)92); return Path.Combine(root, customRelative, safeName); }
        return Path.Combine(root, "Download", "UtauTtsBuilder", safeName);
    }

    private static string SanitizeFileName(string name) { foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_'); return name.Trim(); }

    private async void OnBuildClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim(); var customPath = OutputDirEntry.Text?.Trim(); var author = AuthorEntry.Text?.Trim();
        if (_selectedEngineIndex < 0 || _selectedEngineIndex >= _enginePackages.Count || string.IsNullOrEmpty(name))
        { await DialogService.ShowAlertAsync(this, Loc.T("title_tip"), Loc.T("msg_select_engine_and_name"), Loc.T("btn_ok")); return; }

        var language = GetSelectedLanguage(); var enginePackage = _enginePackages[_selectedEngineIndex];
        float speechRate = (float)SpeedSlider.Value; float pitch = (float)PitchSlider.Value; bool skipExisting = SkipExistingCheckBox.IsChecked;

        var est = EstimateService.Estimate(language, enginePackage); var reclist = ReclistProvider.GetReclist(language);
        bool go = await DialogService.ShowConfirmAsync(this, Loc.T("btn_build"), Loc.F("estimate_confirm_fmt", name, reclist.Count, EstimateService.Format(est)), Loc.T("btn_ok"), Loc.T("action_cancel"));
        if (!go) return;

        string outputDir = ResolveOutputDir(name, customPath);
        try { Directory.CreateDirectory(outputDir); }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, Loc.T("title_cannot_create_dir"), ex.Message, Loc.T("btn_ok")); return; }

        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(enginePackage, progress, language, _iconPath, _portraitPath, author, speechRate, pitch, skipExisting, _selectedCrossLang);

        ProgressBar.IsVisible = true; BuildButton.IsEnabled = false; RefreshBtn.IsEnabled = false;
        string? zipPath = null;
        try
        {
            await builder.BuildAsync(outputDir, name);
            if (ZipCheckBox.IsChecked)
            {
                StatusLabel.Text = Loc.T("prog_compressing");
                try { zipPath = outputDir.TrimEnd('/', (char)92) + ".zip"; if (File.Exists(zipPath)) File.Delete(zipPath); ZipFile.CreateFromDirectory(outputDir, zipPath); }
                catch (Exception ex) { StatusLabel.Text = Loc.F("prog_compress_failed_fmt", ex.Message); }
            }
            await ShowCompletionMenuAsync(outputDir, name, zipPath);
        }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, Loc.T("title_error"), ex.Message, Loc.T("btn_ok")); }
        finally { ProgressBar.IsVisible = false; BuildButton.IsEnabled = true; RefreshBtn.IsEnabled = true; }
    }

    private async Task ShowCompletionMenuAsync(string outputDir, string name, string? zipPath)
    {
        var actions = new List<string>(); var keyMap = new Dictionary<string, string>();
        void Add(string k, string l) { actions.Add(l); keyMap[l] = k; }
        Add("preview", Loc.T("action_view_preview")); Add("edit_oto", Loc.T("action_edit_oto")); Add("gen_ust", Loc.T("action_gen_ust"));
        if (zipPath != null) Add("share_zip", Loc.T("btn_share_zip"));

        var choice = await ShowActionSheetAsync(Loc.T("title_done"), actions);
        if (choice == null) return;
        if (!keyMap.TryGetValue(choice, out var action)) return;

        try
        {
            switch (action)
            {
                case "preview": await Navigation.PushAsync(new PreviewImagePage(outputDir, name)); break;
                case "edit_oto": await Share.Default.RequestAsync(new ShareFileRequest { Title = "OTO", File = new ShareFile(Path.Combine(outputDir, "oto.ini")) }); break;
                case "gen_ust": var ust = await UstTemplateGenerator.GenerateAsync(outputDir, name); await DialogService.ShowAlertAsync(this, "Done", Loc.F("ust_generated_fmt", ust), "OK"); break;
                case "share_zip": await Share.Default.RequestAsync(new ShareFileRequest { Title = "ZIP", File = new ShareFile(zipPath!) }); break;
            }
        }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }
    }

    private async Task RunBatchAsync()
    {
        if (_enginePackages.Count == 0) return;
        var baseName = NameEntry.Text?.Trim(); if (string.IsNullOrEmpty(baseName)) baseName = "voicebank";
        bool ok = await DialogService.ShowConfirmAsync(this, Loc.T("batch_title"), Loc.F("batch_desc_fmt", _enginePackages.Count), Loc.T("batch_start"), Loc.T("action_cancel"));
        if (!ok) return;

        var baseDir = Path.Combine(GetExternalRoot(), "Download", "UtauTtsBuilder");
        var reqs = new List<BatchBuildRequest>();
        for (int i = 0; i < _enginePackages.Count; i++)
        {
            reqs.Add(new BatchBuildRequest { Name = baseName + "_" + SanitizeFileName(_engineLabels[i]), EnginePackage = _enginePackages[i], EngineLabel = _engineLabels[i], Language = GetSelectedLanguage(), SpeechRate = (float)SpeedSlider.Value, Pitch = (float)PitchSlider.Value, IconPath = _iconPath, PortraitPath = _portraitPath, Author = AuthorEntry.Text?.Trim(), BaseOutputDir = baseDir });
        }

        ProgressBar.IsVisible = true; BuildButton.IsEnabled = false;
        try
        {
            var progress = new Progress<string>(m => StatusLabel.Text = m);
            var results = await BatchBuildService.RunAsync(reqs, progress);
            int success = results.Count(r => r.Contains("✅")); int failed = results.Count - success;
            await DialogService.ShowAlertAsync(this, Loc.T("batch_title"), Loc.F("batch_result_fmt", success, failed) + "\n\n" + string.Join("\n", results), Loc.T("btn_ok"));
        }
        finally { ProgressBar.IsVisible = false; BuildButton.IsEnabled = true; }
    }

    private async Task RunCompareAsync()
    {
        if (_enginePackages.Count == 0) return;
        var engines = new List<(string Package, string Label)>();
        for (int i = 0; i < _enginePackages.Count; i++) engines.Add((_enginePackages[i], _engineLabels[i]));

        ProgressBar.IsVisible = true; BuildButton.IsEnabled = false;
        try
        {
            var progress = new Progress<string>(m => StatusLabel.Text = m);
            var results = await QuickCompareService.RunAsync(engines, "あ", "ja-JP", (float)SpeedSlider.Value, (float)PitchSlider.Value, progress);
            var ok = results.Where(r => r.WavPath != null).ToList();
            if (ok.Count == 0) { await DialogService.ShowAlertAsync(this, "Error", "All failed", "OK"); return; }
            var actions = ok.Select(r => r.EngineLabel).ToList(); var choice = await ShowActionSheetAsync("Select", actions);
            if (choice == null) return; var idx = actions.IndexOf(choice); if (idx >= 0) await AudioPreviewer.PlayAsync(ok[idx].WavPath!, choice);
        }
        finally { ProgressBar.IsVisible = false; BuildButton.IsEnabled = true; }
    }

    private async Task ClearCacheAsync()
    {
        bool ok = await DialogService.ShowConfirmAsync(this, "Clear", "Clear cache?", "OK", "Cancel"); if (!ok) return;
        try { var dir = FileSystem.CacheDirectory; foreach (var d in Directory.GetDirectories(dir)) try { Directory.Delete(d, true); } catch { } await DialogService.ShowAlertAsync(this, "Done", "Cache cleared.", "OK"); } catch { }
    }

    private async Task CheckUpdateAsync()
    {
        var info = await UpdateService.CheckAsync(); if (info == null || !info.HasUpdate) return;
        bool go = await DialogService.ShowConfirmAsync(this, Loc.T("update_title"), Loc.F("update_message_fmt", info.CurrentVersion, info.LatestVersion), Loc.T("update_title"), Loc.T("btn_update_later"));
        if (go) try { await Launcher.Default.OpenAsync(info.ReleaseUrl); } catch { }
    }

    private async Task ManualCheckUpdateAsync()
    {
        var info = await UpdateService.CheckAsync();
        if (info == null) { await DialogService.ShowAlertAsync(this, "Error", "Check failed", "OK"); return; }
        if (!info.HasUpdate) { await DialogService.ShowAlertAsync(this, "Update", Loc.F("update_latest_fmt", info.CurrentVersion), "OK"); return; }
        bool go = await DialogService.ShowConfirmAsync(this, Loc.T("update_title"), Loc.F("update_message_fmt", info.CurrentVersion, info.LatestVersion), Loc.T("update_title"), Loc.T("btn_update_later"));
        if (go) try { await Launcher.Default.OpenAsync(info.ReleaseUrl); } catch { }
    }
}
