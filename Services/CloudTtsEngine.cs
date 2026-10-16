using System.Net.Http.Headers;
using System.Text;

namespace UtauTtsBuilder.Services;

public class CloudTtsEngine
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly CloudProvider _provider;
    private string _langCode = "ja-JP";

    public CloudTtsEngine(CloudProvider provider)
    {
        _provider = provider;
    }

    public Task InitializeAsync(string languageCode)
    {
        _langCode = languageCode;
        return Task.CompletedTask;
    }

    /// <summary>合成一段文本到指定 wav 路径</summary>
    public async Task SynthesizeToFileAsync(string text, string outputPath)
    {
        byte[] wavData = _provider switch
        {
            CloudProvider.Azure => await SynthesizeAzureAsync(text),
            CloudProvider.Google => await SynthesizeGoogleAsync(text),
            _ => throw new Exception("未知云端引擎")
        };

        if (wavData == null || wavData.Length < 44)
            throw new Exception("云端返回音频为空");

        await File.WriteAllBytesAsync(outputPath, wavData);
    }

    // ============================================================
    // Azure TTS
    // ============================================================
    private async Task<byte[]> SynthesizeAzureAsync(string text)
    {
        var key = CloudConfigService.AzureApiKey;
        var region = CloudConfigService.AzureRegion;
        var voice = CloudConfigService.AzureVoice;
        if (string.IsNullOrEmpty(key))
            throw new Exception("Azure API Key 未配置");
        if (string.IsNullOrEmpty(voice) || voice.Split('-').Length < 3)
            voice = CloudConfigService.DefaultVoiceFor(_langCode);

        var url = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

        var ssml = $"<speak version='1.0' xml:lang='{_langCode}'>" +
                   $"<voice name='{voice}'>{Escape(text)}</voice></speak>";

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("Ocp-Apim-Subscription-Key", key);
        req.Headers.Add("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
        req.Headers.UserAgent.ParseAdd("UtauTtsBuilder");
        req.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        using var resp = await Http.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new Exception($"Azure 错误 {(int)resp.StatusCode}: {Truncate(err, 200)}");
        }
        return await resp.Content.ReadAsByteArrayAsync();
    }

    // ============================================================
    // Google Cloud TTS
    // ============================================================
    private async Task<byte[]> SynthesizeGoogleAsync(string text)
    {
        var key = CloudConfigService.GoogleApiKey;
        if (string.IsNullOrEmpty(key))
            throw new Exception("Google API Key 未配置");

        var url = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + key;

        var voiceName = _langCode switch
        {
            "ja-JP" => "ja-JP-Neural2-B",
            "zh-CN" => "cmn-CN-Wavenet-A",
            "en-US" => "en-US-Neural2-F",
            _ => "ja-JP-Neural2-B"
        };

        // Google 返回的是 LINEAR16 PCM 裸数据，我们要加 WAV 头
        var json = "{" +
            "\"input\":{\"text\":\"" + EscapeJson(text) + "\"}," +
            "\"voice\":{\"languageCode\":\"" + _langCode + "\",\"name\":\"" + voiceName + "\"}," +
            "\"audioConfig\":{\"audioEncoding\":\"LINEAR16\",\"sampleRateHertz\":16000,\"pitch\":0,\"speakingRate\":1.0}" +
            "}";

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await Http.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new Exception($"Google 错误 {(int)resp.StatusCode}: {Truncate(err, 200)}");
        }

        var body = await resp.Content.ReadAsStringAsync();
        var b64 = ExtractJsonString(body, "audioContent");
        if (string.IsNullOrEmpty(b64))
            throw new Exception("Google 返回无音频");

        var pcm = Convert.FromBase64String(b64);
        // Google 有时会自带 WAV 头，检查一下
        if (pcm.Length > 4 && pcm[0] == 'R' && pcm[1] == 'I' && pcm[2] == 'F' && pcm[3] == 'F')
            return pcm;

        // 否则加一个 WAV 头
        return WrapPcmAsWav(pcm, 16000, 1, 16);
    }

    // ============================================================
    // 工具方法
    // ============================================================
    private static byte[] WrapPcmAsWav(byte[] pcm, int sampleRate, int channels, int bits)
    {
        int dataSize = pcm.Length;
        int totalSize = 36 + dataSize;
        var header = new byte[44];

        Encoding.ASCII.GetBytes("RIFF").CopyTo(header, 0);
        BitConverter.GetBytes(totalSize).CopyTo(header, 4);
        Encoding.ASCII.GetBytes("WAVE").CopyTo(header, 8);
        Encoding.ASCII.GetBytes("fmt ").CopyTo(header, 12);
        BitConverter.GetBytes(16).CopyTo(header, 16);
        BitConverter.GetBytes((short)1).CopyTo(header, 20);
        BitConverter.GetBytes((short)channels).CopyTo(header, 22);
        BitConverter.GetBytes(sampleRate).CopyTo(header, 24);
        BitConverter.GetBytes(sampleRate * channels * bits / 8).CopyTo(header, 28);
        BitConverter.GetBytes((short)(channels * bits / 8)).CopyTo(header, 32);
        BitConverter.GetBytes((short)bits).CopyTo(header, 34);
        Encoding.ASCII.GetBytes("data").CopyTo(header, 36);
        BitConverter.GetBytes(dataSize).CopyTo(header, 40);

        var result = new byte[44 + dataSize];
        Buffer.BlockCopy(header, 0, result, 0, 44);
        Buffer.BlockCopy(pcm, 0, result, 44, dataSize);
        return result;
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static string EscapeJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string Truncate(string s, int max) =>
        s == null ? "" : (s.Length <= max ? s : s.Substring(0, max));

    private static string ExtractJsonString(string json, string key)
    {
        var keyPattern = "\"" + key + "\"";
        int idx = json.IndexOf(keyPattern);
        if (idx < 0) return "";
        int colon = json.IndexOf(':', idx);
        if (colon < 0) return "";
        int startQuote = json.IndexOf('"', colon);
        if (startQuote < 0) return "";
        int endQuote = startQuote + 1;
        while (endQuote < json.Length && json[endQuote] != '"')
        {
            if (json[endQuote] == '\\') endQuote++;
            endQuote++;
        }
        if (endQuote >= json.Length) return "";
        return json.Substring(startQuote + 1, endQuote - startQuote - 1);
    }
}
