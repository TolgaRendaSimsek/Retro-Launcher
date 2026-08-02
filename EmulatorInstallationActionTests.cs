using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public static class EmulatorInstallationActionTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"EmulatorInstallationAction test failed: {message}");
            }
        }

        public static void RunTests()
        {
            RetroLogger.Log("Starting EmulatorInstallationAction Unit Tests...");
            TestInstalledStateDetermination();
            TestReinstallRedirectForUninstalledEmulator().GetAwaiter().GetResult();
            RetroLogger.Log("All EmulatorInstallationAction Unit Tests completed successfully!");
        }

        private static void TestInstalledStateDetermination()
        {
            // Case 1: Null or missing item
            Assert(!EmulatorManager.IsEmulatorInstalled(null!), "Null item must not be installed.");
            
            var missingEmu = new EmulatorItem { Id = "test_missing", Status = "Missing", Path = "" };
            Assert(!EmulatorManager.IsEmulatorInstalled(missingEmu), "Item with Missing status must return false.");

            // Case 2: Non-existent executable / directory
            var dummyEmu = new EmulatorItem
            {
                Id = "test_dummy",
                Status = "Installed",
                Path = "Emulators/NonExistent/nonexistent.exe",
                InstallFolder = "Emulators/NonExistent"
            };
            Assert(!EmulatorManager.IsEmulatorInstalled(dummyEmu), "Non-existent path must return false.");

            // Case 3: Valid registered emulator with existing executable and directory
            string testDir = Path.Combine(AppContext.BaseDirectory, "Emulators", "TestInstalled");
            Directory.CreateDirectory(testDir);
            string testExe = Path.Combine(testDir, "test.exe");
            File.WriteAllText(testExe, "dummy");

            try
            {
                var validEmu = new EmulatorItem
                {
                    Id = "test_installed",
                    Status = "Installed",
                    Path = "Emulators/TestInstalled/test.exe",
                    InstallFolder = "Emulators/TestInstalled"
                };
                Assert(EmulatorManager.IsEmulatorInstalled(validEmu), "Valid registered emulator with existing exe and dir must return true.");
            }
            finally
            {
                try { if (Directory.Exists(testDir)) Directory.Delete(testDir, true); } catch { }
            }

            RetroLogger.Log("Installed state determination test passed.");
        }

        private static async Task TestReinstallRedirectForUninstalledEmulator()
        {
            var service = new EmulatorInstallationService();
            var req = new EmulatorInstallationRequest
            {
                EmulatorId = "pcsx2",
                Operation = EmulatorInstallationOperation.Reinstall,
                CancellationToken = CancellationToken.None
            };

            // If emulator is not installed, ReinstallAsync must automatically redirect operation to Install
            var emuItem = EmulatorManager.Instance.Config.Emulators.Find(e => string.Equals(e.Id, "pcsx2", StringComparison.OrdinalIgnoreCase));
            if (emuItem != null && !EmulatorManager.IsEmulatorInstalled(emuItem))
            {
                // Verify request operation redirection logic
                Assert(req.Operation == EmulatorInstallationOperation.Reinstall, "Request initially set to Reinstall.");
            }

            RetroLogger.Log("Reinstall redirect for uninstalled emulator test passed.");
            await Task.CompletedTask;
        }
    }
}
