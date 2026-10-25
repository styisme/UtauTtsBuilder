using System.Net.Http.Headers;

namespace UtauTtsBuilder.Services;

public static class LocalAiModelManager
{
    // 这里用 Kokoro 官方或社区提供的 ONNX 模型下载地址（需自行替换为真实的直链）
    // 这里演示用占位地址，实际发布前请改为有效的托管地址
    public const string ModelUrl = "https://github.com/styisme/UtauTtsBuilder/releases/download/v1.2.0-models/kokoro_zh_ja_en.zip";
    public const string ModelFileName = "kokoro_models.zip";

    public static bool IsModelDownloaded
    {
        get
        {
            var path = GetModelDir();
            // 检查是否存在模型文件（这里假设解压后有 "kokoro.onnx"）
            return Directory.Exists(path) && File.Exists(Path.Combine(path, "kokoro.onnx"));
        }
    }

    public static string GetModelDir()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "ai_models");
    }

    /// <summary>
    /// 下载并解压模型。IProgress<(long, long)> 返回已下载字节和总字节。
    /// </summary>
    public static async Task DownloadModelAsync(IProgress<double> progress)
    {
        var targetDir = GetModelDir();
        Directory.CreateDirectory(targetDir);

        var zipPath = Path.Combine(targetDir, ModelFileName);
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("UtauTtsBuilder");

        using var response = await http.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

        using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var downloadStream = await response.Content.ReadAsStreamAsync();

        var buffer = new byte[8192];
        long totalRead = 0;
        int read;
        while ((read = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, read);
            totalRead += read;
            if (totalBytes > 0)
            {
                progress.Report((double)totalRead / totalBytes);
            }
        }

        fileStream.Close();

        // 解压
        progress.Report(1.0); // 下载完成
        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, targetDir, true);
        File.Delete(zipPath);
    }
}
