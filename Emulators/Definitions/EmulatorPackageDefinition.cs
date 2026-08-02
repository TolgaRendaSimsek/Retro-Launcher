using System;
using System.Collections.Generic;

namespace RetroLauncher.Emulators.Definitions
{
    public enum EmulatorReleaseChannel
    {
        Stable,
        Beta,
        Nightly,
        Dev
    }

    public enum EmulatorReleaseSourceType
    {
        GitHubLatestRelease,
        GitHubReleaseList,
        GitHubRollingTag,
        GitHubBinaryRepository,
        LocalCustom
    }

    public class EmulatorPackageDefinition
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string GitHubOwner { get; set; } = "";
        public string GitHubRepository { get; set; } = "";
        public List<string> SupportedPlatforms { get; set; } = new();
        public List<string> IncludeAssetPatterns { get; set; } = new();
        public List<string> ExcludeAssetPatterns { get; set; } = new();
        public List<string> SupportedArchiveTypes { get; set; } = new();
        public List<string> ExecutableCandidates { get; set; } = new();
        public string InstallDirectoryName { get; set; } = "";
        public string LaunchArgumentsTemplate { get; set; } = "";
        public bool RequiresBios { get; set; }
        public List<string> BiosDirectoryCandidates { get; set; } = new();
        public EmulatorReleaseChannel ReleaseChannel { get; set; } = EmulatorReleaseChannel.Stable;
        public EmulatorReleaseSourceType ReleaseSourceType { get; set; } = EmulatorReleaseSourceType.GitHubLatestRelease;
        public List<string> PreservedDirectories { get; set; } = new();
        public List<string> PreservedFiles { get; set; } = new();

        // Extra metadata preserved for UI/Launcher compatibility
        public string ConsoleName { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> SupportedRomExtensions { get; set; } = new();
        public bool RequiresFirmware { get; set; }
        public string OfficialProjectUrl { get; set; } = "";
        public string OfficialDownloadUrl { get; set; } = "";
        public string LicenseNoticeUrl { get; set; } = "";
        public bool SupportsPortableMode { get; set; }
        public bool DefaultEnabled { get; set; } = true;
    }
}
