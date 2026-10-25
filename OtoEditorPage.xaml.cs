using UtauTtsBuilder.Services;
using UtauTtsBuilder.Controls;

namespace UtauTtsBuilder;

public partial class OtoEditorPage : ContentPage
{
    private string _dirPath;
    private List<string> _wavFiles = new();
    private int _currentIndex = 0;
    private Dictionary<string, string[]> _otoData = new(); // 文件名 -> [别名, offset, consonant, cutoff, pre, overlap]
    
    private float _offsetMs = 0, _consonantMs = 0, _cutoffMs = 0;
    private double _maxDurationMs = 1000;

    public OtoEditorPage(string dirPath)
    {
        InitializeComponent();
        _dirPath = dirPath;
        LoadFiles();
        LoadOto();
        
        if (_wavFiles.Count > 0) LoadWav(0);
    }

    private void LoadFiles()
    {
        _wavFiles = Directory.GetFiles(_dirPath, "*.wav").OrderBy(f => f).ToList();
    }

    private void LoadOto()
    {
        var otoPath = Path.Combine(_dirPath, "oto.ini");
        if (!File.Exists(otoPath)) return;
        
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var lines = File.ReadAllLines(otoPath, System.Text.Encoding.GetEncoding("shift_jis"));
        foreach (var line in lines)
        {
            var idx = line.IndexOf('=');
            if (idx <= 0) continue;
            var file = line.Substring(0, idx);
            var parts = line.Substring(idx + 1).Split(',');
            _otoData[file] = parts;
        }
    }

    private void LoadWav(int index)
    {
        if (index < 0 || index >= _wavFiles.Count) return;
        _currentIndex = index;
        var file = _wavFiles[index];
        var fileName = Path.GetFileName(file);
        
        FileNameLabel.Text = fileName;
        
        // 波形
        var data = WaveformRenderer.GetWaveformData(file, 200);
        Waveform.WaveformData = data;
        
        // 时长
        _maxDurationMs = WavProcessor.GetDurationMs(file);
        
        // 读取 oto 参数
        if (_otoData.TryGetValue(fileName, out var parts) && parts.Length >= 4)
        {
            _offsetMs = float.Parse(parts[1]);
            _consonantMs = float.Parse(parts[2]);
            _cutoffMs = float.Parse(parts[3]);
        }
        else
        {
            _offsetMs = 0;
            _consonantMs = (float)(_maxDurationMs * 0.2);
            _cutoffMs = (float)(_maxDurationMs * 0.8);
        }
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        OffsetLabel.Text = $"{_offsetMs:F0} ms";
        ConsonantLabel.Text = $"{_consonantMs:F0} ms";
        CutoffLabel.Text = $"{_cutoffMs:F0} ms";

        // 更新线位置（百分比）
        double width = Waveform.Width > 0 ? Waveform.Width : 300;
        AbsoluteLayout.SetLayoutBounds(OffsetLine, new Rect(width * (_offsetMs / _maxDurationMs), 0, 2, 1));
        AbsoluteLayout.SetLayoutBounds(ConsonantLine, new Rect(width * (_consonantMs / _maxDurationMs), 0, 2, 1));
        AbsoluteLayout.SetLayoutBounds(CutoffLine, new Rect(width * (_cutoffMs / _maxDurationMs), 0, 2, 1));
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateUI();
    }

    private void OnPrevClicked(object sender, EventArgs e) => LoadWav(_currentIndex - 1);
    private void OnNextClicked(object sender, EventArgs e) => LoadWav(_currentIndex + 1);

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var fileName = Path.GetFileName(_wavFiles[_currentIndex]);
        if (!_otoData.ContainsKey(fileName))
            _otoData[fileName] = new[] { Path.GetFileNameWithoutExtension(fileName), "0", "0", "0", "0", "0" };
            
        _otoData[fileName][1] = _offsetMs.ToString("F0");
        _otoData[fileName][2] = _consonantMs.ToString("F0");
        _otoData[fileName][3] = _cutoffMs.ToString("F0");

        // 写入文件
        var otoPath = Path.Combine(_dirPath, "oto.ini");
        var sb = new System.Text.StringBuilder();
        foreach (var kv in _otoData)
        {
            sb.AppendLine($"{kv.Key}={string.Join(",", kv.Value)}");
        }
        
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var sjis = System.Text.Encoding.GetEncoding("shift_jis");
        await File.WriteAllTextAsync(otoPath, sb.ToString(), sjis);
        
        await DialogService.ShowAlertAsync(this, "Save", "oto.ini saved successfully.", "OK");
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
