using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher
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
            // First check if the emulatorId itself is already a path that exists
            if (File.Exists(EmulatorId)) return Path.GetFullPath(EmulatorId);

            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emu != null && !string.IsNullOrEmpty(emu.ExecutablePath))
            {
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emu.ExecutablePath));
            }

            // Also check if any emulator in config matches by name/id
            var emuByPath = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.ExecutablePath, EmulatorId, StringComparison.OrdinalIgnoreCase));
            if (emuByPath != null)
            {
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emuByPath.ExecutablePath));
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EmulatorId));
        }

        public ProcessStartInfo BuildLaunchCommand(Game game)
        {
            string exePath = GetExecutablePath();
            string romPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, game.RomPath));
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
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", EmulatorId, "screenshots"));
        }
    }
}
