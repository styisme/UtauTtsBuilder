namespace UtauTtsBuilder;

public partial class AlertPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _tcs = new();
    private readonly string[] _buttons;
    private bool _closing;

    public AlertPage(string title, string message, params string[] buttons)
    {
        InitializeComponent();

        TitleLabel.Text = title ?? "";
        MessageLabel.Text = message ?? "";
        _buttons = buttons.Length > 0 ? buttons : new[] { "OK" };

        ApplyTheme();
        BuildButtons();
    }

    public Task<bool> WaitResultAsync() => _tcs.Task;

    private void ApplyTheme()
    {
        DialogBox.SetDynamicResource(Border.BackgroundColorProperty, "Md3SurfaceContainer");
        TitleLabel.SetDynamicResource(Label.TextColorProperty, "Md3OnSurface");
        MessageLabel.SetDynamicResource(Label.TextColorProperty, "Md3OnSurfaceVariant");
    }

    private void BuildButtons()
    {
        ButtonsRow.Clear();

        if (_buttons.Length == 1)
        {
            var btn = new Button
            {
                Text = _buttons[0],
                HeightRequest = 44,
                CornerRadius = 22,
                Padding = new Thickness(24, 0),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            };
            btn.SetDynamicResource(Button.BackgroundColorProperty, "Md3Primary");
            btn.SetDynamicResource(Button.TextColorProperty, "Md3OnPrimary");
            btn.Clicked += async (s, e) => await CloseAsync(true);
            ButtonsRow.Add(btn);
            return;
        }

        for (int i = 0; i < _buttons.Length; i++)
        {
            bool isLast = i == _buttons.Length - 1;
            var btn = new Button
            {
                Text = _buttons[i],
                HeightRequest = 44,
                CornerRadius = 22,
                Padding = new Thickness(20, 0),
                FontSize = 14,
                FontAttributes = isLast ? FontAttributes.Bold : FontAttributes.None,
                BorderWidth = isLast ? 0 : 1
            };

            if (isLast)
            {
                btn.SetDynamicResource(Button.BackgroundColorProperty, "Md3Primary");
                btn.SetDynamicResource(Button.TextColorProperty, "Md3OnPrimary");
            }
            else
            {
                btn.BackgroundColor = Colors.Transparent;
                btn.SetDynamicResource(Button.TextColorProperty, "Md3Primary");
                btn.SetDynamicResource(Button.BorderColorProperty, "Md3Outline");
            }

            bool result = isLast;
            btn.Clicked += async (s, e) => await CloseAsync(result);
            ButtonsRow.Add(btn);
        }
    }

    private async Task CloseAsync(bool result)
    {
        if (_closing) return;
        _closing = true;

        await Navigation.PopModalAsync();
        _tcs.TrySetResult(result);
    }

    protected override bool OnBackButtonPressed()
    {
        if (!_closing)
        {
            _closing = true;
            Dispatcher.Dispatch(async () =>
            {
                await Navigation.PopModalAsync();
                _tcs.TrySetResult(_buttons.Length == 1);
            });
        }
        return true;
    }
}
