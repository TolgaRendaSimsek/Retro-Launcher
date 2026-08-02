using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher.Emulators.Adapters
{
    public class DuckStationAdapter : IEmulatorAdapter
    {
        public string EmulatorId => "duckstation";

        public bool IsInstalled()
        {
            return EmulatorManager.Instance.VerifyExecutable(EmulatorId);
        }

        public bool CanRun(Game game)
        {
            return string.Equals(game.Platform, "Sony PlayStation 1", StringComparison.OrdinalIgnoreCase);
        }

        public string GetExecutablePath()
        {
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu == null || string.IsNullOrEmpty(emu.ExecutablePath)) return "";
            string fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.ExecutablePath));

            // Create portable.txt beside the executable path to force DuckStation portable mode
            if (File.Exists(fullPath))
            {
                try
                {
                    string dir = Path.GetDirectoryName(fullPath) ?? "";
                    if (!string.IsNullOrEmpty(dir))
                    {
                        string portableFile = Path.Combine(dir, "portable.txt");
                        if (!File.Exists(portableFile))
                        {
                            File.WriteAllText(portableFile, "");
                        }
                    }
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to create portable.txt for DuckStation: {ex.Message}", "WARNING");
                }
            }

            return fullPath;
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

            string defaultArgs = emu?.DefaultLaunchArguments ?? "-fullscreen";
            if (!defaultArgs.Contains("-nogui"))
            {
                defaultArgs += " -nogui";
            }

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
                throw new FileNotFoundException("Emulator executable or ROM path is missing.");
            }

            return await Task.Run(() =>
            {
                ProcessStartInfo psi = BuildLaunchCommand(game);
                Process? process = Process.Start(psi);
                if (process == null)
                {
                    throw new Exception("Failed to start DuckStation process.");
                }
                return process;
            });
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, game.RomPath));
            if (string.IsNullOrEmpty(game.RomPath) || (!File.Exists(romPath) && !Directory.Exists(romPath))) return false;

            return true;
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Emulators", "PS1", "screenshots"));
        }

        public GlobalControllerConfig ImportControllerConfiguration()
        {
            string iniPath = GetIniPath();
            var globalConfig = new GlobalControllerConfig();

            if (!File.Exists(iniPath)) return globalConfig;

            try
            {
                var ini = IniFileParser.ParseFile(iniPath);
                for (int i = 1; i <= 4; i++)
                {
                    string sectionName = $"Pad{i}";
                    if (ini.ContainsKey(sectionName))
                    {
                        var sec = ini[sectionName];
                        var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == i) ?? new PlayerControllerConfig { PlayerIndex = i };
                        
                        if (sec.TryGetValue("Type", out string? typeVal)) player.ControllerType = typeVal;
                        if (sec.TryGetValue("Deadzone", out string? dzVal) && float.TryParse(dzVal, out float dz)) player.Deadzone = dz;
                        if (sec.TryGetValue("Sensitivity", out string? sensVal) && float.TryParse(sensVal, out float sens)) player.Sensitivity = sens;
                    }
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"DuckStation controller import warning: {ex.Message}", "WARNING");
            }

            return globalConfig;
        }

        public bool ExportControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            return ApplyGlobalControllerConfiguration(globalConfig);
        }

        public bool ApplyGlobalControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            string iniPath = GetIniPath();
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
                    string sectionName = $"Pad{i}";
                    if (!ini.ContainsKey(sectionName)) ini[sectionName] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    
                    var sec = ini[sectionName];
                    var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == i) ?? new PlayerControllerConfig { PlayerIndex = i };

                    sec["Type"] = player.ControllerType;
                    sec["Deadzone"] = player.Deadzone.ToString("F2");
                    sec["Sensitivity"] = player.Sensitivity.ToString("F2");
                    sec["TriggerThreshold"] = player.TriggerThreshold.ToString("F2");
                    sec["InvertLeftStickX"] = player.InvertLeftStickX.ToString();
                    sec["InvertLeftStickY"] = player.InvertLeftStickY.ToString();
                    sec["InvertRightStickX"] = player.InvertRightStickX.ToString();
                    sec["InvertRightStickY"] = player.InvertRightStickY.ToString();
                    sec["EnableRumble"] = player.EnableRumble.ToString();

                    foreach (var kvp in player.ButtonMappings)
                    {
                        sec[kvp.Key] = kvp.Value;
                    }
                }

                // Apply Hotkeys
                string hotkeySecName = "Hotkeys";
                if (!ini.ContainsKey(hotkeySecName)) ini[hotkeySecName] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var hotkeySec = ini[hotkeySecName];
                hotkeySec["Pause"] = globalConfig.Hotkeys.Pause;
                hotkeySec["SaveState"] = globalConfig.Hotkeys.SaveState;
                hotkeySec["LoadState"] = globalConfig.Hotkeys.LoadState;
                hotkeySec["FastForward"] = globalConfig.Hotkeys.FastForward;
                hotkeySec["Screenshot"] = globalConfig.Hotkeys.Screenshot;
                hotkeySec["ToggleMenu"] = globalConfig.Hotkeys.ToggleMenu;

                IniFileParser.WriteFile(iniPath, ini);
                return true;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"DuckStation controller config update failed: {ex.Message}", "ERROR");
                return false;
            }
        }

        private string GetIniPath()
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(AppContext.BaseDirectory, "Emulators", "DuckStation") : Path.GetDirectoryName(exePath) ?? "";
            
            string portableIni = Path.Combine(emuDir, "settings.ini");
            if (File.Exists(portableIni) || File.Exists(Path.Combine(emuDir, "portable.txt")))
            {
                return portableIni;
            }

            string localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DuckStation");
            return Path.Combine(localAppData, "settings.ini");
        }
    }
}
