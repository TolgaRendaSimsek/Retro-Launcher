using System;

namespace RetroLauncher.Core.Models
{
    public class EmulatorUpdateInfo
    {
        public string EmulatorId { get; set; } = "";
        public string InstalledVersion { get; set; } = "";
        public string AvailableVersion { get; set; } = "";
        public string InstalledReleaseTag { get; set; } = "";
        public string AvailableReleaseTag { get; set; } = "";
        public bool IsUpdateAvailable { get; set; }
        public string CurrentChannel { get; set; } = "Stable";
        public DateTime? PublishedAt { get; set; }
        public string SelectedAsset { get; set; } = "";
        public string Error { get; set; } = "";
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public string DisplayStatus { get; set; } = "Unknown";
    }

    public class UpdateCheckResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public EmulatorUpdateInfo? UpdateInfo { get; set; }
    }
}
