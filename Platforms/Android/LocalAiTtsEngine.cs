namespace UtauTtsBuilder.Platforms.Android;

/// <summary>
/// 本地 AI 推理引擎的占位类。
/// 如果将来你集成了 KokoroSharp，可以在此调用真实模型。
/// </summary>
public class LocalAiTtsEngine : IDisposable
{
    private bool _initialized = false;

    public async Task InitializeAsync(string languageCode)
    {
        if (!Services.LocalAiModelManager.IsModelDownloaded)
            throw new Exception(Services.Loc.T("ai_not_ready_error"));

        // TODO: 初始化 ONNX InferenceSession
        await Task.Delay(100);
        _initialized = true;
    }

    public async Task SynthesizeToFileAsync(string text, string outputPath)
    {
        if (!_initialized) throw new Exception("AI 引擎未初始化");

        // TODO: 调用 KokoroSharp 或手写 ONNX 推理，将生成数据写入 outputPath
        // 下面提供一段极小音量的占位 WAV 数据，防止流程崩溃
        await Task.Delay(50); // 模拟耗时
        var samples = new float[44100]; // 1秒静音
        Services.WavProcessor.WriteWav16(outputPath, samples, 44100, 1);
    }

    public void Dispose() { }
}
