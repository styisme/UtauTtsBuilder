using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class LogViewerPage : ContentPage
{
    public LogViewerPage()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Reload();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = Loc.T("log_viewer_title");
        RefreshBtn.Text = Loc.T("log_refresh");
        ShareBtn.Text = Loc.T("log_share");
        ClearBtn.Text = Loc.T("log_clear");
    }

    private void Reload()
    {
        try
        {
            var path = Logger.GetCurrentFilePath();

            long size = 0;
            try { if (File.Exists(path)) size = new FileInfo(path).Length; } catch { }

            InfoLabel.Text = $"{path}  ({size / 1024.0:F1} KB)";

            var text = Logger.ReadRecent(500);
            if (string.IsNullOrEmpty(text)) text = "(empty log)";
            LogLabel.Text = text;
        }
        catch (Exception ex)
        {
            LogLabel.Text = "(failed: " + ex.Message + ")";
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e) => Reload();

    private async void OnShareClicked(object sender, EventArgs e)
    {
        try
        {
            var path = Logger.GetCurrentFilePath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                await DialogService.ShowAlertAsync(this,
                    Loc.T("title_error"), "No log file", Loc.T("btn_ok"));
                return;
            }
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = Loc.T("log_share"),
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this,
                Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnClearClicked(object sender, EventArgs e)
    {
        bool ok = await DialogService.ShowConfirmAsync(this,
            Loc.T("log_clear"),
            Loc.T("log_clear_confirm"),
            Loc.T("btn_clear_action"),
            Loc.T("action_cancel"));
        if (!ok) return;
        Logger.Clear();
        Reload();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
