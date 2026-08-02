using System;

namespace RetroLauncher.Core.Models
{
    public class PackageDownloadProgress
    {
        public long BytesDownloaded { get; set; }
        public long? TotalBytes { get; set; }
        public int Percentage { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public string CurrentStage { get; set; } = "";
    }
}
