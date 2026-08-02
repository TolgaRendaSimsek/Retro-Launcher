using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class PPSSPPAdapter : IEmulatorAdapter
    {
        public string EmulatorId => "ppsspp";

        public bool IsInstalled()
        {
            return EmulatorManager.Instance.VerifyExecutable(EmulatorId);
        }

        public bool CanRun(Game game)
        {
            return string.Equals(game.Platform, "Sony PlayStation Portable", StringComparison.OrdinalIgnoreCase);
        }

        public string GetExecutablePath()
        {
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu == null || string.IsNullOrEmpty(emu.ExecutablePath)) return "";
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.ExecutablePath));
        }

        public ProcessStartInfo BuildLaunchCommand(Game game)
        {
            string exePath = GetExecutablePath();
            string romPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, game.RomPath));
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false
            };

            string defaultArgs = emu?.DefaultLaunchArguments ?? "";
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
                throw new FileNotFoundException("PPSSPP executable or PSP ROM is missing.");
            }

            return await Task.Run(() =>
            {
                ProcessStartInfo psi = BuildLaunchCommand(game);
                Process? process = Process.Start(psi);
                if (process == null) throw new Exception("Failed to start PPSSPP process.");
                return process;
            });
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, game.RomPath));
            return File.Exists(romPath) || Directory.Exists(romPath);
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(AppContext.BaseDirectory, "Emulators", "PPSSPP") : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "memstick", "PSP", "SCREENSHOT");
        }

        public GlobalControllerConfig ImportControllerConfiguration()
        {
            string iniPath = GetControlsIniPath();
            var globalConfig = new GlobalControllerConfig();

            if (!File.Exists(iniPath)) return globalConfig;

            try
            {
                var ini = IniFileParser.ParseFile(iniPath);
                if (ini.ContainsKey("Control"))
                {
                    var sec = ini["Control"];
                    var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == 1) ?? new PlayerControllerConfig { PlayerIndex = 1 };
                    if (sec.TryGetValue("AnalogDeadzone", out string? dzVal) && float.TryParse(dzVal, out float dz)) player.Deadzone = dz;
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"PPSSPP controller import warning: {ex.Message}", "WARNING");
            }

            return globalConfig;
        }

        public bool ExportControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            return ApplyGlobalControllerConfiguration(globalConfig);
        }

        public bool ApplyGlobalControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            string iniPath = GetControlsIniPath();
            string? dir = Path.GetDirectoryName(iniPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                var ini = File.Exists(iniPath) ? IniFileParser.ParseFile(iniPath) : new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

                if (!ini.ContainsKey("Control")) ini["Control"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var sec = ini["Control"];

                var p1 = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == 1) ?? new PlayerControllerConfig { PlayerIndex = 1 };
                sec["AnalogDeadzone"] = p1.Deadzone.ToString("F2");
                sec["AnalogSensitivity"] = p1.Sensitivity.ToString("F2");

                IniFileParser.WriteFile(iniPath, ini);
                return true;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"PPSSPP controller config update failed: {ex.Message}", "ERROR");
                return false;
            }
        }

        private string GetControlsIniPath()
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(AppContext.BaseDirectory, "Emulators", "PPSSPP") : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "memstick", "PSP", "SYSTEM", "controls.ini");
        }
    }
}
