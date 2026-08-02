using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Core.Abstractions
{
    public enum InstallationFailureReason
    {
        None,
        ReleaseNotFound,
        CompatibleAssetNotFound,
        DownloadFailed,
        ValidationFailed,
        ExtractionFailed,
        ExecutableNotFound,
        EmulatorIsRunning,
        BackupFailed,
        InstallationFailed,
        RollbackFailed,
        Cancellation
    }

    public class EmulatorInstallationProgress
    {
        public string EmulatorId { get; set; } = "";
        public PackageInstallStage Stage { get; set; }
        public string CurrentStep { get; set; } = "";
        public int Percentage { get; set; }
    }

    public class InstalledEmulatorInfo
    {
        public string EmulatorId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string InstalledVersion { get; set; } = "";
        public string ReleaseTag { get; set; } = "";
        public DateTime InstalledAt { get; set; }
        public string InstallationPath { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string SourceRepository { get; set; } = "";
        public string SourceAssetName { get; set; } = "";
        public string SourceDownloadUrl { get; set; } = "";
        public long DownloadedArchiveSize { get; set; }
        public string SHA256 { get; set; } = "";
        public string Architecture { get; set; } = "";
        public string ReleaseChannel { get; set; } = "";
    }

    public enum EmulatorInstallationOperation
    {
        Install,
        Repair,
        Update,
        Reinstall,
        Uninstall
    }

    public class EmulatorInstallationRequest
    {
        public string EmulatorId { get; set; } = "";
        public string OperationId { get; set; } = Guid.NewGuid().ToString("N");
        public string? TargetReleaseTag { get; set; } // Null triggers latest release
        public EmulatorInstallationOperation Operation { get; set; } = EmulatorInstallationOperation.Install;
        public bool? UninstallKeepUserData { get; set; }
        public IProgress<EmulatorInstallationProgress>? Progress { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

    public interface IEmulatorInstallationService
    {
        Task<PackageInstallResult> InstallAsync(EmulatorInstallationRequest request);
        Task<PackageInstallResult> RepairAsync(EmulatorInstallationRequest request);
        Task<PackageInstallResult> UpdateAsync(EmulatorInstallationRequest request);
        Task<PackageInstallResult> ReinstallAsync(EmulatorInstallationRequest request);
        Task<PackageInstallResult> UninstallAsync(EmulatorInstallationRequest request);
    }
}
