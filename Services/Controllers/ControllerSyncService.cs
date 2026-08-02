using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher.Services.Controllers
{
    public class ControllerSyncResult
    {
        public string EmulatorId { get; set; } = "";
        public string EmulatorName { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Exception? Exception { get; set; }
    }

    public interface IControllerSyncService
    {
        bool IsEmulatorRunning(string emulatorId);
        Task<bool> EnsureEmulatorNotRunningAsync(string emulatorId, IWin32Window? owner = null);

        Task<ControllerSyncResult> ApplyGlobalProfileToEmulatorAsync(string emulatorId, bool skipRunningCheck = false, IWin32Window? owner = null);
        Task<ControllerSyncResult> ImportFromEmulatorAsync(string emulatorId);
        Task<ControllerSyncResult> ExportToEmulatorAsync(string emulatorId, IWin32Window? owner = null);

        Task<List<ControllerSyncResult>> SyncAllEmulatorsAsync(IWin32Window? owner = null);
    }

    public class ControllerSyncService : IControllerSyncService
    {
        private static ControllerSyncService? _instance;
        public static ControllerSyncService Instance => _instance ??= new ControllerSyncService();

        public bool IsEmulatorRunning(string emulatorId)
        {
            if (string.IsNullOrEmpty(emulatorId)) return false;

            var adapter = EmulatorAdapterRegistry.GetAdapterByEmulatorId(emulatorId);
            string exePath = adapter?.GetExecutablePath() ?? "";
            string exeName = !string.IsNullOrEmpty(exePath) ? Path.GetFileNameWithoutExtension(exePath) : emulatorId;

            try
            {
                var processes = Process.GetProcessesByName(exeName);
                if (processes.Length > 0) return true;

                // Alternate process names for known emulators
                string[] extraNames = emulatorId.ToLower().Trim() switch
                {
                    "pcsx2" => new[] { "pcsx2-qt", "pcsx2x64", "pcsx2" },
                    "duckstation" => new[] { "duckstation-qt", "duckstation-nogui", "duckstation" },
                    "rpcs3" => new[] { "rpcs3" },
                    "dolphin" => new[] { "dolphin", "dolphinwx" },
                    "ppsspp" => new[] { "ppssppwindows64", "ppssppwindows", "ppsspp" },
                    _ => Array.Empty<string>()
                };

                foreach (var name in extraNames)
                {
                    if (Process.GetProcessesByName(name).Length > 0) return true;
                }
            }
            catch { }

            return false;
        }

        public async Task<bool> EnsureEmulatorNotRunningAsync(string emulatorId, IWin32Window? owner = null)
        {
            if (!IsEmulatorRunning(emulatorId)) return true;

            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase));
            string name = emuItem?.Name ?? emulatorId;

            DialogResult answer = MessageBox.Show(
                owner,
                $"Emulator '{name}' is currently running. Would you like to close it before applying controller settings?",
                "Emulator Running",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    var adapter = EmulatorAdapterRegistry.GetAdapterByEmulatorId(emulatorId);
                    string exePath = adapter?.GetExecutablePath() ?? "";
                    string exeName = !string.IsNullOrEmpty(exePath) ? Path.GetFileNameWithoutExtension(exePath) : emulatorId;

                    var processes = Process.GetProcessesByName(exeName).ToList();
                    foreach (var proc in processes)
                    {
                        try
                        {
                            proc.CloseMainWindow();
                            if (!proc.WaitForExit(3000))
                            {
                                proc.Kill();
                            }
                        }
                        catch { }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to close emulator process: {ex.Message}", "WARNING");
                    return false;
                }
            });
        }

        public async Task<ControllerSyncResult> ApplyGlobalProfileToEmulatorAsync(string emulatorId, bool skipRunningCheck = false, IWin32Window? owner = null)
        {
            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase));
            string displayName = emuItem?.Name ?? emulatorId;

            if (!skipRunningCheck)
            {
                bool canProceed = await EnsureEmulatorNotRunningAsync(emulatorId, owner);
                if (!canProceed)
                {
                    return new ControllerSyncResult
                    {
                        EmulatorId = emulatorId,
                        EmulatorName = displayName,
                        Success = false,
                        Message = "Operation cancelled because emulator is running."
                    };
                }
            }

            try
            {
                var adapter = EmulatorAdapterRegistry.GetAdapterByEmulatorId(emulatorId);
                if (adapter == null)
                {
                    return new ControllerSyncResult
                    {
                        EmulatorId = emulatorId,
                        EmulatorName = displayName,
                        Success = false,
                        Message = "No adapter registered for this emulator."
                    };
                }

                var globalConfig = GlobalControllerConfigManager.Instance.Config;
                bool success = adapter.ApplyGlobalControllerConfiguration(globalConfig);

                return new ControllerSyncResult
                {
                    EmulatorId = emulatorId,
                    EmulatorName = displayName,
                    Success = success,
                    Message = success ? "Global controller profile applied successfully." : "Failed to apply global controller profile."
                };
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Error applying global controller profile for {emulatorId}: {ex.Message}", "ERROR");
                return new ControllerSyncResult
                {
                    EmulatorId = emulatorId,
                    EmulatorName = displayName,
                    Success = false,
                    Message = ex.Message,
                    Exception = ex
                };
            }
        }

        public async Task<ControllerSyncResult> ImportFromEmulatorAsync(string emulatorId)
        {
            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase));
            string displayName = emuItem?.Name ?? emulatorId;

            return await Task.Run(() =>
            {
                try
                {
                    var adapter = EmulatorAdapterRegistry.GetAdapterByEmulatorId(emulatorId);
                    if (adapter == null)
                    {
                        return new ControllerSyncResult
                        {
                            EmulatorId = emulatorId,
                            EmulatorName = displayName,
                            Success = false,
                            Message = "No adapter registered for this emulator."
                        };
                    }

                    var importedConfig = adapter.ImportControllerConfiguration();
                    GlobalControllerConfigManager.Instance.Config.Players = importedConfig.Players;
                    GlobalControllerConfigManager.Instance.Save();

                    return new ControllerSyncResult
                    {
                        EmulatorId = emulatorId,
                        EmulatorName = displayName,
                        Success = true,
                        Message = "Imported controller configuration from emulator successfully."
                    };
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Error importing controller config for {emulatorId}: {ex.Message}", "ERROR");
                    return new ControllerSyncResult
                    {
                        EmulatorId = emulatorId,
                        EmulatorName = displayName,
                        Success = false,
                        Message = ex.Message,
                        Exception = ex
                    };
                }
            });
        }

        public async Task<ControllerSyncResult> ExportToEmulatorAsync(string emulatorId, IWin32Window? owner = null)
        {
            return await ApplyGlobalProfileToEmulatorAsync(emulatorId, false, owner);
        }

        public async Task<List<ControllerSyncResult>> SyncAllEmulatorsAsync(IWin32Window? owner = null)
        {
            var results = new List<ControllerSyncResult>();
            var emulators = EmulatorManager.Instance.Config.Emulators;

            foreach (var emu in emulators)
            {
                try
                {
                    var result = await ApplyGlobalProfileToEmulatorAsync(emu.Id, false, owner);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new ControllerSyncResult
                    {
                        EmulatorId = emu.Id,
                        EmulatorName = emu.Name,
                        Success = false,
                        Message = ex.Message,
                        Exception = ex
                    });
                }
            }

            return results;
        }
    }
}
