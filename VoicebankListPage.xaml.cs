using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class VoicebankListPage : ContentPage
{
    public VoicebankListPage()
    {
        Logger.Info("VoicebankListPage", "Constructing");
        InitializeComponent();
        ApplyLocalization();
        Reload();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = Loc.T("list_title");
        RefreshBtn.Text = Loc.T("btn_refresh_engines");
    }

    private void Reload()
    {
        ItemsContainer.Clear();
        var items = VoicebankScanner.ScanAll();

        if (items.Count == 0)
        {
            var empty = new Label
            {
                Text = Loc.T("list_empty"),
                FontSize = 14,
                TextColor = (Color)Application.Current!.Resources["Md3OnSurfaceVariant"],
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            ItemsContainer.Add(empty);
            return;
        }

        foreach (var vb in items)
        {
            ItemsContainer.Add(BuildItem(vb));
        }
    }

    private Border BuildItem(VoicebankInfo vb)
    {
        var nameLabel = new Label
        {
            Text = vb.Name,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["Md3OnSurface"]
        };

        var metaLabel = new Label
        {
            Text = $"{vb.WavCount} wav · {vb.SizeDisplay}",
            FontSize = 12,
            TextColor = (Color)Application.Current!.Resources["Md3OnSurfaceVariant"]
        };

        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { nameLabel, metaLabel }
        };

        var border = new Border
        {
            Padding = new Thickness(16, 12),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 12
            },
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

        var sPreview = Loc.T("action_view_preview");
        actions.Add(sPreview); keys[sPreview] = "view_preview";

        var sShare = Loc.T("list_share_zip");
        if (vb.ZipPath != null)
        {
            actions.Add(sShare); keys[sShare] = "share_zip";
        }
        var sOto = Loc.T("action_edit_oto");
        actions.Add(sOto); keys[sOto] = "edit_oto";
        var sUst = Loc.T("action_gen_ust");
        actions.Add(sUst); keys[sUst] = "gen_ust";
        var sDel = Loc.T("list_delete");
        actions.Add(sDel); keys[sDel] = "delete";

        var page = new ActionSheetPage(vb.Name, actions, Loc.T("action_cancel"));
        var task = page.WaitResultAsync();
        await Navigation.PushAsync(page);
        var choice = await task;

        if (choice == null) return;
        if (!keys.TryGetValue(choice, out var action)) return;

        switch (action)
        {
            case "view_preview":
                await Navigation.PushAsync(new PreviewImagePage(vb.DirectoryPath, vb.Name));
                break;

            case "share_zip":
                try
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = Loc.T("share_zip_title"),
                        File = new ShareFile(vb.ZipPath!)
                    });
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;

            case "edit_oto":
                try
                {
                    var oto = Path.Combine(vb.DirectoryPath, "oto.ini");
                    if (!File.Exists(oto))
                    {
                        await DialogService.ShowAlertAsync(this,
                            Loc.T("title_error"), "oto.ini not found", Loc.T("btn_ok"));
                        return;
                    }
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = Loc.T("action_edit_oto"),
                        File = new ShareFile(oto)
                    });
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;

            case "gen_ust":
                try
                {
                    var ustPath = await UstTemplateGenerator.GenerateAsync(
                        vb.DirectoryPath, vb.Name);
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_done"),
                        Loc.F("ust_generated_fmt", ustPath), Loc.T("btn_ok"));
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;

            case "delete":
                bool ok = await DialogService.ShowConfirmAsync(this,
                    Loc.T("list_delete"),
                    Loc.F("list_delete_confirm_fmt", vb.Name),
                    Loc.T("btn_delete_yes"),
                    Loc.T("action_cancel"));
                if (!ok) return;

                try
                {
                    Directory.Delete(vb.DirectoryPath, true);
                    if (vb.ZipPath != null && File.Exists(vb.ZipPath))
                        File.Delete(vb.ZipPath);
                    Reload();
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAlertAsync(this,
                        Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
                }
                break;
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e) => Reload();

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
