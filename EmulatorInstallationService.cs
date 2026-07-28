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

            string opId = request.OperationId;
            EmulatorInstallDiagnosticsLogger.StartSession(opId, request.EmulatorId);

            PackageInstallResult? result = null;
            try
            {
                result = await InstallInternalAsync(request);
                return result;
            }
            catch (Exception ex)
            {
                EmulatorInstallDiagnosticsLogger.SetException(opId, ex);
                result = new PackageInstallResult
                {
                    Success = false,
                    PackageId = request.EmulatorId,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
                return result;
            }
            finally
            {
                bool success = result != null && result.Success;
                string msg = result?.ErrorMessage ?? "";
                EmulatorInstallDiagnosticsLogger.CompleteSession(opId, success, msg);
            }
        }

        private async Task<PackageInstallResult> InstallInternalAsync(EmulatorInstallationRequest request)
        {
            string opId = request.OperationId;

            // Step 1: Resolve the emulator definition
            ReportProgress(request, PackageInstallStage.ResolvingRelease, "Resolving GitHub release...", 5);
            var definition = _definitionProvider.GetById(request.EmulatorId);
            if (definition == null)
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, $"Error: Emulator definition '{request.EmulatorId}' not found.");
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
            ReportProgress(request, PackageInstallStage.ResolvingRelease, "Resolving GitHub release...", 10);
            foreach (var exeCandidate in definition.ExecutableCandidates)
            {
                string processName = Path.GetFileNameWithoutExtension(exeCandidate);
                var activeProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
                if (activeProcesses.Any())
                {
                    EmulatorInstallDiagnosticsLogger.LogToSession(opId, $"Error: Cannot update '{definition.DisplayName}' because the emulator '{processName}' is currently running.");
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
            ReportProgress(request, PackageInstallStage.ResolvingRelease, "Resolving GitHub release...", 15);
            ReleaseInfo? selectedRelease = null;

            var query = new ReleaseQuery
            {
                Owner = definition.GitHubOwner,
                Repository = definition.GitHubRepository,
                Channel = definition.ReleaseChannel == EmulatorReleaseChannel.Stable ? ReleaseChannel.Stable : ReleaseChannel.Preview
            };

            string apiEndpoint = $"https://api.github.com/repos/{definition.GitHubOwner}/{definition.GitHubRepository}/releases";
            if (!string.IsNullOrEmpty(request.TargetReleaseTag))
            {
                apiEndpoint += $"/tags/{request.TargetReleaseTag}";
                EmulatorInstallDiagnosticsLogger.AddGitHubApiEndpoint(opId, apiEndpoint);
                query.Tag = request.TargetReleaseTag;
                var releaseRes = await _releaseProvider.GetReleaseByTagAsync(query, request.CancellationToken);
                if (releaseRes.Success) selectedRelease = releaseRes.Data;
            }
            else
            {
                apiEndpoint += "/latest";
                EmulatorInstallDiagnosticsLogger.AddGitHubApiEndpoint(opId, apiEndpoint);
                var latestRes = await _releaseProvider.GetLatestReleaseAsync(query, request.CancellationToken);
                if (latestRes.Success) selectedRelease = latestRes.Data;
            }

            if (selectedRelease == null)
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, $"Error: Unable to retrieve release for '{definition.DisplayName}' from repository '{definition.GitHubOwner}/{definition.GitHubRepository}'.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.ResolvingRelease,
                    ErrorMessage = $"Unable to retrieve release tag for '{definition.DisplayName}' from repository '{definition.GitHubOwner}/{definition.GitHubRepository}'."
                };
            }

            EmulatorInstallDiagnosticsLogger.SetReleaseTag(opId, selectedRelease.Tag);

            // Step 4: Select compatible asset
            ReportProgress(request, PackageInstallStage.SelectingAsset, "Selecting Windows package...", 18);
            var newSelector = (IReleaseAssetSelectorNew)_assetSelector;
            var selectorResult = newSelector.SelectAsset(definition, selectedRelease);

            if (selectedRelease.Assets != null)
            {
                EmulatorInstallDiagnosticsLogger.SetCandidateAssetNames(opId, selectedRelease.Assets.Select(a => a.Name));
            }

            if (!selectorResult.Success || selectorResult.SelectedAsset == null)
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, "Error: No compatible Windows x64 package was found in the latest GitHub release.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.SelectingAsset,
                    ErrorMessage = "No compatible Windows x64 package was found in the latest GitHub release.",
                    Version = selectedRelease.Tag
                };
            }

            var asset = selectorResult.SelectedAsset;
            EmulatorInstallDiagnosticsLogger.SetSelectedAssetAndScore(opId, asset.Name, 1000);

            // Step 5: Download package to temp dir
            ReportProgress(request, PackageInstallStage.Downloading, "Downloading 0.0 MB...", 20);
            string tempDir = Path.Combine(AppContext.BaseDirectory, ApplicationSettingsService.Instance.Download.DownloadTempDir);
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string archivePath = Path.Combine(tempDir, $"{definition.Id}_{Guid.NewGuid():N}{Path.GetExtension(asset.Name)}");
            EmulatorInstallDiagnosticsLogger.SetTemporaryFilePath(opId, archivePath);
            EmulatorInstallDiagnosticsLogger.SetExpectedSize(opId, asset.Size);

            var downloadProgress = new Progress<DownloadProgress>(p =>
            {
                int pct = 20 + (int)((p.Percentage >= 0 ? p.Percentage : 0) * 0.50);
                double downloadedMb = p.BytesDownloaded / 1024.0 / 1024.0;
                if (p.TotalBytes.HasValue)
                {
                    double totalMb = p.TotalBytes.Value / 1024.0 / 1024.0;
                    ReportProgress(request, PackageInstallStage.Downloading, $"Downloading {downloadedMb:F1} MB / {totalMb:F1} MB...", pct);
                }
                else
                {
                    ReportProgress(request, PackageInstallStage.Downloading, $"Downloading {downloadedMb:F1} MB...", pct);
                }
            });

            var downloadReq = new DownloadRequest
            {
                EmulatorId = definition.Id,
                OperationId = request.OperationId,
                Url = asset.DownloadUrl,
                DestinationPath = archivePath,
                ExpectedSize = asset.Size,
                Progress = downloadProgress,
                CancellationToken = request.CancellationToken
            };

            var downloadResult = await _downloadManager.DownloadAsync(downloadReq);
            if (!downloadResult.Success)
            {
                CleanFile(archivePath);
                EmulatorInstallDiagnosticsLogger.SetHttpStatusCode(opId, downloadResult.StatusCode.HasValue ? (int)downloadResult.StatusCode.Value : 0);
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, $"Error: Download failed. Message: {downloadResult.ErrorMessage}");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = $"Download failed: {downloadResult.ErrorMessage}",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    HttpStatusCode = downloadResult.StatusCode,
                    Version = selectedRelease.Tag
                };
            }

            // Check if downloaded archive exists and is non-empty
            if (!File.Exists(archivePath))
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, "Error: Downloaded archive file does not exist.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = "Downloaded archive file does not exist.",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    Version = selectedRelease.Tag
                };
            }

            long archiveSize = new FileInfo(archivePath).Length;
            EmulatorInstallDiagnosticsLogger.SetDownloadedSize(opId, archiveSize);

            if (archiveSize <= 0)
            {
                CleanFile(archivePath);
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, "Error: Downloaded archive file is empty.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = "Downloaded archive file is empty.",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    Version = selectedRelease.Tag
                };
            }

            // Step 6: Validate integrity via IPackageVerifier
            ReportProgress(request, PackageInstallStage.ValidatingDownload, "Validating download...", 75);
            var verifyResult = await _packageVerifier.VerifyPackageAsync(archivePath, asset.Size, asset.Sha256, request.CancellationToken);
            if (!verifyResult.Success)
            {
                CleanFile(archivePath);
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, $"Error: Integrity verification failed: {verifyResult.Message}");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.ValidatingDownload,
                    ErrorMessage = $"Package verification failed: {verifyResult.Message}",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    DownloadedFileSize = archiveSize,
                    Version = selectedRelease.Tag
                };
            }

            string calculatedHash = verifyResult.CalculatedHash ?? "";

            // Step 7: Extract and Deploy (Transactional staging, backup, normalization, and rollback)
            ReportProgress(request, PackageInstallStage.Extracting, "Extracting files...", 80);
            string finalDestPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, definition.InstallDirectoryName));
            EmulatorInstallDiagnosticsLogger.SetArchiveType(opId, Path.GetExtension(asset.Name).ToLower().TrimStart('.'));
            EmulatorInstallDiagnosticsLogger.SetExtractionDestination(opId, finalDestPath);

            var extractionProgress = new Progress<ArchiveExtractionProgress>(p =>
            {
                int pct = 80 + (int)(p.Percentage * 0.15); // Scale extraction progress to 80%-95%
                ReportProgress(request, PackageInstallStage.Extracting, "Extracting files...", pct);
            });

            var extractionReq = new ArchiveExtractionRequest
            {
                ArchivePath = archivePath,
                DestinationPath = finalDestPath,
                CancellationToken = request.CancellationToken,
                Progress = extractionProgress,
                ExecutableCandidates = definition.ExecutableCandidates,
                PackageId = definition.Id,
                OperationId = request.OperationId,
                ExpectedSize = asset.Size
            };

            var extractionResult = await _archiveExtractor.ExtractAsync(extractionReq);
            CleanFile(archivePath); // Done with downloaded archive file
            
            bool archiveCleaned = !File.Exists(archivePath);
            EmulatorInstallDiagnosticsLogger.SetCleanupResult(opId, archiveCleaned ? "Successfully cleaned up temp archive" : "Failed to clean up temp archive");

            if (!extractionResult.Success)
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, $"Error: Extraction failed: {extractionResult.ErrorMessage}");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Extracting,
                    ErrorMessage = $"Extraction failed: {extractionResult.ErrorMessage}",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    DownloadedFileSize = archiveSize,
                    Version = selectedRelease.Tag,
                    InstallDirectory = finalDestPath
                };
            }

            // Step 8: Locating executable
            ReportProgress(request, PackageInstallStage.LocatingExecutable, "Locating executable...", 96);
            string finalExePath = extractionResult.MainExecutablePath ?? "";
            
            if (extractionResult.DiscoveredExecutables != null)
            {
                EmulatorInstallDiagnosticsLogger.SetDiscoveredExecutables(opId, extractionResult.DiscoveredExecutables);
            }

            if (string.IsNullOrEmpty(finalExePath) || !File.Exists(finalExePath))
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, "Error: Located executable is missing inside deployed folder.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.LocatingExecutable,
                    ErrorMessage = "Located executable is missing inside deployed folder.",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    DownloadedFileSize = archiveSize,
                    Version = selectedRelease.Tag,
                    InstallDirectory = finalDestPath
                };
            }

            EmulatorInstallDiagnosticsLogger.SetFinalExecutablePath(opId, finalExePath);

            // Security & correctness check: Ensure executable resides inside the destination folder
            string canonicalDest = Path.GetFullPath(finalDestPath) + Path.DirectorySeparatorChar;
            string canonicalExe = Path.GetFullPath(finalExePath);

            if (!canonicalExe.StartsWith(canonicalDest, StringComparison.OrdinalIgnoreCase))
            {
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, "Error: Deployment validation failed: executable is located outside the intended directory.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.LocatingExecutable,
                    ErrorMessage = "Deployment validation failed: executable is located outside the intended emulator directory.",
                    SelectedAssetName = asset.Name,
                    ArchivePath = archivePath,
                    DownloadedFileSize = archiveSize,
                    Version = selectedRelease.Tag,
                    InstallDirectory = finalDestPath,
                    ExecutablePath = finalExePath
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

            // Step 10: Update registry records (Registering emulator)
            ReportProgress(request, PackageInstallStage.Registering, "Registering emulator...", 98);
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
                EmulatorInstallDiagnosticsLogger.LogToSession(opId, "Error: Failed to update launcher registry.");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = definition.Id,
                    FailedStage = PackageInstallStage.Registering,
                    ErrorMessage = "Failed to update and save the emulator registration in the local config registry."
                };
            }

            ReportProgress(request, PackageInstallStage.Completed, "Installed successfully.", 100);

            return new PackageInstallResult
            {
                Success = true,
                PackageId = definition.Id,
                Version = installedVersion,
                InstallDirectory = finalDestPath,
                ExecutablePath = finalExePath,
                FailedStage = PackageInstallStage.Completed,
                SelectedAssetName = asset.Name,
                ArchivePath = archivePath,
                DownloadedFileSize = archiveSize
            };
        }

        private static void ReportProgress(EmulatorInstallationRequest request, PackageInstallStage stage, string step, int pct)
        {
            EmulatorInstallDiagnosticsLogger.SetStage(request.OperationId, stage.ToString());
            EmulatorInstallDiagnosticsLogger.LogToSession(request.OperationId, $"Progress: {step} ({pct}%)");

            request.Progress?.Report(new EmulatorInstallationProgress
            {
                EmulatorId = request.EmulatorId,
                Stage = stage,
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
