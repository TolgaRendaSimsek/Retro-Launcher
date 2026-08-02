using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Services.BIOS
{
    public enum BiosSyncState
    {
        NotStarted,
        Scanning,
        Syncing,
        SyncedSuccessfully,
        NoCompatibleBiosFound,
        EmulatorNotInstalled,
        Failed
    }

    public class BiosSyncResult
    {
        public string EmulatorId { get; set; } = "";
        public string EmulatorName { get; set; } = "";
        public bool IsInstalled { get; set; }
        public BiosSyncState State { get; set; } = BiosSyncState.NotStarted;
        public int CopiedCount { get; set; }
        public int SkippedCount { get; set; }
        public string DestinationPath { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }

    public class BiosSyncProgress
    {
        public string EmulatorId { get; set; } = "";
        public string EmulatorName { get; set; } = "";
        public BiosSyncState State { get; set; }
        public string Message { get; set; } = "";
        public int CopiedCount { get; set; }
        public int SkippedCount { get; set; }
    }

    public interface IBiosSynchronizationService
    {
        Task<BiosSyncResult> SyncEmulatorBiosAsync(
            string emulatorId,
            IProgress<BiosSyncProgress>? progress = null,
            CancellationToken cancellationToken = default);

        Task<List<BiosSyncResult>> SyncAllEmulatorsBiosAsync(
            IProgress<BiosSyncProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }

    public class BiosSynchronizationService : IBiosSynchronizationService
    {
        private static BiosSynchronizationService? _instance;
        public static BiosSynchronizationService Instance => _instance ??= new BiosSynchronizationService();

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _emulatorLocks = new(StringComparer.OrdinalIgnoreCase);

        public async Task<BiosSyncResult> SyncEmulatorBiosAsync(
            string emulatorId,
            IProgress<BiosSyncProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var sem = _emulatorLocks.GetOrAdd(emulatorId, _ => new SemaphoreSlim(1, 1));
            bool acquired = await sem.WaitAsync(0, cancellationToken);
            if (!acquired)
            {
                return new BiosSyncResult
                {
                    EmulatorId = emulatorId,
                    EmulatorName = GetEmulatorDisplayName(emulatorId),
                    IsInstalled = true,
                    State = BiosSyncState.Failed,
                    ErrorMessage = "A BIOS synchronization operation is already in progress for this emulator."
                };
            }

            try
            {
                return await Task.Run(() => PerformSync(emulatorId, progress, cancellationToken), cancellationToken);
            }
            finally
            {
                sem.Release();
            }
        }

        public async Task<List<BiosSyncResult>> SyncAllEmulatorsBiosAsync(
            IProgress<BiosSyncProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<BiosSyncResult>();
            var emulators = EmulatorManager.Instance.Config.Emulators;

            foreach (var emu in emulators)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var result = await SyncEmulatorBiosAsync(emu.Id, progress, cancellationToken);
                    results.Add(result);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Requirement 10: The global sync operation must continue with other emulators if one emulator fails
                    results.Add(new BiosSyncResult
                    {
                        EmulatorId = emu.Id,
                        EmulatorName = emu.Name,
                        IsInstalled = true,
                        State = BiosSyncState.Failed,
                        ErrorMessage = ex.Message,
                        Exception = ex
                    });
                }
            }

            return results;
        }

        private BiosSyncResult PerformSync(
            string emulatorId,
            IProgress<BiosSyncProgress>? progress,
            CancellationToken cancellationToken)
        {
            string displayName = GetEmulatorDisplayName(emulatorId);
            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase));

            string resolvedExePath = emuItem != null ? BiosManager.Instance.ResolvePath(emuItem.Path) : "";
            bool isInstalled = !string.IsNullOrEmpty(resolvedExePath) && File.Exists(resolvedExePath);

            if (!isInstalled)
            {
                var notInstalledResult = new BiosSyncResult
                {
                    EmulatorId = emulatorId,
                    EmulatorName = displayName,
                    IsInstalled = false,
                    State = BiosSyncState.EmulatorNotInstalled,
                    ErrorMessage = "Emulator is not installed or executable is missing."
                };

                progress?.Report(new BiosSyncProgress
                {
                    EmulatorId = emulatorId,
                    EmulatorName = displayName,
                    State = BiosSyncState.EmulatorNotInstalled,
                    Message = "Emulator not installed"
                });

                return notInstalledResult;
            }

            // Report scanning state
            progress?.Report(new BiosSyncProgress
            {
                EmulatorId = emulatorId,
                EmulatorName = displayName,
                State = BiosSyncState.Scanning,
                Message = "Scanning BIOS files..."
            });

            string emuDir = Path.GetDirectoryName(resolvedExePath) ?? AppContext.BaseDirectory;
            string destPath = GetDestinationBiosDirectory(emulatorId, emuDir);

            if (!Directory.Exists(destPath))
            {
                try { Directory.CreateDirectory(destPath); } catch { }
            }

            // Scan central BIOS directory for compatible files
            List<string> compatibleFiles = ScanCentralBiosDirectory(emulatorId);

            if (!compatibleFiles.Any())
            {
                var noBiosResult = new BiosSyncResult
                {
                    EmulatorId = emulatorId,
                    EmulatorName = displayName,
                    IsInstalled = true,
                    State = BiosSyncState.NoCompatibleBiosFound,
                    CopiedCount = 0,
                    SkippedCount = 0,
                    DestinationPath = destPath,
                    ErrorMessage = "No compatible BIOS files found in central BIOS directory."
                };

                progress?.Report(new BiosSyncProgress
                {
                    EmulatorId = emulatorId,
                    EmulatorName = displayName,
                    State = BiosSyncState.NoCompatibleBiosFound,
                    Message = "No compatible BIOS found"
                });

                return noBiosResult;
            }

            // Report syncing state
            progress?.Report(new BiosSyncProgress
            {
                EmulatorId = emulatorId,
                EmulatorName = displayName,
                State = BiosSyncState.Syncing,
                Message = "Syncing..."
            });

            int copied = 0;
            int skipped = 0;

            foreach (var srcFile in compatibleFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string fileName = Path.GetFileName(srcFile);
                string destFile = Path.Combine(destPath, fileName);

                if (File.Exists(destFile))
                {
                    if (AreFilesIdentical(srcFile, destFile))
                    {
                        skipped++;
                        continue;
                    }
                }

                File.Copy(srcFile, destFile, overwrite: true);
                copied++;
            }

            // Execute special emulator hooks (e.g., portable.txt for DuckStation, firmware install for RPCS3)
            ExecutePostSyncHooks(emulatorId, emuDir, destPath);

            var successResult = new BiosSyncResult
            {
                EmulatorId = emulatorId,
                EmulatorName = displayName,
                IsInstalled = true,
                State = BiosSyncState.SyncedSuccessfully,
                CopiedCount = copied,
                SkippedCount = skipped,
                DestinationPath = destPath
            };

            progress?.Report(new BiosSyncProgress
            {
                EmulatorId = emulatorId,
                EmulatorName = displayName,
                State = BiosSyncState.SyncedSuccessfully,
                Message = "Synced successfully",
                CopiedCount = copied,
                SkippedCount = skipped
            });

            return successResult;
        }

        private string GetEmulatorDisplayName(string emulatorId)
        {
            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase));
            if (emuItem != null && !string.IsNullOrEmpty(emuItem.Name)) return emuItem.Name;

            var defProvider = new JsonEmulatorPackageDefinitionProvider();
            var def = defProvider.GetById(emulatorId);
            if (def != null && !string.IsNullOrEmpty(def.DisplayName)) return def.DisplayName;

            return emulatorId.ToUpper();
        }

        private string GetDestinationBiosDirectory(string emulatorId, string emuDir)
        {
            return emulatorId.ToLower().Trim() switch
            {
                "duckstation" => Path.Combine(emuDir, "bios"),
                "pcsx2" => Path.Combine(emuDir, "bios"),
                "rpcs3" => Path.Combine(emuDir, "dev_flash"),
                "ppsspp" => Path.Combine(emuDir, "bios"),
                "dolphin" => Path.Combine(emuDir, "User", "GC"),
                "retroarch" => Path.Combine(emuDir, "system"),
                _ => Path.Combine(emuDir, "bios")
            };
        }

        private List<string> ScanCentralBiosDirectory(string emulatorId)
        {
            var rootsToScan = new List<string>
            {
                BiosManager.GetCentralizedBiosRoot(),
                Path.Combine(AppContext.BaseDirectory, "BIOS")
            };

            var subFolders = GetCentralSubfoldersForEmulator(emulatorId);
            var results = new List<string>();

            var validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".bin", ".rom", ".pup", ".prx", ".img", ".szs"
            };

            foreach (var centralRoot in rootsToScan.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(centralRoot)) continue;

                foreach (var subFolder in subFolders)
                {
                    string targetDir = Path.Combine(centralRoot, subFolder.Replace('/', Path.DirectorySeparatorChar));
                    if (Directory.Exists(targetDir))
                    {
                        try
                        {
                            var files = Directory.GetFiles(targetDir, "*.*", SearchOption.AllDirectories)
                                .Where(f => validExtensions.Contains(Path.GetExtension(f)))
                                .ToList();
                            results.AddRange(files);
                        }
                        catch { }
                    }
                }

                if (!results.Any())
                {
                    try
                    {
                        var emuSubDirs = Directory.GetDirectories(centralRoot, $"*{emulatorId}*", SearchOption.AllDirectories);
                        foreach (var dir in emuSubDirs)
                        {
                            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                                .Where(f => validExtensions.Contains(Path.GetExtension(f)))
                                .ToList();
                            results.AddRange(files);
                        }
                    }
                    catch { }
                }
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private List<string> GetCentralSubfoldersForEmulator(string emulatorId)
        {
            return emulatorId.ToLower().Trim() switch
            {
                "duckstation" => new List<string> { "DuckStation/PS1", "DuckStation" },
                "pcsx2" => new List<string> { "PCSX2/PS2", "PCSX2" },
                "rpcs3" => new List<string> { "RPCS3/PS3", "RPCS3" },
                "ppsspp" => new List<string> { "PPSSPP/PSP", "PPSSPP" },
                "dolphin" => new List<string> { "Dolphin/GameCube", "Dolphin/Wii", "Dolphin" },
                "retroarch" => new List<string> { "RetroArch" },
                _ => new List<string> { emulatorId }
            };
        }

        private bool AreFilesIdentical(string file1, string file2)
        {
            try
            {
                var fi1 = new FileInfo(file1);
                var fi2 = new FileInfo(file2);

                if (fi1.Length != fi2.Length) return false;
                if (fi1.Length == 0) return true;

                using var sha = SHA256.Create();
                using var stream1 = File.OpenRead(file1);
                using var stream2 = File.OpenRead(file2);

                byte[] hash1 = sha.ComputeHash(stream1);
                byte[] hash2 = sha.ComputeHash(stream2);

                return hash1.SequenceEqual(hash2);
            }
            catch
            {
                return false;
            }
        }

        private void ExecutePostSyncHooks(string emulatorId, string emuDir, string destPath)
        {
            try
            {
                if (string.Equals(emulatorId, "duckstation", StringComparison.OrdinalIgnoreCase))
                {
                    string portableFile = Path.Combine(emuDir, "portable.txt");
                    if (!File.Exists(portableFile))
                    {
                        File.WriteAllText(portableFile, "");
                    }
                }
                else if (string.Equals(emulatorId, "rpcs3", StringComparison.OrdinalIgnoreCase))
                {
                    string rpcs3Exe = Path.Combine(emuDir, "rpcs3.exe");
                    if (File.Exists(rpcs3Exe))
                    {
                        var pupFiles = Directory.GetFiles(destPath, "*.pup", SearchOption.AllDirectories);
                        foreach (var pup in pupFiles)
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = rpcs3Exe,
                                Arguments = $"--installfw \"{pup}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            try
                            {
                                using var p = Process.Start(psi);
                                p?.WaitForExit(15000);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
