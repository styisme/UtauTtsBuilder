namespace UtauTtsBuilder.Services;

public enum CloudProvider
{
    None,
    Azure,
    Google
}

public static class CloudConfigService
{
    private const string KeyAzureApiKey = "cloud_azure_key";
    private const string KeyAzureRegion = "cloud_azure_region";
    private const string KeyAzureVoice = "cloud_azure_voice";
    private const string KeyGoogleApiKey = "cloud_google_key";

    public static string AzureApiKey
    {
        get => Preferences.Default.Get(KeyAzureApiKey, "");
        set => Preferences.Default.Set(KeyAzureApiKey, value);
    }

    public static string AzureRegion
    {
        get => Preferences.Default.Get(KeyAzureRegion, "eastasia");
        set => Preferences.Default.Set(KeyAzureRegion, value);
    }

    public static string AzureVoice
    {
        get => Preferences.Default.Get(KeyAzureVoice, "ja-JP-NanamiNeural");
        set => Preferences.Default.Set(KeyAzureVoice, value);
    }

    public static string GoogleApiKey
    {
        get => Preferences.Default.Get(KeyGoogleApiKey, "");
        set => Preferences.Default.Set(KeyGoogleApiKey, value);
    }

    public static bool AzureConfigured =>
        !string.IsNullOrWhiteSpace(AzureApiKey);

    public static bool GoogleConfigured =>
        !string.IsNullOrWhiteSpace(GoogleApiKey);

    /// <summary>云端引擎的虚拟包名</summary>
    public const string AzureEnginePackage = "__cloud.azure";
    public const string GoogleEnginePackage = "__cloud.google";

    public static bool IsCloudEngine(string? package) =>
        package == AzureEnginePackage || package == GoogleEnginePackage;

    public static CloudProvider GetProvider(string? package) => package switch
    {
        AzureEnginePackage => CloudProvider.Azure,
        GoogleEnginePackage => CloudProvider.Google,
        _ => CloudProvider.None
    };

    public static string DefaultVoiceFor(string langCode) => langCode switch
    {
        "ja-JP" => "ja-JP-NanamiNeural",
        "zh-CN" => "zh-CN-XiaoxiaoNeural",
        "en-US" => "en-US-JennyNeural",
        _ => "ja-JP-NanamiNeural"
    };
}
