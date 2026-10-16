using System.Text.Json;

namespace UtauTtsBuilder.Services;

public class UpdateInfo
{
    public string LatestVersion { get; set; } = "";
    public string CurrentVersion { get; set; } = "";
    public string ReleaseUrl { get; set; } = "";
    public bool HasUpdate { get; set; }
}

public static class UpdateService
{
    private const string RepoApi = "https://api.github.com/repos/styisme/UtauTtsBuilder/releases/latest";
    private const string KeyLastCheck = "update_last_check";
    private const string KeySkipVersion = "update_skip_version";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(8)
    };

    static UpdateService()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("UtauTtsBuilder");
        Http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    public static string CurrentVersion =>
        AppInfo.Current.VersionString ?? "0.0.0";

    public static string SkippedVersion =>
        Preferences.Default.Get(KeySkipVersion, "");

    public static void SkipVersion(string v)
        => Preferences.Default.Set(KeySkipVersion, v);

    public static async Task<UpdateInfo?> CheckAsync()
    {
        try
        {
            var json = await Http.GetStringAsync(RepoApi);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tag = root.TryGetProperty("tag_name", out var t)
                ? t.GetString() ?? "" : "";
            var htmlUrl = root.TryGetProperty("html_url", out var h)
                ? h.GetString() ?? "" : "";

            var latest = tag.TrimStart('v', 'V');
            var current = CurrentVersion;

            if (string.IsNullOrEmpty(latest)) return null;

            var hasUpdate = CompareVersion(latest, current) > 0
                && latest != SkippedVersion;

            Preferences.Default.Set(KeyLastCheck,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            return new UpdateInfo
            {
                LatestVersion = latest,
                CurrentVersion = current,
                ReleaseUrl = htmlUrl,
                HasUpdate = hasUpdate
            };
        }
        catch
        {
            return null;
        }
    }

    private static int CompareVersion(string a, string b)
    {
        var pa = a.Split('.');
        var pb = b.Split('.');
        int len = Math.Max(pa.Length, pb.Length);
        for (int i = 0; i < len; i++)
        {
            int va = 0, vb = 0;
            if (i < pa.Length) int.TryParse(pa[i], out va);
            if (i < pb.Length) int.TryParse(pb[i], out vb);
            if (va != vb) return va - vb;
        }
        return 0;
    }
}
