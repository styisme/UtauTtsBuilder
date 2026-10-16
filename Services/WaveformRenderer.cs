namespace UtauTtsBuilder.Services;

public static class WaveformRenderer
{
    /// <summary>
    /// 读取 wav 文件，降采样为 bars 个（min, max）对。
    /// 返回的数组长度为 bars * 2，格式 [min0, max0, min1, max1, ...]
    /// </summary>
    public static float[] GetWaveformData(string wavPath, int bars = 200)
    {
        try
        {
            var info = WavProcessor.ReadWav(wavPath);
            int bytesPerSample = info.BitsPerSample / 8;
            int frames = info.DataLength / bytesPerSample / info.Channels;
            if (frames <= 0) return Array.Empty<float>();

            int perBar = Math.Max(1, frames / bars);
            var result = new float[bars * 2];

            for (int b = 0; b < bars; b++)
            {
                int start = b * perBar;
                int end = Math.Min(start + perBar, frames);
                float max = 0f;

                for (int i = start; i < end; i++)
                {
                    float v = DecodeSample(info, i * bytesPerSample);
                    var abs = Math.Abs(v);
                    if (abs > max) max = abs;
                }

                result[b * 2] = 0;
                result[b * 2 + 1] = max;
            }
            return result;
        }
        catch
        {
            return Array.Empty<float>();
        }
    }

    private static float DecodeSample(WavProcessor.WavInfo info, int offset)
    {
        int bits = info.BitsPerSample;
        var d = info.RawData;
        if (offset < 0 || offset >= d.Length) return 0;
        if (bits == 8) return (d[offset] - 128) / 128f;
        if (bits == 16 && offset + 1 < d.Length)
            return BitConverter.ToInt16(d, offset) / 32768f;
        if (bits == 24 && offset + 2 < d.Length)
            return (d[offset] | (d[offset + 1] << 8) | ((sbyte)d[offset + 2] << 16)) / 8388608f;
        if (bits == 32 && offset + 3 < d.Length)
            return BitConverter.ToInt32(d, offset) / 2147483648f;
        return 0;
    }
}
