using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public interface IPackageCatalogProvider
    {
        Task<List<PackageManifest>> GetCatalogAsync(string source, CancellationToken token);
    }

    public interface IPackageDownloader
    {
        Task<string> DownloadAsync(string url, IProgress<int>? progress, CancellationToken token);
    }

    public interface IPackageVerifier
    {
        Task<bool> VerifyAsync(string filePath, string expectedHash, CancellationToken token);
    }

    public interface IPackageRepository
    {
        void Load();
        void Save();
        List<InstalledPackage> GetAll();
        InstalledPackage? GetById(string packageId);
        void AddOrUpdate(InstalledPackage package);
        void Remove(string packageId);
    }

    public interface IPackageUpdateService
    {
        bool IsUpdateAvailable(string currentVersion, string latestVersion);
    }

    public interface IPackageInstaller
    {
        Task<bool> InstallAsync(PackageManifest package, IProgress<int>? progress, CancellationToken token);
        Task<bool> RepairAsync(PackageManifest package, IProgress<int>? progress, CancellationToken token);
        Task<bool> RemoveAsync(string packageId, CancellationToken token);
        Task<bool> InstallManualAsync(string archivePath, PackageManifest metadata, IProgress<int>? progress, CancellationToken token);
        bool VerifyHealth(string packageId);
    }
}
