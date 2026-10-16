using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class LogViewerPage : ContentPage
{
    private bool _subscribed;

    public LogViewerPage()
    {
        Logger.Info("LogViewerPage", "Constructing");
        InitializeComponent();
        ApplyLocalization();
        Logger.Info("LogViewerPage", "Constructed OK");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Logger.Info("LogViewerPage", "Appearing");
        if (!_subscribed)
        {
            Logger.LogChanged += OnLogChanged;
            _subscribed = true;
        }
        Reload();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Logger.Info("LogViewerPage", "Disappearing");
        if (_subscribed)
        {
            Logger.LogChanged -= OnLogChanged;
            _subscribed = false;
        }
    }

    private void OnLogChanged()
    {
        // 日志更新时刷新（在 UI 线程）
        Dispatcher.Dispatch(Reload);
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
            var lines = Logger.GetLineCount();
            var size = 0L;
            try { if (File.Exists(path)) size = new FileInfo(path).Length; } catch { }

            InfoLabel.Text = $"{path}\n{lines} lines · {size / 1024.0:F1} KB";

            var text = Logger.ReadRecent(800);
            if (string.IsNullOrWhiteSpace(text)) text = "(empty)";
            LogLabel.Text = text;

            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(80);
                try { await LogScroll.ScrollToAsync(0, LogLabel.Height, false); }
                catch { }
            });
        }
        catch (Exception ex)
        {
            LogLabel.Text = "(failed to load log: " + ex.Message + ")";
            Logger.Error("LogViewerPage", ex);
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
            Logger.Error("LogViewerPage", ex);
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
