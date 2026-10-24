using AndroidTts = global::Android.Speech.Tts.TextToSpeech;

namespace UtauTtsBuilder.Platforms.Android;

public class TtsSynthesizer : Java.Lang.Object, AndroidTts.IOnInitListener
{
    private AndroidTts? _tts;
    private TaskCompletionSource<bool>? _initTcs;
    private readonly string? _enginePackage;
    private readonly object _lock = new();
    private string _pendingLanguage = "ja-JP";
    private float _pendingSpeechRate = 0.85f;
    private float _pendingPitch = 1.0f;
    private bool _forceBuild = false;

    public TtsSynthesizer(string? enginePackage = null)
    {
        _enginePackage = enginePackage;
    }

    public async Task InitializeAsync(
        string languageCode = "ja-JP",
        float speechRate = 0.85f,
        float pitch = 1.0f,
        bool forceBuild = false)
    {
        _pendingLanguage = languageCode;
        _pendingSpeechRate = Math.Clamp(speechRate, 0.5f, 1.5f);
        _pendingPitch = Math.Clamp(pitch, 0.5f, 2.0f);
        _forceBuild = forceBuild;

        _initTcs = new TaskCompletionSource<bool>();
        _tts = new AndroidTts(
            global::Microsoft.Maui.ApplicationModel.Platform.AppContext,
            this,
            _enginePackage);

        var completed = await Task.WhenAny(_initTcs.Task, Task.Delay(5000));
        if (completed != _initTcs.Task)
            throw new Exception("TTS 初始化超时");
        await _initTcs.Task;
    }

    public void OnInit(global::Android.Speech.Tts.OperationResult status)
    {
        if (status == global::Android.Speech.Tts.OperationResult.Success && _tts != null)
        {
            global::Java.Util.Locale locale = _pendingLanguage switch
            {
                "zh-CN" => global::Java.Util.Locale.China,
                "en-US" => global::Java.Util.Locale.Us,
                _ => global::Java.Util.Locale.Japanese
            };

            var result = _tts.SetLanguage(locale);
            bool supported = result != global::Android.Speech.Tts.LanguageAvailableResult.MissingData &&
                             result != global::Android.Speech.Tts.LanguageAvailableResult.NotSupported;

            if (!supported)
            {
                if (_forceBuild)
                {
                    Logger.Warn("TTS", "Target lang not supported, forcing build with default locale.");
                    // 尝试设置为系统默认语言，如果不传则使用引擎默认
                    _tts.SetLanguage(global::Java.Util.Locale.Default);
                }
                else
                {
                    _initTcs?.TrySetException(new Exception(
                        "该 TTS 引擎不支持语言 " + _pendingLanguage + "，请在设置中开启强制制作或换引擎"));
                    return;
                }
            }
            else
            {
                _tts.SetSpeechRate(_pendingSpeechRate);
                _tts.SetPitch(_pendingPitch);
            }
            
            _initTcs?.TrySetResult(true);
        }
        else
        {
            _initTcs?.TrySetException(new Exception("TTS 初始化失败: " + status));
        }
    }

    public Task SynthesizeToFileAsync(string text, string outputPath)
    {
        if (_tts == null)
            throw new InvalidOperationException("TTS 未初始化");

        var tcs = new TaskCompletionSource<bool>();
        var utteranceId = Guid.NewGuid().ToString();

        lock (_lock)
        {
            _tts.SetOnUtteranceProgressListener(new ProgressListener(tcs));
            if (File.Exists(outputPath)) File.Delete(outputPath);
            var file = new global::Java.IO.File(outputPath);
            var result = _tts.SynthesizeToFile(text, new global::Android.OS.Bundle(), file, utteranceId);
            if (result != global::Android.Speech.Tts.OperationResult.Success)
                throw new Exception("SynthesizeToFile 调用失败: " + result);
        }

        var completed = Task.WhenAny(tcs.Task, Task.Delay(10000)).Result;
        if (completed != tcs.Task)
            throw new Exception("合成 " + text + " 超时");
        return tcs.Task;
    }

    public new void Dispose()
    {
        try { _tts?.Stop(); } catch { }
        try { _tts?.Shutdown(); } catch { }
        try { _tts?.Dispose(); } catch { }
        base.Dispose();
    }

    private class ProgressListener : global::Android.Speech.Tts.UtteranceProgressListener
    {
        private readonly TaskCompletionSource<bool> _tcs;
        public ProgressListener(TaskCompletionSource<bool> tcs) { _tcs = tcs; }

        public override void OnDone(string? utteranceId) { _tcs.TrySetResult(true); }
        public override void OnError(string? utteranceId)
        {
            _tcs.TrySetException(new Exception("TTS 合成错误: " + utteranceId));
        }
        public override void OnStart(string? utteranceId) { }
    }
}
