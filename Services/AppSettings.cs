namespace UtauTtsBuilder.Services;

public static class AppSettings
{
    private const string KeyForceBuild = "force_build_enabled";
    private const string KeyAiVoiceIndex = "ai_voice_index";
    private const string KeyVowelStretch = "vowel_stretch_enabled";

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

    /// <summary>是否开启元音拉伸（测试功能，默认关闭）</summary>
    public static bool EnableVowelStretch
    {
        get => Preferences.Default.Get(KeyVowelStretch, false);
        set => Preferences.Default.Set(KeyVowelStretch, value);
    }
}
