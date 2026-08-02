using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetroLauncher.Tests.Unit
{
    public static class ControllerSyncServiceTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"ControllerSyncService test failed: {message}");
            }
        }

        public static void RunTests()
        {
            RetroLogger.Log("Starting ControllerSyncService Unit Tests...");
            TestGlobalControllerConfigPersistence();
            TestAdapterControllerSynchronization().GetAwaiter().GetResult();
            TestPreserveNonControllerSettings();
            RetroLogger.Log("All ControllerSyncService Unit Tests completed successfully!");
        }

        private static void TestGlobalControllerConfigPersistence()
        {
            var manager = GlobalControllerConfigManager.Instance;
            manager.Config.AutoSyncOnLaunch = true;
            manager.Config.Players[0].Deadzone = 0.20f;
            manager.Config.Players[0].Sensitivity = 1.25f;
            manager.Config.Hotkeys.Pause = "Space";
            manager.Save();

            manager.Load();
            Assert(manager.Config.AutoSyncOnLaunch == true, "AutoSyncOnLaunch must persist.");
            Assert(Math.Abs(manager.Config.Players[0].Deadzone - 0.20f) < 0.01f, "Deadzone must persist.");
            Assert(manager.Config.Hotkeys.Pause == "Space", "Hotkey must persist.");

            RetroLogger.Log("GlobalControllerConfig persistence test passed.");
        }

        private static async Task TestAdapterControllerSynchronization()
        {
            string[] emuIds = new[] { "pcsx2", "duckstation", "rpcs3", "dolphin", "ppsspp" };

            foreach (var emuId in emuIds)
            {
                var result = await ControllerSyncService.Instance.ApplyGlobalProfileToEmulatorAsync(emuId, skipRunningCheck: true);
                Assert(result.Success, $"Applying global profile to {emuId} must succeed.");
            }

            var allResults = await ControllerSyncService.Instance.SyncAllEmulatorsAsync();
            Assert(allResults.Count == EmulatorManager.Instance.Config.Emulators.Count, "SyncAllEmulators must process all configured emulators.");

            RetroLogger.Log("Adapter controller synchronization test passed.");
        }

        private static void TestPreserveNonControllerSettings()
        {
            string tempDir = Path.Combine(AppContext.BaseDirectory, "TestIniPreserve");
            Directory.CreateDirectory(tempDir);
            string iniPath = Path.Combine(tempDir, "settings.ini");

            try
            {
                string originalContent = "[Graphics]\nRenderer = Vulkan\nResolution = 4K\n\n[Audio]\nVolume = 100\n\n[Pad1]\nType = Old";
                File.WriteAllText(iniPath, originalContent);

                var ini = IniFileParser.ParseFile(iniPath);
                ini["Pad1"]["Type"] = "XInput";
                ini["Pad1"]["Deadzone"] = "0.20";
                IniFileParser.WriteFile(iniPath, ini);

                string updatedContent = File.ReadAllText(iniPath);
                Assert(updatedContent.Contains("Renderer = Vulkan"), "Preserve Graphics section Renderer setting.");
                Assert(updatedContent.Contains("Resolution = 4K"), "Preserve Graphics section Resolution setting.");
                Assert(updatedContent.Contains("Volume = 100"), "Preserve Audio section Volume setting.");
                Assert(updatedContent.Contains("Type = XInput"), "Update Pad1 Type setting.");

                RetroLogger.Log("Non-controller settings preservation test passed.");
            }
            finally
            {
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
