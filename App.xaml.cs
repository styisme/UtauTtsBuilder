using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Logger.Init();
        Logger.Info("App", "App constructed");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        ThemeService.Apply();
        Logger.Info("App", "Window created");
        return new Window(new NavigationPage(new MainPage()));
    }
}
