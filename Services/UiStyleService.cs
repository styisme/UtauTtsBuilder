namespace UtauTtsBuilder.Services;

public enum UiStyle
{
    Md3,
    Harmony,
    Uwp,
    Android4
}

public static class UiStyleService
{
    private const string KeyStyle = "ui_style";

    public static UiStyle Style
    {
        get
        {
            var s = Preferences.Default.Get(KeyStyle, "md3");
            return s switch
            {
                "harmony" => UiStyle.Harmony,
                "uwp" => UiStyle.Uwp,
                "android4" => UiStyle.Android4,
                _ => UiStyle.Md3
            };
        }
        set
        {
            var s = value switch
            {
                UiStyle.Harmony => "harmony",
                UiStyle.Uwp => "uwp",
                UiStyle.Android4 => "android4",
                _ => "md3"
            };
            Preferences.Default.Set(KeyStyle, s);
        }
    }

    public static string GetDisplayName(UiStyle s) => s switch
    {
        UiStyle.Md3 => "Material 3",
        UiStyle.Harmony => "鸿蒙 HarmonyOS",
        UiStyle.Uwp => "Windows UWP",
        UiStyle.Android4 => "Android 4 Holo",
        _ => "Material 3"
    };

    public static void Apply(ResourceDictionary d, UiStyle style)
    {
        switch (style)
        {
            case UiStyle.Harmony: ApplyHarmony(d); return;
            case UiStyle.Uwp: ApplyUwp(d); return;
            case UiStyle.Android4: ApplyAndroid4(d); return;
        }
    }

    // ===== 鸿蒙 =====
    private static void ApplyHarmony(ResourceDictionary d)
    {
        d["Md3Primary"] = Color.FromArgb("#007DFF");
        d["Md3OnPrimary"] = Colors.White;
        d["Md3PrimaryContainer"] = Color.FromArgb("#D6E4FF");
        d["Md3OnPrimaryContainer"] = Color.FromArgb("#001A33");
        d["Md3SecondaryContainer"] = Color.FromArgb("#E8F0FE");
        d["Md3OnSecondaryContainer"] = Color.FromArgb("#001A33");
        d["Md3Surface"] = Color.FromArgb("#F1F3F5");
        d["Md3OnSurface"] = Colors.Black;
        d["Md3SurfaceVariant"] = Color.FromArgb("#E5E5E5");
        d["Md3OnSurfaceVariant"] = Color.FromArgb("#666666");
        d["Md3Outline"] = Color.FromArgb("#CCCCCC");
        d["Md3OutlineVariant"] = Color.FromArgb("#E5E5E5");
        d["Md3SurfaceContainer"] = Colors.White;
        d["UiBtnCorner"] = 16;
        d["UiBtnHeight"] = 48;
        d["UiBtnSmallHeight"] = 40;
    }

    // ===== UWP =====
    private static void ApplyUwp(ResourceDictionary d)
    {
        d["Md3Primary"] = Color.FromArgb("#0078D4");
        d["Md3OnPrimary"] = Colors.White;
        d["Md3PrimaryContainer"] = Color.FromArgb("#CCE4F7");
        d["Md3OnPrimaryContainer"] = Color.FromArgb("#002D4A");
        d["Md3SecondaryContainer"] = Color.FromArgb("#E5E5E5");
        d["Md3OnSecondaryContainer"] = Color.FromArgb("#1A1A1A");
        d["Md3Surface"] = Color.FromArgb("#F3F3F3");
        d["Md3OnSurface"] = Color.FromArgb("#1A1A1A");
        d["Md3SurfaceVariant"] = Color.FromArgb("#EDEDED");
        d["Md3OnSurfaceVariant"] = Color.FromArgb("#5A5A5A");
        d["Md3Outline"] = Color.FromArgb("#8A8A8A");
        d["Md3OutlineVariant"] = Color.FromArgb("#D2D2D2");
        d["Md3SurfaceContainer"] = Colors.White;
        d["UiBtnCorner"] = 2;
        d["UiBtnHeight"] = 36;
        d["UiBtnSmallHeight"] = 32;
    }

    // ===== Android 4 Holo（重做）=====
    // 参考真正的 Holo Dark 配色：
    //   背景 #000000，主色 #33B5E5，卡片 #1A1A1A
    //   次要文字 #999999，主文字 #CCCCCC
    //   按钮：深灰底 #2E2E2E 或 全透明 + 青色描边
    private static void ApplyAndroid4(ResourceDictionary d)
    {
        d["Md3Primary"] = Color.FromArgb("#33B5E5");
        d["Md3OnPrimary"] = Color.FromArgb("#000000");
        d["Md3PrimaryContainer"] = Color.FromArgb("#1A3D4D");
        d["Md3OnPrimaryContainer"] = Color.FromArgb("#B3E5FC");
        d["Md3SecondaryContainer"] = Color.FromArgb("#2E2E2E");
        d["Md3OnSecondaryContainer"] = Color.FromArgb("#CCCCCC");
        d["Md3Surface"] = Color.FromArgb("#000000");
        d["Md3OnSurface"] = Color.FromArgb("#CCCCCC");
        d["Md3SurfaceVariant"] = Color.FromArgb("#1A1A1A");
        d["Md3OnSurfaceVariant"] = Color.FromArgb("#999999");
        d["Md3Outline"] = Color.FromArgb("#555555");
        d["Md3OutlineVariant"] = Color.FromArgb("#333333");
        d["Md3SurfaceContainer"] = Color.FromArgb("#1A1A1A");
        d["UiBtnCorner"] = 3;
        d["UiBtnHeight"] = 44;
        d["UiBtnSmallHeight"] = 36;
    }
}
