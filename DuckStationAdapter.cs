using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher
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
            if (!EmulatorManager.Instance.VerifyExecutable(EmulatorId)) return false;

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
    }
}
