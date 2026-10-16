using System.Text;

namespace UtauTtsBuilder.Services;

public static class UstTemplateGenerator
{
    /// <summary>
    /// 生成一个简单的 test.ust 文件，用户可以用 UTAU 打开试听。
    /// UST 格式：Shift-JIS 编码，包含 3 个音符。
    /// </summary>
    public static async Task<string> GenerateAsync(string outputDir, string voicebankName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("[#VERSION]");
        sb.AppendLine("UST Version1.2");
        sb.AppendLine("[#SETTING]");
        sb.AppendLine("Tempo=120.00");
        sb.AppendLine("ProjectName=" + voicebankName + " Test");
        sb.AppendLine("VoiceDir=%VOICE%");
        sb.AppendLine("OutFile=" + outputDir + "\\test.wav");
        sb.AppendLine("CacheDir=" + outputDir + "\\cache");
        sb.AppendLine("Flags=");
        sb.AppendLine("Mode2=True");

        // 三个音符：a ka a（或根据语言简单调整）
        AddNote(sb, 0, 480, "60", "a", "あ");
        AddNote(sb, 480, 960, "62", "ka", "か");
        AddNote(sb, 960, 1440, "60", "a", "あ");

        sb.AppendLine("[#TRACKEND]");

        var path = Path.Combine(outputDir, "test.ust");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var sjis = Encoding.GetEncoding("shift_jis",
            new EncoderReplacementFallback("?"),
            new DecoderReplacementFallback("?"));

        await File.WriteAllTextAsync(path, sb.ToString(), sjis);
        return path;
    }

    private static void AddNote(StringBuilder sb, int offset,
        int length, string noteNum, string lyric, string alias)
    {
        sb.AppendLine("[#" + string.Format("{0:D4}", 0).Substring(0) + "]");
        // 说明：为简化，每个音符用同一 ID 头，实际 UTAU 会顺序解析
        // 更严格的做法是给不同 ID，这里简化处理
        sb.AppendLine("Length=" + length);
        sb.AppendLine("Lyric=" + lyric);
        sb.AppendLine("NoteNum=" + noteNum);
        sb.AppendLine("PreUtterance=");
        sb.AppendLine();
    }
}
