using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace UtauTtsBuilder.Platforms.Android;

public class LocalAiTtsEngine : IDisposable
{
    private InferenceSession? _session;
    private float[] _currentStyle = Array.Empty<float>();
    private Dictionary<long, float[]> _voices = new();

    public async Task InitializeAsync(string languageCode)
    {
        var dir = Services.LocalAiModelManager.GetModelDir();
        var modelPath = Path.Combine(dir, "kokoro.onnx");
        var voicesPath = Path.Combine(dir, "voices.bin");

        if (!File.Exists(modelPath) || !File.Exists(voicesPath))
            throw new Exception(Services.Loc.T("ai_not_ready_error"));

        await Task.Run(() =>
        {
            // 1. 加载 ONNX 模型
            var options = new SessionOptions();
            options.IntraOpNumThreads = 4; // 多线程加速
            _session = new InferenceSession(modelPath, options);

            // 2. 加载音色包
            // voices.bin 包含 512 个音色，每个音色为 256 个 Float32（1024 字节）
            var voiceBytes = File.ReadAllBytes(voicesPath);
            int bytesPerVoice = 1024; 
            int voiceCount = voiceBytes.Length / bytesPerVoice;

            for (int i = 0; i < voiceCount; i++)
            {
                var floats = new float[256];
                Buffer.BlockCopy(voiceBytes, i * bytesPerVoice, floats, 0, bytesPerVoice);
                _voices[i] = floats;
            }

            // 3. 设置当前选中的音色
            int index = Services.AppSettings.AiVoiceIndex;
            if (!_voices.ContainsKey(index)) index = 0;
            _currentStyle = _voices[index];
        });
    }

    public async Task SynthesizeToFileAsync(string text, string outputPath)
    {
        if (_session == null) throw new Exception("AI 引擎未初始化");

        await Task.Run(() =>
        {
            // 1. 文本转 ID
            var tokens = Services.KokoroPhonemizer.GetTokenIds(text);

            // 2. 构造输入 Tensor
            var inputIds = new DenseTensor<long>(new[] { 1, tokens.Length });
            for (int i = 0; i < tokens.Length; i++) inputIds[0, i] = tokens[i];

            var styleTensor = new DenseTensor<float>(new[] { 1, _currentStyle.Length });
            for (int i = 0; i < _currentStyle.Length; i++) styleTensor[0, i] = _currentStyle[i];

            var speedTensor = new DenseTensor<float>(new[] { 1 });
            speedTensor[0] = 0.85f;

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                NamedOnnxValue.CreateFromTensor("style", styleTensor),
                NamedOnnxValue.CreateFromTensor("speed", speedTensor)
            };

            // 3. 执行推理
            using var results = _session.Run(inputs);
            
            // 4. 取输出音频
            var audioTensor = results.First(r => r.Name == "audio").AsTensor<float>();
            var audioArray = audioTensor.ToArray();

            // 5. 写入 WAV
            Services.WavProcessor.WriteWav16(outputPath, audioArray, 24000, 1);
        });
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
