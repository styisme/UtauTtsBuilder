namespace UtauTtsBuilder.Services;

public enum VoicebankLanguage
{
    Japanese,
    JapaneseCvvc,
    JapaneseVcv,
    Chinese,
    English,
    Custom
}

public class ReclistEntry
{
    public string Alias { get; set; } = "";
    public string TtsText { get; set; } = "";
    public string FileName { get; set; } = "";
    public bool IsTransition { get; set; }
}

public static class ReclistProvider
{
    private const string KeyCustomReclist = "custom_reclist";
    private const string KeyCustomLanguage = "custom_language_code";
    private const string KeyCustomName = "custom_reclist_name";

    public static bool HasCustomReclist =>
        !string.IsNullOrWhiteSpace(Preferences.Default.Get(KeyCustomReclist, ""));

    public static string CustomReclistName =>
        Preferences.Default.Get(KeyCustomName, Loc.T("custom_default_name"));

    public static void SaveCustomReclist(
        string name, string languageCode, Dictionary<string, string> reclist)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var kv in reclist)
            sb.Append(kv.Key).Append('=').Append(kv.Value).Append('\n');
        Preferences.Default.Set(KeyCustomReclist, sb.ToString());
        Preferences.Default.Set(KeyCustomLanguage, languageCode);
        Preferences.Default.Set(KeyCustomName, name);
    }

    public static void ClearCustomReclist()
    {
        Preferences.Default.Remove(KeyCustomReclist);
        Preferences.Default.Remove(KeyCustomLanguage);
        Preferences.Default.Remove(KeyCustomName);
    }

    private static List<ReclistEntry> LoadCustomReclist()
    {
        var raw = Preferences.Default.Get(KeyCustomReclist, "");
        var result = new List<ReclistEntry>();
        if (string.IsNullOrWhiteSpace(raw)) return result;

        foreach (var line in raw.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            var idx = trimmed.IndexOf('=');
            if (idx <= 0) continue;
            var k = trimmed.Substring(0, idx).Trim();
            var v = trimmed.Substring(idx + 1).Trim();
            if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(v)) continue;

            result.Add(new ReclistEntry
            {
                Alias = k,
                TtsText = v,
                FileName = SanitizeFileName(k)
            });
        }
        return result;
    }

    private static string SanitizeFileName(string alias)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in alias)
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (c == ' ' || c == '_') sb.Append('_');
        }
        var result = sb.ToString();
        return string.IsNullOrEmpty(result) ? "entry" : result;
    }

    public static List<ReclistEntry> GetReclist(VoicebankLanguage lang) => lang switch
    {
        VoicebankLanguage.Japanese => ConvertCvDict(JapaneseCvReclist.CvList),
        VoicebankLanguage.JapaneseCvvc => BuildJapaneseCvvc(),
        VoicebankLanguage.JapaneseVcv => BuildJapaneseVcv(),
        VoicebankLanguage.Chinese => ConvertCvDict(ChineseCvList),
        VoicebankLanguage.English => ConvertCvDict(EnglishCvList),
        VoicebankLanguage.Custom => LoadCustomReclist(),
        _ => ConvertCvDict(JapaneseCvReclist.CvList)
    };

    private static List<ReclistEntry> ConvertCvDict(Dictionary<string, string> dict)
    {
        return dict.Select(kv => new ReclistEntry
        {
            Alias = kv.Key,
            TtsText = kv.Key,
            FileName = kv.Value,
            IsTransition = false
        }).ToList();
    }

    public static string GetLanguageCode(VoicebankLanguage lang) => lang switch
    {
        VoicebankLanguage.Japanese => "ja-JP",
        VoicebankLanguage.JapaneseCvvc => "ja-JP",
        VoicebankLanguage.JapaneseVcv => "ja-JP",
        VoicebankLanguage.Chinese => "zh-CN",
        VoicebankLanguage.English => "en-US",
        VoicebankLanguage.Custom => Preferences.Default.Get(KeyCustomLanguage, "ja-JP"),
        _ => "ja-JP"
    };

    public static string GetDisplayName(VoicebankLanguage lang) => lang switch
    {
        VoicebankLanguage.Japanese => Loc.T("reclist_ja_cv"),
        VoicebankLanguage.JapaneseCvvc => Loc.T("reclist_ja_cvvc"),
        VoicebankLanguage.JapaneseVcv => Loc.T("reclist_ja_vcv"),
        VoicebankLanguage.Chinese => Loc.T("reclist_zh_cv"),
        VoicebankLanguage.English => Loc.T("reclist_en_cv"),
        VoicebankLanguage.Custom => Loc.F("reclist_custom_fmt", CustomReclistName),
        _ => Loc.T("reclist_ja_cv")
    };

    // ============================================================
    // CVVC / VCV
    // ============================================================
    private static List<ReclistEntry> BuildJapaneseCvvc()
    {
        var list = new List<ReclistEntry>();
        foreach (var kv in JapaneseCvReclist.CvList)
        {
            list.Add(new ReclistEntry
            {
                Alias = kv.Key, TtsText = kv.Key, FileName = kv.Value, IsTransition = false
            });
        }
        var vowels = new (string Roma, string Kana)[]
        {
            ("a", "あ"), ("i", "い"), ("u", "う"), ("e", "え"), ("o", "お")
        };
        foreach (var v1 in vowels)
            foreach (var v2 in vowels)
            {
                if (v1.Roma == v2.Roma) continue;
                list.Add(new ReclistEntry
                {
                    Alias = v1.Roma + " " + v2.Roma,
                    TtsText = v1.Kana + v2.Kana,
                    FileName = v1.Roma + "_" + v2.Roma,
                    IsTransition = true
                });
            }
        var cons = new (string Roma, string Kana)[]
        {
            ("k", "か"), ("s", "さ"), ("t", "た"), ("n", "な"),
            ("h", "は"), ("m", "ま"), ("y", "や"), ("w", "わ"), ("r", "ら")
        };
        foreach (var v in vowels)
            foreach (var c in cons)
            {
                list.Add(new ReclistEntry
                {
                    Alias = v.Roma + " " + c.Roma,
                    TtsText = v.Kana + c.Kana,
                    FileName = v.Roma + "_" + c.Roma,
                    IsTransition = true
                });
            }
        return list;
    }

    private static List<ReclistEntry> BuildJapaneseVcv()
    {
        var list = new List<ReclistEntry>();
        var vowels = new (string Roma, string Kana)[]
        {
            ("a", "あ"), ("i", "い"), ("u", "う"), ("e", "え"), ("o", "お")
        };
        foreach (var v in vowels)
            list.Add(new ReclistEntry
            {
                Alias = v.Roma, TtsText = v.Kana, FileName = v.Roma, IsTransition = false
            });
        list.Add(new ReclistEntry
        {
            Alias = "n", TtsText = "ん", FileName = "n", IsTransition = false
        });
        foreach (var v in vowels)
            list.Add(new ReclistEntry
            {
                Alias = v.Roma + " n", TtsText = v.Kana + "ん",
                FileName = v.Roma + "_n", IsTransition = true
            });
        foreach (var v1 in vowels)
            foreach (var v2 in vowels)
            {
                if (v1.Roma == v2.Roma) continue;
                list.Add(new ReclistEntry
                {
                    Alias = v1.Roma + " " + v2.Roma,
                    TtsText = v1.Kana + v2.Kana,
                    FileName = v1.Roma + "_" + v2.Roma,
                    IsTransition = true
                });
            }
        var rows = new (string Roma, string Kana)[]
        {
            ("k", "か"), ("g", "が"), ("s", "さ"), ("z", "ざ"),
            ("t", "た"), ("d", "だ"), ("n", "な"),
            ("h", "は"), ("b", "ば"), ("p", "ぱ"),
            ("m", "ま"), ("y", "や"), ("r", "ら"), ("w", "わ")
        };
        foreach (var c in rows)
            foreach (var v in vowels)
                list.Add(new ReclistEntry
                {
                    Alias = v.Roma + " " + c.Roma + "a",
                    TtsText = v.Kana + c.Kana,
                    FileName = v.Roma + "_" + c.Roma + "a",
                    IsTransition = true
                });
        return list;
    }

    // ============================================================
    // 内置 CV 数据
    // ============================================================
    public static readonly Dictionary<string, string> ChineseCvList = new()
    {
        ["啊"] = "a", ["哦"] = "o", ["鹅"] = "e", ["衣"] = "i", ["乌"] = "u", ["鱼"] = "v",
        ["爱"] = "ai", ["奥"] = "ao", ["欧"] = "ou", ["安"] = "an", ["恩"] = "en", ["昂"] = "ang",
        ["巴"] = "ba", ["波"] = "bo", ["逼"] = "bi", ["布"] = "bu",
        ["怕"] = "pa", ["坡"] = "po", ["皮"] = "pi", ["铺"] = "pu",
        ["妈"] = "ma", ["摸"] = "mo", ["米"] = "mi", ["母"] = "mu",
        ["发"] = "fa", ["佛"] = "fo", ["福"] = "fu",
        ["大"] = "da", ["地"] = "di", ["杜"] = "du",
        ["他"] = "ta", ["梯"] = "ti", ["图"] = "tu",
        ["那"] = "na", ["你"] = "ni", ["奴"] = "nu",
        ["拉"] = "la", ["里"] = "li", ["路"] = "lu",
        ["哥"] = "ge", ["古"] = "gu", ["可"] = "ke", ["哭"] = "ku",
        ["喝"] = "he", ["胡"] = "hu",
        ["机"] = "ji", ["家"] = "jia", ["节"] = "jie", ["九"] = "jiu",
        ["七"] = "qi", ["恰"] = "qia", ["切"] = "qie", ["秋"] = "qiu",
        ["西"] = "xi", ["下"] = "xia", ["写"] = "xie", ["修"] = "xiu",
        ["知"] = "zhi", ["扎"] = "zha", ["者"] = "zhe",
        ["吃"] = "chi", ["插"] = "cha", ["车"] = "che",
        ["是"] = "shi", ["沙"] = "sha", ["蛇"] = "she",
        ["日"] = "ri", ["热"] = "re",
        ["字"] = "zi", ["杂"] = "za", ["则"] = "ze",
        ["次"] = "ci", ["擦"] = "ca", ["册"] = "ce",
        ["思"] = "si", ["撒"] = "sa", ["色"] = "se",
    };

    public static readonly Dictionary<string, string> EnglishCvList = new()
    {
        ["ah"] = "ah", ["ay"] = "ay", ["ee"] = "ee", ["eh"] = "eh",
        ["ih"] = "ih", ["oh"] = "oh", ["oo"] = "oo", ["uh"] = "uh",
        ["bah"] = "ba", ["bay"] = "bae", ["bee"] = "be", ["boh"] = "bo", ["boo"] = "bu",
        ["dah"] = "da", ["day"] = "dae", ["dee"] = "de", ["doh"] = "do", ["doo"] = "du",
        ["fah"] = "fa", ["fay"] = "fae", ["fee"] = "fe", ["foh"] = "fo", ["foo"] = "fu",
        ["gah"] = "ga", ["gay"] = "gae", ["gee"] = "ge", ["goh"] = "go", ["goo"] = "gu",
        ["hah"] = "ha", ["hay"] = "hae", ["hee"] = "he", ["hoh"] = "ho", ["hoo"] = "hu",
        ["kah"] = "ka", ["kay"] = "kae", ["kee"] = "ke", ["koh"] = "ko", ["koo"] = "ku",
        ["lah"] = "la", ["lay"] = "lae", ["lee"] = "le", ["loh"] = "lo", ["loo"] = "lu",
        ["mah"] = "ma", ["may"] = "mae", ["mee"] = "me", ["moh"] = "mo", ["moo"] = "mu",
        ["nah"] = "na", ["nay"] = "nae", ["nee"] = "ne", ["noh"] = "no", ["noo"] = "nu",
        ["pah"] = "pa", ["pay"] = "pae", ["pee"] = "pe", ["poh"] = "po", ["poo"] = "pu",
        ["rah"] = "ra", ["ray"] = "rae", ["ree"] = "re", ["roh"] = "ro", ["roo"] = "ru",
        ["sah"] = "sa", ["say"] = "sae", ["see"] = "se", ["soh"] = "so", ["soo"] = "su",
        ["tah"] = "ta", ["tay"] = "tae", ["tee"] = "te", ["toh"] = "to", ["too"] = "tu",
        ["vah"] = "va", ["vay"] = "vae", ["vee"] = "ve", ["voh"] = "vo", ["voo"] = "vu",
        ["wah"] = "wa", ["way"] = "wae", ["wee"] = "we", ["woh"] = "wo", ["woo"] = "wu",
        ["yah"] = "ya", ["yay"] = "yae", ["yee"] = "ye", ["yoh"] = "yo", ["yoo"] = "yu",
        ["zah"] = "za", ["zay"] = "zae", ["zee"] = "ze", ["zoh"] = "zo", ["zoo"] = "zu",
    };
}
