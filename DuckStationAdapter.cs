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
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emu.ExecutablePath));
        }

        public ProcessStartInfo BuildLaunchCommand(Game game)
        {
            string exePath = GetExecutablePath();
            string romPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, game.RomPath));
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            string defaultArgs = emu?.DefaultLaunchArguments ?? "-fullscreen";
            if (!defaultArgs.Contains("-nogui"))
            {
                defaultArgs += " -nogui";
            }

            return new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"{defaultArgs} \"{romPath}\"",
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
                    throw new Exception("Failed to start DuckStation process.");
                }
                return process;
            });
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, game.RomPath));
            if (string.IsNullOrEmpty(game.RomPath) || (!File.Exists(romPath) && !Directory.Exists(romPath))) return false;

            return true;
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emulators", "PS1", "screenshots"));
        }
    }
}
