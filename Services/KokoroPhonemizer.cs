namespace UtauTtsBuilder.Services;

/// <summary>
/// 简易 G2P（文本转音素）映射器。
/// 将 UTAU 音节映射为 Kokoro 使用的 IPA 音素 ID。
/// </summary>
public static class KokoroPhonemizer
{
    // Kokoro 核心词表映射 (简化版，覆盖基础日/中/英)
    private static readonly Dictionary<string, int> Vocab = new()
    {
        { "_", 0 }, { ".", 1 }, { ",", 2 }, { "!", 3 }, { "?", 4 },
        { "a", 10 }, { "i", 11 }, { "u", 12 }, { "e", 13 }, { "o", 14 },
        { "k", 20 }, { "s", 21 }, { "t", 22 }, { "n", 23 }, { "h", 24 }, { "m", 25 }, { "y", 26 }, { "r", 27 }, { "w", 28 },
        { "g", 30 }, { "z", 31 }, { "d", 32 }, { "b", 33 }, { "p", 34 }, { "v", 35 },
        { "ʃ", 40 }, { "tʃ", 41 }, { "ts", 42 }, { "dʒ", 43 },
        { "ɴ", 50 }, { "ɲ", 51 }, { "ɾ", 52 }
    };

    public static long[] GetTokenIds(string text)
    {
        var phonemes = SplitToPhonemes(text);
        var ids = new List<long>();
        
        // 起始符
        ids.Add(0);

        foreach (var p in phonemes)
        {
            if (Vocab.TryGetValue(p, out int id))
                ids.Add(id);
            else
                ids.Add(10); // fallback to 'a'
        }

        // 结束符
        ids.Add(0);
        return ids.ToArray();
    }

    private static List<string> SplitToPhonemes(string text)
    {
        var result = new List<string>();
        string t = text.ToLowerInvariant();

        // 日语特殊拗音/促音
        if (t == "っ") return new List<string> { "_" }; // 停顿

        // 简单二合音素拆分
        if (t.StartsWith("ky")) return new List<string> { "k", "y", t.Substring(2) };
        if (t.StartsWith("sh")) return new List<string> { "ʃ", t.Substring(2) };
        if (t.StartsWith("ch")) return new List<string> { "tʃ", t.Substring(2) };
        if (t.StartsWith("ts")) return new List<string> { "ts", t.Substring(2) };

        // 逐字符匹配（适用于大部分 CV）
        foreach (char c in t)
        {
            if (c == ' ') continue;
            string s = c.ToString();
            
            // 处理假名与罗马音的直接映射（因为 ReclistProvider 生成的多为罗马音 TtsText）
            // 如果是纯元音
            if ("aiueo".Contains(s) && result.Count > 0 && "aiueo".Contains(result.Last()[0]))
            {
                // 连续元音，直通
                result.Add(s);
            }
            else if ("kstnhmrbpgyzwj".Contains(s))
            {
                // 辅音开头
                result.Add(s);
            }
            else if ("aiueo".Contains(s))
            {
                result.Add(s);
            }
            else
            {
                // 假名强制匹配（如果是直接传了假名）
                switch (c)
                {
                    case 'あ': result.Add("a"); break;
                    case 'い': result.Add("i"); break;
                    case 'う': result.Add("u"); break;
                    case 'え': result.Add("e"); break;
                    case 'お': result.Add("o"); break;
                    case 'か': result.AddRange(new[] { "k", "a" }); break;
                    case 'き': result.AddRange(new[] { "k", "i" }); break;
                    // ... 可以按需扩展完整 50 音
                    default: result.Add("a"); break; // 未知全部按 a 处理
                }
            }
        }

        return result;
    }
}
