using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class EmulatorInstallationService : IEmulatorInstallationService
    {
        private readonly IEmulatorPackageDefinitionProvider _definitionProvider;
        private readonly IReleaseProvider _releaseProvider;
        private readonly IReleaseAssetSelector _assetSelector;
        private readonly IDownloadManager _downloadManager;
        private readonly IArchiveExtractor _archiveExtractor;
        private readonly IEmuPackageVerifier _packageVerifier;

        public EmulatorInstallationService(
            IEmulatorPackageDefinitionProvider? definitionProvider = null,
            IReleaseProvider? releaseProvider = null,
            IReleaseAssetSelector? assetSelector = null,
            IDownloadManager? downloadManager = null,
            IArchiveExtractor? archiveExtractor = null,
            IEmuPackageVerifier? packageVerifier = null)
        {
            _definitionProvider = definitionProvider ?? new JsonEmulatorPackageDefinitionProvider();
            _releaseProvider = releaseProvider ?? new GitHubReleaseProvider();
            _assetSelector = assetSelector ?? new ReleaseAssetSelector();
            _downloadManager = downloadManager ?? new DownloadManager();
            _archiveExtractor = archiveExtractor ?? new SecureArchiveExtractor();
            _packageVerifier = packageVerifier ?? new EmuPackageVerifier();
        }

        public async Task<PackageInstallResult> InstallAsync(EmulatorInstallationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Step 1: Resolve the emulator definition
            ReportProgress(request.Progress, request.EmulatorId, "Resolving emulator definition", 5);
            var definition = _definitionProvider.GetById(request.EmulatorId);
            if (definition == null)
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = request.EmulatorId,
                    FailedStage = PackageInstallStage.ResolvingRelease,
                    ErrorMessage = $"Emulator definition '{request.EmulatorId}' not found."
                };
            }

            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(x => string.Equals(x.Id, definition.Id, StringComparison.OrdinalIgnoreCase));
            if (emuItem != null && !string.IsNullOrEmpty(emuItem.ReleaseChannel))
            {
                if (Enum.TryParse<EmulatorReleaseChannel>(emuItem.ReleaseChannel, out var parsedChannel))
                {
                    definition.ReleaseChannel = parsedChannel;
                }
            }

            // Step 2: Check whether the emulator is currently running
            ReportProgress(request.Progress, request.EmulatorId, "Checking running processes", 10);
            foreach (var exeCandidate in definition.ExecutableCandidates)
            {
                string processName = Path.GetFileNameWithoutExtension(exeCandidate);
                var activeProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
                if (activeProcesses.Any())
                {
                    return new PackageInstallResult
                    {
                        Success = false,
                        PackageId = definition.Id,
                        FailedStage = PackageInstallStage.ResolvingRelease,
                        ErrorMessage = $"Cannot update '{definition.DisplayName}' because the emulator '{processName}' is currently running. Please close the application and retry."
                    };
                }
            }

            // Step 3: Fetch latest or specific release
            ReportProgress(request.Progress, request.EmulatorId, "Fetching official release information", 15);
            ReleaseInfo? selectedRelease = null;

            var query = new ReleaseQuery
            {
                Owner = definition.GitHubOwner,
                Repository = definition.GitHubRepository,
                Channel = definition.ReleaseChannel == EmulatorReleaseChannel.Stable ? ReleaseChannel.Stable : ReleaseChannel.Preview
            };

            if (!string.IsNullOrEmpty(request.TargetReleaseTag))
            {
                query.Tag = request.TargetReleaseTag;
                var releaseRes = await _releaseProvider.GetReleaseByTagAsync(query, request.CancellationToken);
                if (releaseRes.Success) selectedRelease = releaseRes.Data;
            }
            else
            {
                var latestRes = await _releaseProvider.GetLatestReleaseAsync(query, request.CancellationToken);
                if (latestRes.Success) selectedRelease = latestRes.Data;
            }

            if (selectedRelease == null)
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.ResolvingRelease,
                    ErrorMessage = $"Unable to retrieve release tag for '{definition.DisplayName}' from repository '{definition.GitHubOwner}/{definition.GitHubRepository}'."
                };
            }

            // Step 4: Select compatible asset
            var newSelector = (IReleaseAssetSelectorNew)_assetSelector;
            var selectorResult = newSelector.SelectAsset(definition, selectedRelease);
            if (!selectorResult.Success || selectorResult.SelectedAsset == null)
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.SelectingAsset,
                    ErrorMessage = $"Could not identify a compatible Windows package for '{definition.DisplayName}': {selectorResult.Message}"
                };
            }

            var asset = selectorResult.SelectedAsset;

            // Step 5: Download package to temp dir
            ReportProgress(request.Progress, request.EmulatorId, "Downloading package archive", 20);
            string tempDir = Path.Combine(AppContext.BaseDirectory, ApplicationSettingsService.Instance.Download.DownloadTempDir);
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string archivePath = Path.Combine(tempDir, $"{definition.Id}_{Guid.NewGuid():N}{Path.GetExtension(asset.Name)}");

            var downloadProgress = new Progress<DownloadProgress>(p =>
            {
                int pct = 20 + (int)((p.Percentage >= 0 ? p.Percentage : 0) * 0.50);
                ReportProgress(request.Progress, request.EmulatorId, $"Downloading: {(p.BytesDownloaded / 1024.0 / 1024.0):F1} MB", pct);
            });

            var downloadReq = new DownloadRequest
            {
                EmulatorId = definition.Id,
                Url = asset.DownloadUrl,
                DestinationPath = archivePath,
                ExpectedSize = asset.Size,
                Progress = downloadProgress,
                CancellationToken = request.CancellationToken
            };

            var downloadResult = await _downloadManager.DownloadAsync(downloadReq);
            if (!downloadResult.Success)
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = $"Download failed: {downloadResult.ErrorMessage}"
                };
            }

            // Check if downloaded archive exists and is non-empty
            if (!File.Exists(archivePath))
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = "Downloaded archive file does not exist."
                };
            }

            if (new FileInfo(archivePath).Length <= 0)
            {
                CleanFile(archivePath);
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = "Downloaded archive file is empty."
                };
            }

            // Step 6: Validate integrity via IPackageVerifier
            ReportProgress(request.Progress, request.EmulatorId, "Verifying downloaded archive", 75);
            var verifyResult = await _packageVerifier.VerifyPackageAsync(archivePath, asset.Size, asset.Sha256, request.CancellationToken);
            if (!verifyResult.Success)
            {
                CleanFile(archivePath);
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.ValidatingDownload,
                    ErrorMessage = $"Package verification failed: {verifyResult.Message}"
                };
            }

            string calculatedHash = verifyResult.CalculatedHash ?? "";

            // Step 7: Extract and Deploy (Transactional staging, backup, normalization, and rollback)
            ReportProgress(request.Progress, request.EmulatorId, "Extracting package contents", 80);
            string finalDestPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, definition.InstallDirectoryName));

            var extractionProgress = new Progress<ArchiveExtractionProgress>(p =>
            {
                int pct = 80 + (int)(p.Percentage * 0.15); // Scale extraction progress to 80%-95%
                ReportProgress(request.Progress, request.EmulatorId, "Extracting package contents", pct);
            });

            var extractionReq = new ArchiveExtractionRequest
            {
                ArchivePath = archivePath,
                DestinationPath = finalDestPath,
                CancellationToken = request.CancellationToken,
                Progress = extractionProgress,
                ExecutableCandidates = definition.ExecutableCandidates,
                PackageId = definition.Id,
                OperationId = Guid.NewGuid().ToString("N"),
                ExpectedSize = asset.Size
            };

            var extractionResult = await _archiveExtractor.ExtractAsync(extractionReq);
            CleanFile(archivePath); // Done with downloaded archive file

            if (!extractionResult.Success)
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Extracting,
                    ErrorMessage = $"Extraction failed: {extractionResult.ErrorMessage}"
                };
            }

            string finalExePath = extractionResult.MainExecutablePath ?? "";
            if (string.IsNullOrEmpty(finalExePath) || !File.Exists(finalExePath))
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.LocatingExecutable,
                    ErrorMessage = "Located executable is missing inside deployed folder."
                };
            }

            // Security & correctness check: Ensure executable resides inside the destination folder
            string canonicalDest = Path.GetFullPath(finalDestPath) + Path.DirectorySeparatorChar;
            string canonicalExe = Path.GetFullPath(finalExePath);

            if (!canonicalExe.StartsWith(canonicalDest, StringComparison.OrdinalIgnoreCase))
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.LocatingExecutable,
                    ErrorMessage = "Deployment validation failed: executable is located outside the intended emulator directory."
                };
            }

            // Read version metadata
            string installedVersion = "Unknown";
            try
            {
                var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(finalExePath);
                installedVersion = fileVersion.ProductVersion ?? fileVersion.FileVersion ?? selectedRelease.Tag ?? "Unknown";
            }
            catch { }

            // Step 10: Update registry records
            var infoRecord = new InstalledEmulatorInfo
            {
                EmulatorId = definition.Id,
                DisplayName = definition.DisplayName,
                InstalledVersion = installedVersion,
                ReleaseTag = selectedRelease.Tag,
                InstalledAt = DateTime.UtcNow,
                InstallationPath = finalDestPath,
                ExecutablePath = finalExePath,
                SourceRepository = $"{definition.GitHubOwner}/{definition.GitHubRepository}",
                SourceAssetName = asset.Name,
                SourceDownloadUrl = asset.DownloadUrl,
                DownloadedArchiveSize = asset.Size,
                SHA256 = calculatedHash,
                Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
                ReleaseChannel = definition.ReleaseChannel.ToString()
            };

            // Write install.json inside target directory
            WriteInstallationManifest(finalDestPath, infoRecord);

            // Update emulators.json config and verify it succeeded
            bool registrySaved = UpdateLauncherRegistry(infoRecord, definition);
            if (!registrySaved)
            {
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Registering,
                    ErrorMessage = "Failed to update and save the emulator registration in the local config registry."
                };
            }

            ReportProgress(request.Progress, request.EmulatorId, "Installation complete", 100);

            return new PackageInstallResult
            {
                Success = true,
                PackageId = definition.Id,
                Version = installedVersion,
                InstallDirectory = finalDestPath,
                ExecutablePath = finalExePath,
                FailedStage = PackageInstallStage.Completed
            };
        }

        private static void ReportProgress(IProgress<EmulatorInstallationProgress>? progress, string id, string step, int pct)
        {
            progress?.Report(new EmulatorInstallationProgress
            {
                EmulatorId = id,
                CurrentStep = step,
                Percentage = pct
            });
        }

        private static void RestoreUserFolders(string sourceBackup, string destFolder)
        {
            string[] userDirs = { 
                "bios", "saves", "configs", "screenshots", "games", "roms",
                "dev_hdd0", "dev_flash", "GuiConfigs", "cache" 
            };
            foreach (var dirName in userDirs)
            {
                string src = Path.Combine(sourceBackup, dirName);
                string dst = Path.Combine(destFolder, dirName);
                if (Directory.Exists(src))
                {
                    try
                    {
                        if (Directory.Exists(dst))
                        {
                            if (string.Equals(dirName, "cache", StringComparison.OrdinalIgnoreCase))
                            {
                                try { Directory.Delete(dst, true); } catch { }
                                Directory.Move(src, dst);
                            }
                            else
                            {
                                CopyDirectoryRecursively(src, dst);
                            }
                        }
                        else
                        {
                            Directory.Move(src, dst);
                        }
                    }
                    catch (Exception ex)
                    {
                        RetroLogger.Log($"Failed to restore user folder '{dirName}' from backup: {ex.Message}", "WARNING");
                    }
                }
            }

            string[] configFiles = { "config.yml", "games.yml", "portable.txt" };
            foreach (var file in configFiles)
            {
                string srcFile = Path.Combine(sourceBackup, file);
                string dstFile = Path.Combine(destFolder, file);
                if (File.Exists(srcFile))
                {
                    try
                    {
                        File.Copy(srcFile, dstFile, true);
                    }
                    catch (Exception ex)
                    {
                        RetroLogger.Log($"Failed to restore config file '{file}' from backup: {ex.Message}", "WARNING");
                    }
                }
            }
        }

        private static void CopyDirectoryRecursively(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectoryRecursively(subDir, destSubDir);
            }
        }

        private static void WriteInstallationManifest(string targetPath, InstalledEmulatorInfo info)
        {
            try
            {
                string manifestPath = Path.Combine(targetPath, "install.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(info, options);
                File.WriteAllText(manifestPath, json);
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Failed to write manifest record: {ex.Message}", "WARNING");
            }
        }

        private static bool UpdateLauncherRegistry(InstalledEmulatorInfo info, EmulatorPackageDefinition definition)
        {
            try
            {
                var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(x => string.Equals(x.Id, info.EmulatorId, StringComparison.OrdinalIgnoreCase));
                if (emu != null)
                {
                    emu.InstalledVersion = info.InstalledVersion;
                    emu.ExecutablePath = Path.GetRelativePath(AppContext.BaseDirectory, info.ExecutablePath).Replace('\\', '/');
                    emu.InstallFolder = Path.GetRelativePath(AppContext.BaseDirectory, info.InstallationPath).Replace('\\', '/');
                    emu.Status = "Installed";
                    emu.SelectedAssetName = info.SourceAssetName;
                    emu.GithubRepository = info.SourceRepository;
                    emu.InstallationTimestamp = info.InstalledAt;
                    EmulatorManager.Instance.SaveEmulators();
                    RetroLogger.Log($"Updated Launcher Config for '{info.EmulatorId}' to tag '{info.ReleaseTag}'.");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Failed to update emulators.json: {ex.Message}", "WARNING");
                return false;
            }
        }

        private static void CleanFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }

        private static void CleanDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch { }
        }
    }
}
