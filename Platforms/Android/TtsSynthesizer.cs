using Android.OS;
using Java.Util;
using AndroidTts = Android.Speech.Tts.TextToSpeech; // 使用别名解决命名冲突

namespace UtauTtsBuilder.Platforms.Android;

public class TtsSynthesizer : Java.Lang.Object, AndroidTts.IOnInitListener
{
    private AndroidTts? _tts;
    private TaskCompletionSource<bool>? _initTcs;
    private readonly string? _enginePackage;
    private readonly object _lock = new();

    public TtsSynthesizer(string? enginePackage = null)
    {
        _enginePackage = enginePackage;
    }

    public async Task InitializeAsync()
    {
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

    public void OnInit(Android.Speech.Tts.OperationResult status)
    {
        if (status == Android.Speech.Tts.OperationResult.Success && _tts != null)
        {
            var result = _tts.SetLanguage(Locale.Japanese);
            if (result == Android.Speech.Tts.LanguageAvailableResult.MissingData ||
                result == Android.Speech.Tts.LanguageAvailableResult.NotSupported)
            {
                _initTcs?.TrySetException(
                    new Exception("该 TTS 引擎不支持日语 (ja-JP)"));
                return;
            }
            _tts.SetSpeechRate(0.85f);
            _tts.SetPitch(1.0f);
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
            var file = new Java.IO.File(outputPath);
            var result = _tts.SynthesizeToFile(text, new Bundle(), file, utteranceId);
            if (result != Android.Speech.Tts.OperationResult.Success)
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

    // 继承自 Android 原生的 UtteranceProgressListener，修复 CS0115 错误
    private class ProgressListener : Android.Speech.Tts.UtteranceProgressListener
    {
        private readonly TaskCompletionSource<bool> _tcs;
        public ProgressListener(TaskCompletionSource<bool> tcs) { _tcs = tcs; }

        public override void OnDone(Java.Lang.String? utteranceId)
        {
            _tcs.TrySetResult(true);
        }

        public override void OnError(Java.Lang.String? utteranceId)
        {
            _tcs.TrySetException(new Exception("TTS 合成错误: " + utteranceId));
        }

        public override void OnStart(Java.Lang.String? utteranceId) { }
    }
}
