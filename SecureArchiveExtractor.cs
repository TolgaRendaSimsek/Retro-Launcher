using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace RetroLauncher
{
    public class SecureArchiveExtractor : IArchiveExtractor
    {
        public bool CanExtract(string archiveType)
        {
            return string.Equals(archiveType, "zip", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(archiveType, "7z", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<ArchiveExtractionResult> ExtractAsync(ArchiveExtractionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Pre-Extraction Archive Validation
            if (!File.Exists(request.ArchivePath))
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.FileNotFound,
                    ErrorMessage = $"Archive file not found at '{request.ArchivePath}'."
                };
            }

            var fileInfo = new FileInfo(request.ArchivePath);
            if (fileInfo.Length <= 0)
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.InvalidArchive,
                    ErrorMessage = "Archive file is empty (size is 0)."
                };
            }

            // Verify expected size if provided
            if (request.ExpectedSize.HasValue && request.ExpectedSize.Value > 0)
            {
                if (fileInfo.Length != request.ExpectedSize.Value)
                {
                    return new ArchiveExtractionResult
                    {
                        Success = false,
                        FailureReason = ExtractionFailureReason.InvalidArchive,
                        ErrorMessage = $"Downloaded file size ({fileInfo.Length} bytes) does not match expected size ({request.ExpectedSize.Value} bytes)."
                    };
                }
            }

            string extension = Path.GetExtension(request.ArchivePath).ToLower();
            string archiveType = (extension == ".7z") ? "7z" : "zip";

            // Verify signature
            if (!ValidateArchiveSignature(request.ArchivePath, archiveType))
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.InvalidArchive,
                    ErrorMessage = $"Invalid signature: File does not match expected {archiveType.ToUpper()} format magic bytes."
                };
            }

            // 2. Set up staging sandbox and backup directories
            string packageId = string.IsNullOrWhiteSpace(request.PackageId) ? "default" : request.PackageId;
            string operationId = string.IsNullOrWhiteSpace(request.OperationId) ? Guid.NewGuid().ToString("N") : request.OperationId;

            string rootTempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "install", packageId, operationId);
            string stagingDir = Path.Combine(rootTempDir, "staging");
            string backupDir = Path.Combine(rootTempDir, "backup");
            string stagingCanonical = Path.GetFullPath(stagingDir) + Path.DirectorySeparatorChar;

            bool backedUp = false;
            bool deployed = false;

            // 3. Extraction Timeout and Cancellation Wrapper
            int timeoutSeconds = request.TimeoutSeconds > 0 ? request.TimeoutSeconds : 300;
            using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(request.CancellationToken, timeoutCts.Token))
            {
                var token = linkedCts.Token;
                try
                {
                    if (Directory.Exists(stagingDir)) Directory.Delete(stagingDir, true);
                    Directory.CreateDirectory(stagingDir);

                    // Perform the actual extraction on a background thread to keep UI responsive
                    ArchiveExtractionResult extractResult = await Task.Run<ArchiveExtractionResult>(async () =>
                    {
                        if (string.Equals(archiveType, "zip", StringComparison.OrdinalIgnoreCase))
                        {
                            return await ExtractZipAsync(request, stagingDir, stagingCanonical, token);
                        }
                        else
                        {
                            return await Extract7zAsync(request, stagingDir, stagingCanonical, token);
                        }
                    }, token);

                    if (!extractResult.Success)
                    {
                        return extractResult;
                    }

                    // 4. Normalization of redundant single top-level directory
                    string activeRoot = stagingDir;
                    var rootFiles = Directory.GetFiles(stagingDir);
                    var rootDirs = Directory.GetDirectories(stagingDir);

                    if (rootFiles.Length == 0 && rootDirs.Length == 1)
                    {
                        activeRoot = rootDirs[0];
                        RetroLogger.Log($"Nested top-level root folder detected in archive: '{activeRoot}'");
                    }

                    // 5. Executable discovery
                    string? matchingExePath = null;
                    var allExeFiles = Directory.GetFiles(activeRoot, "*.exe", SearchOption.AllDirectories);

                    var validCandidates = new List<(string FullPath, string RelativePath, int Score)>();
                    var allDiscoveredPaths = new List<string>();

                    foreach (var exeFile in allExeFiles)
                    {
                        string relPath = Path.GetRelativePath(activeRoot, exeFile);
                        allDiscoveredPaths.Add(relPath);

                        string relPathLower = relPath.ToLowerInvariant();
                        string fileNameLower = Path.GetFileName(exeFile).ToLowerInvariant();

                        // Split components for directory checking
                        var pathComponents = relPathLower.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                        // Reject if:
                        // - contains "temp" or "updater" in directory names
                        // - filename contains "crash" or "uninstall"
                        bool rejected = false;
                        foreach (var component in pathComponents)
                        {
                            if (component == "temp" || component == "updater" || component.Contains("temp") || component.Contains("updater"))
                            {
                                rejected = true;
                                break;
                            }
                        }

                        if (fileNameLower.Contains("crash") || fileNameLower.Contains("uninstall") || fileNameLower.Contains("updater") || fileNameLower.Contains("unins"))
                        {
                            rejected = true;
                        }

                        if (rejected)
                        {
                            continue;
                        }

                        // Calculate Score
                        int score = -9999;
                        bool matchesCandidate = false;
                        for (int i = 0; i < request.ExecutableCandidates.Count; i++)
                        {
                            // Compare case-insensitively with candidate
                            string candidate = request.ExecutableCandidates[i].ToLowerInvariant().Replace('\\', '/');
                            string relPathNormalized = relPathLower.Replace('\\', '/');

                            if (relPathNormalized.EndsWith(candidate) || fileNameLower == candidate)
                            {
                                score = 1000 - i;
                                matchesCandidate = true;
                                break;
                            }
                        }

                        if (request.ExecutableCandidates.Any() && !matchesCandidate)
                        {
                            continue;
                        }

                        // Tie breaker: prefer closer to root (less depth components)
                        int depth = pathComponents.Length;
                        score -= depth;

                        validCandidates.Add((exeFile, relPath, score));
                    }

                    if (validCandidates.Any())
                    {
                        // Order by score descending, then by relative path lexicographically ascending
                        var best = validCandidates
                            .OrderByDescending(c => c.Score)
                            .ThenBy(c => c.RelativePath, StringComparer.OrdinalIgnoreCase)
                            .First();
                        
                        matchingExePath = best.FullPath;
                    }

                    if (matchingExePath == null)
                    {
                        string discoveredList = allDiscoveredPaths.Any() 
                            ? string.Join(", ", allDiscoveredPaths) 
                            : "none";
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.NoExecutableFound,
                            ErrorMessage = $"No matching emulator executable was found in the extracted package. Discovered executables: {discoveredList}"
                        };
                    }

                    // 6. Transactional Deployment to request.DestinationPath
                    string destDir = Path.GetFullPath(request.DestinationPath);

                    // Backup existing installation
                    if (Directory.Exists(destDir))
                    {
                        try
                        {
                            if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
                            Directory.Move(destDir, backupDir);
                            backedUp = true;
                            RetroLogger.Log($"Backed up existing installation to backup path '{backupDir}'");
                        }
                        catch (Exception ex)
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.StagingCleanupFailed,
                                ErrorMessage = $"Failed to back up existing installation directory: {ex.Message}"
                            };
                        }
                    }

                    // Move normalized staging files to target directory
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destDir)!);
                        Directory.Move(activeRoot, destDir);
                        deployed = true;
                        RetroLogger.Log($"Deployed staging package content to destination '{destDir}'");
                    }
                    catch (Exception ex)
                    {
                        // Roll back immediately if target move fails
                        if (backedUp)
                        {
                            try
                            {
                                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
                                Directory.Move(backupDir, destDir);
                            }
                            catch (Exception rollEx)
                            {
                                RetroLogger.Log($"CRITICAL: Rollback failed during deployment failure! {rollEx.Message}", "ERROR");
                            }
                        }
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.ExtractionException,
                            ErrorMessage = $"Staging deployment phase failed: {ex.Message}"
                        };
                    }

                    // Restore configurations/saves from backup
                    if (backedUp && Directory.Exists(backupDir))
                    {
                        RestoreUserFolders(backupDir, destDir);
                        CleanDirectory(backupDir);
                    }

                    // Locate final exe path relative to target
                    string finalExePath = "";
                    if (matchingExePath != null)
                    {
                        string relativeExe = Path.GetRelativePath(activeRoot, matchingExePath);
                        finalExePath = Path.Combine(destDir, relativeExe);
                    }

                    return new ArchiveExtractionResult
                    {
                        Success = true,
                        ExtractedRootPath = destDir,
                        MainExecutablePath = finalExePath
                    };
                }
                catch (OperationCanceledException)
                {
                    // Roll back if cancellation occurred during deployment
                    if (backedUp && !deployed)
                    {
                        try
                        {
                            if (Directory.Exists(request.DestinationPath)) Directory.Delete(request.DestinationPath, true);
                            Directory.Move(backupDir, request.DestinationPath);
                        }
                        catch { }
                    }

                    if (request.CancellationToken.IsCancellationRequested)
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.Cancellation,
                            ErrorMessage = "Extraction cancelled by user."
                        };
                    }
                    else
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.Cancellation,
                            ErrorMessage = "Extraction timed out."
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Roll back on general exceptions
                    if (backedUp && !deployed)
                    {
                        try
                        {
                            if (Directory.Exists(request.DestinationPath)) Directory.Delete(request.DestinationPath, true);
                            Directory.Move(backupDir, request.DestinationPath);
                        }
                        catch { }
                    }

                    return new ArchiveExtractionResult
                    {
                        Success = false,
                        FailureReason = ExtractionFailureReason.ExtractionException,
                        ErrorMessage = $"Archive extraction error: {ex.Message}"
                    };
                }
                finally
                {
                    // Staging and temp folder cleanup
                    CleanDirectory(rootTempDir);
                }
            }
        }

        private static async Task<ArchiveExtractionResult> ExtractZipAsync(ArchiveExtractionRequest request, string stagingDir, string stagingCanonical, CancellationToken token)
        {
            using (var fileStream = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                int fileCount = zip.Entries.Count;
                if (fileCount > request.MaxFileCount)
                {
                    return new ArchiveExtractionResult
                    {
                        Success = false,
                        FailureReason = ExtractionFailureReason.LimitExceededFileCount,
                        ErrorMessage = $"Archive contains too many files (Limit: {request.MaxFileCount})."
                    };
                }

                long totalUncompressedSize = 0;
                foreach (var entry in zip.Entries)
                {
                    totalUncompressedSize += entry.Length;
                    if (entry.Length > request.MaxSingleFileSize)
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.LimitExceededSingleFileSize,
                            ErrorMessage = $"File '{entry.FullName}' size exceeds single-file extraction limit."
                        };
                    }

                    if (entry.CompressedLength > 0)
                    {
                        double ratio = (double)entry.Length / entry.CompressedLength;
                        if (ratio > request.MaxCompressionRatio)
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.LimitExceededCompressionRatio,
                                ErrorMessage = $"File '{entry.FullName}' exceeds compression ratio limits."
                            };
                        }
                    }
                }

                if (totalUncompressedSize > request.MaxTotalSize)
                {
                    return new ArchiveExtractionResult
                    {
                        Success = false,
                        FailureReason = ExtractionFailureReason.LimitExceededTotalSize,
                        ErrorMessage = $"Archive uncompressed size exceeds total size limit."
                    };
                }

                int filesExtracted = 0;
                long bytesExtracted = 0;

                foreach (var entry in zip.Entries)
                {
                    token.ThrowIfCancellationRequested();

                    string entryKey = entry.FullName;
                    if (string.IsNullOrEmpty(entryKey)) continue;

                    // Security check: Reject absolute paths, root identifiers, or directory escapes
                    if (Path.IsPathRooted(entryKey) || entryKey.Contains(":") || entryKey.StartsWith("/") || entryKey.StartsWith("\\"))
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.PathTraversalAttempt,
                            ErrorMessage = $"Absolute or rooted path rejected: '{entryKey}'."
                        };
                    }

                    string fullTargetPath = Path.GetFullPath(Path.Combine(stagingDir, entryKey));
                    if (!fullTargetPath.StartsWith(stagingCanonical, StringComparison.OrdinalIgnoreCase))
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.PathTraversalAttempt,
                            ErrorMessage = $"Security Alert: Path traversal attempt detected! Entry '{entryKey}' escapes staging directory."
                        };
                    }

                    if (entryKey.EndsWith("/") || entryKey.EndsWith("\\"))
                    {
                        Directory.CreateDirectory(fullTargetPath);
                        continue;
                    }

                    string? parentDir = Path.GetDirectoryName(fullTargetPath);
                    if (parentDir != null && !Directory.Exists(parentDir))
                    {
                        Directory.CreateDirectory(parentDir);
                    }

                    using (var entryStream = entry.Open())
                    using (var targetFileStream = new FileStream(fullTargetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await entryStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            await targetFileStream.WriteAsync(buffer, 0, bytesRead, token);
                            bytesExtracted += bytesRead;
                        }
                    }

                    filesExtracted++;

                    if (request.Progress != null)
                    {
                        int percent = totalUncompressedSize > 0
                            ? (int)((double)bytesExtracted / totalUncompressedSize * 100)
                            : (int)((double)filesExtracted / fileCount * 100);

                        request.Progress.Report(new ArchiveExtractionProgress
                        {
                            FilesExtracted = filesExtracted,
                            TotalFiles = fileCount,
                            BytesExtracted = bytesExtracted,
                            TotalBytes = totalUncompressedSize,
                            Percentage = Math.Min(percent, 100),
                            CurrentFileName = entryKey
                        });
                    }
                }
            }

            return new ArchiveExtractionResult { Success = true };
        }

        private static async Task<ArchiveExtractionResult> Extract7zAsync(ArchiveExtractionRequest request, string stagingDir, string stagingCanonical, CancellationToken token)
        {
            try
            {
                using (var fileStream = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var archive = ArchiveFactory.OpenArchive(fileStream))
                {
                    if (archive.Entries.Any(e => e.IsEncrypted))
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.InvalidArchive,
                            ErrorMessage = "Archive is encrypted/password-protected."
                        };
                    }

                    int fileCount = archive.Entries.Count();
                    if (fileCount > request.MaxFileCount)
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.LimitExceededFileCount,
                            ErrorMessage = $"Archive contains too many files (Limit: {request.MaxFileCount})."
                        };
                    }

                    long totalUncompressedSize = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.IsDirectory) continue;

                        totalUncompressedSize += entry.Size;
                        if (entry.Size > request.MaxSingleFileSize)
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.LimitExceededSingleFileSize,
                                ErrorMessage = $"File '{entry.Key}' size exceeds single-file extraction limit."
                            };
                        }

                        if (entry.CompressedSize > 0)
                        {
                            double ratio = (double)entry.Size / entry.CompressedSize;
                            if (ratio > request.MaxCompressionRatio)
                            {
                                return new ArchiveExtractionResult
                                {
                                    Success = false,
                                    FailureReason = ExtractionFailureReason.LimitExceededCompressionRatio,
                                    ErrorMessage = $"File '{entry.Key}' exceeds compression ratio limits."
                                };
                            }
                        }
                    }

                    if (totalUncompressedSize > request.MaxTotalSize)
                    {
                        return new ArchiveExtractionResult
                        {
                            Success = false,
                            FailureReason = ExtractionFailureReason.LimitExceededTotalSize,
                            ErrorMessage = $"Archive uncompressed size exceeds total size limit."
                        };
                    }

                    int filesExtracted = 0;
                    long bytesExtracted = 0;

                    foreach (var entry in archive.Entries)
                    {
                        token.ThrowIfCancellationRequested();

                        // Reject symbolic links to block symlink attacks
                        if (entry.LinkTarget != null)
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.UnsafeSymbolicLink,
                                ErrorMessage = $"Archive entry '{entry.Key}' is an unsafe symbolic link, which is disallowed for security."
                            };
                        }

                        string entryKey = entry.Key ?? "";
                        if (string.IsNullOrEmpty(entryKey)) continue;

                        // Security check: Absolute/rooted checks
                        if (Path.IsPathRooted(entryKey) || entryKey.Contains(":") || entryKey.StartsWith("/") || entryKey.StartsWith("\\"))
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.PathTraversalAttempt,
                                ErrorMessage = $"Absolute or rooted path rejected: '{entryKey}'."
                            };
                        }

                        string fullTargetPath = Path.GetFullPath(Path.Combine(stagingDir, entryKey));
                        if (!fullTargetPath.StartsWith(stagingCanonical, StringComparison.OrdinalIgnoreCase))
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.PathTraversalAttempt,
                                ErrorMessage = $"Security Alert: Path traversal attempt detected! Entry '{entryKey}' escapes staging directory."
                            };
                        }

                        if (entry.IsDirectory)
                        {
                            Directory.CreateDirectory(fullTargetPath);
                            continue;
                        }

                        string? parentDir = Path.GetDirectoryName(fullTargetPath);
                        if (parentDir != null && !Directory.Exists(parentDir))
                        {
                            Directory.CreateDirectory(parentDir);
                        }

                        using (var entryStream = entry.OpenEntryStream())
                        using (var targetFileStream = new FileStream(fullTargetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await Task.Run(() => entryStream.Read(buffer, 0, buffer.Length), token)) > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                await targetFileStream.WriteAsync(buffer, 0, bytesRead, token);
                                bytesExtracted += bytesRead;
                            }
                        }

                        filesExtracted++;

                        if (request.Progress != null)
                        {
                            int percent = totalUncompressedSize > 0
                                ? (int)((double)bytesExtracted / totalUncompressedSize * 100)
                                : (int)((double)filesExtracted / fileCount * 100);

                            request.Progress.Report(new ArchiveExtractionProgress
                            {
                                FilesExtracted = filesExtracted,
                                TotalFiles = fileCount,
                                BytesExtracted = bytesExtracted,
                                TotalBytes = totalUncompressedSize,
                                Percentage = Math.Min(percent, 100),
                                CurrentFileName = entryKey
                            });
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.InvalidArchive,
                    ErrorMessage = "Archive is encrypted/password-protected."
                };
            }
            catch (ArchiveException ex)
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.InvalidArchive,
                    ErrorMessage = $"Unsupported or invalid 7z archive: {ex.Message}"
                };
            }

            return new ArchiveExtractionResult { Success = true };
        }

        private static bool ValidateArchiveSignature(string filePath, string archiveType)
        {
            try
            {
                byte[] buffer = new byte[6];
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead < 4) return false;
                }

                if (string.Equals(archiveType, "zip", StringComparison.OrdinalIgnoreCase))
                {
                    // PK\x03\x04
                    return buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
                }
                else if (string.Equals(archiveType, "7z", StringComparison.OrdinalIgnoreCase))
                {
                    // 7z\xBC\xAF\x27\x1C
                    if (buffer.Length < 6) return false;
                    return buffer[0] == 0x37 && buffer[1] == 0x7A && buffer[2] == 0xBC && buffer[3] == 0xAF && buffer[4] == 0x27 && buffer[5] == 0x1C;
                }
            }
            catch
            {
                return false;
            }
            return false;
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

            string[] configFiles = { "config.yml", "portable.txt" };
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
