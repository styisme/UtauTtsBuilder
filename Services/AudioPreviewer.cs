namespace UtauTtsBuilder.Services;

public static class AudioPreviewer
{
    /// <summary>通过系统分享面板把 wav 交给系统播放器</summary>
    public static async Task PlayAsync(string wavPath, string title)
    {
        if (!File.Exists(wavPath))
            throw new FileNotFoundException("File not found", wavPath);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(wavPath)
        });
    }

    /// <summary>随机抽取 n 个 wav 文件</summary>
    public static List<string> SampleRandom(string dir, int n)
    {
        try
        {
            var files = Directory.GetFiles(dir, "*.wav");
            if (files.Length <= n) return files.ToList();

            var rnd = new Random();
            return files.OrderBy(_ => rnd.Next()).Take(n).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}
