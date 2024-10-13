﻿using System.Text.Json.Serialization;

namespace Common.Server.Entities;

public sealed class GitHubReleaseEntity
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


[JsonSerializable(typeof(List<GitHubReleaseEntity>))]
public sealed partial class GitHubReleaseContext : JsonSerializerContext;
