using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class PianoRollPreviewPage : ContentPage
{
    private string _dirPath;

    public PianoRollPreviewPage(string dirPath)
    {
        InitializeComponent();
        _dirPath = dirPath;
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        var text = SequenceEntry.Text ?? "";
        var syllables = text.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (syllables.Length == 0) return;

        StatusLabel.Text = "正在拼接音频...";
        var combinedPath = Path.Combine(FileSystem.CacheDirectory, "preview_seq.wav");
        
        try
        {
            var allSamples = new List<float>();
            foreach (var syl in syllables)
            {
                var file = Path.Combine(_dirPath, syl + ".wav");
                if (!File.Exists(file))
                    file = Directory.GetFiles(_dirPath, "*.wav").FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == syl);

                if (File.Exists(file))
                {
                    var info = WavProcessor.ReadWav(file);
                    var samples = WavProcessor.DecodeToFloat(info.RawData, info.BitsPerSample, info.Channels);
                    allSamples.AddRange(samples);
                    allSamples.AddRange(new float[4410]); // 100ms pause
                }
            }

            WavProcessor.WriteWav16(combinedPath, allSamples.ToArray(), 44100, 1);
            StatusLabel.Text = "播放中...";
            await AudioPreviewer.PlayAsync(combinedPath, "序列试听");
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK");
            StatusLabel.Text = "播放失败。";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
