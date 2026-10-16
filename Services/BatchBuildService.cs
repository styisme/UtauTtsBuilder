namespace UtauTtsBuilder.Services;

public class BatchBuildRequest
{
    public string Name { get; set; } = "";
    public string EnginePackage { get; set; } = "";
    public string EngineLabel { get; set; } = "";
    public VoicebankLanguage Language { get; set; }
    public float SpeechRate { get; set; } = 0.85f;
    public float Pitch { get; set; } = 1.0f;
    public string? IconPath { get; set; }
    public string? PortraitPath { get; set; }
    public string? Author { get; set; }
    public string BaseOutputDir { get; set; } = "";
}

public static class BatchBuildService
{
    public static async Task<List<string>> RunAsync(
        List<BatchBuildRequest> requests,
        IProgress<string> progress)
    {
        var results = new List<string>();

        for (int i = 0; i < requests.Count; i++)
        {
            var req = requests[i];
            var tag = $"[{i + 1}/{requests.Count}]";
            progress.Report($"{tag} {req.EngineLabel} → {req.Name}");

            try
            {
                var safeName = SanitizeName(req.Name);
                var outDir = Path.Combine(req.BaseOutputDir, safeName);
                Directory.CreateDirectory(outDir);

                var builder = new VoicebankBuilder(
                    req.EnginePackage, progress, req.Language,
                    req.IconPath, req.PortraitPath, req.Author,
                    req.SpeechRate, req.Pitch, true);

                await builder.BuildAsync(outDir, req.Name);
                results.Add($"{tag} ✅ {req.Name}");
            }
            catch (Exception ex)
            {
                results.Add($"{tag} ❌ {req.Name}: {ex.Message}");
            }
        }
        return results;
    }

    private static string SanitizeName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }
}
