using System;
using System.IO;
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

            if (!File.Exists(request.ArchivePath))
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.FileNotFound,
                    ErrorMessage = $"Archive file not found at '{request.ArchivePath}'."
                };
            }

            string stagingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", $"staging_{Guid.NewGuid():N}");
            string stagingCanonical = Path.GetFullPath(stagingDir) + Path.DirectorySeparatorChar;

            try
            {
                Directory.CreateDirectory(stagingDir);

                // Run parsing and extraction in a background thread to prevent UI freezing
                return await Task.Run(() =>
                {
                    using (var archive = ArchiveFactory.OpenArchive(request.ArchivePath, new SharpCompress.Readers.ReaderOptions()))
                    {
                        // 1. Enforce Pre-Extraction Security Limit Checks (Archive Bomb Defense)
                        int fileCount = 0;
                        long totalUncompressedSize = 0;

                        foreach (var entry in archive.Entries)
                        {
                            if (entry.IsDirectory) continue;

                            fileCount++;
                            totalUncompressedSize += entry.Size;

                            if (fileCount > request.MaxFileCount)
                            {
                                return new ArchiveExtractionResult
                                {
                                    Success = false,
                                    FailureReason = ExtractionFailureReason.LimitExceededFileCount,
                                    ErrorMessage = $"Archive contains too many files (Limit: {request.MaxFileCount})."
                                };
                            }

                            if (entry.Size > request.MaxSingleFileSize)
                            {
                                return new ArchiveExtractionResult
                                {
                                    Success = false,
                                    FailureReason = ExtractionFailureReason.LimitExceededSingleFileSize,
                                    ErrorMessage = $"File '{entry.Key}' size exceeds the single-file extraction limit ({entry.Size} > {request.MaxSingleFileSize} bytes)."
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
                                        ErrorMessage = $"File '{entry.Key}' exceeds maximum permitted compression ratio ({ratio:F2}x > {request.MaxCompressionRatio}x)."
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
                                ErrorMessage = $"Archive uncompressed size exceeds limit ({totalUncompressedSize} > {request.MaxTotalSize} bytes)."
                            };
                        }

                        // 2. Perform Extraction into Staging Sandbox
                        int filesExtracted = 0;
                        long bytesExtracted = 0;

                        foreach (var entry in archive.Entries)
                        {
                            request.CancellationToken.ThrowIfCancellationRequested();

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
                            if (string.IsNullOrEmpty(entryKey))
                            {
                                continue;
                            }

                            // Security: Block absolute, rooted, or drive-qualified paths
                            if (Path.IsPathRooted(entryKey) || entryKey.Contains(":") || entryKey.StartsWith("/") || entryKey.StartsWith("\\"))
                            {
                                return new ArchiveExtractionResult
                                {
                                    Success = false,
                                    FailureReason = ExtractionFailureReason.PathTraversalAttempt,
                                    ErrorMessage = $"Archive entry '{entryKey}' has an absolute or rooted path, which is disallowed for security."
                                };
                            }

                            // Security: Zip Slip check
                            string fullTargetPath = Path.GetFullPath(Path.Combine(stagingDir, entryKey));
                            if (!fullTargetPath.StartsWith(stagingCanonical, StringComparison.OrdinalIgnoreCase))
                            {
                                return new ArchiveExtractionResult
                                {
                                    Success = false,
                                    FailureReason = ExtractionFailureReason.PathTraversalAttempt,
                                    ErrorMessage = $"Security Alert: Path traversal attempt detected! Entry '{entryKey}' targets a path outside staging directory."
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

                            // Streaming extraction copy
                            using (var entryStream = entry.OpenEntryStream())
                            using (var targetFileStream = new FileStream(fullTargetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                var buffer = new byte[8192];
                                int bytesRead;
                                while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    request.CancellationToken.ThrowIfCancellationRequested();
                                    targetFileStream.Write(buffer, 0, bytesRead);
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
                                    Percentage = percent,
                                    CurrentFileName = entryKey
                                });
                            }
                        }

                        // 3. Normalize structure & Find Executable candidate
                        string activeRoot = stagingDir;
                        var rootFiles = Directory.GetFiles(stagingDir);
                        var rootDirs = Directory.GetDirectories(stagingDir);

                        // If exactly one folder and no files, drill down
                        if (rootFiles.Length == 0 && rootDirs.Length == 1)
                        {
                            activeRoot = rootDirs[0];
                            RetroLogger.Log($"Nested root folder detected in archive: {activeRoot}");
                        }

                        string? matchingExePath = null;
                        foreach (var candidate in request.ExecutableCandidates)
                        {
                            string fullCandidatePath = Path.Combine(activeRoot, candidate);
                            if (File.Exists(fullCandidatePath))
                            {
                                matchingExePath = fullCandidatePath;
                                break;
                            }
                        }

                        // Recursively search for candidates if not found at root
                        if (matchingExePath == null)
                        {
                            foreach (var candidate in request.ExecutableCandidates)
                            {
                                var matches = Directory.GetFiles(activeRoot, candidate, SearchOption.AllDirectories);
                                if (matches.Length > 0)
                                {
                                    matchingExePath = matches[0];
                                    break;
                                }
                            }
                        }

                        if (matchingExePath == null)
                        {
                            return new ArchiveExtractionResult
                            {
                                Success = false,
                                FailureReason = ExtractionFailureReason.NoExecutableFound,
                                ErrorMessage = "No matching emulator executable was found in the extracted package."
                            };
                        }

                        // 4. Safe Copy to Destination (preserve bios, saves, configs)
                        string destDir = request.DestinationPath;
                        if (!Directory.Exists(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }

                        CopyDirectoryPreservingUserFolders(activeRoot, destDir);

                        // Locate final relative exe path inside destination
                        string relativeExePath = Path.GetRelativePath(activeRoot, matchingExePath);
                        string finalExePath = Path.Combine(destDir, relativeExePath);

                        return new ArchiveExtractionResult
                        {
                            Success = true,
                            ExtractedRootPath = destDir,
                            MainExecutablePath = finalExePath
                        };
                    }
                }, request.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.Cancellation,
                    ErrorMessage = "Extraction cancelled by user."
                };
            }
            catch (Exception ex)
            {
                return new ArchiveExtractionResult
                {
                    Success = false,
                    FailureReason = ExtractionFailureReason.ExtractionException,
                    ErrorMessage = $"Extraction exception occurred: {ex.Message}"
                };
            }
            finally
            {
                // Staging cleanup
                if (!request.PreserveStagingForDiagnostics && Directory.Exists(stagingDir))
                {
                    try
                    {
                        Directory.Delete(stagingDir, true);
                    }
                    catch (Exception cleanupEx)
                    {
                        RetroLogger.Log($"Staging cleanup failed for {stagingDir}: {cleanupEx.Message}", "WARNING");
                    }
                }
            }
        }

        private static void CopyDirectoryPreservingUserFolders(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir).ToLower();
                // Exclude sensitive user configurations or emulator dependencies to preserve state
                if (dirName == "bios" || dirName == "saves" || dirName == "configs" || 
                    dirName == "screenshots" || dirName == "games" || dirName == "roms")
                {
                    continue;
                }

                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectoryPreservingUserFolders(subDir, destSubDir);
            }
        }
    }
}
