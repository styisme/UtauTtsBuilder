using System.IO.Compression;
using UtauTtsBuilder.Platforms.Android;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class MainPage : ContentPage
{
    private List<TtsEngineInfo> _engines = new();
    private string? _selectedOutputDir;

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

    private async void OnPickFolderClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FolderPicker.Default.PickAsync();
            if (result != null && result.IsSuccessful && result.Folder != null)
            {
                var path = result.Folder.Path;
                if (string.IsNullOrEmpty(path) || path.StartsWith("content://"))
                {
                    await DisplayAlert("提示",
                        "该目录不支持直接写入，请选择『内部存储』下的文件夹。", "好");
                    return;
                }
                _selectedOutputDir = path;
                OutputDirLabel.Text = "当前目录: " + path;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("选择目录失败", ex.Message, "好");
        }
    }

    private string GetDefaultOutputDir(string name)
    {
#if ANDROID
        var downloadPath = global::Android.OS.Environment
            .GetExternalStoragePublicDirectory(
                global::Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
        var root = downloadPath ?? FileSystem.AppDataDirectory;
        return Path.Combine(root, "UtauTtsBuilder", name);
#else
        return Path.Combine(FileSystem.AppDataDirectory, "voicebanks", name);
#endif
    }

    private async void OnBuildClicked(object sender, EventArgs e)
    {
        int engineIdx = EnginePicker.SelectedIndex;
        var name = NameEntry.Text?.Trim();

        if (engineIdx < 0 || string.IsNullOrEmpty(name))
        {
            await DisplayAlert("提示", "请选择引擎并输入声库名称", "确定");
            return;
        }

        // 确定输出目录
        string baseDir = _selectedOutputDir ?? GetDefaultOutputDir(name);
        // 如果用户选了目录，就在里面再加一层 {name}
        string outputDir = _selectedOutputDir != null
            ? Path.Combine(baseDir, name)
            : baseDir;

        try
        {
            Directory.CreateDirectory(outputDir);
        }
        catch (Exception ex)
        {
            await DisplayAlert("无法创建目录", ex.Message, "确定");
            return;
        }

        var engine = _engines[engineIdx];
        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(engine.Name, progress);

        ProgressBar.IsVisible = true;
        BuildButton.IsEnabled = false;
        RefreshBtn.IsEnabled = false;
        PickDirBtn.IsEnabled = false;

        string? zipPath = null;

        try
        {
            await builder.BuildAsync(outputDir, name);

            // ===== 自动压缩 ZIP =====
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

            // ===== 完成弹窗 =====
            string msg = "声库已生成到:\n" + outputDir;
            if (zipPath != null) msg += "\n\nZIP: " + zipPath;

            if (zipPath != null && File.Exists(zipPath))
            {
                var action = await DisplayAlert("完成", msg,
                    "分享 ZIP", "关闭");
                if (action)
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
                await DisplayAlert("完成", msg, "好");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", ex.Message, "确定");
        }
        finally
        {
            ProgressBar.IsVisible = false;
            BuildButton.IsEnabled = true;
            RefreshBtn.IsEnabled = true;
            PickDirBtn.IsEnabled = true;
        }
    }
}
