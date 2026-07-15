using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher
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
                        processToKill.Kill(true); // Terminate process and its children
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to terminate process for game {gameId}: {ex.Message}");
                }
            }
        }

        public async Task LaunchGameAsync(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game), "No game selected.");
            }

            // Prevent launching the same game twice
            if (IsGameRunning(game.Id))
            {
                throw new InvalidOperationException("This game is already running!");
            }

            // 1. Validate ROM/game path
            string resolvedRom = ResolvePath(game.RomPath);
            if (string.IsNullOrEmpty(game.RomPath) || (!File.Exists(resolvedRom) && !Directory.Exists(resolvedRom)))
            {
                throw new FileNotFoundException($"ROM file or game folder not found at:\n'{game.RomPath}'");
            }

            // 2. Resolve target adapter
            var adapter = EmulatorAdapterRegistry.GetAdapter(game);

            // 3. Verify emulator installation
            if (!adapter.IsInstalled())
            {
                string exePath = adapter.GetExecutablePath();
                string folder = !string.IsNullOrEmpty(exePath) ? Path.GetDirectoryName(exePath) ?? "Unknown" : "Unknown";
                string expected = !string.IsNullOrEmpty(exePath) ? Path.GetFileName(exePath) : "Executable containing 'duckstation'";
                
                if (string.Equals(adapter.EmulatorId, "duckstation", StringComparison.OrdinalIgnoreCase))
                {
                    expected = "Executable containing 'duckstation' (e.g. duckstation-qt-x64-ReleaseLTCG.exe)";
                }

                throw new FileNotFoundException(
                    $"Emulator executable not found!\n\n" +
                    $"Expected File: {expected}\n" +
                    $"Searched Folder: {folder}"
                );
            }

            // 4. Verify BIOS or firmware requirements
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, adapter.EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu != null)
            {
                // Rescan BIOS/firmware folder right before verification to check real filesystem
                BiosManager.Instance.DetectBiosStatus();

                if (emu.RequiresBIOS)
                {
                    // Find the matching centralized BIOS configuration
                    var biosItem = BiosManager.Instance.BiosItems.FirstOrDefault(b => 
                        string.Equals(b.Emulator, emu.Name, StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(b.Console, game.Platform, StringComparison.OrdinalIgnoreCase));

                    bool hasRealBios = biosItem != null && BiosManager.Instance.CheckRealBiosExists(biosItem.Console);
                    if (!hasRealBios)
                    {
                        string defaultFolder = biosItem != null ? BiosManager.Instance.GetDefaultFolderForConsole(biosItem.Console) : "BIOS";
                        string resolvedFolder = ResolvePath(defaultFolder);

                        var detectedFiles = new List<string>();
                        if (Directory.Exists(resolvedFolder))
                        {
                            try
                            {
                                detectedFiles = Directory.GetFiles(resolvedFolder, "*.*", SearchOption.AllDirectories)
                                    .Select(Path.GetFileName)
                                    .Where(name => name != null)
                                    .Select(name => name!)
                                    .ToList();
                            }
                            catch { }
                        }
                        string detectedText = detectedFiles.Any() ? string.Join(", ", detectedFiles) : "(No files found)";

                        throw new InvalidOperationException(
                            $"The platform '{game.Platform}' requires a BIOS file to run, which is missing from the centralized BIOS directory!\n\n" +
                            $"Searched Path: {resolvedFolder}\n" +
                            $"Detected Filenames: {detectedText}\n\n" +
                            $"Please open the BIOS/Firmware Manager and import a legally obtained BIOS file."
                        );
                    }

                    // Synchronize the BIOS to the emulator's expected directory before launching
                    if (biosItem != null)
                    {
                        BiosManager.Instance.SyncBiosToEmulator(biosItem);
                    }
                }
                else if (emu.RequiresFirmware)
                {
                    // For firmware like RPCS3, synchronize it as well
                    var fwItem = BiosManager.Instance.BiosItems.FirstOrDefault(b => 
                        string.Equals(b.Emulator, emu.Name, StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(b.Console, game.Platform, StringComparison.OrdinalIgnoreCase));

                    bool hasRealFw = fwItem != null && BiosManager.Instance.CheckRealBiosExists(fwItem.Console);
                    if (!hasRealFw)
                    {
                        string defaultFolder = fwItem != null ? BiosManager.Instance.GetDefaultFolderForConsole(fwItem.Console) : "BIOS";
                        string resolvedFolder = ResolvePath(defaultFolder);

                        var detectedFiles = new List<string>();
                        if (Directory.Exists(resolvedFolder))
                        {
                            try
                            {
                                detectedFiles = Directory.GetFiles(resolvedFolder, "*.*", SearchOption.AllDirectories)
                                    .Select(Path.GetFileName)
                                    .Where(name => name != null)
                                    .Select(name => name!)
                                    .ToList();
                            }
                            catch { }
                        }
                        string detectedText = detectedFiles.Any() ? string.Join(", ", detectedFiles) : "(No files found)";

                        throw new InvalidOperationException(
                            $"The platform '{game.Platform}' requires emulator system firmware to run, which is missing from the centralized BIOS directory!\n\n" +
                            $"Searched Path: {resolvedFolder}\n" +
                            $"Detected Filenames: {detectedText}\n\n" +
                            $"Please open the BIOS/Firmware Manager and import the firmware file."
                        );
                    }

                    if (fwItem != null)
                    {
                        BiosManager.Instance.SyncBiosToEmulator(fwItem);
                    }
                }
            }

            // 5. Apply per-game launch arguments & start process
            ProcessStartInfo psi = adapter.BuildLaunchCommand(game);
            
            // If the game has specific launch arguments, append them
            if (!string.IsNullOrEmpty(game.LaunchArguments))
            {
                psi.Arguments += " " + game.LaunchArguments;
            }

            // Start the emulator process
            Process? process = Process.Start(psi);
            if (process == null)
            {
                throw new Exception($"Failed to start emulator process: {psi.FileName}");
            }

            lock (_lock)
            {
                _runningGames[game.Id] = process;
            }

            // Notify UI
            GameStarted?.Invoke(this, game.Id);

            // 6. Track playtime and update status asynchronously until process exits
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

                    // Update playtime in library database
                    game.TotalPlaytimeMinutes = PlaytimeManager.Instance.GetTotalPlaytime(game.Id);
                    game.LastPlayed = PlaytimeManager.Instance.GetOrCreateRecord(game.Id).LastPlayed;
                    
                    var libraryManager = new GameLibraryManager();
                    libraryManager.UpdateGame(game);

                    // Update playtime in social system
                    var fs = new MockFriendsService();
                    var profile = fs.GetLocalProfile();
                    profile.TotalPlayTimeMinutes = libraryManager.Games.Sum(g => g.TotalPlaytimeMinutes);
                    fs.SaveLocalProfile(profile);
                    fs.UpdateMyStatus(ActivityStatus.Online, "");
                    fs.LogActivity($"Finished playing {game.Title} (Session: {sessionMins} mins)");

                    // Notify UI
                    GameExited?.Invoke(this, game.Id);
                }
                catch (Exception ex)
                {
                    lock (_lock)
                    {
                        _runningGames.Remove(game.Id);
                    }
                    Debug.WriteLine($"Error tracking playtime/session for game {game.Title}: {ex.Message}");
                    GameExited?.Invoke(this, game.Id);
                }
            });
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }
    }
}
