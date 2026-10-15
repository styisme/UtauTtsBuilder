namespace UtauTtsBuilder.Services;

/// <summary>
/// 从 Android 12+ 系统动态颜色中读取 MD3 色板，覆盖到资源字典。
/// 失败时静默返回 false，调用方会保留基线色板。
/// </summary>
public static class ThemeHelper
{
    public static bool TryApplyDynamicColors(ResourceDictionary dict, bool dark)
    {
#if ANDROID
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            return false;

        try
        {
            var ctx = global::Microsoft.Maui.ApplicationModel.Platform.AppContext;
            var res = ctx.Resources;
            var theme = ctx.Theme;

            // (key, lightRes, darkRes) —— MD3 token → Android 系统动态色资源名
            var map = new (string Key, string Light, string Dark)[]
            {
                ("Md3Primary",              "system_accent1_600", "system_accent1_200"),
                ("Md3OnPrimary",            "system_accent1_0",   "system_accent1_800"),
                ("Md3PrimaryContainer",     "system_accent1_100", "system_accent1_800"),
                ("Md3OnPrimaryContainer",   "system_accent1_10",  "system_accent1_100"),
                ("Md3SecondaryContainer",   "system_accent2_100", "system_accent2_800"),
                ("Md3OnSecondaryContainer", "system_accent2_10",  "system_accent2_100"),
                ("Md3Surface",              "system_neutral1_10", "system_neutral1_900"),
                ("Md3OnSurface",            "system_neutral1_900","system_neutral1_100"),
                ("Md3SurfaceVariant",       "system_neutral2_100","system_neutral2_700"),
                ("Md3OnSurfaceVariant",     "system_neutral2_700","system_neutral2_200"),
                ("Md3Outline",              "system_neutral2_500","system_neutral2_400"),
                ("Md3OutlineVariant",       "system_neutral2_200","system_neutral2_600"),
                ("Md3SurfaceContainer",     "system_neutral1_50", "system_neutral1_800"),
            };

            bool applied = false;
            foreach (var (key, lightRes, darkRes) in map)
            {
                var resName = dark ? darkRes : lightRes;
                var c = ReadColor(res, theme, resName);
                if (c.HasValue)
                {
                    dict[key] = c.Value;
                    applied = true;
                }
            }
            return applied;
        }
        catch
        {
            return false;
        }
#else
        return false;
#endif
    }

#if ANDROID
    private static Color? ReadColor(
        global::Android.Content.Res.Resources res,
        global::Android.Content.Res.Resources.Theme? theme,
        string name)
    {
        try
        {
            int id = res.GetIdentifier(name, "color", "android");
            if (id == 0) return null;
            int argb = res.GetColor(id, theme);
            return Color.FromRgba(
                (byte)((argb >> 16) & 0xFF),
                (byte)((argb >> 8) & 0xFF),
                (byte)(argb & 0xFF),
                (byte)((argb >> 24) & 0xFF));
        }
        catch { return null; }
    }
#endif
}
