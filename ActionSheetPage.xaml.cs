namespace UtauTtsBuilder;

public partial class ActionSheetPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _tcs = new();
    private readonly List<string> _actions;
    private bool _closing;

    public ActionSheetPage(string title, List<string> actions, string cancelText)
    {
        InitializeComponent();
        _actions = actions;
        TitleLabel.Text = title;
        CancelBtn.Text = cancelText;
        BuildActions();
    }

    public Task<string?> WaitResultAsync() => _tcs.Task;

    private void BuildActions()
    {
        ActionsContainer.Clear();

        foreach (var action in _actions)
        {
            var label = new Label
            {
                Text = action,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current!.Resources["Md3OnSurface"]
            };

            var border = new Border
            {
                StrokeThickness = 0,
                Padding = new Thickness(20, 16),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 12
                },
                Content = label
            };
            border.SetDynamicResource(Border.BackgroundColorProperty, "Md3SurfaceContainer");

            var captured = action;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await CloseAsync(captured);
            border.GestureRecognizers.Add(tap);

            ActionsContainer.Add(border);
        }
    }

    /// <summary>
    /// 关键：先 PopAsync 完成，再设置 TCS 结果。
    /// 这样调用方 await 到结果时，导航栈已经稳定，Push 新页面就不会冲突。
    /// </summary>
    private async Task CloseAsync(string? result)
    {
        if (_closing) return;
        _closing = true;

        await Navigation.PopAsync();
        _tcs.TrySetResult(result);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }

    protected override bool OnBackButtonPressed()
    {
        if (!_closing)
        {
            _closing = true;
            Dispatcher.Dispatch(async () =>
            {
                await Navigation.PopAsync();
                _tcs.TrySetResult(null);
            });
        }
        return true;
    }
}
