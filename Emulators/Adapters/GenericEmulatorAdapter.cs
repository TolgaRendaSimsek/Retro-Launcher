using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroLauncher.Emulators.Adapters
{
    public class GenericEmulatorAdapter : IEmulatorAdapter
    {
        public string EmulatorId { get; }

        public GenericEmulatorAdapter(string emulatorId)
        {
            EmulatorId = emulatorId;
        }

        public bool IsInstalled()
        {
            return EmulatorManager.Instance.VerifyExecutable(EmulatorId);
        }

        public bool CanRun(Game game)
        {
            return true; // Fallback matches anything
        }

        public string GetExecutablePath()
        {
            if (File.Exists(EmulatorId)) return Path.GetFullPath(EmulatorId);

            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu != null && !string.IsNullOrEmpty(emu.ExecutablePath))
            {
                return ApplicationPaths.ResolveWritablePath(emu.ExecutablePath);
            }

            var emuByPath = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.ExecutablePath, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emuByPath != null)
            {
                return ApplicationPaths.ResolveWritablePath(emuByPath.ExecutablePath);
            }

            return ApplicationPaths.ResolveWritablePath(EmulatorId);
        }

        public ProcessStartInfo BuildLaunchCommand(Game game)
        {
            string exePath = GetExecutablePath();
            string romPath = ApplicationPaths.ResolveWritablePath(game.RomPath);
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase) || string.Equals(e.ExecutablePath, EmulatorId, StringComparison.OrdinalIgnoreCase));
            string defaultArgs = emu?.DefaultLaunchArguments ?? "";

            string args = defaultArgs;
            if (!string.IsNullOrEmpty(args))
            {
                args += $" \"{romPath}\"";
            }
            else
            {
                args = $"\"{romPath}\"";
            }

            return new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = true
            };
        }

        public async Task<Process> LaunchGameAsync(Game game)
        {
            if (!ValidateGame(game))
            {
                throw new FileNotFoundException("Emulator executable or ROM path is missing.");
            }

            return await Task.Run(() =>
            {
                ProcessStartInfo psi = BuildLaunchCommand(game);
                Process? process = Process.Start(psi);
                if (process == null)
                {
                    throw new Exception($"Failed to start process for emulator {EmulatorId}.");
                }
                return process;
            });
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = ApplicationPaths.ResolveWritablePath(game.RomPath);
            if (string.IsNullOrEmpty(game.RomPath) || (!File.Exists(romPath) && !Directory.Exists(romPath))) return false;

            return true;
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(ApplicationPaths.EmulatorsDir, EmulatorId) : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "screenshots");
        }

        public GlobalControllerConfig ImportControllerConfiguration()
        {
            string cfgFile = GetGenericConfigFile();
            var globalConfig = new GlobalControllerConfig();

            if (File.Exists(cfgFile))
            {
                try
                {
                    string json = File.ReadAllText(cfgFile);
                    globalConfig = JsonSerializer.Deserialize<GlobalControllerConfig>(json) ?? globalConfig;
                }
                catch { }
            }

            return globalConfig;
        }

        public bool ExportControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            return ApplyGlobalControllerConfiguration(globalConfig);
        }

        public bool ApplyGlobalControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            string cfgFile = GetGenericConfigFile();
            string? dir = Path.GetDirectoryName(cfgFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                string json = JsonSerializer.Serialize(globalConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(cfgFile, json);
                return true;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Generic emulator controller update failed: {ex.Message}", "ERROR");
                return false;
            }
        }

        private string GetGenericConfigFile()
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(ApplicationPaths.EmulatorsDir, EmulatorId) : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "controller_profile.json");
        }
    }
}
