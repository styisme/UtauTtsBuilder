using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class VoicebankListPage : ContentPage
{
    public VoicebankListPage()
    {
        InitializeComponent();
        ApplyLocalization();
        Reload();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = Loc.T("list_title");
        RefreshBtn.Text = Loc.T("list_refresh");
    }

    private void Reload()
    {
        ItemsContainer.Clear();
        var items = VoicebankScanner.ScanAll();

        if (items.Count == 0)
        {
            ItemsContainer.Add(new Label { Text = Loc.T("list_empty"), HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 40, 0, 0) });
            return;
        }

        foreach (var vb in items) ItemsContainer.Add(BuildItem(vb));
    }

    private Border BuildItem(VoicebankInfo vb)
    {
        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = {
                new Label { Text = vb.Name, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = (Color)Application.Current!.Resources["Md3OnSurface"] },
                new Label { Text = $"{vb.WavCount} wav · {vb.SizeDisplay}", FontSize = 12, TextColor = (Color)Application.Current!.Resources["Md3OnSurfaceVariant"] }
            }
        };

        var border = new Border
        {
            Padding = new Thickness(16, 12),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Content = textStack
        };
        border.SetDynamicResource(Border.BackgroundColorProperty, "Md3SurfaceContainer");

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => await ShowActionsAsync(vb);
        border.GestureRecognizers.Add(tap);

        return border;
    }

    private async Task ShowActionsAsync(VoicebankInfo vb)
    {
        var actions = new List<string>();
        var keys = new Dictionary<string, string>();

        void AddAction(string key, string label) { actions.Add(label); keys[label] = key; }

        AddAction("editor", "Oto 微调器");
        AddAction("preview", "序列试听");
        AddAction("export_ds", "导出 DiffSinger 数据集");
        AddAction("share", "分享 ZIP");
        AddAction("delete", "删除声库");

        var page = new ActionSheetPage(vb.Name, actions, Loc.T("action_cancel"));
        var task = page.WaitResultAsync();
        await Navigation.PushAsync(page);
        var choice = await task;

        if (choice == null) return;
        if (!keys.TryGetValue(choice, out var action)) return;

        try
        {
            switch (action)
            {
                case "editor":
                    await Navigation.PushAsync(new OtoEditorPage(vb.DirectoryPath));
                    break;
                case "preview":
                    await Navigation.PushAsync(new PianoRollPreviewPage(vb.DirectoryPath));
                    break;
                case "export_ds":
                    await ExportDiffsingerAsync(vb);
                    break;
                case "share":
                    await Share.Default.RequestAsync(new ShareFileRequest { Title = "Share", File = new ShareFile(vb.ZipPath!) });
                    break;
                case "delete":
                    if (await DialogService.ShowConfirmAsync(this, "删除", $"确定删除 {vb.Name}?", "删除", "取消"))
                    {
                        Directory.Delete(vb.DirectoryPath, true);
                        if (vb.ZipPath != null && File.Exists(vb.ZipPath)) File.Delete(vb.ZipPath);
                        Reload();
                    }
                    break;
            }
        }
        catch (Exception ex) { await DialogService.ShowAlertAsync(this, "Error", ex.Message, "OK"); }
    }

    private async Task ExportDiffsingerAsync(VoicebankInfo vb)
    {
        try
        {
            await DialogService.ShowAlertAsync(this, "导出中", "正在为您打包 DiffSinger 数据集，请稍候...", "OK");
            var zip = await DiffsingerExporter.ExportAsync(vb.DirectoryPath, vb.Name);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "分享 DiffSinger 数据集",
                File = new ShareFile(zip)
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, "导出失败", ex.Message, "OK");
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e) => Reload();
    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
