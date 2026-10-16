using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class PreviewImagePage : ContentPage
{
    private readonly string _outputDir;
    private readonly string _name;
    private int _wavCount;
    private string? _previewPath;

    public PreviewImagePage(string outputDir, string name)
    {
        InitializeComponent();
        _outputDir = outputDir;
        _name = name;

        ApplyLocalization();
        LoadAsync();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = _name;
        WaveSectionLabel.Text = Loc.T("preview_waveform");
        PlayBtn.Text = Loc.T("preview_play");
        SampleBtn.Text = Loc.T("preview_sample");
        RegenerateBtn.Text = Loc.T("preview_regen");
    }

    private async void LoadAsync()
    {
        try
        {
            var wavs = Directory.GetFiles(_outputDir, "*.wav");
            _wavCount = wavs.Length;
            WaveInfoLabel.Text = Loc.F("preview_wav_count_fmt", _wavCount);

            // 波形：抽第一个 wav
            if (wavs.Length > 0)
            {
                var data = WaveformRenderer.GetWaveformData(wavs[0], 200);
                Waveform.WaveformData = data;
                WaveInfoLabel.Text += "  ·  " + Path.GetFileName(wavs[0]);
            }

            // 预览图：已有就显示，没有就生成
            _previewPath = Path.Combine(_outputDir, "preview.png");
            if (!File.Exists(_previewPath))
            {
                var iconPath = FindIcon();
                await PreviewImageGenerator.GenerateAsync(
                    _outputDir, _name, _wavCount, iconPath);
            }

            if (File.Exists(_previewPath))
            {
                PreviewImage.Source = ImageSource.FromFile(_previewPath);
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, "Error", ex.Message, Loc.T("btn_ok"));
        }
    }

    private string? FindIcon()
    {
        foreach (var ext in new[] { ".png", ".jpg", ".jpeg", ".bmp" })
        {
            var p = Path.Combine(_outputDir, "icon" + ext);
            if (File.Exists(p)) return p;
        }
        return null;
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        var wavs = Directory.GetFiles(_outputDir, "*.wav");
        if (wavs.Length == 0)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), "No wav files", Loc.T("btn_ok"));
            return;
        }
        try
        {
            await AudioPreviewer.PlayAsync(wavs[0],
                Loc.T("preview_play") + " - " + Path.GetFileName(wavs[0]));
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnSampleClicked(object sender, EventArgs e)
    {
        var samples = AudioPreviewer.SampleRandom(_outputDir, 3);
        if (samples.Count == 0)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), "No wav files", Loc.T("btn_ok"));
            return;
        }

        var actions = samples.Select(Path.GetFileName).ToList()!;
        var page = new ActionSheetPage(Loc.T("preview_sample"), actions!, Loc.T("action_cancel"));
        var task = page.WaitResultAsync();
        await Navigation.PushAsync(page);
        var choice = await task;
        if (choice == null) return;

        var idx = actions.IndexOf(choice);
        if (idx < 0) return;

        try
        {
            await AudioPreviewer.PlayAsync(samples[idx], choice!);
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnRegenerateClicked(object sender, EventArgs e)
    {
        try
        {
            var iconPath = FindIcon();
            var p = await PreviewImageGenerator.GenerateAsync(
                _outputDir, _name, _wavCount, iconPath);
            if (p != null && File.Exists(p))
            {
                // 强制刷新 Image
                PreviewImage.Source = null;
                PreviewImage.Source = ImageSource.FromFile(p);
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
