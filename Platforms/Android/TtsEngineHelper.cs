namespace UtauTtsBuilder.Platforms.Android;

public class TtsEngineInfo
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public override string ToString() => Label + " (" + Name + ")";
}

public static class TtsEngineHelper
{
    public static List<TtsEngineInfo> GetAvailableEngines()
    {
        var result = new List<TtsEngineInfo>();
        global::Android.Speech.Tts.TextToSpeech? tts = null;
        try
        {
            tts = new global::Android.Speech.Tts.TextToSpeech(
                global::Microsoft.Maui.ApplicationModel.Platform.AppContext, null);
            var defaultEngine = tts.DefaultEngine;
            var engines = tts.Engines;
            if (engines != null)
            {
                foreach (var e in engines)
                {
                    var name = e.Name ?? string.Empty;
                    result.Add(new TtsEngineInfo
                    {
                        Name = name,
                        Label = e.Label ?? name,
                        IsDefault = name == defaultEngine
                    });
                }
            }
        }
        finally
        {
            tts?.Shutdown();
            tts?.Dispose();
        }
        return result;
    }
}
