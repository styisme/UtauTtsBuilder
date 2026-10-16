namespace UtauTtsBuilder.Services;

public static class EstimateService
{
    // 不同引擎的平均每音节耗时估算（毫秒）
    public static int GetMsPerSyllable(string? enginePackage)
    {
        if (string.IsNullOrEmpty(enginePackage)) return 400;
        if (CloudConfigService.IsCloudEngine(enginePackage))
        {
            var p = CloudConfigService.GetProvider(enginePackage);
            return p == CloudProvider.Azure ? 900 : 1100;
        }
        return 400;
    }

    public static TimeSpan Estimate(VoicebankLanguage lang, string? enginePackage)
    {
        var reclist = ReclistProvider.GetReclist(lang);
        var ms = GetMsPerSyllable(enginePackage) * reclist.Count;
        return TimeSpan.FromMilliseconds(ms);
    }

    public static string Format(TimeSpan ts)
    {
        if (ts.TotalSeconds < 60)
            return $"{(int)ts.TotalSeconds} 秒";
        var min = (int)ts.TotalMinutes;
        var sec = ts.Seconds;
        return sec > 0 ? $"{min} 分 {sec} 秒" : $"{min} 分";
    }
}
