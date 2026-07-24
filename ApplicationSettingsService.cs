using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RetroLauncher
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        private static ApplicationSettingsService? _instance;
        public static ApplicationSettingsService Instance => _instance ??= new ApplicationSettingsService();

        public NetworkSettings Network { get; set; } = new();
        public GitHubSettings GitHub { get; set; } = new();
        public CacheSettings Cache { get; set; } = new();
        public DownloadSettings Download { get; set; } = new();
        public InstallationSettings Installation { get; set; } = new();

        // Preserve legacy settings
        public Dictionary<string, string> DefaultEmulators { get; set; } = new();
        public bool IsFirstRun { get; set; } = true;
        public int WindowWidth { get; set; } = 1100;
        public int WindowHeight { get; set; } = 650;
        public int WindowLeft { get; set; } = -1;
        public int WindowTop { get; set; } = -1;
        public bool IsMaximized { get; set; } = false;

        public ApplicationSettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;

                        if (root.TryGetProperty("isFirstRun", out var isFirstRunProp)) IsFirstRun = isFirstRunProp.GetBoolean();
                        else if (root.TryGetProperty("IsFirstRun", out isFirstRunProp)) IsFirstRun = isFirstRunProp.GetBoolean();

                        if (root.TryGetProperty("windowWidth", out var wWidth)) WindowWidth = wWidth.GetInt32();
                        else if (root.TryGetProperty("WindowWidth", out wWidth)) WindowWidth = wWidth.GetInt32();

                        if (root.TryGetProperty("windowHeight", out var wHeight)) WindowHeight = wHeight.GetInt32();
                        else if (root.TryGetProperty("WindowHeight", out wHeight)) WindowHeight = wHeight.GetInt32();

                        if (root.TryGetProperty("windowLeft", out var wLeft)) WindowLeft = wLeft.GetInt32();
                        else if (root.TryGetProperty("WindowLeft", out wLeft)) WindowLeft = wLeft.GetInt32();

                        if (root.TryGetProperty("windowTop", out var wTop)) WindowTop = wTop.GetInt32();
                        else if (root.TryGetProperty("WindowTop", out wTop)) WindowTop = wTop.GetInt32();

                        if (root.TryGetProperty("isMaximized", out var isMax)) IsMaximized = isMax.GetBoolean();
                        else if (root.TryGetProperty("IsMaximized", out isMax)) IsMaximized = isMax.GetBoolean();

                        if (root.TryGetProperty("defaultEmulators", out var defEmus))
                        {
                            DefaultEmulators = JsonSerializer.Deserialize<Dictionary<string, string>>(defEmus.GetRawText()) ?? new();
                        }
                        else if (root.TryGetProperty("DefaultEmulators", out defEmus))
                        {
                            DefaultEmulators = JsonSerializer.Deserialize<Dictionary<string, string>>(defEmus.GetRawText()) ?? new();
                        }

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        if (root.TryGetProperty("network", out var netProp)) Network = JsonSerializer.Deserialize<NetworkSettings>(netProp.GetRawText(), options) ?? new();
                        if (root.TryGetProperty("gitHub", out var ghProp)) GitHub = JsonSerializer.Deserialize<GitHubSettings>(ghProp.GetRawText(), options) ?? new();
                        if (root.TryGetProperty("cache", out var cacheProp)) Cache = JsonSerializer.Deserialize<CacheSettings>(cacheProp.GetRawText(), options) ?? new();
                        if (root.TryGetProperty("download", out var dlProp)) Download = JsonSerializer.Deserialize<DownloadSettings>(dlProp.GetRawText(), options) ?? new();
                        if (root.TryGetProperty("installation", out var instProp)) Installation = JsonSerializer.Deserialize<InstallationSettings>(instProp.GetRawText(), options) ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Error migrating/loading settings: {ex.Message}", "ERROR");
            }
        }

        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                
                string? dir = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Failed to save settings: {ex.Message}", "ERROR");
            }
        }

        public List<string> ValidateSettings()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(GitHub.BaseUrl) || !Uri.TryCreate(GitHub.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                errors.Add("GitHub Base URL must be a valid HTTP or HTTPS absolute URL.");
            }

            if (string.IsNullOrWhiteSpace(Download.DownloadTempDir) || Download.DownloadTempDir.Contains(".."))
            {
                errors.Add("Download temporary directory path is invalid or contains unsafe directory traversal segments.");
            }

            if (string.IsNullOrWhiteSpace(Installation.EmulatorInstallationRoot) || Installation.InstallationRootContainsTraversal())
            {
                errors.Add("Emulator installation root path is invalid or contains unsafe directory traversal segments.");
            }

            return errors;
        }
    }

    public static class InstallationSettingsExtensions
    {
        public static bool InstallationRootContainsTraversal(this InstallationSettings settings)
        {
            return settings.EmulatorInstallationRoot.Contains("..") || Path.IsPathRooted(settings.EmulatorInstallationRoot) && !Path.GetFullPath(settings.EmulatorInstallationRoot).StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase);
        }
    }
}
