namespace UtauTtsBuilder;

public partial class AlertPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _tcs = new();
    private readonly string[] _buttons;

    public AlertPage(string title, string message, params string[] buttons)
    {
        InitializeComponent();

        TitleLabel.Text = title ?? "";
        MessageLabel.Text = message ?? "";
        _buttons = buttons.Length > 0 ? buttons : new[] { "OK" };

        // 应用主题色
        ApplyTheme();

        // 构建按钮
        BuildButtons();
    }

    public Task<bool> WaitResultAsync() => _tcs.Task;

    private void ApplyTheme()
    {
        var res = Application.Current!.Resources;

        DialogBox.SetDynamicResource(Border.BackgroundColorProperty, "Md3SurfaceContainer");
        TitleLabel.SetDynamicResource(Label.TextColorProperty, "Md3OnSurface");
        MessageLabel.SetDynamicResource(Label.TextColorProperty, "Md3OnSurfaceVariant");
    }

    private void BuildButtons()
    {
        ButtonsRow.Clear();

        // 单按钮 → 主色填充
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

        // 两个按钮 → 第一个是取消（轮廓），第二个是确定（填充）
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

            bool result = isLast;  // 最后一个按钮返回 true
            btn.Clicked += async (s, e) => await CloseAsync(result);
            ButtonsRow.Add(btn);
        }
    }

    private async Task CloseAsync(bool result)
    {
        _tcs.TrySetResult(result);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        // 单按钮时返回 true，双按钮时返回 false（视为取消）
        _tcs.TrySetResult(_buttons.Length == 1);
        Dispatcher.Dispatch(async () => await Navigation.PopModalAsync());
        return true;
    }
}
