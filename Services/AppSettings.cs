namespace UtauTtsBuilder.Services;

public static class AppSettings
{
    private const string KeyForceBuild = "force_build_enabled";

    public static bool ForceBuild
    {
        get => Preferences.Default.Get(KeyForceBuild, false);
        set => Preferences.Default.Set(KeyForceBuild, value);
    }
}
