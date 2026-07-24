using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher
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
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emu.ExecutablePath));
        }

        public ProcessStartInfo BuildLaunchCommand(Game game)
        {
            string exePath = GetExecutablePath();
            string romPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, game.RomPath));
            var emu = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, EmulatorId, StringComparison.OrdinalIgnoreCase));
            
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false
            };

            // If it's a directory format game, construct the launch argument pointing to the EBOOT.BIN
            string finalRomTarget = romPath;
            if (Directory.Exists(romPath))
            {
                string eboot1 = Path.Combine(romPath, "PS3_GAME", "USRDIR", "EBOOT.BIN");
                string eboot2 = Path.Combine(romPath, "USRDIR", "EBOOT.BIN");
                if (File.Exists(eboot1)) finalRomTarget = eboot1;
                else if (File.Exists(eboot2)) finalRomTarget = eboot2;
            }

            string defaultArgs = emu?.DefaultLaunchArguments ?? "--fullscreen";
            if (!defaultArgs.Contains("--no-gui"))
            {
                defaultArgs += " --no-gui";
            }

            var parts = defaultArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                psi.ArgumentList.Add(part);
            }

            psi.ArgumentList.Add(finalRomTarget);

            return psi;
        }

        public async Task<Process> LaunchGameAsync(Game game)
        {
            ValidateLaunchRequirements(game);

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

        public void ValidateLaunchRequirements(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                throw new FileNotFoundException("RPCS3 emulator is not installed.");
            }

            string romPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, game.RomPath));
            if (string.IsNullOrEmpty(game.RomPath) || (!File.Exists(romPath) && !Directory.Exists(romPath)))
            {
                throw new FileNotFoundException($"PlayStation 3 game file or folder not found at:\n'{game.RomPath}'");
            }

            if (Directory.Exists(romPath))
            {
                string eboot1 = Path.Combine(romPath, "PS3_GAME", "USRDIR", "EBOOT.BIN");
                string eboot2 = Path.Combine(romPath, "USRDIR", "EBOOT.BIN");
                if (!File.Exists(eboot1) && !File.Exists(eboot2))
                {
                    throw new InvalidOperationException(
                        $"Invalid PlayStation 3 directory structure.\n\n" +
                        $"Expected EBOOT.BIN was not found inside standard path:\n" +
                        $"'{Path.Combine(game.RomPath, "PS3_GAME", "USRDIR")}'\n\n" +
                        $"Please verify that you have chosen the correct disc folder dump."
                    );
                }
            }
        }

        public bool ValidateGame(Game game)
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;

            string romPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, game.RomPath));
            if (string.IsNullOrEmpty(game.RomPath)) return false;

            if (File.Exists(romPath))
            {
                string ext = Path.GetExtension(romPath).ToLower();
                return ext == ".pkg" || ext == ".iso" || ext == ".bin";
            }

            if (Directory.Exists(romPath))
            {
                string eboot1 = Path.Combine(romPath, "PS3_GAME", "USRDIR", "EBOOT.BIN");
                string eboot2 = Path.Combine(romPath, "USRDIR", "EBOOT.BIN");
                return File.Exists(eboot1) || File.Exists(eboot2);
            }

            return false;
        }

        public string GetSaveFolder(Game game)
        {
            return SaveManager.Instance.DetectSaveFolder(EmulatorId);
        }

        public string GetScreenshotFolder(Game game)
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emulators", "PS3", "screenshots"));
        }
    }
}
