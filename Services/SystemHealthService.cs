using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RetroLauncher.Services
{
    public class SystemHealthService : ISystemHealthService
    {
        private readonly IEmulatorPackageDefinitionProvider _definitionProvider;
        private readonly IEmulatorUpdateService _updateService;

        public SystemHealthService(
            IEmulatorPackageDefinitionProvider? definitionProvider = null,
            IEmulatorUpdateService? updateService = null)
        {
            _definitionProvider = definitionProvider ?? new JsonEmulatorPackageDefinitionProvider();
            _updateService = updateService ?? new EmulatorUpdateService();
        }

        public async Task<HealthCheckResult> RunHealthCheckAsync(IProgress<int>? progress, CancellationToken cancellationToken)
        {
            var result = new HealthCheckResult();
            var completedList = new List<HealthCheckItem>();
            var lockObj = new object();

            // List of health check tasks to run
            var tasks = new List<Func<Task<List<HealthCheckItem>>>>();

            // 1. Application Checks
            tasks.Add(async () => CheckApplicationHealth());

            // 2. Emulator Checks
            var emus = EmulatorManager.Instance.Config.Emulators;
            foreach (var emu in emus)
            {
                var capturedEmu = emu;
                tasks.Add(async () => await CheckEmulatorHealthAsync(capturedEmu, cancellationToken));
            }

            // 3. Game Checks
            var libraryManager = new GameLibraryManager();
            var games = libraryManager.Games;
            foreach (var game in games)
            {
                var capturedGame = game;
                tasks.Add(async () => CheckGameHealth(capturedGame));
            }

            int totalTasks = tasks.Count;
            int completedTasks = 0;

            if (totalTasks == 0)
            {
                result.Items = completedList;
                return result;
            }

            // Safe concurrency throttling using SemaphoreSlim
            var semaphore = new SemaphoreSlim(4);
            var runningTasks = tasks.Select(async taskFunc =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var taskItems = await taskFunc();
                    lock (lockObj)
                    {
                        completedList.AddRange(taskItems);
                    }
                }
                finally
                {
                    semaphore.Release();
                    int val = Interlocked.Increment(ref completedTasks);
                    progress?.Report((int)((double)val / totalTasks * 100));
                }
            });

            await Task.WhenAll(runningTasks);

            result.Items = completedList;
            result.HealthyCount = completedList.Count(i => i.Status == HealthStatus.Healthy);
            result.WarningCount = completedList.Count(i => i.Status == HealthStatus.Warning);
            result.ErrorCount = completedList.Count(i => i.Status == HealthStatus.Error);
            result.UnknownCount = completedList.Count(i => i.Status == HealthStatus.Unknown);

            return result;
        }

        public async Task<bool> ExecuteFixAsync(HealthCheckItem item, CancellationToken cancellationToken)
        {
            if (item == null || item.FixAction == HealthFixAction.None) return false;

            switch (item.FixAction)
            {
                case HealthFixAction.ClearStaleStaging:
                    try
                    {
                        string tempDir = Path.Combine(AppContext.BaseDirectory, "temp");
                        if (Directory.Exists(tempDir))
                        {
                            foreach (var dir in Directory.GetDirectories(tempDir))
                            {
                                string name = Path.GetFileName(dir);
                                if (name.StartsWith("staging_", StringComparison.OrdinalIgnoreCase) ||
                                    name.StartsWith("backup_", StringComparison.OrdinalIgnoreCase))
                                {
                                    Directory.Delete(dir, true);
                                }
                            }
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        RetroLogger.Log($"Health fix ClearStaleStaging failed: {ex.Message}", "ERROR");
                        return false;
                    }

                case HealthFixAction.OpenLogsFolder:
                    try
                    {
                        string logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
                        if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
                        Process.Start("explorer.exe", $"\"{logsDir}\"");
                        return true;
                    }
                    catch
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        private List<HealthCheckItem> CheckApplicationHealth()
        {
            var items = new List<HealthCheckItem>();

            // Config path write check
            string configDir = AppContext.BaseDirectory;
            var configCheck = new HealthCheckItem
            {
                Title = "Application Configuration Directory",
                Description = "Verifies that the application can read and write configuration settings.",
                TechnicalDetail = $"Path: {configDir}"
            };

            try
            {
                string testFile = Path.Combine(configDir, "health_write_test.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                configCheck.Status = HealthStatus.Healthy;
            }
            catch (Exception ex)
            {
                configCheck.Status = HealthStatus.Error;
                configCheck.SuggestedFix = "Ensure Retro Launcher is not running from a write-protected directory.";
                configCheck.TechnicalDetail += $"\nError: {ex.Message}";
            }
            items.Add(configCheck);

            // Temp path write check
            string tempDir = Path.Combine(AppContext.BaseDirectory, "temp");
            var tempCheck = new HealthCheckItem
            {
                Title = "Temporary Directory Access",
                Description = "Verifies that Retro Launcher can write temporary package downloads.",
                TechnicalDetail = $"Path: {tempDir}"
            };

            try
            {
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                string testFile = Path.Combine(tempDir, "write_test.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                tempCheck.Status = HealthStatus.Healthy;
            }
            catch (Exception ex)
            {
                tempCheck.Status = HealthStatus.Error;
                tempCheck.SuggestedFix = "Change permissions on the 'temp' directory or run the launcher as administrator.";
                tempCheck.TechnicalDetail += $"\nError: {ex.Message}";
            }
            items.Add(tempCheck);

            // Disk space check
            var diskCheck = new HealthCheckItem
            {
                Title = "Disk Space Availability",
                Description = "Verifies that there is enough free disk space to download and install emulators."
            };

            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "C:");
                long freeMb = drive.AvailableFreeSpace / (1024 * 1024);
                diskCheck.TechnicalDetail = $"Drive: {drive.Name}, Free Space: {freeMb} MB";
                if (freeMb > 500)
                {
                    diskCheck.Status = HealthStatus.Healthy;
                }
                else
                {
                    diskCheck.Status = HealthStatus.Warning;
                    diskCheck.SuggestedFix = "Free up some space on your disk to ensure emulator installations succeed.";
                }
            }
            catch (Exception ex)
            {
                diskCheck.Status = HealthStatus.Unknown;
                diskCheck.TechnicalDetail = $"Error: {ex.Message}";
            }
            items.Add(diskCheck);

            // Stale staging directories check
            var staleCheck = new HealthCheckItem
            {
                Title = "Stale Staging Directories",
                Description = "Checks for leftover staging folders inside the temp directory."
            };

            try
            {
                bool hasStale = false;
                if (Directory.Exists(tempDir))
                {
                    var dirs = Directory.GetDirectories(tempDir);
                    int staleCount = dirs.Count(d => {
                        string name = Path.GetFileName(d);
                        return name.StartsWith("staging_", StringComparison.OrdinalIgnoreCase) ||
                               name.StartsWith("backup_", StringComparison.OrdinalIgnoreCase);
                    });
                    if (staleCount > 0)
                    {
                        hasStale = true;
                        staleCheck.Status = HealthStatus.Warning;
                        staleCheck.SuggestedFix = "Clear the stale staging directories to reclaim disk space.";
                        staleCheck.FixAction = HealthFixAction.ClearStaleStaging;
                        staleCheck.TechnicalDetail = $"Found {staleCount} stale directory entries in temp folder.";
                    }
                }
                if (!hasStale)
                {
                    staleCheck.Status = HealthStatus.Healthy;
                    staleCheck.TechnicalDetail = "No leftover staging directories found.";
                }
            }
            catch (Exception ex)
            {
                staleCheck.Status = HealthStatus.Unknown;
                staleCheck.TechnicalDetail = $"Error: {ex.Message}";
            }
            items.Add(staleCheck);

            // Network check without blocking
            var netCheck = new HealthCheckItem
            {
                Title = "GitHub API Network Check",
                Description = "Checks connection status to the GitHub API endpoints."
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher");
                    var response = client.GetAsync("https://api.github.com", HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        netCheck.Status = HealthStatus.Healthy;
                        netCheck.TechnicalDetail = "GitHub API is online and responding.";
                    }
                    else
                    {
                        netCheck.Status = HealthStatus.Warning;
                        netCheck.TechnicalDetail = $"GitHub returned code: {response.StatusCode}";
                        netCheck.SuggestedFix = "Check your internet connection or try again later if GitHub is experiencing issues.";
                    }
                }
            }
            catch (Exception ex)
            {
                netCheck.Status = HealthStatus.Warning;
                netCheck.TechnicalDetail = $"Connection timed out or failed: {ex.Message}";
                netCheck.SuggestedFix = "Check your network settings. Locally installed emulators remain usable.";
            }
            items.Add(netCheck);

            return items;
        }

        private async Task<List<HealthCheckItem>> CheckEmulatorHealthAsync(EmulatorItem emu, CancellationToken cancellationToken)
        {
            var items = new List<HealthCheckItem>();
            var definition = _definitionProvider.GetById(emu.Id);

            if (definition == null)
            {
                items.Add(new HealthCheckItem
                {
                    Title = $"Definition for {emu.Name}",
                    Description = "Verifies that a valid json definition file exists for the emulator.",
                    Status = HealthStatus.Error,
                    RelatedEmulatorId = emu.Id,
                    SuggestedFix = "Restore default emulator definitions."
                });
                return items;
            }

            // Executable path check
            string resolvedPath = string.IsNullOrEmpty(emu.Path) ? "" : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.Path));
            bool exeExists = !string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath);

            var exeCheck = new HealthCheckItem
            {
                Title = $"{emu.Name} Executable Existence",
                Description = "Verifies that the emulator executable file exists on disk.",
                RelatedEmulatorId = emu.Id,
                TechnicalDetail = $"Expected Path: {emu.Path}"
            };

            if (exeExists)
            {
                exeCheck.Status = HealthStatus.Healthy;
                
                // Expected directory check (is it manual or standard?)
                if (!string.IsNullOrEmpty(emu.InstallFolder))
                {
                    string standardPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.InstallFolder));
                    string resolvedDir = Path.GetDirectoryName(resolvedPath) ?? "";
                    if (!resolvedDir.StartsWith(standardPath, StringComparison.OrdinalIgnoreCase))
                    {
                        items.Add(new HealthCheckItem
                        {
                            Title = $"{emu.Name} Installation Path Classification",
                            Description = "Classifies whether the emulator installation is manual or automatic.",
                            Status = HealthStatus.Healthy,
                            RelatedEmulatorId = emu.Id,
                            TechnicalDetail = "Manual configuration active. Update checks will continue based on repository configuration."
                        });
                    }
                }
            }
            else
            {
                exeCheck.Status = HealthStatus.Error;
                exeCheck.SuggestedFix = "Click Install/Update or locate the executable path manually.";
                exeCheck.FixAction = HealthFixAction.SelectExecutable;
            }
            items.Add(exeCheck);

            // Running check
            var runCheck = new HealthCheckItem
            {
                Title = $"{emu.Name} Process Execution Status",
                Description = "Checks if there is an active running instance of the emulator.",
                RelatedEmulatorId = emu.Id,
                Status = HealthStatus.Healthy
            };

            foreach (var exeCandidate in definition.ExecutableCandidates)
            {
                string processName = Path.GetFileNameWithoutExtension(exeCandidate);
                var active = Process.GetProcessesByName(processName);
                if (active.Any())
                {
                    runCheck.Status = HealthStatus.Warning;
                    runCheck.TechnicalDetail = $"Process '{processName}' is currently running.";
                    runCheck.SuggestedFix = "Close the emulator before attempting updates or repairs.";
                    break;
                }
            }
            if (runCheck.Status == HealthStatus.Healthy)
            {
                runCheck.TechnicalDetail = "No active processes detected.";
            }
            items.Add(runCheck);

            // BIOS / Firmware existence checks
            if (definition.RequiresBios)
            {
                bool biosExists = BiosManager.Instance.CheckRealBiosExists(definition.ConsoleName);
                var biosCheck = new HealthCheckItem
                {
                    Title = $"{emu.Name} System BIOS Status",
                    Description = $"Verifies that a valid BIOS is present in the centralized directory for '{definition.ConsoleName}'.",
                    RelatedEmulatorId = emu.Id
                };

                if (biosExists)
                {
                    biosCheck.Status = HealthStatus.Healthy;
                    biosCheck.TechnicalDetail = "BIOS file detected in centralized BIOS manager.";
                }
                else
                {
                    biosCheck.Status = HealthStatus.Error;
                    biosCheck.SuggestedFix = "Import a legally obtained BIOS file inside the BIOS/Firmware Manager.";
                    biosCheck.FixAction = HealthFixAction.OpenBiosManager;
                }
                items.Add(biosCheck);
            }

            if (definition.RequiresFirmware)
            {
                bool fwExists = BiosManager.Instance.CheckRealBiosExists(definition.ConsoleName);
                var fwCheck = new HealthCheckItem
                {
                    Title = $"{emu.Name} System Firmware Status",
                    Description = $"Verifies that system firmware is ready in the centralized directory for '{definition.ConsoleName}'.",
                    RelatedEmulatorId = emu.Id
                };

                if (fwExists)
                {
                    fwCheck.Status = HealthStatus.Healthy;
                    fwCheck.TechnicalDetail = "Firmware file detected in centralized BIOS manager.";
                }
                else
                {
                    fwCheck.Status = HealthStatus.Error;
                    fwCheck.SuggestedFix = "Import the required system firmware update package.";
                    fwCheck.FixAction = HealthFixAction.OpenBiosManager;
                }
                items.Add(fwCheck);
            }

            // Update status check
            if (exeExists)
            {
                try
                {
                    var updateInfo = await _updateService.CheckForUpdateAsync(emu.Id, definition.ReleaseChannel, cancellationToken);
                    var updateCheckItem = new HealthCheckItem
                    {
                        Title = $"{emu.Name} Version Status",
                        Description = "Checks if a newer build of the emulator is available on GitHub.",
                        RelatedEmulatorId = emu.Id,
                        TechnicalDetail = $"Installed: {updateInfo.InstalledVersion}, Available: {updateInfo.AvailableVersion}"
                    };

                    if (updateInfo.DisplayStatus == "Update available")
                    {
                        updateCheckItem.Status = HealthStatus.Warning;
                        updateCheckItem.SuggestedFix = "Update the emulator to the latest version.";
                        updateCheckItem.FixAction = HealthFixAction.RepairInstallation;
                    }
                    else if (updateInfo.DisplayStatus == "Unable to check")
                    {
                        updateCheckItem.Status = HealthStatus.Warning;
                        updateCheckItem.TechnicalDetail += "\n(Unable to reach update server)";
                        updateCheckItem.SuggestedFix = "Retry connection checks or continue using the locally installed version.";
                        updateCheckItem.FixAction = HealthFixAction.RetryUpdateCheck;
                    }
                    else
                    {
                        updateCheckItem.Status = HealthStatus.Healthy;
                    }
                    items.Add(updateCheckItem);
                }
                catch { }
            }

            return items;
        }

        private List<HealthCheckItem> CheckGameHealth(Game game)
        {
            var items = new List<HealthCheckItem>();

            // ROM existence check
            string resolvedRom = string.IsNullOrEmpty(game.RomPath) ? "" : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, game.RomPath));
            bool romExists = !string.IsNullOrEmpty(resolvedRom) && (File.Exists(resolvedRom) || Directory.Exists(resolvedRom));

            var romCheck = new HealthCheckItem
            {
                Title = $"ROM Path for '{game.Title}'",
                Description = "Verifies that the ROM file or directory exists on the system.",
                RelatedGameId = game.Id,
                TechnicalDetail = $"Path: {game.RomPath}"
            };

            if (romExists)
            {
                romCheck.Status = HealthStatus.Healthy;

                // Game path readable check
                try
                {
                    if (File.Exists(resolvedRom))
                    {
                        using (var stream = File.OpenRead(resolvedRom)) { }
                    }
                    else if (Directory.Exists(resolvedRom))
                    {
                        var files = Directory.GetFiles(resolvedRom);
                    }
                }
                catch (Exception ex)
                {
                    items.Add(new HealthCheckItem
                    {
                        Title = $"Read Access to '{game.Title}' ROM",
                        Description = "Verifies that the emulator has permissions to read the game ROM file.",
                        Status = HealthStatus.Error,
                        RelatedGameId = game.Id,
                        SuggestedFix = "Check folder write/read security descriptors or unlock the file.",
                        TechnicalDetail = $"Error: {ex.Message}"
                    });
                }
            }
            else
            {
                romCheck.Status = HealthStatus.Error;
                romCheck.SuggestedFix = "Select the ROM path location manually in game metadata settings.";
                romCheck.FixAction = HealthFixAction.SelectRomLocation;
            }
            items.Add(romCheck);

            // Assigned emulator existence check
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, game.EmulatorId, StringComparison.OrdinalIgnoreCase));
            var emuCheck = new HealthCheckItem
            {
                Title = $"Emulator Assigned for '{game.Title}'",
                Description = "Verifies that a compatible, registered emulator is assigned to launch the game.",
                RelatedGameId = game.Id
            };

            if (emu != null)
            {
                emuCheck.Status = HealthStatus.Healthy;
                
                string emuExe = string.IsNullOrEmpty(emu.Path) ? "" : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.Path));
                if (!File.Exists(emuExe))
                {
                    items.Add(new HealthCheckItem
                    {
                        Title = $"Assigned Emulator Installation for '{game.Title}'",
                        Description = "Verifies that the assigned emulator is installed and ready to launch.",
                        Status = HealthStatus.Error,
                        RelatedGameId = game.Id,
                        RelatedEmulatorId = emu.Id,
                        SuggestedFix = "Run the installer for the assigned emulator.",
                        FixAction = HealthFixAction.InstallEmulator
                    });
                }
            }
            else
            {
                emuCheck.Status = HealthStatus.Error;
                emuCheck.SuggestedFix = "Choose a valid emulator from the game's configuration profile.";
            }
            items.Add(emuCheck);

            return items;
        }
    }
}
