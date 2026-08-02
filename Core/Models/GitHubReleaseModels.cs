using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RetroLauncher.Core.Models
{
    public class GitHubRateLimitInfo
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTime ResetTime { get; set; }
    }

    public class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = "";

        [JsonPropertyName("download_count")]
        public int DownloadCount { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("draft")]
        public bool IsDraft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool IsPrerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = "";

        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset> Assets { get; set; } = new();
    }

    public class GitHubApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public System.Net.HttpStatusCode? StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public GitHubRateLimitInfo? RateLimit { get; set; }
        public string? ETag { get; set; }
    }
}
