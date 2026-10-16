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
        DynamicSwitch.IsToggled = ThemeService.DynamicColorEnabled;
        _initializing = false;
        RefreshThemeButtons();
    }

    private void RefreshThemeButtons()
    {
        var mode = ThemeService.Mode;
        SetSelected(ThemeAutoBtn, mode == ThemeMode.Auto);
        SetSelected(ThemeLightBtn, mode == ThemeMode.Light);
        SetSelected(ThemeDarkBtn, mode == ThemeMode.Dark);
    }

    private void SetSelected(Button btn, bool selected)
    {
        if (selected)
        {
            btn.SetDynamicResource(Button.BackgroundColorProperty, "Md3Primary");
            btn.SetDynamicResource(Button.TextColorProperty, "Md3OnPrimary");
            btn.SetDynamicResource(Button.BorderColorProperty, "Md3Primary");
            btn.BorderWidth = 0;
        }
        else
        {
            btn.SetDynamicResource(Button.BackgroundColorProperty, "Md3SurfaceContainer");
            btn.SetDynamicResource(Button.TextColorProperty, "Md3Primary");
            btn.SetDynamicResource(Button.BorderColorProperty, "Md3Outline");
            btn.BorderWidth = 1;
        }
    }

    private void OnThemeModeButtonClicked(object sender, EventArgs e)
    {
        if (sender == ThemeAutoBtn) ThemeService.Mode = ThemeMode.Auto;
        else if (sender == ThemeLightBtn) ThemeService.Mode = ThemeMode.Light;
        else if (sender == ThemeDarkBtn) ThemeService.Mode = ThemeMode.Dark;

        ThemeService.Apply();
        RefreshThemeButtons();
    }

    private void OnDynamicToggled(object sender, ToggledEventArgs e)
    {
        if (_initializing) return;
        ThemeService.DynamicColorEnabled = e.Value;
        ThemeService.Apply();
    }
}
