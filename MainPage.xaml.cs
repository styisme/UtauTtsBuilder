using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _engines = new();
    private string? _iconPath;
    private string? _portraitPath;

    public MainPage()
    {
        InitializeComponent();
        InitLanguagePicker();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ThemeService.Apply();
        if (_engines.Count == 0) LoadEngines();
    }

    private void InitLanguagePicker()
    {
        var items = new List<string>
        {
            ReclistProvider.GetDisplayName(VoicebankLanguage.Japanese),
            ReclistProvider.GetDisplayName(VoicebankLanguage.Chinese),
            ReclistProvider.GetDisplayName(VoicebankLanguage.English),
        };

        if (ReclistProvider.HasCustomReclist)
        {
            items.Add(ReclistProvider.GetDisplayName(VoicebankLanguage.Custom));
            ClearReclistBtn.IsVisible = true;
        }
        else
        {
            ClearReclistBtn.IsVisible = false;
        }

        LanguagePicker.ItemsSource = items;
        LanguagePicker.SelectedIndex = 0;
    }

    private VoicebankLanguage GetSelectedLanguage()
    {
        int idx = LanguagePicker.SelectedIndex;
        if (idx == 1) return VoicebankLanguage.Chinese;
        if (idx == 2) return VoicebankLanguage.English;
        if (idx == 3 && ReclistProvider.HasCustomReclist) return VoicebankLanguage.Custom;
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
            StatusLabel.Text = "已发现 " + _engines.Count + " 个 TTS 引擎";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "加载 TTS 引擎失败: " + ex.Message;
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

    // ===== 导入自定义音素表 =====
    private async void OnImportReclistClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "选择音素表文件"
            });
            if (result == null) return;

            string content;
            using (var stream = await result.OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync();
            }

            var reclist = ParseReclist(content, out var langFromFile);
            if (reclist.Count == 0)
            {
                await DisplayAlertAsync("解析失败",
                    "音素表里没有解析到任何音节。\n\n" +
                    "格式要求（每行一条）：\n" +
                    "  别名=文件名\n" +
                    "  别名,文件名\n" +
                    "  别名 文件名\n\n" +
                    "以 # 开头的行为注释，可选指定 TTS 语言：\n" +
                    "  # language: ja-JP",
                    "好");
                return;
            }

            // 确定 TTS 发音语言
            string ttsLang = langFromFile;
            if (string.IsNullOrEmpty(ttsLang))
            {
                var choice = await DisplayActionSheet(
                    "选择 TTS 发音语言", "取消", null,
                    "日语 (ja-JP)", "中文 (zh-CN)", "英语 (en-US)");
                if (choice == "取消" || string.IsNullOrEmpty(choice)) return;
                if (choice.StartsWith("中文")) ttsLang = "zh-CN";
                else if (choice.StartsWith("英语")) ttsLang = "en-US";
                else ttsLang = "ja-JP";
            }

            string name = Path.GetFileNameWithoutExtension(result.FileName);
            if (string.IsNullOrEmpty(name)) name = "自定义";

            ReclistProvider.SaveCustomReclist(name, ttsLang, reclist);

            InitLanguagePicker();
            LanguagePicker.SelectedIndex = LanguagePicker.ItemsSource.Count - 1;

            await DisplayAlertAsync("导入成功",
                $"音素表: {name}\n" +
                $"音节数: {reclist.Count}\n" +
                $"TTS 语言: {ttsLang}",
                "好");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("导入失败", ex.Message, "好");
        }
    }

    private async void OnClearReclistClicked(object sender, EventArgs e)
    {
        bool ok = await DisplayAlertAsync("清除自定义音素表",
            "确定要清除吗？", "清除", "取消");
        if (!ok) return;
        ReclistProvider.ClearCustomReclist();
        InitLanguagePicker();
    }

    /// <summary>
    /// 解析音素表文本。
    /// 支持格式：
    ///   别名=文件名
    ///   别名,文件名
    ///   别名  文件名（空格 / tab）
    ///   # language: ja-JP  （可选，指定 TTS 语言）
    /// </summary>
    private static Dictionary<string, string> ParseReclist(
        string content, out string languageCode)
    {
        var result = new Dictionary<string, string>();
        languageCode = "";

        var lines = content.Replace("\r\n", "\n").Split('\n');
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 注释：检查是否指定语言
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

            // 找分隔符
            int sep = -1;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '=' || c == ',' || c == '\t' || c == ' ')
                {
                    sep = i;
                    break;
                }
            }
            if (sep <= 0) continue;

            var alias = line.Substring(0, sep).Trim();
            var fileName = line.Substring(sep + 1).Trim();
            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(fileName)) continue;

            result[alias] = fileName;
        }

        return result;
    }

    // ===== 自动填写声库名 =====
    private void OnEngineChanged(object sender, EventArgs e)
    {
        if (EnginePicker.SelectedIndex < 0 || EnginePicker.SelectedIndex >= _engines.Count)
            return;
        if (!string.IsNullOrWhiteSpace(NameEntry.Text))
            return;

        var label = _engines[EnginePicker.SelectedIndex].Label;
        if (string.IsNullOrWhiteSpace(label)) return;

        int idx = -1;
        for (int i = 2; i < label.Length; i++)
        {
            if (char.IsDigit(label[i])) { idx = i; break; }
        }
        var name = idx > 0 ? label.Substring(0, idx).TrimEnd('.', ' ', '-') : label;
        if (!string.IsNullOrWhiteSpace(name))
            NameEntry.Text = name;
    }

    // ===== 图标 =====
    private async void OnPickIconClicked(object sender, EventArgs e)
    {
        var p = await PickImageAsync("选择图标");
        if (p != null) { _iconPath = p; IconLabel.Text = "已选择: " + Path.GetFileName(p); }
    }

    private void OnClearIconClicked(object sender, EventArgs e)
    {
        _iconPath = null; IconLabel.Text = "未选择";
    }

    // ===== 立绘 =====
    private async void OnPickPortraitClicked(object sender, EventArgs e)
    {
        var p = await PickImageAsync("选择立绘");
        if (p != null) { _portraitPath = p; PortraitLabel.Text = "已选择: " + Path.GetFileName(p); }
    }

    private void OnClearPortraitClicked(object sender, EventArgs e)
    {
        _portraitPath = null; PortraitLabel.Text = "未选择";
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
            {
                await src.CopyToAsync(dst);
            }
            return local;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("选择图片失败", ex.Message, "好");
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
            await DisplayAlertAsync("提示", "请选择引擎并输入声库名称", "确定");
            return;
        }

        string outputDir = ResolveOutputDir(name, customPath);

        try { Directory.CreateDirectory(outputDir); }
        catch (Exception ex)
        {
            await DisplayAlertAsync("无法创建目录", ex.Message, "确定");
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
                StatusLabel.Text = "正在压缩 ZIP...";
                try
                {
                    zipPath = outputDir.TrimEnd('/', (char)92) + ".zip";
                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    ZipFile.CreateFromDirectory(outputDir, zipPath);
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = "压缩失败: " + ex.Message;
                }
            }

            string msg = "声库已生成到:" + Environment.NewLine + outputDir;
            if (zipPath != null)
                msg += Environment.NewLine + Environment.NewLine + "ZIP: " + zipPath;

            if (zipPath != null && File.Exists(zipPath))
            {
                bool share = await DisplayAlertAsync("完成", msg, "分享 ZIP", "关闭");
                if (share)
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "分享声库 ZIP",
                        File = new ShareFile(zipPath)
                    });
                }
            }
            else
            {
                await DisplayAlertAsync("完成", msg, "好");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("错误", ex.Message, "确定");
        }
        finally
        {
            ProgressBar.IsVisible = false;
            BuildButton.IsEnabled = true;
            RefreshBtn.IsEnabled = true;
        }
    }
}
