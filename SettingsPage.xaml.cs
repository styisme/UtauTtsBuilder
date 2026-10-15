using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class SettingsPage : ContentPage
{
    private bool _initializing;

    public SettingsPage()
    {
        InitializeComponent();
        LoadCurrent();
    }

    private void LoadCurrent()
    {
        _initializing = true;
        var mode = ThemeService.Mode;
        ThemeAuto.IsChecked = mode == ThemeMode.Auto;
        ThemeLight.IsChecked = mode == ThemeMode.Light;
        ThemeDark.IsChecked = mode == ThemeMode.Dark;
        DynamicSwitch.IsToggled = ThemeService.DynamicColorEnabled;
        _initializing = false;
    }

    private void OnThemeModeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_initializing || !e.Value) return;

        if (sender == ThemeAuto) ThemeService.Mode = ThemeMode.Auto;
        else if (sender == ThemeLight) ThemeService.Mode = ThemeMode.Light;
        else if (sender == ThemeDark) ThemeService.Mode = ThemeMode.Dark;

        ThemeService.Apply();
    }

    private void OnDynamicToggled(object sender, ToggledEventArgs e)
    {
        if (_initializing) return;
        ThemeService.DynamicColorEnabled = e.Value;
        ThemeService.Apply();
    }
}
