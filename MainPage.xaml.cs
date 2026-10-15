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

    private async void OnBuildClicked(object sender, EventArgs e)
    {
        int engineIdx = EnginePicker.SelectedIndex;
        var name = NameEntry.Text?.Trim();
        var outputDir = OutputDirEntry.Text?.Trim();

        if (engineIdx < 0 || string.IsNullOrEmpty(name))
        {
            await DisplayAlert("提示", "请选择引擎并输入声库名称", "确定");
            return;
        }

        if (string.IsNullOrEmpty(outputDir))
            outputDir = Path.Combine(FileSystem.AppDataDirectory, "voicebanks", name);

        Directory.CreateDirectory(outputDir);

        var engine = _engines[engineIdx];
        var progress = new Progress<string>(msg => { StatusLabel.Text = msg; });
        var builder = new VoicebankBuilder(engine.Name, progress);

        ProgressBar.IsVisible = true;
        BuildButton.IsEnabled = false;
        RefreshBtn.IsEnabled = false;

        try
        {
            await builder.BuildAsync(outputDir, name);
            await DisplayAlert("完成", "声库已生成:\n" + outputDir, "好");
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
        }
    }
}
