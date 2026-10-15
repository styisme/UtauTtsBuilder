using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _engines = new();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_engines.Count == 0) LoadEngines();
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

    /// <summary>
    /// 获取基础公共存储目录：/storage/emulated/0/
    /// </summary>
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

    /// <summary>
    /// 决定输出目录
    ///  - 无自定义路径: /storage/emulated/0/Download/UtauTtsBuilder/{name}
    ///  - 有自定义路径: /storage/emulated/0/{自定义路径}/{name}
    /// </summary>
    private string ResolveOutputDir(string name, string? customRelative)
    {
        var root = GetExternalRoot();

        if (!string.IsNullOrEmpty(customRelative))
        {
            // 去掉前导斜杠，规范化
            customRelative = customRelative.Trim().TrimStart('/', '\\');
            return Path.Combine(root, customRelative, name);
        }

        return Path.Combine(root, "Download", "UtauTtsBuilder", name);
    }

    private async void OnBuildClicked(object sender, EventArgs e)
    {
        int engineIdx = EnginePicker.SelectedIndex;
        var name = NameEntry.Text?.Trim();
        var customPath = OutputDirEntry.Text?.Trim();

        if (engineIdx < 0 || string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("提示", "请选择引擎并输入声库名称", "确定");
            return;
        }

        string outputDir = ResolveOutputDir(name, customPath);

        try
        {
            Directory.CreateDirectory(outputDir);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("无法创建目录", ex.Message, "确定");
            return;
        }

        var engine = _engines[engineIdx];
        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(engine.Name, progress);

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
                    zipPath = outputDir.TrimEnd('/', '\\') + ".zip";
                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    ZipFile.CreateFromDirectory(outputDir, zipPath);
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = "压缩失败: " + ex.Message;
                }
            }

            string msg = "声库已生成到:\n" + outputDir;
            if (zipPath != null) msg += "\n\nZIP: " + zipPath;

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
