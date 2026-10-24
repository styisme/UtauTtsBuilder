using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class SettingsPage : ContentPage
{
    private bool _initializing;

    public SettingsPage()
    {
        InitializeComponent();
        ApplyLocalization();
        LoadCurrent();
    }

    private void ApplyLocalization()
    {
        TopTitle.Text = Loc.T("settings_title");
        AppearanceLabel.Text = Loc.T("settings_appearance");
        ThemeAutoBtn.Text = Loc.T("settings_theme_auto");
        ThemeLightBtn.Text = Loc.T("settings_theme_light");
        ThemeDarkBtn.Text = Loc.T("settings_theme_dark");
        UiStyleLabel.Text = Loc.T("settings_ui_style");
        StyleMd3Btn.Text = "Material 3";
        StyleHarmonyBtn.Text = "鸿蒙 HarmonyOS";
        StyleUwpBtn.Text = "Windows UWP";
        StyleA4Btn.Text = "Android 4 Holo";
        LanguageSectionLabel.Text = Loc.T("settings_language");
        LangAutoBtn.Text = Loc.T("lang_auto");
        LangZhBtn.Text = "中文";
        LangEnBtn.Text = "English";
        LangJaBtn.Text = "日本語";
        DynamicLabel.Text = Loc.T("settings_dynamic");
        DynamicTitle.Text = Loc.T("settings_use_wallpaper");
        DynamicHint.Text = Loc.T("settings_dynamic_hint");
        DynamicFooter.Text = Loc.T("settings_dynamic_footer");
        
        AdvancedLabel.Text = Loc.T("settings_advanced");
        ForceBuildTitle.Text = Loc.T("settings_force_build");
        ForceBuildHint.Text = Loc.T("settings_force_build_hint");

        CloudBtn.Text = Loc.T("settings_cloud");
        AboutBtn.Text = Loc.T("settings_about");
    }

    private void LoadCurrent()
    {
        _initializing = true;
        DynamicSwitch.IsToggled = ThemeService.DynamicColorEnabled;
        ForceBuildSwitch.IsToggled = AppSettings.ForceBuild;
        _initializing = false;
        RefreshThemeButtons();
        RefreshUiStyleButtons();
        RefreshLanguageButtons();
    }

    private void RefreshThemeButtons()
    {
        var m = ThemeService.Mode;
        SetSelected(ThemeAutoBtn, m == ThemeMode.Auto);
        SetSelected(ThemeLightBtn, m == ThemeMode.Light);
        SetSelected(ThemeDarkBtn, m == ThemeMode.Dark);
    }

    private void RefreshUiStyleButtons()
    {
        var s = UiStyleService.Style;
        SetSelected(StyleMd3Btn, s == UiStyle.Md3);
        SetSelected(StyleHarmonyBtn, s == UiStyle.Harmony);
        SetSelected(StyleUwpBtn, s == UiStyle.Uwp);
        SetSelected(StyleA4Btn, s == UiStyle.Android4);
    }

    private void RefreshLanguageButtons()
    {
        var m = Loc.Mode;
        SetSelected(LangAutoBtn, m == UiLanguage.Auto);
        SetSelected(LangZhBtn, m == UiLanguage.Chinese);
        SetSelected(LangEnBtn, m == UiLanguage.English);
        SetSelected(LangJaBtn, m == UiLanguage.Japanese);
    }

    private void SetSelected(Button btn, bool selected)
    {
        if (selected)
        {
            btn.SetDynamicResource(Button.BackgroundColorProperty, "Md3Primary");
            btn.SetDynamicResource(Button.TextColorProperty, "Md3OnPrimary");
            btn.BorderWidth = 0;
        }
        else
        {
            btn.SetDynamicResource(Button.BackgroundColorProperty, "Md3SurfaceContainer");
            btn.SetDynamicResource(Button.TextColorProperty, "Md3OnSurface");
            btn.BorderWidth = 1;
        }
    }

    private async Task ApplyThemeWithAnimation()
    {
        await this.FadeToAsync(0.4, 90, Easing.CubicOut);
        ThemeService.Apply();
        await this.FadeToAsync(1.0, 180, Easing.CubicIn);
    }

    private async void OnThemeModeButtonClicked(object sender, EventArgs e)
    {
        if (sender == ThemeAutoBtn) ThemeService.Mode = ThemeMode.Auto;
        else if (sender == ThemeLightBtn) ThemeService.Mode = ThemeMode.Light;
        else if (sender == ThemeDarkBtn) ThemeService.Mode = ThemeMode.Dark;
        await ApplyThemeWithAnimation();
        RefreshThemeButtons();
    }

    private async void OnUiStyleClicked(object sender, EventArgs e)
    {
        if (sender == StyleMd3Btn) UiStyleService.Style = UiStyle.Md3;
        else if (sender == StyleHarmonyBtn) UiStyleService.Style = UiStyle.Harmony;
        else if (sender == StyleUwpBtn) UiStyleService.Style = UiStyle.Uwp;
        else if (sender == StyleA4Btn) UiStyleService.Style = UiStyle.Android4;
        await ApplyThemeWithAnimation();
        RefreshUiStyleButtons();
    }

    private async void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        if (sender == LangAutoBtn) Loc.Mode = UiLanguage.Auto;
        else if (sender == LangZhBtn) Loc.Mode = UiLanguage.Chinese;
        else if (sender == LangEnBtn) Loc.Mode = UiLanguage.English;
        else if (sender == LangJaBtn) Loc.Mode = UiLanguage.Japanese;

        await this.FadeToAsync(0.4, 90, Easing.CubicOut);
        ApplyLocalization();
        RefreshLanguageButtons();
        await this.FadeToAsync(1.0, 180, Easing.CubicIn);
    }

    private async void OnDynamicToggled(object sender, ToggledEventArgs e)
    {
        if (_initializing) return;
        ThemeService.DynamicColorEnabled = e.Value;
        await ApplyThemeWithAnimation();
    }

    private void OnForceBuildToggled(object sender, ToggledEventArgs e)
    {
        if (_initializing) return;
        AppSettings.ForceBuild = e.Value;
    }

    private async void OnCloudSettingsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CloudSettingsPage());
    private async void OnAboutClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AboutPage());
    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
