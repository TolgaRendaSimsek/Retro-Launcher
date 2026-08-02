using System;
using System.Collections.Generic;

namespace RetroLauncher.Core.Models
{
    public class BiosItem
    {
        public string Console { get; set; } = "";
        public string Path { get; set; } = "";
        public string Status { get; set; } = "Missing";
        public string FileName { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string InstalledVersion { get; set; } = "";
        public string Emulator { get; set; } = "";
        public string Platform { get; set; } = "";

        // camelCase aliases for JSON serialization compatibility
        public string console { get => Console; set => Console = value; }
        public string path { get => Path; set => Path = value; }
        public string status { get => Status; set => Status = value; }
        public string fileName { get => FileName; set => FileName = value; }
        public string sha256 { get => Sha256; set => Sha256 = value; }
        public string installedVersion { get => InstalledVersion; set => InstalledVersion = value; }
        public string emulator { get => Emulator; set => Emulator = value; }
        public string platform { get => Platform; set => Platform = value; }
    }

    public class BiosConfig
    {
        public List<BiosItem> BiosItems { get; set; } = new();
    }

    public class BiosPackageInfo
    {
        public string Console { get; set; } = "";
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string ArchiveType { get; set; } = "";
        public string TargetFolder { get; set; } = "";

        // camelCase aliases for JSON serialization compatibility
        public string console { get => Console; set => Console = value; }
        public string version { get => Version; set => Version = value; }
        public string downloadUrl { get => DownloadUrl; set => DownloadUrl = value; }
        public string fileName { get => FileName; set => FileName = value; }
        public string sha256 { get => Sha256; set => Sha256 = value; }
        public string archiveType { get => ArchiveType; set => ArchiveType = value; }
        public string targetFolder { get => TargetFolder; set => TargetFolder = value; }
    }
}
