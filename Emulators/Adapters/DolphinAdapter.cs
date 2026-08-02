using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher.Emulators.Adapters
{
    public class DolphinAdapter : IEmulatorAdapter
    {
        public string EmulatorId => "dolphin";

        public bool IsInstalled()
        {
            return EmulatorManager.Instance.VerifyExecutable(EmulatorId);
        }

        public bool CanRun(Game game)
        {
            return string.Equals(game.Platform, "Nintendo GameCube", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(game.Platform, "Nintendo Wii", StringComparison.OrdinalIgnoreCase);
        }

        public string GetExecutablePath()
        {
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu == null || string.IsNullOrEmpty(emu.ExecutablePath)) return "";
            return ApplicationPaths.ResolveWritablePath(emu.ExecutablePath);
        }

        public ProcessStartInfo BuildLaunchCommand(Game game)
        {
            string exePath = GetExecutablePath();
            string romPath = ApplicationPaths.ResolveWritablePath(game.RomPath);
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false
            };

            string defaultArgs = emu?.DefaultLaunchArguments ?? "-e";
            var parts = defaultArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                psi.ArgumentList.Add(part);
            }

            psi.ArgumentList.Add(romPath);
            return psi;
        }

        public async Task<Process> LaunchGameAsync(Game game)
        {
            if (!ValidateGame(game))
            {
                throw new FileNotFoundException("Dolphin executable or game ROM is missing.");
            }

            return await Task.Run(() =>
            {
                ProcessStartInfo psi = BuildLaunchCommand(game);
                Process? process = Process.Start(psi);
                if (process == null) throw new Exception("Failed to start Dolphin process.");
                return process;
            });
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = ApplicationPaths.ResolveWritablePath(game.RomPath);
            return File.Exists(romPath) || Directory.Exists(romPath);
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(ApplicationPaths.EmulatorsDir, "Dolphin") : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "User", "ScreenShots");
        }

        public GlobalControllerConfig ImportControllerConfiguration()
        {
            string iniPath = GetGCPadIniPath();
            var globalConfig = new GlobalControllerConfig();

            if (!File.Exists(iniPath)) return globalConfig;

            try
            {
                var ini = IniFileParser.ParseFile(iniPath);
                for (int i = 1; i <= 4; i++)
                {
                    string sectionName = $"GCPad{i}";
                    if (ini.ContainsKey(sectionName))
                    {
                        var sec = ini[sectionName];
                        var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == i) ?? new PlayerControllerConfig { PlayerIndex = i };
                        if (sec.TryGetValue("Device", out string? devVal)) player.DeviceGuidOrName = devVal;
                    }
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Dolphin controller import warning: {ex.Message}", "WARNING");
            }

            return globalConfig;
        }

        public bool ExportControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            return ApplyGlobalControllerConfiguration(globalConfig);
        }

        public bool ApplyGlobalControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            string iniPath = GetGCPadIniPath();
            string? dir = Path.GetDirectoryName(iniPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                var ini = File.Exists(iniPath) ? IniFileParser.ParseFile(iniPath) : new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

                for (int i = 1; i <= 4; i++)
                {
                    string sectionName = $"GCPad{i}";
                    if (!ini.ContainsKey(sectionName)) ini[sectionName] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    var sec = ini[sectionName];
                    var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == i) ?? new PlayerControllerConfig { PlayerIndex = i };

                    sec["Device"] = string.IsNullOrEmpty(player.DeviceGuidOrName) ? "DInput/0/Keyboard Mouse" : player.DeviceGuidOrName;
                    sec["Main Stick/Dead Zone"] = (player.Deadzone * 100).ToString("F1");
                    sec["C-Stick/Dead Zone"] = (player.Deadzone * 100).ToString("F1");

                    foreach (var kvp in player.ButtonMappings)
                    {
                        sec[kvp.Key] = kvp.Value;
                    }
                }

                IniFileParser.WriteFile(iniPath, ini);
                return true;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Dolphin controller config update failed: {ex.Message}", "ERROR");
                return false;
            }
        }

        private string GetGCPadIniPath()
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(ApplicationPaths.EmulatorsDir, "Dolphin") : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "User", "Config", "GCPadNew.ini");
        }
    }
}
