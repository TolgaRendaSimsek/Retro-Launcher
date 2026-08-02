using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher.Emulators.Adapters
{
    public class RPCS3Adapter : IEmulatorAdapter
    {
        public string EmulatorId => "rpcs3";

        public bool IsInstalled()
        {
            return EmulatorManager.Instance.VerifyExecutable(EmulatorId);
        }

        public bool CanRun(Game game)
        {
            return string.Equals(game.Platform, "Sony PlayStation 3", StringComparison.OrdinalIgnoreCase);
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

            string defaultArgs = emu?.DefaultLaunchArguments ?? "--no-gui";
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
                throw new FileNotFoundException("RPCS3 executable or EBOOT.BIN / PS3 game directory is missing.");
            }

            return await Task.Run(() =>
            {
                ProcessStartInfo psi = BuildLaunchCommand(game);
                Process? process = Process.Start(psi);
                if (process == null)
                {
                    throw new Exception("Failed to start RPCS3 process.");
                }
                return process;
            });
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, game.RomPath));
            if (string.IsNullOrEmpty(game.RomPath)) return false;

            if (File.Exists(romPath)) return true;

            if (Directory.Exists(romPath))
            {
                string ebootFile = Path.Combine(romPath, "PS3_GAME", "USRDIR", "EBOOT.BIN");
                if (File.Exists(ebootFile)) return true;

                string directEboot = Path.Combine(romPath, "EBOOT.BIN");
                if (File.Exists(directEboot)) return true;
            }

            return false;
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Emulators", "RPCS3", "captures"));
        }

        public GlobalControllerConfig ImportControllerConfiguration()
        {
            string yamlPath = GetInputConfigPath();
            var globalConfig = new GlobalControllerConfig();

            if (!File.Exists(yamlPath)) return globalConfig;

            try
            {
                string[] lines = File.ReadAllLines(yamlPath);
                for (int i = 1; i <= 4; i++)
                {
                    string playerHeader = $"Player {i} Pad:";
                    if (lines.Any(l => l.Contains(playerHeader)))
                    {
                        var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == i) ?? new PlayerControllerConfig { PlayerIndex = i };
                        player.ControllerType = "XInput";
                    }
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"RPCS3 controller import warning: {ex.Message}", "WARNING");
            }

            return globalConfig;
        }

        public bool ExportControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            return ApplyGlobalControllerConfiguration(globalConfig);
        }

        public bool ApplyGlobalControllerConfiguration(GlobalControllerConfig globalConfig)
        {
            string configPath = GetInputConfigPath();
            string? dir = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                // RPCS3 YAML pad configuration creation / update
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("# RPCS3 Input Configuration - Generated by Retro Launcher");
                for (int i = 1; i <= 4; i++)
                {
                    var player = globalConfig.Players.FirstOrDefault(p => p.PlayerIndex == i) ?? new PlayerControllerConfig { PlayerIndex = i };
                    sb.AppendLine($"Player {i} Pad:");
                    sb.AppendLine($"  Handler: {player.ControllerType}");
                    sb.AppendLine($"  Device: {player.DeviceGuidOrName}");
                    sb.AppendLine($"  Left Stick Deadzone: {player.Deadzone:F2}");
                    sb.AppendLine($"  Right Stick Deadzone: {player.Deadzone:F2}");
                    sb.AppendLine($"  Trigger Threshold: {player.TriggerThreshold:F2}");
                    sb.AppendLine($"  Vibration: {player.EnableRumble}");
                }

                File.WriteAllText(configPath, sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"RPCS3 controller config update failed: {ex.Message}", "ERROR");
                return false;
            }
        }

        private string GetInputConfigPath()
        {
            string exePath = GetExecutablePath();
            string emuDir = string.IsNullOrEmpty(exePath) ? Path.Combine(AppContext.BaseDirectory, "Emulators", "RPCS3") : Path.GetDirectoryName(exePath) ?? "";
            return Path.Combine(emuDir, "config_input.yml");
        }
    }
}
