using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class ReclistPreviewPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _tcs = new();
    private readonly Dictionary<string, string> _reclist;
    private readonly string _languageCode;

    public ReclistPreviewPage(
        string fileName, Dictionary<string, string> reclist, string languageCode)
    {
        InitializeComponent();
        _reclist = reclist;
        _languageCode = languageCode;

        TitleLabel.Text = fileName;
        SummaryLabel.Text = Loc.F("reclist_preview_summary_fmt",
            reclist.Count, languageCode);
        ConfirmBtn.Text = Loc.T("reclist_preview_confirm");

        var lines = new List<string>();
        int shown = 0;
        foreach (var kv in reclist)
        {
            if (shown++ >= 20) break;
            lines.Add($"{kv.Key,-12}→  {kv.Value}");
        }
        if (reclist.Count > 20)
            lines.Add($"... (+{reclist.Count - 20} more)");
        PreviewText.Text = string.Join("\n", lines);
    }

    public Task<bool> WaitResultAsync() => _tcs.Task;

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(true);
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(false);
        await Navigation.PopAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        _tcs.TrySetResult(false);
        return base.OnBackButtonPressed();
    }
}
