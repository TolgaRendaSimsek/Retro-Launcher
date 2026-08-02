using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher.Services.Updates
{
    public class UpdateInfo
    {
        public string Version { get; set; } = "";
        public string Name { get; set; } = "";
        public string Changelog { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Channel { get; set; } = "";
    }

    public static class UpdateManager
    {
        public const string CurrentVersion = "1.0.0";
        private static readonly HttpClient HttpClient;

        static UpdateManager()
        {
            HttpClient = new HttpClient();
            // GitHub API requires a User-Agent header
            HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RetroLauncher", "1.0"));
        }

        public static async Task CheckForUpdatesAsync(Form parentForm)
        {
            try
            {
                var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater_settings.json");
                var settings = new UpdaterSettings();
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    settings = JsonSerializer.Deserialize<UpdaterSettings>(json) ?? new UpdaterSettings();
                }

                UpdateInfo? update = await FetchLatestUpdateInfo(settings.UpdateChannel);
                if (update == null) return;

                // Compare versions
                if (Version.TryParse(update.Version, out Version? latest) &&
                    Version.TryParse(CurrentVersion, out Version? current))
                {
                    if (latest > current)
                    {
                        // Check if user skipped this version
                        if (settings.SkippedVersion == update.Version)
                        {
                            return;
                        }

                        // Show dialog
                        parentForm.Invoke((MethodInvoker)delegate
                        {
                            using (var dialog = new UpdateDialog(update, settings, settingsPath))
                            {
                                dialog.ShowDialog(parentForm);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
            }
        }

        public static async Task<UpdateInfo?> FetchLatestUpdateInfo(string channel)
        {
            // 1. Mock Local Update Check (to allow testing local JSON data)
            string mockPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mock_update.json");
            if (File.Exists(mockPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(mockPath);
                    var mockUpdate = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (mockUpdate != null && mockUpdate.Channel.Equals(channel, StringComparison.OrdinalIgnoreCase))
                    {
                        return mockUpdate;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading mock update: {ex.Message}");
                }
            }

            // 2. Real API Update Check
            try
            {
                string repoOwner = "RetroLauncher"; // Replace with real repo owner
                string repoName = "RetroLauncher";   // Replace with real repo name

                if (channel.Equals("nightly", StringComparison.OrdinalIgnoreCase))
                {
                    // Nightly channel checks the latest successful workflow runs
                    string runsUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/actions/runs?status=success&branch=main";
                    var runsResponse = await HttpClient.GetStringAsync(runsUrl);
                    using (JsonDocument doc = JsonDocument.Parse(runsResponse))
                    {
                        var runs = doc.RootElement.GetProperty("workflow_runs");
                        if (runs.GetArrayLength() > 0)
                        {
                            var latestRun = runs[0];
                            long runId = latestRun.GetProperty("id").GetInt64();
                            string headSha = latestRun.GetProperty("head_sha").GetString() ?? "unknown";
                            string updatedAt = latestRun.GetProperty("updated_at").GetString() ?? "";

                            // Extract short SHA for build tag
                            string shortSha = headSha.Length > 7 ? headSha.Substring(0, 7) : headSha;
                            string versionTag = $"1.0.1-nightly+{shortSha}";

                            // Retrieve artifacts url
                            string artifactsUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/actions/runs/{runId}/artifacts";
                            
                            // Note: Download url will be set to the actions artifact API URL.
                            // In real scenarios, users will need to auth or we link to a custom host.
                            return new UpdateInfo
                            {
                                Version = versionTag,
                                Name = $"RetroLauncher Nightly (Build {shortSha})",
                                Changelog = $"Automatic build from commit {headSha}.\nBuilt on {updatedAt}.",
                                DownloadUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/actions/runs/{runId}/artifacts",
                                Channel = "nightly"
                            };
                        }
                    }
                }
                else
                {
                    // Stable and Beta check Releases API
                    string releasesUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases";
                    var response = await HttpClient.GetStringAsync(releasesUrl);
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        foreach (var release in doc.RootElement.EnumerateArray())
                        {
                            bool isPrerelease = release.GetProperty("prerelease").GetBoolean();
                            bool isDraft = release.GetProperty("draft").GetBoolean();

                            if (isDraft) continue;

                            // Filter based on channel
                            if (channel.Equals("stable", StringComparison.OrdinalIgnoreCase) && isPrerelease)
                            {
                                continue;
                            }

                            string tagName = release.GetProperty("tag_name").GetString() ?? "";
                            // Clean version prefix e.g. "v1.1.0" -> "1.1.0"
                            string cleanVersion = tagName.StartsWith("v", StringComparison.OrdinalIgnoreCase) 
                                ? tagName.Substring(1) 
                                : tagName;

                            string name = release.GetProperty("name").GetString() ?? "";
                            string changelog = release.GetProperty("body").GetString() ?? "";
                            string downloadUrl = "";

                            // Find target release asset (Windows zip package)
                            var assets = release.GetProperty("assets");
                            foreach (var asset in assets.EnumerateArray())
                            {
                                string assetName = asset.GetProperty("name").GetString() ?? "";
                                if (assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && 
                                    assetName.Contains("RetroLauncher", StringComparison.OrdinalIgnoreCase))
                                {
                                    downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                    break;
                                }
                            }

                            if (string.IsNullOrEmpty(downloadUrl) && assets.GetArrayLength() > 0)
                            {
                                // Fallback to first asset if pattern check fails
                                downloadUrl = assets[0].GetProperty("browser_download_url").GetString() ?? "";
                            }

                            if (!string.IsNullOrEmpty(downloadUrl))
                            {
                                return new UpdateInfo
                                {
                                    Version = cleanVersion,
                                    Name = name,
                                    Changelog = changelog,
                                    DownloadUrl = downloadUrl,
                                    Channel = isPrerelease ? "beta" : "stable"
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API update check failed: {ex.Message}");
            }

            return null;
        }
    }
}
