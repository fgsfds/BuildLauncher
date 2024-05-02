using System.Text.Json.Serialization;

namespace Common.Helpers
{
    public sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("draft")]
        public bool IsDraft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool IsPrerelease { get; set; }

        [JsonPropertyName("assets")]
        public GitHubReleaseAsset[] Assets { get; set; }

        [JsonPropertyName("body")]
        public string Description { get; set; }
    }

    public sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string FileName { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedDate { get; set; }
    }


    [JsonSerializable(typeof(List<GitHubRelease>))]
    public sealed partial class GitHubReleaseContext : JsonSerializerContext { }


    public sealed class CommonRelease
    {
        public readonly string Url;
        public readonly string Version;

        public CommonRelease(string url, string version)
        {
            Url = url;
            Version = version;
        }
    }
}
