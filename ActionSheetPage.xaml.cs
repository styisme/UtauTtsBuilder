namespace UtauTtsBuilder;

public partial class ActionSheetPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _tcs = new();
    private readonly List<string> _actions;

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
            tap.Tapped += async (s, e) => await OnActionSelected(captured);
            border.GestureRecognizers.Add(tap);

            ActionsContainer.Add(border);
        }
    }

    private async Task OnActionSelected(string action)
    {
        _tcs.TrySetResult(action);
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Navigation.PopAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        _tcs.TrySetResult(null);
        return base.OnBackButtonPressed();
    }
}
