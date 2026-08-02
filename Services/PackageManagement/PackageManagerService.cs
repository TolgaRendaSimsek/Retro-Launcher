using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Services.PackageManagement
{
    public class PackageManagerService
    {
        private static PackageManagerService? _instance;
        public static PackageManagerService Instance => _instance ??= new PackageManagerService();

        private readonly IPackageRepository _repository;
        private readonly IPackageDownloader _downloader;
        private readonly IPackageVerifier _verifier;
        private readonly List<IArchiveExtractor> _extractors;
        private readonly IPackageUpdateService _updateService;

        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "package_manager.log");
        private static readonly object LogLock = new object();

        public PackageManagerService(
            IPackageRepository? repository = null,
            IPackageDownloader? downloader = null,
            IPackageVerifier? verifier = null,
            IEnumerable<IArchiveExtractor>? extractors = null,
            IPackageUpdateService? updateService = null)
        {
            _repository = repository ?? new JsonInstalledPackageRepository();
            _downloader = downloader ?? new HttpPackageDownloader();
            _verifier = verifier ?? new Sha256PackageVerifier();
            _extractors = extractors?.ToList() ?? new List<IArchiveExtractor>
            {
                new SecureArchiveExtractor()
            };
            _updateService = updateService ?? new PackageUpdateService();
        }

        // --- Logging ---
        public void Log(string message, string level = "INFO")
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                lock (LogLock)
                {
                    File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}");
                }
            }
            catch { }
        }

        // --- Catalog & Updates ---
        public IPackageRepository Repository => _repository;
        public IPackageUpdateService UpdateService => _updateService;

        public bool VerifyHealth(string packageId)
        {
            var record = _repository.GetById(packageId);
            if (record == null) return false;

            foreach (var relativeFile in record.verificationFiles)
            {
                string fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, record.installedPath, relativeFile));
                if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
                {
                    if (record.status != PackageStatus.Broken)
                    {
                        record.status = PackageStatus.Broken;
                        _repository.Save();
                    }
                    return false;
                }
            }

            if (record.status == PackageStatus.Broken)
            {
                record.status = PackageStatus.Installed;
                _repository.Save();
            }
            return true;
        }

        // --- Operations ---
        public async Task<bool> InstallPackageAsync(PackageManifest package, IProgress<int>? progress, CancellationToken token)
        {
            Log($"Starting install task for: {package.name} (Version: {package.version})");
            string? tempFile = null;

            try
            {
                progress?.Report(5);
                token.ThrowIfCancellationRequested();

                // 1. Download
                Log($"Downloading archive from: {package.downloadUrl}");
                var downloadProgress = new Progress<PackageDownloadProgress>(p =>
                {
                    progress?.Report(5 + (int)(p.Percentage * 0.70));
                });
                string operationId = Guid.NewGuid().ToString("N");
                tempFile = await _downloader.DownloadAsync(package.downloadUrl, downloadProgress, token, package.id, operationId, package.fileName);

                // 2. Verify Checksum
                progress?.Report(80);
                token.ThrowIfCancellationRequested();
                Log("Checking SHA256 integrity...");
                bool verified = await _verifier.VerifyAsync(tempFile, package.sha256, token);
                if (!verified)
                {
                    throw new InvalidDataException("SHA256 signature verification failed.");
                }

                // 3. Extract & Register
                bool ok = await ExtractAndRegisterAsync(tempFile, package, progress, token);
                if (ok)
                {
                    Log($"Package {package.name} installed successfully.");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"Installation of {package.name} failed: {ex.Message}", "ERROR");
                return false;
            }
            finally
            {
                try
                {
                    if (tempFile != null && File.Exists(tempFile)) File.Delete(tempFile);
                }
                catch { }
            }
        }

        public async Task<bool> RepairPackageAsync(PackageManifest package, IProgress<int>? progress, CancellationToken token)
        {
            Log($"Repairing package: {package.name}");
            return await InstallPackageAsync(package, progress, token);
        }

        public async Task<bool> RemovePackageAsync(string packageId, CancellationToken token)
        {
            Log($"Removing package ID: {packageId}");
            token.ThrowIfCancellationRequested();

            var record = _repository.GetById(packageId);
            if (record == null)
            {
                Log($"Package {packageId} not registered in local repository.", "WARNING");
                return false;
            }

            try
            {
                string targetDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, record.installedPath));
                if (Directory.Exists(targetDir))
                {
                    DeleteDirectoryPreservingFiles(targetDir, record.preservedPaths);
                }

                _repository.Remove(packageId);
                Log($"Package {packageId} removed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Failed to remove package {packageId}: {ex.Message}", "ERROR");
                return false;
            }
        }

        public async Task<bool> InstallManualPackageAsync(string archivePath, PackageManifest metadata, IProgress<int>? progress, CancellationToken token)
        {
            Log($"Manual installation from archive: {archivePath}");
            try
            {
                if (!File.Exists(archivePath))
                {
                    throw new FileNotFoundException("Local file not found.");
                }

                progress?.Report(20);
                bool ok = await ExtractAndRegisterAsync(archivePath, metadata, progress, token);
                if (ok)
                {
                    Log($"Manual package {metadata.name} installed successfully.");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"Manual installation of {metadata.name} failed: {ex.Message}", "ERROR");
                return false;
            }
        }

        // --- Internals ---
        private async Task<bool> ExtractAndRegisterAsync(string archivePath, PackageManifest package, IProgress<int>? progress, CancellationToken token)
        {
            string baseFolder = GetFolderForType(package.packageType);
            string destDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseFolder, package.installFolder));
            
            // Backup files configured to preserve
            string backupDir = Path.Combine(Path.GetTempPath(), $"Preserve_Backup_{package.id}_{Guid.NewGuid():N}");
            bool hasBackup = false;

            if (Directory.Exists(destDir))
            {
                Directory.CreateDirectory(backupDir);
                foreach (var relativeFile in package.preservedPaths)
                {
                    string source = Path.Combine(destDir, relativeFile);
                    if (File.Exists(source))
                    {
                        string dest = Path.Combine(backupDir, relativeFile);
                        string? parent = Path.GetDirectoryName(dest);
                        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent)) Directory.CreateDirectory(parent);
                        File.Copy(source, dest, true);
                        hasBackup = true;
                    }
                    else if (Directory.Exists(source))
                    {
                        string dest = Path.Combine(backupDir, relativeFile);
                        CopyDirectory(source, dest);
                        hasBackup = true;
                    }
                }
            }

            // Select Extractor
            var extractor = _extractors.FirstOrDefault(e => e.CanExtract(package.archiveType));
            if (extractor == null)
            {
                throw new NotSupportedException($"Archive type '{package.archiveType}' is not supported.");
            }

            progress?.Report(85);
            token.ThrowIfCancellationRequested();
            Log($"Extracting archive to: {destDir}");

            var extractRequest = new ArchiveExtractionRequest
            {
                ArchivePath = archivePath,
                DestinationPath = destDir,
                CancellationToken = token,
                Progress = new Progress<ArchiveExtractionProgress>(p => progress?.Report(85 + (int)(p.Percentage * 0.10))),
                ExecutableCandidates = !string.IsNullOrEmpty(package.executablePath) ? new List<string> { package.executablePath } : new List<string>(),
                PackageId = package.id,
                OperationId = Guid.NewGuid().ToString("N"),
                ExpectedSize = package.downloadSize > 0 ? package.downloadSize : null
            };

            var extractResult = await extractor.ExtractAsync(extractRequest);
            if (!extractResult.Success)
            {
                throw new Exception($"Extraction failed: {extractResult.ErrorMessage}");
            }

            // Restore backed-up user settings
            if (hasBackup && Directory.Exists(backupDir))
            {
                Log("Restoring preserved user configurations...");
                CopyDirectory(backupDir, destDir);
                try
                {
                    Directory.Delete(backupDir, true);
                }
                catch { }
            }

            progress?.Report(95);
            token.ThrowIfCancellationRequested();

            // Update Repository
            var record = _repository.GetById(package.id);
            if (record == null)
            {
                record = new InstalledPackage { packageId = package.id };
            }

            record.installedVersion = package.version;
            record.installedPath = Path.Combine(baseFolder, package.installFolder).Replace('\\', '/');
            record.installedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            record.executablePath = package.executablePath;
            record.status = PackageStatus.Installed;
            record.sourceUrl = package.downloadUrl;
            record.verificationFiles = package.supportedPlatforms.Concat(new[] { package.executablePath }).Where(s => !string.IsNullOrEmpty(s)).ToList();
            record.preservedPaths = package.preservedPaths;

            _repository.AddOrUpdate(record);
            progress?.Report(100);
            return true;
        }

        private static string GetFolderForType(PackageType type)
        {
            return type switch
            {
                PackageType.Emulator => "Emulators",
                PackageType.Theme => "Themes",
                PackageType.Shader => "Shaders",
                PackageType.Mod => "Mods",
                PackageType.LanguagePack => "Languages",
                PackageType.Plugin => "Plugins",
                PackageType.Tool => "Tools",
                PackageType.Firmware => "Bios",
                _ => "Packages"
            };
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;

            if (!Directory.Exists(destinationDir)) Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string targetSubDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, targetSubDir);
            }
        }

        private void DeleteDirectoryPreservingFiles(string dir, List<string> preservePaths)
        {
            var root = new DirectoryInfo(dir);
            if (!root.Exists) return;

            foreach (FileInfo file in root.GetFiles())
            {
                string rel = Path.GetRelativePath(dir, file.FullName).Replace('\\', '/');
                if (!IsPreserved(rel, preservePaths))
                {
                    try { file.Delete(); } catch { }
                }
            }

            foreach (DirectoryInfo sub in root.GetDirectories())
            {
                string rel = Path.GetRelativePath(dir, sub.FullName).Replace('\\', '/');
                if (!IsPreserved(rel, preservePaths))
                {
                    try
                    {
                        sub.Delete(true);
                    }
                    catch
                    {
                        DeleteDirectoryPreservingFiles(sub.FullName, preservePaths.Select(p => p.Substring(rel.Length).TrimStart('/')).ToList());
                    }
                }
            }
        }

        private bool IsPreserved(string path, List<string> preservePaths)
        {
            foreach (var p in preservePaths)
            {
                if (string.Equals(path, p, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase) ||
                    p.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
