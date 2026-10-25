namespace UtauTtsBuilder.Services;

public static class AppSettings
{
    private const string KeyForceBuild = "force_build_enabled";
    private const string KeyAiVoiceIndex = "ai_voice_index";

    public static bool ForceBuild
    {
        get => Preferences.Default.Get(KeyForceBuild, false);
        set => Preferences.Default.Set(KeyForceBuild, value);
    }

    public static int AiVoiceIndex
    {
        get => Preferences.Default.Get(KeyAiVoiceIndex, 0);
        set => Preferences.Default.Set(KeyAiVoiceIndex, value);
    }
}
