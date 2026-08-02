using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RetroLauncher.Services.Updates
{
    public enum ApplicationUpdateStatus
    {
        NotChecked,
        Checking,
        UpToDate,
        UpdateAvailable,
        CheckFailed,
        NoCompatibleAsset,
        Downloading,
        ReadyToInstall,
        Installing,
        RestartRequired,
        Failed
    }

    public sealed class ApplicationUpdateCheckResult
    {
        public bool CheckSucceeded { get; init; }
        public bool UpdateAvailable { get; init; }
        public Version? CurrentVersion { get; init; }
        public Version? LatestVersion { get; init; }
        public string? ReleaseTag { get; init; }
        public string? ReleaseName { get; init; }
        public string? ReleaseNotes { get; init; }
        public DateTimeOffset? PublishedAt { get; init; }
        public string? AssetName { get; init; }
        public long? AssetSize { get; init; }
        public Uri? DownloadUri { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public static ApplicationUpdateCheckResult Success(
            bool updateAvailable,
            Version currentVer,
            Version latestVer,
            string tag,
            string name,
            string notes,
            DateTimeOffset? publishedAt,
            string assetName,
            long assetSize,
            Uri downloadUri)
        {
            return new ApplicationUpdateCheckResult
            {
                CheckSucceeded = true,
                UpdateAvailable = updateAvailable,
                CurrentVersion = currentVer,
                LatestVersion = latestVer,
                ReleaseTag = tag,
                ReleaseName = name,
                ReleaseNotes = notes,
                PublishedAt = publishedAt,
                AssetName = assetName,
                AssetSize = assetSize,
                DownloadUri = downloadUri
            };
        }

        public static ApplicationUpdateCheckResult UpToDateResult(Version currentVer, Version latestVer, string tag)
        {
            return new ApplicationUpdateCheckResult
            {
                CheckSucceeded = true,
                UpdateAvailable = false,
                CurrentVersion = currentVer,
                LatestVersion = latestVer,
                ReleaseTag = tag
            };
        }

        public static ApplicationUpdateCheckResult Fail(string errorCode, string message, Version? currentVer = null)
        {
            return new ApplicationUpdateCheckResult
            {
                CheckSucceeded = false,
                UpdateAvailable = false,
                CurrentVersion = currentVer,
                ErrorCode = errorCode,
                ErrorMessage = message
            };
        }
    }

    public class GitHubAppReleaseDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("assets")]
        public List<GitHubAppReleaseAssetDto> Assets { get; set; } = new();
    }

    public class GitHubAppReleaseAssetDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
