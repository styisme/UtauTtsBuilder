using System.Net.Http.Headers;

namespace UtauTtsBuilder.Services;

public static class LocalAiModelManager
{
    public const string ModelUrl = "https://github.com/thewh1teagle/kokoro-onnx/releases/download/model-files-v1.1/kokoro-v1.0.int8.onnx";
    public const string VoicesUrl = "https://github.com/thewh1teagle/kokoro-onnx/releases/download/model-files-v1.1/voices-v1.0.bin";
    
    public const string ModelFileName = "kokoro.onnx";
    public const string VoicesFileName = "voices.bin";

    public static bool IsModelDownloaded
    {
        get
        {
            var path = GetModelDir();
            return Directory.Exists(path) 
                && File.Exists(Path.Combine(path, ModelFileName))
                && File.Exists(Path.Combine(path, VoicesFileName));
        }
    }

    public static string GetModelDir()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "ai_models");
    }

    public static async Task DownloadModelAsync(IProgress<double> progress)
    {
        var targetDir = GetModelDir();
        Directory.CreateDirectory(targetDir);

        await DownloadFileAsync(ModelUrl, Path.Combine(targetDir, ModelFileName), 
            p => progress.Report(p * 0.8)); // 前 80% 是模型

        await DownloadFileAsync(VoicesUrl, Path.Combine(targetDir, VoicesFileName), 
            p => progress.Report(0.8 + p * 0.2)); // 后 20% 是音色包

        progress.Report(1.0);
    }

    private static async Task DownloadFileAsync(string url, string savePath, Action<double> onProgress)
    {
        var tempPath = savePath + ".downloading";
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("UtauTtsBuilder");

        try
        {
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var downloadStream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[8192];
            long totalRead = 0;
            int read;
            while ((read = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;
                if (totalBytes > 0) onProgress((double)totalRead / totalBytes);
            }

            fileStream.Close();
            if (File.Exists(savePath)) File.Delete(savePath);
            File.Move(tempPath, savePath);
        }
        finally
        {
            if (File.Exists(tempPath)) try { File.Delete(tempPath); } catch { }
        }
    }
}
