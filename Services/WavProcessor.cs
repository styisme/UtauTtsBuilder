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
        if (Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF") throw new InvalidDataException("不是 RIFF");
        if (Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE") throw new InvalidDataException("不是 WAVE");

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
        
        // 1. 裁掉首尾静音
        resampled = TrimSilence(resampled, TargetSampleRate);
        
        // 2. 元音拉伸 (Beta 测试功能，根据设置决定是否开启)
        if (AppSettings.EnableVowelStretch)
        {
            resampled = StretchVowel(resampled, TargetSampleRate, 0.8f);
        }

        Normalize(resampled, 0.89f);
        WriteWav16(outputPath, resampled, TargetSampleRate, TargetChannels);
    }

    private static float[] StretchVowel(float[] samples, int sampleRate, float targetSeconds)
    {
        int targetSamples = (int)(sampleRate * targetSeconds);
        if (samples.Length >= targetSamples || samples.Length < sampleRate / 10) return samples;

        // 寻找能量最大的 200ms 片段作为循环区
        int windowSize = sampleRate / 5;
        int bestStart = 0;
        float maxEnergy = 0;

        for (int i = 0; i < samples.Length - windowSize; i += 441)
        {
            float energy = 0;
            for (int j = 0; j < windowSize; j += 10) energy += Math.Abs(samples[i + j]);
            if (energy > maxEnergy) { maxEnergy = energy; bestStart = i; }
        }

        var loop = new float[windowSize];
        Array.Copy(samples, bestStart, loop, 0, windowSize);

        int resultLen = Math.Max(targetSamples, samples.Length);
        var result = new float[resultLen];
        Array.Copy(samples, 0, result, 0, samples.Length);

        int writePos = samples.Length;
        int fadeLen = sampleRate / 100; // 10ms crossfade

        while (writePos < resultLen)
        {
            int copyLen = Math.Min(windowSize, resultLen - writePos);
            for (int i = 0; i < copyLen; i++)
            {
                float fadeIn = i < fadeLen ? (float)i / fadeLen : 1f;
                float fadeOut = i < fadeLen ? 1f - fadeIn : 1f;
                result[writePos + i] = result[writePos + i] * fadeOut + loop[i] * fadeIn;
            }
            writePos += copyLen;
        }
        return result;
    }

    public static float[] TrimSilence(float[] samples, int sampleRate)
    {
        const float threshold = 0.005f;
        int padding = (int)(sampleRate * 0.008);
        int start = 0;
        while (start < samples.Length && Math.Abs(samples[start]) < threshold) start++;
        int end = samples.Length - 1;
        while (end > start && Math.Abs(samples[end]) < threshold) end--;

        if (start >= end) return new float[Math.Max(padding * 2, 100)];

        start = Math.Max(0, start - padding);
        end = Math.Min(samples.Length - 1, end + padding);

        int len = end - start + 1;
        var result = new float[len];
        Array.Copy(samples, start, result, 0, len);
        return result;
    }

    public static float[] DecodeToFloat(byte[] data, int bits, int channels)
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
            else if (bits == 24) v = (data[offset] | (data[offset + 1] << 8) | ((sbyte)data[offset + 2] << 16)) / 8388608f;
            else if (bits == 32) v = BitConverter.ToInt32(data, offset) / 2147483648f;
            result[i] = v;
        }
        return result;
    }

    public static float[] ToMono(float[] input, int channels)
    {
        if (channels == 1) return input;
        int frames = input.Length / channels;
        var result = new float[frames];
        for (int i = 0; i < frames; i++)
        {
            float sum = 0;
            for (int c = 0; c < channels; c++) sum += input[i * channels + c];
            result[i] = sum / channels;
        }
        return result;
    }

    public static float[] Resample(float[] input, int srcRate, int dstRate)
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

    public static void Normalize(float[] samples, float peak)
    {
        float max = 0f;
        foreach (var s in samples) if (Math.Abs(s) > max) max = Math.Abs(s);
        if (max < 1e-6f || max >= peak) return;
        float gain = peak / max;
        for (int i = 0; i < samples.Length; i++) samples[i] *= gain;
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
