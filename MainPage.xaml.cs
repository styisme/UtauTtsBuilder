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

        // 每次回到主页时重新应用主题（设置页可能改过）
        ThemeService.Apply();

        if (_engines.Count == 0) LoadEngines();
    }

    private void InitLanguagePicker()
    {
        LanguagePicker.ItemsSource = new List<string>
        {
            ReclistProvider.GetDisplayName(VoicebankLanguage.Japanese),
            ReclistProvider.GetDisplayName(VoicebankLanguage.Chinese),
            ReclistProvider.GetDisplayName(VoicebankLanguage.English),
        };
        LanguagePicker.SelectedIndex = 0;
    }

    private VoicebankLanguage GetSelectedLanguage() =>
        LanguagePicker.SelectedIndex switch
        {
            1 => VoicebankLanguage.Chinese,
            2 => VoicebankLanguage.English,
            _ => VoicebankLanguage.Japanese
        };

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
