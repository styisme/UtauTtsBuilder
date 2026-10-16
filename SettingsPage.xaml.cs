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
        Title = Loc.T("settings_title");
        AppearanceLabel.Text = Loc.T("settings_appearance");
        ThemeAutoBtn.Text = Loc.T("settings_theme_auto");
        ThemeLightBtn.Text = Loc.T("settings_theme_light");
        ThemeDarkBtn.Text = Loc.T("settings_theme_dark");
        LanguageSectionLabel.Text = Loc.T("settings_language");
        LangAutoBtn.Text = Loc.T("lang_auto");
        LangZhBtn.Text = "中文";
        LangEnBtn.Text = "English";
        LangJaBtn.Text = "日本語";
        DynamicLabel.Text = Loc.T("settings_dynamic");
        DynamicTitle.Text = Loc.T("settings_use_wallpaper");
        DynamicHint.Text = Loc.T("settings_dynamic_hint");
        DynamicFooter.Text = Loc.T("settings_dynamic_footer");
    }

    private void LoadCurrent()
    {
        _initializing = true;
        DynamicSwitch.IsToggled = ThemeService.DynamicColorEnabled;
        _initializing = false;
        RefreshThemeButtons();
        RefreshLanguageButtons();
    }

    private void RefreshThemeButtons()
    {
        var mode = ThemeService.Mode;
        SetSelected(ThemeAutoBtn, mode == ThemeMode.Auto);
        SetSelected(ThemeLightBtn, mode == ThemeMode.Light);
        SetSelected(ThemeDarkBtn, mode == ThemeMode.Dark);
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

    private async void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        if (sender == LangAutoBtn) Loc.Mode = UiLanguage.Auto;
        else if (sender == LangZhBtn) Loc.Mode = UiLanguage.Chinese;
        else if (sender == LangEnBtn) Loc.Mode = UiLanguage.English;
        else if (sender == LangJaBtn) Loc.Mode = UiLanguage.Japanese;

        ApplyLocalization();
        RefreshLanguageButtons();

        // 简单提示返回主页后生效
        await DisplayAlertAsync(Loc.T("settings_title"),
            Loc.T("settings_language") + " ✓",
            Loc.T("btn_ok"));
    }

    private void OnDynamicToggled(object sender, ToggledEventArgs e)
    {
        if (_initializing) return;
        ThemeService.DynamicColorEnabled = e.Value;
        ThemeService.Apply();
    }
}
