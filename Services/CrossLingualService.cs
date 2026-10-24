namespace UtauTtsBuilder.Services;

public enum CrossLingualMode
{
    Native,
    ZhToJa,
    JaToZh,
    EnToJa,
    EnToZh,
    JaToEn,
    ZhToEn,
    Smart
}

public static class CrossLingualService
{
    public static string GetDisplayName(CrossLingualMode mode) => mode switch
    {
        CrossLingualMode.Native => Loc.T("cross_native"),
        CrossLingualMode.ZhToJa => "中 → 日",
        CrossLingualMode.JaToZh => "日 → 中",
        CrossLingualMode.EnToJa => "英 → 日",
        CrossLingualMode.EnToZh => "英 → 中",
        CrossLingualMode.JaToEn => "日 → 英",
        CrossLingualMode.ZhToEn => "中 → 英",
        CrossLingualMode.Smart => Loc.T("cross_smart"),
        _ => "Native"
    };

    /// <summary>
    /// 将原始文本转换为目标 TTS 读取的文本
    /// </summary>
    public static string Transform(string originalText, string targetLangCode, CrossLingualMode mode)
    {
        if (mode == CrossLingualMode.Native) return originalText;

        if (mode == CrossLingualMode.Smart)
        {
            mode = targetLangCode switch
            {
                "zh-CN" => CrossLingualMode.JaToZh, // 假设默认识别日文
                "en-US" => CrossLingualMode.JaToEn,
                _ => CrossLingualMode.Native
            };
        }

        // 用罗马音作为通用桥梁
        string roman = ExtractRoman(originalText);
        if (string.IsNullOrEmpty(roman)) return originalText;

        return mode switch
        {
            CrossLingualMode.ZhToJa => RomanToJa(roman),
            CrossLingualMode.JaToZh => RomanToZh(roman),
            CrossLingualMode.EnToJa => RomanToJa(roman),
            CrossLingualMode.EnToZh => RomanToZh(roman),
            CrossLingualMode.JaToEn => RomanToEn(roman),
            CrossLingualMode.ZhToEn => RomanToEn(roman),
            _ => originalText
        };
    }

    // 提取核心罗马音（基于现有数据表反查）
    private static string ExtractRoman(string text)
    {
        // 查日文
        var ja = JapaneseCvReclist.CvList;
        foreach (var kv in ja) if (kv.Key == text) return kv.Value;
        // 查中文
        var zh = ReclistProvider.ChineseCvList;
        foreach (var kv in zh) if (kv.Key == text) return kv.Value;
        // 查英文
        var en = ReclistProvider.EnglishCvList;
        foreach (var kv in en) if (kv.Key == text) return kv.Value;

        // 如果是字母，直接返回（大多是罗马音/拼音）
        if (text.All(char.IsLetter) && text.All(c => c < 128)) return text;

        return text;
    }

    private static string RomanToZh(string roman)
    {
        foreach (var kv in ReclistProvider.ChineseCvList)
            if (kv.Value.Equals(roman, StringComparison.OrdinalIgnoreCase)) return kv.Key;
        // Fallback
        return roman;
    }

    private static string RomanToJa(string roman)
    {
        foreach (var kv in JapaneseCvReclist.CvList)
            if (kv.Value.Equals(roman, StringComparison.OrdinalIgnoreCase)) return kv.Key;
        // Fallback
        return roman;
    }

    private static string RomanToEn(string roman)
    {
        foreach (var kv in ReclistProvider.EnglishCvList)
            if (kv.Value.Equals(roman, StringComparison.OrdinalIgnoreCase)) return kv.Key;
        return roman;
    }
}
