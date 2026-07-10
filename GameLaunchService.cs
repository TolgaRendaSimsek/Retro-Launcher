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
                throw new FileNotFoundException($"Emulator executable not found at:\n'{adapter.GetExecutablePath()}'");
            }

            // 4. Verify BIOS or firmware requirements
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, adapter.EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu != null)
            {
                if (emu.RequiresBIOS)
                {
                    var bios = BiosManager.Instance.BiosItems.FirstOrDefault(b => string.Equals(b.Console, game.Platform, StringComparison.OrdinalIgnoreCase));
                    if (bios == null || bios.Status != "Ready")
                    {
                        throw new InvalidOperationException($"The platform '{game.Platform}' requires a BIOS file to run, which is missing. Please configure it in the BIOS/Firmware Manager.");
                    }
                }
                else if (emu.RequiresFirmware)
                {
                    var fw = BiosManager.Instance.BiosItems.FirstOrDefault(b => string.Equals(b.Console, game.Platform, StringComparison.OrdinalIgnoreCase));
                    if (fw == null || fw.Status != "Ready")
                    {
                        throw new InvalidOperationException($"The platform '{game.Platform}' requires emulator system firmware to run, which is missing. Please configure it in the BIOS/Firmware Manager.");
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
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }
    }
}
