using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class DiffSingerExportPage : ContentPage
{
    private string _dirPath;
    private string _vbName;

    public DiffSingerExportPage(string dirPath, string name)
    {
        InitializeComponent();
        _dirPath = dirPath;
        _vbName = name;
        DescLabel.Text = $"正在为声库「{name}」提取丰富特征。开启数据增强将增加生成时间，但能有效防止模型过拟合。";
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        ExportBtn.IsEnabled = false;
        ProgressBar.IsVisible = true;
        ProgressBar.Progress = 0;

        var options = new DiffsingerExporter.ExportOptions
        {
            ExtractF0 = ChkF0.IsChecked,
            ExtractEnergy = ChkEnergy.IsChecked,
            DataAugmentation = ChkAug.IsChecked,
            GenerateConfig = ChkConfig.IsChecked
        };

        var progress = new Progress<string>(msg => StatusLabel.Text = msg);

        try
        {
            // 由于导出可能耗时，用 Task.Run 避免 UI 卡死
            var zip = await Task.Run(() => DiffsingerExporter.ExportAsync(_dirPath, _vbName, options, progress));
            
            ProgressBar.Progress = 1;
            StatusLabel.Text = "生成完毕！";
            
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "分享 DiffSinger 数据集",
                File = new ShareFile(zip)
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, "导出失败", ex.Message, "OK");
            StatusLabel.Text = "导出失败。";
        }
        finally
        {
            ExportBtn.IsEnabled = true;
            ProgressBar.IsVisible = false;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
