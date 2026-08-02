using System;

namespace RetroLauncher.Core.Models
{
    public class DuckStationPackageInfo
    {
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string ArchiveType { get; set; } = "";
        public string ExecutablePath { get; set; } = "";

        // camelCase aliases for JSON serialization compatibility
        public string version { get => Version; set => Version = value; }
        public string downloadUrl { get => DownloadUrl; set => DownloadUrl = value; }
        public string fileName { get => FileName; set => FileName = value; }
        public string sha256 { get => Sha256; set => Sha256 = value; }
        public string archiveType { get => ArchiveType; set => ArchiveType = value; }
        public string executablePath { get => ExecutablePath; set => ExecutablePath = value; }
    }
}
