using System;
using System.Collections.Generic;

namespace RetroLauncher
{
    public enum EmulatorReleaseSourceType
    {
        GitHubLatestRelease,
        GitHubReleaseList,
        GitHubRollingTag,
        GitHubBinaryRepository,
        OfficialDownloadMetadata,
        ManualOnly
    }

    public enum EmulatorReleaseChannel
    {
        Stable,
        Nightly,
        Dev
    }

    public enum SupportedOperatingSystem
    {
        Windows,
        Linux,
        macOS
    }

    public enum CpuArchitecture
    {
        X64,
        X86,
        Arm64
    }

    public enum EmulatorArchiveType
    {
        Zip,
        SevenZip,
        TarGz,
        Exe
    }

    public class EmulatorDefinition
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string ConsoleName { get; set; } = "";
        public string Description { get; set; } = "";
        public string RepositoryOwner { get; set; } = "";
        public string RepositoryName { get; set; } = "";
        public EmulatorReleaseSourceType ReleaseSourceType { get; set; } = EmulatorReleaseSourceType.GitHubLatestRelease;
        public EmulatorReleaseChannel ReleaseChannel { get; set; } = EmulatorReleaseChannel.Stable;
        public List<SupportedOperatingSystem> SupportedOperatingSystems { get; set; } = new();
        public List<CpuArchitecture> SupportedArchitectures { get; set; } = new();
        public List<string> SupportedRomExtensions { get; set; } = new();
        public string InstallationDirectoryName { get; set; } = "";
        public List<string> ExecutableCandidates { get; set; } = new();
        public List<string> AssetSelectionRules { get; set; } = new();
        public EmulatorArchiveType ArchiveType { get; set; } = EmulatorArchiveType.Zip;
        public bool RequiresBios { get; set; }
        public bool RequiresFirmware { get; set; }
        public string OfficialProjectUrl { get; set; } = "";
        public string OfficialDownloadUrl { get; set; } = "";
        public string LicenseNoticeUrl { get; set; } = "";
        public string LaunchArgumentTemplate { get; set; } = "";
        public bool SupportsPortableMode { get; set; }
        public bool DefaultEnabled { get; set; } = true;
    }
}
