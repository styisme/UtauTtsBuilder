namespace UtauTtsBuilder.Services;

public enum ThemeMode
{
    Auto,
    Light,
    Dark
}

public static class ThemeService
{
    private const string KeyMode = "theme_mode";
    private const string KeyDynamic = "theme_dynamic";

    public static ThemeMode Mode
    {
        get => (ThemeMode)Preferences.Default.Get(KeyMode, (int)ThemeMode.Auto);
        set => Preferences.Default.Set(KeyMode, (int)value);
    }

    public static bool DynamicColorEnabled
    {
        get => Preferences.Default.Get(KeyDynamic, true);
        set => Preferences.Default.Set(KeyDynamic, value);
    }

    public static bool IsDark
    {
        get
        {
            return Mode switch
            {
                ThemeMode.Light => false,
                ThemeMode.Dark => true,
                _ => Application.Current?.RequestedTheme == AppTheme.Dark
            };
        }
    }

    public static void Apply()
    {
        var dict = Application.Current?.Resources;
        if (dict == null) return;

        var style = UiStyleService.Style;

        // 非 MD3 风格：直接铺该风格的固定色板
        if (style != UiStyle.Md3)
        {
            UiStyleService.Apply(dict, style);
            return;
        }

        // MD3：深浅 + 动态色
        ApplyBaseline(dict, IsDark);
        if (DynamicColorEnabled)
        {
            ThemeHelper.TryApplyDynamicColors(dict, IsDark);
        }
    }

    private static void ApplyBaseline(ResourceDictionary dict, bool dark)
    {
        if (dark)
        {
            dict["Md3Primary"] = Color.FromArgb("#D0BCFF");
            dict["Md3OnPrimary"] = Color.FromArgb("#381E72");
            dict["Md3PrimaryContainer"] = Color.FromArgb("#4F378B");
            dict["Md3OnPrimaryContainer"] = Color.FromArgb("#EADDFF");
            dict["Md3SecondaryContainer"] = Color.FromArgb("#4A4458");
            dict["Md3OnSecondaryContainer"] = Color.FromArgb("#E8DEF8");
            dict["Md3Surface"] = Color.FromArgb("#141218");
            dict["Md3OnSurface"] = Color.FromArgb("#E6E0E9");
            dict["Md3SurfaceVariant"] = Color.FromArgb("#49454F");
            dict["Md3OnSurfaceVariant"] = Color.FromArgb("#CAC4D0");
            dict["Md3Outline"] = Color.FromArgb("#938F99");
            dict["Md3OutlineVariant"] = Color.FromArgb("#49454F");
            dict["Md3SurfaceContainer"] = Color.FromArgb("#211F26");
        }
        else
        {
            dict["Md3Primary"] = Color.FromArgb("#6750A4");
            dict["Md3OnPrimary"] = Color.FromArgb("#FFFFFF");
            dict["Md3PrimaryContainer"] = Color.FromArgb("#EADDFF");
            dict["Md3OnPrimaryContainer"] = Color.FromArgb("#21005D");
            dict["Md3SecondaryContainer"] = Color.FromArgb("#E8DEF8");
            dict["Md3OnSecondaryContainer"] = Color.FromArgb("#1D192B");
            dict["Md3Surface"] = Color.FromArgb("#FEF7FF");
            dict["Md3OnSurface"] = Color.FromArgb("#1D1B20");
            dict["Md3SurfaceVariant"] = Color.FromArgb("#E7E0EC");
            dict["Md3OnSurfaceVariant"] = Color.FromArgb("#49454F");
            dict["Md3Outline"] = Color.FromArgb("#79747E");
            dict["Md3OutlineVariant"] = Color.FromArgb("#CAC4D0");
            dict["Md3SurfaceContainer"] = Color.FromArgb("#F3EDF7");
        }
        dict["UiBtnCorner"] = 28;
        dict["UiBtnHeight"] = 56;
        dict["UiBtnSmallHeight"] = 48;
    }
}
