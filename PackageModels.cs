using System;
using System.Collections.Generic;

namespace RetroLauncher
{
    public enum PackageType
    {
        Emulator,
        Theme,
        Shader,
        Mod,
        LanguagePack,
        Plugin,
        Tool,
        Firmware
    }

    public enum PackageStatus
    {
        NotInstalled,
        Downloading,
        Installing,
        Installed,
        UpdateAvailable,
        Broken,
        Disabled,
        Failed
    }

    public class PackageManifest
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public PackageType packageType { get; set; } = PackageType.Mod;
        public string version { get; set; } = "1.0.0";
        public string author { get; set; } = "";
        public string repositoryUrl { get; set; } = "";
        public string downloadUrl { get; set; } = "";
        public string fileName { get; set; } = "";
        public string archiveType { get; set; } = "zip";
        public string sha256 { get; set; } = "";
        public long downloadSize { get; set; } = 0;
        public string installFolder { get; set; } = "";
        public string executablePath { get; set; } = "";
        public List<string> supportedPlatforms { get; set; } = new();
        public List<string> dependencies { get; set; } = new();
        public List<string> preservedPaths { get; set; } = new();
        public string launchArguments { get; set; } = "";
        public string releaseNotes { get; set; } = "";
        public bool isEnabled { get; set; } = true;
    }

    public class InstalledPackage
    {
        public string packageId { get; set; } = "";
        public string installedVersion { get; set; } = "";
        public string installedPath { get; set; } = "";
        public string installedAt { get; set; } = "";
        public string executablePath { get; set; } = "";
        public PackageStatus status { get; set; } = PackageStatus.NotInstalled;
        public string sourceUrl { get; set; } = "";
        
        // Metadata fields kept for health checking and updates
        public List<string> verificationFiles { get; set; } = new();
        public List<string> preservedPaths { get; set; } = new();
    }

    public class InstalledPackagesConfig
    {
        public List<InstalledPackage> InstalledPackages { get; set; } = new();
    }
}
