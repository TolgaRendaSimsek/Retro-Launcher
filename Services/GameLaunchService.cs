using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RetroLauncher.Emulators;
using RetroLauncher.Emulators.Adapters;
using RetroLauncher.Services.Logging;

namespace RetroLauncher.Services
{
    public class GameLaunchService
    {
        private static GameLaunchService? _instance;
        public static GameLaunchService Instance => _instance ??= new GameLaunchService();

        private readonly Dictionary<string, Process> _runningGames = new();
        private readonly object _lock = new();

        public event EventHandler<string>? GameStarted;
        public event EventHandler<string>? GameExited;

        public bool IsGameRunning(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId)) return false;
            lock (_lock)
            {
                if (_runningGames.TryGetValue(gameId, out var process))
                {
                    try
                    {
                        if (!process.HasExited) return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        public void StopGame(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId)) return;
            Process? processToKill = null;
            lock (_lock)
            {
                if (_runningGames.TryGetValue(gameId, out var process))
                {
                    processToKill = process;
                }
            }

            if (processToKill != null)
            {
                try
                {
                    if (!processToKill.HasExited)
                    {
                        processToKill.Kill(true);
                    }
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to terminate process for game {gameId}: {ex.Message}", "WARNING");
                }
            }
        }

        public async Task LaunchGameAsync(Game game)
        {
            string emulatorId = "Unknown";
            string exePath = "Unknown";
            string workingDir = "Unknown";
            string argsText = "None";

            try
            {
                // Step 1: Loading game & basic validation
                LogStep("Loading game...");
                if (game == null)
                {
                    throw new ArgumentNullException(nameof(game), "No game selected for launch.");
                }

                RetroLogger.Log($"[LaunchStep] Loading game: '{game.Title}' (ID: '{game.Id}', Platform: '{game.Platform}')");

                if (IsGameRunning(game.Id))
                {
                    throw new InvalidOperationException($"The game '{game.Title}' is already running!");
                }

                // Step 2: Validating ROM path & File permissions
                LogStep($"Resolving ROM path: '{game.RomPath}'...");
                if (string.IsNullOrWhiteSpace(game.RomPath))
                {
                    throw new FileNotFoundException("ROM path is not specified for this game.");
                }

                string resolvedRom = ResolvePath(game.RomPath);
                bool romExists = File.Exists(resolvedRom) || Directory.Exists(resolvedRom);
                if (!romExists)
                {
                    throw new FileNotFoundException(
                        $"Game ROM file or folder not found at path:\n'{resolvedRom}'\n\nPlease verify that the file exists or update the ROM location in Game Properties.",
                        resolvedRom
                    );
                }

                // Verify file access permissions if file
                if (File.Exists(resolvedRom))
                {
                    try
                    {
                        using (var testStream = File.Open(resolvedRom, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            // Permission check passed
                        }
                    }
                    catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                    {
                        throw new UnauthorizedAccessException($"Permission denied or file locked when opening ROM file:\n'{resolvedRom}'\n\nDetails: {ex.Message}", ex);
                    }
                }

                // Step 3: Resolving emulator adapter
                LogStep($"Resolving emulator for platform '{game.Platform}'...");
                var adapter = EmulatorAdapterRegistry.GetAdapter(game);
                if (adapter == null)
                {
                    throw new InvalidOperationException($"No emulator adapter registered for platform '{game.Platform}'.");
                }

                emulatorId = adapter.EmulatorId;

                // Step 4: Resolving executable path & verifying installation
                LogStep($"Resolving executable path for emulator '{emulatorId}'...");
                exePath = adapter.GetExecutablePath();
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    throw new FileNotFoundException($"No executable path configured for emulator '{emulatorId}'. Please set the executable path in Emulator Manager.");
                }

                if (!File.Exists(exePath))
                {
                    string folder = Path.GetDirectoryName(exePath) ?? "Unknown";
                    string fileName = Path.GetFileName(exePath);
                    throw new FileNotFoundException(
                        $"Emulator executable not found!\n\n" +
                        $"Expected File: {fileName}\n" +
                        $"Searched Directory: {folder}\n\n" +
                        $"Please install or repair '{emulatorId}' in Emulator Manager.",
                        exePath
                    );
                }

                // Test executable read permissions
                try
                {
                    using (var testExeStream = File.Open(exePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        // Permission check passed
                    }
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    throw new UnauthorizedAccessException($"Permission denied or file locked when accessing emulator executable:\n'{exePath}'\n\nDetails: {ex.Message}", ex);
                }

                // Step 5: Checking BIOS / Firmware requirements
                LogStep($"Checking BIOS/firmware requirements for '{emulatorId}'...");
                var emuConfig = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase));
                if (emuConfig != null)
                {
                    // Rescan BIOS folder right before verification
                    BiosManager.Instance.DetectBiosStatus();

                    if (emuConfig.RequiresBIOS)
                    {
                        var biosItem = BiosManager.Instance.BiosItems.FirstOrDefault(b => 
                            string.Equals(b.Emulator, emuConfig.Name, StringComparison.OrdinalIgnoreCase) || 
                            string.Equals(b.Console, game.Platform, StringComparison.OrdinalIgnoreCase));

                        bool hasRealBios = biosItem != null && BiosManager.Instance.CheckRealBiosExists(biosItem.Console);
                        if (!hasRealBios)
                        {
                            string defaultFolder = biosItem != null ? BiosManager.Instance.GetDefaultFolderForConsole(biosItem.Console) : "BIOS";
                            string resolvedFolder = ResolvePath(defaultFolder);

                            throw new InvalidOperationException(
                                $"The platform '{game.Platform}' requires a valid BIOS file to run, which is missing from the central BIOS directory!\n\n" +
                                $"Searched Path: {resolvedFolder}\n\n" +
                                $"Please open the BIOS/Firmware Manager and import a legally obtained BIOS file."
                            );
                        }

                        // Synchronize BIOS asynchronously
                        if (biosItem != null)
                        {
                            await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync(emuConfig.Id);
                        }
                    }
                    else if (emuConfig.RequiresFirmware)
                    {
                        var fwItem = BiosManager.Instance.BiosItems.FirstOrDefault(b => 
                            string.Equals(b.Emulator, emuConfig.Name, StringComparison.OrdinalIgnoreCase) || 
                            string.Equals(b.Console, game.Platform, StringComparison.OrdinalIgnoreCase));

                        bool hasRealFw = fwItem != null && BiosManager.Instance.CheckRealBiosExists(fwItem.Console);
                        if (!hasRealFw)
                        {
                            string defaultFolder = fwItem != null ? BiosManager.Instance.GetDefaultFolderForConsole(fwItem.Console) : "BIOS";
                            string resolvedFolder = ResolvePath(defaultFolder);

                            throw new InvalidOperationException(
                                $"The platform '{game.Platform}' requires system firmware to run, which is missing from the central BIOS directory!\n\n" +
                                $"Searched Path: {resolvedFolder}\n\n" +
                                $"Please open the BIOS/Firmware Manager and import the required firmware file."
                            );
                        }

                        if (fwItem != null)
                        {
                            await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync(emuConfig.Id);
                        }
                    }
                }

                // Step 6: Pre-launch Controller Auto-Sync
                if (emuConfig != null && (emuConfig.AutoSyncController || GlobalControllerConfigManager.Instance.Config.AutoSyncOnLaunch))
                {
                    try
                    {
                        await ControllerSyncService.Instance.ApplyGlobalProfileToEmulatorAsync(emuConfig.Id, skipRunningCheck: true);
                    }
                    catch (Exception ex)
                    {
                        RetroLogger.Log($"Pre-launch auto controller sync warning for {emuConfig.Name}: {ex.Message}", "WARNING");
                    }
                }

                // Step 7: Validating working directory & permissions
                LogStep("Validating working directory...");
                workingDir = Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory;
                if (!Directory.Exists(workingDir))
                {
                    throw new DirectoryNotFoundException($"Emulator working directory not found:\n'{workingDir}'");
                }

                // Step 8: Building launch command line
                LogStep("Building command line...");
                ProcessStartInfo psi = adapter.BuildLaunchCommand(game);
                if (psi == null)
                {
                    throw new InvalidOperationException($"Failed to build launch command line for emulator '{emulatorId}'.");
                }

                psi.FileName = exePath;
                psi.WorkingDirectory = workingDir;
                psi.UseShellExecute = false;

                if (!string.IsNullOrWhiteSpace(game.LaunchArguments))
                {
                    var extraParts = game.LaunchArguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var extra in extraParts)
                    {
                        psi.ArgumentList.Add(extra);
                    }
                }

                argsText = string.Join(" ", psi.ArgumentList.Select(a => a.Contains(" ") ? $"\"{a}\"" : a));
                RetroLogger.Log($"[LaunchStep] Command line built:\n  File: '{psi.FileName}'\n  Args: {argsText}\n  WorkDir: '{psi.WorkingDirectory}'");

                // Double-check file & directory existence right before process start
                if (!File.Exists(psi.FileName))
                {
                    throw new FileNotFoundException($"Executable file does not exist right before launch:\n'{psi.FileName}'", psi.FileName);
                }
                if (!Directory.Exists(psi.WorkingDirectory))
                {
                    throw new DirectoryNotFoundException($"Working directory does not exist right before launch:\n'{psi.WorkingDirectory}'");
                }

                // Step 9: Starting process
                LogStep("Starting process...");
                Process? process = await Task.Run(() => Process.Start(psi));
                if (process == null)
                {
                    throw new InvalidOperationException($"Process.Start returned null for executable:\n'{psi.FileName}'");
                }

                LogStep($"Process started successfully. PID: {process.Id}");

                // Monitor process startup for 1 second to detect early exit
                await Task.Delay(1000);
                if (process.HasExited)
                {
                    int exitCode = process.ExitCode;
                    if (exitCode != 0)
                    {
                        throw new InvalidOperationException(
                            $"Emulator process exited immediately after launch with Exit Code {exitCode}.\n\n" +
                            $"Executable: {psi.FileName}\n" +
                            $"Working Directory: {psi.WorkingDirectory}\n\n" +
                            $"Please check emulator graphics driver, DirectX/Vulkan runtimes, or BIOS configuration."
                        );
                    }
                }

                // Step 10: Process tracking & session management
                lock (_lock)
                {
                    _runningGames[game.Id] = process;
                }

                GameStarted?.Invoke(this, game.Id);

                _ = Task.Run(() =>
                {
                    try
                    {
                        PlaytimeManager.Instance.StartSession(game.Id, process.Id);
                        process.EnableRaisingEvents = true;
                        process.WaitForExit();

                        lock (_lock)
                        {
                            _runningGames.Remove(game.Id);
                        }

                        int sessionMins = PlaytimeManager.Instance.EndSession(game.Id);

                        game.TotalPlaytimeMinutes = PlaytimeManager.Instance.GetTotalPlaytime(game.Id);
                        game.LastPlayed = PlaytimeManager.Instance.GetOrCreateRecord(game.Id).LastPlayed;
                        
                        var libraryManager = new GameLibraryManager();
                        libraryManager.UpdateGame(game);

                        var fs = new MockFriendsService();
                        var profile = fs.GetLocalProfile();
                        profile.TotalPlayTimeMinutes = libraryManager.Games.Sum(g => g.TotalPlaytimeMinutes);
                        fs.SaveLocalProfile(profile);
                        fs.UpdateMyStatus(ActivityStatus.Online, "");
                        fs.LogActivity($"Finished playing {game.Title} (Session: {sessionMins} mins)");

                        GameExited?.Invoke(this, game.Id);
                    }
                    catch (Exception ex)
                    {
                        lock (_lock)
                        {
                            _runningGames.Remove(game.Id);
                        }
                        RetroLogger.Log($"Error tracking playtime/session for game {game.Title}: {ex.Message}", "WARNING");
                        GameExited?.Invoke(this, game.Id);
                    }
                });
            }
            catch (FileNotFoundException ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
            catch (DirectoryNotFoundException ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
            catch (IOException ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
            catch (Win32Exception ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
            catch (Exception ex)
            {
                LogDetailedError(game, emulatorId, exePath, workingDir, argsText, ex);
                throw;
            }
        }

        private static void LogStep(string stepName)
        {
            RetroLogger.Log($"[LaunchStep] {stepName}");
        }

        private static void LogDetailedError(Game? game, string emulatorId, string exePath, string workingDir, string argsText, Exception ex)
        {
            string detailedLog = $@"
================================================================================
[LAUNCH ERROR DETAILED LOG]
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Game: {game?.Title ?? "Unknown"} (ID: {game?.Id ?? "Unknown"})
Platform: {game?.Platform ?? "Unknown"}
ROM Path: {game?.RomPath ?? "Unknown"}
Emulator ID: {emulatorId}
Executable Path: {exePath}
Working Directory: {workingDir}
Command Line Args: {argsText}
Exception Type: {ex.GetType().FullName}
Exception Message: {ex.Message}
Stack Trace:
{ex.StackTrace}
================================================================================";

            RetroLogger.Log(detailedLog, "ERROR");
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }
    }
}
