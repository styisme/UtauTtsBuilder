using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // 启动时应用主题（可能在设置里被改过）
        ThemeService.Apply();

        return new Window(new NavigationPage(new MainPage()));
    }
}
