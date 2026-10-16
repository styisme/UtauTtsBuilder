using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class LogViewerPage : ContentPage
{
    public LogViewerPage()
    {
        Logger.Info("LogViewerPage", "Constructing");
        InitializeComponent();
        ApplyLocalization();
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
            LogLabel.Text = Logger.ReadRecent(800);
            // 自动滚到底部
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(80);
                await LogScroll.ScrollToAsync(0, LogLabel.Height, false);
            });
        }
        catch (Exception ex)
        {
            LogLabel.Text = "(failed to load log: " + ex.Message + ")";
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
