using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RetroLauncher.Core.Utilities;

namespace RetroLauncher.Services.Updates
{
    public interface IGitHubApplicationReleaseClient
    {
        Task<ApplicationUpdateCheckResult> CheckForLatestReleaseAsync(
            IApplicationVersionProvider versionProvider,
            bool allowPrerelease = false,
            CancellationToken cancellationToken = default);
    }

    public class GitHubApplicationReleaseClient : IGitHubApplicationReleaseClient
    {
        private const string RepositoryOwner = "TolgaRendaSimsek";
        private const string RepositoryName = "Retro-Launcher";
        private const string ApiEndpoint = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest";

        private readonly HttpClient _httpClient;

        public GitHubApplicationReleaseClient(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<ApplicationUpdateCheckResult> CheckForLatestReleaseAsync(
            IApplicationVersionProvider versionProvider,
            bool allowPrerelease = false,
            CancellationToken cancellationToken = default)
        {
            Version currentVersion = versionProvider.InstalledVersion;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoint);
                request.Headers.UserAgent.ParseAdd("RetroLauncher");
                request.Headers.Accept.ParseAdd("application/vnd.github+json");
                request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return ApplicationUpdateCheckResult.Fail(
                        $"HTTP_{(int)response.StatusCode}",
                        $"GitHub API request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).",
                        currentVersion);
                }

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                var release = JsonSerializer.Deserialize<GitHubAppReleaseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (release == null)
                {
                    return ApplicationUpdateCheckResult.Fail("MALFORMED_JSON", "Failed to deserialize GitHub release JSON response.", currentVersion);
                }

                if (release.Draft)
                {
                    return ApplicationUpdateCheckResult.UpToDateResult(currentVersion, currentVersion, release.TagName);
                }

                if (release.Prerelease && !allowPrerelease)
                {
                    return ApplicationUpdateCheckResult.UpToDateResult(currentVersion, currentVersion, release.TagName);
                }

                // Parse Remote Version
                if (!TryParseTagVersion(release.TagName, out Version? remoteVersion) || remoteVersion == null)
                {
                    return ApplicationUpdateCheckResult.Fail("INVALID_TAG_VERSION", $"Could not parse remote version tag '{release.TagName}'.", currentVersion);
                }

                // Check Version Comparison
                if (remoteVersion <= currentVersion)
                {
                    return ApplicationUpdateCheckResult.UpToDateResult(currentVersion, remoteVersion, release.TagName);
                }

                // Select Compatible Asset
                var asset = SelectCompatibleAsset(release);
                if (asset == null || string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl))
                {
                    return ApplicationUpdateCheckResult.Fail(
                        "NO_COMPATIBLE_ASSET",
                        $"Release {release.TagName} is newer ({remoteVersion}), but no compatible Windows x64 package asset was found.",
                        currentVersion);
                }

                if (!Uri.TryCreate(asset.BrowserDownloadUrl, UriKind.Absolute, out Uri? downloadUri) || downloadUri.Scheme != Uri.UriSchemeHttps)
                {
                    return ApplicationUpdateCheckResult.Fail("INVALID_ASSET_URL", "Asset download URL is not a valid HTTPS URL.", currentVersion);
                }

                return ApplicationUpdateCheckResult.Success(
                    updateAvailable: true,
                    currentVer: currentVersion,
                    latestVer: remoteVersion,
                    tag: release.TagName,
                    name: !string.IsNullOrWhiteSpace(release.Name) ? release.Name : release.TagName,
                    notes: release.Body ?? string.Empty,
                    publishedAt: release.PublishedAt,
                    assetName: asset.Name,
                    assetSize: asset.Size,
                    downloadUri: downloadUri);
            }
            catch (OperationCanceledException)
            {
                return ApplicationUpdateCheckResult.Fail("CANCELLED", "Update check was cancelled.", currentVersion);
            }
            catch (Exception ex)
            {
                return ApplicationUpdateCheckResult.Fail("REQUEST_EXCEPTION", ex.Message, currentVersion);
            }
        }

        public static bool TryParseTagVersion(string tagName, out Version? version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(tagName)) return false;

            string clean = tagName.Trim();
            if (clean.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(1);
            }
            else if (clean.StartsWith("release-", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(8);
            }

            int dashIdx = clean.IndexOf('-');
            if (dashIdx >= 0)
            {
                clean = clean.Substring(0, dashIdx);
            }

            string[] parts = clean.Split('.');
            if (parts.Length == 2)
            {
                clean += ".0";
            }

            return Version.TryParse(clean, out version);
        }

        public static GitHubAppReleaseAssetDto? SelectCompatibleAsset(GitHubAppReleaseDto release)
        {
            if (release.Assets == null || release.Assets.Count == 0) return null;

            return release.Assets.FirstOrDefault(a =>
            {
                if (string.IsNullOrWhiteSpace(a.Name)) return false;
                string name = a.Name.ToLowerInvariant();

                bool isTargetPlatform = name.Contains("win-x64") || name.Contains("win64") || name.Contains("windows-x64");
                bool isTargetApp = name.Contains("retrolauncher");
                bool isSupportedExt = name.EndsWith(".zip") || name.EndsWith(".exe");

                bool isRejected = name.Contains("source") ||
                                  name.Contains("symbols") ||
                                  name.Contains(".pdb") ||
                                  name.Contains("debug") ||
                                  name.Contains("arm") ||
                                  name.Contains("linux") ||
                                  name.Contains("macos") ||
                                  name.Contains(".sha256");

                return isTargetPlatform && isTargetApp && isSupportedExt && !isRejected;
            });
        }
    }
}
