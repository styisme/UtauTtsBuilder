using System.Text;

namespace UtauTtsBuilder.Services;

public static class WavProcessor
{
    public const int TargetSampleRate = 44100;
    public const int TargetBitsPerSample = 16;
    public const int TargetChannels = 1;

    public class WavInfo
    {
        public int SampleRate;
        public int Channels;
        public int BitsPerSample;
        public int DataLength;
        public long DataOffset;
        public byte[] RawData = Array.Empty<byte>();
    }

    public static WavInfo ReadWav(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 44) throw new InvalidDataException("WAV 文件过小");
        if (Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF")
            throw new InvalidDataException("不是 RIFF 文件");
        if (Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE")
            throw new InvalidDataException("不是 WAVE 文件");

        int pos = 12;
        int sampleRate = 0, channels = 0, bits = 0;
        int dataOffset = 0, dataLen = 0;

        while (pos + 8 <= bytes.Length)
        {
            var chunkId = Encoding.ASCII.GetString(bytes, pos, 4);
            int chunkSize = BitConverter.ToInt32(bytes, pos + 4);
            int chunkStart = pos + 8;

            if (chunkId == "fmt ")
            {
                channels = BitConverter.ToInt16(bytes, chunkStart + 2);
                sampleRate = BitConverter.ToInt32(bytes, chunkStart + 4);
                bits = BitConverter.ToInt16(bytes, chunkStart + 14);
            }
            else if (chunkId == "data")
            {
                dataOffset = chunkStart;
                dataLen = Math.Min(chunkSize, bytes.Length - chunkStart);
                if (sampleRate > 0) break;
            }

            pos = chunkStart + chunkSize;
            if (chunkSize % 2 != 0) pos++;
        }

        if (sampleRate == 0 || channels == 0 || bits == 0 || dataLen == 0)
            throw new InvalidDataException("WAV 头解析失败");

        var rawData = new byte[dataLen];
        Array.Copy(bytes, dataOffset, rawData, 0, dataLen);

        return new WavInfo
        {
            SampleRate = sampleRate,
            Channels = channels,
            BitsPerSample = bits,
            DataLength = dataLen,
            DataOffset = dataOffset,
            RawData = rawData
        };
    }

    public static void ConvertToUtauFormat(string inputPath, string outputPath)
    {
        var info = ReadWav(inputPath);
        var samples = DecodeToFloat(info.RawData, info.BitsPerSample, info.Channels);
        var mono = ToMono(samples, info.Channels);
        var resampled = Resample(mono, info.SampleRate, TargetSampleRate);

        // 首尾静音裁剪
        resampled = TrimSilence(resampled, TargetSampleRate);

        Normalize(resampled, 0.89f);
        WriteWav16(outputPath, resampled, TargetSampleRate, TargetChannels);
    }

    /// <summary>
    /// 裁掉首尾的静音段，两端各留 8ms padding。
    /// 阈值 0.005f（约 -46dB），足够保留轻微呼吸音。
    /// </summary>
    public static float[] TrimSilence(float[] samples, int sampleRate)
    {
        const float threshold = 0.005f;
        int padding = (int)(sampleRate * 0.008); // 8ms

        int start = 0;
        while (start < samples.Length && Math.Abs(samples[start]) < threshold)
            start++;

        int end = samples.Length - 1;
        while (end > start && Math.Abs(samples[end]) < threshold)
            end--;

        // 全是静音，直接返回（返回短静音，防止空数组崩溃）
        if (start >= end)
        {
            var tiny = new float[Math.Max(padding * 2, 100)];
            return tiny;
        }

        start = Math.Max(0, start - padding);
        end = Math.Min(samples.Length - 1, end + padding);

        int len = end - start + 1;
        var result = new float[len];
        Array.Copy(samples, start, result, 0, len);
        return result;
    }

    private static float[] DecodeToFloat(byte[] data, int bits, int channels)
    {
        int bytesPerSample = bits / 8;
        int sampleCount = data.Length / bytesPerSample;
        var result = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            int offset = i * bytesPerSample;
            float v = 0f;
            if (bits == 8) v = (data[offset] - 128) / 128f;
            else if (bits == 16) v = BitConverter.ToInt16(data, offset) / 32768f;
            else if (bits == 24) v = (data[offset] | (data[offset + 1] << 8)
                    | ((sbyte)data[offset + 2] << 16)) / 8388608f;
            else if (bits == 32) v = BitConverter.ToInt32(data, offset) / 2147483648f;
            result[i] = v;
        }
        return result;
    }

    private static float[] ToMono(float[] input, int channels)
    {
        if (channels == 1) return input;
        int frames = input.Length / channels;
        var result = new float[frames];
        for (int i = 0; i < frames; i++)
        {
            float sum = 0;
            for (int c = 0; c < channels; c++)
                sum += input[i * channels + c];
            result[i] = sum / channels;
        }
        return result;
    }

    private static float[] Resample(float[] input, int srcRate, int dstRate)
    {
        if (srcRate == dstRate) return input;
        double ratio = (double)dstRate / srcRate;
        int outLen = (int)(input.Length * ratio);
        var output = new float[outLen];
        for (int i = 0; i < outLen; i++)
        {
            double srcPos = i / ratio;
            int i0 = (int)srcPos;
            int i1 = Math.Min(i0 + 1, input.Length - 1);
            double frac = srcPos - i0;
            output[i] = (float)(input[i0] * (1 - frac) + input[i1] * frac);
        }
        return output;
    }

    private static void Normalize(float[] samples, float peak)
    {
        float max = 0f;
        foreach (var s in samples)
        {
            var abs = Math.Abs(s);
            if (abs > max) max = abs;
        }
        if (max < 1e-6f || max >= peak) return;
        float gain = peak / max;
        for (int i = 0; i < samples.Length; i++)
            samples[i] *= gain;
    }

    public static void WriteWav16(string path, float[] samples, int sampleRate, int channels)
    {
        const int bytesPerSample = 2;
        int dataSize = samples.Length * bytesPerSample;

        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + dataSize);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(sampleRate * channels * bytesPerSample);
        bw.Write((short)(channels * bytesPerSample));
        bw.Write((short)(bytesPerSample * 8));

        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSize);

        foreach (var s in samples)
        {
            var clamped = Math.Max(-1f, Math.Min(1f, s));
            bw.Write((short)(clamped * 32767));
        }
    }

    public static double GetDurationMs(string wavPath)
    {
        var info = ReadWav(wavPath);
        int bytesPerSample = info.BitsPerSample / 8;
        int frames = info.DataLength / bytesPerSample / info.Channels;
        return frames / (double)info.SampleRate * 1000.0;
    }
}
