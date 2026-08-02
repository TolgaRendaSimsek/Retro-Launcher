using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public static class BiosSynchronizationServiceTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Test assertion failed: {message}");
            }
        }

        public static void RunTests()
        {
            RetroLogger.Log("Starting BiosSynchronizationService Unit Tests...");
            TestBiosSyncOperationsAsync().GetAwaiter().GetResult();
            RetroLogger.Log("All BiosSynchronizationService Unit Tests completed successfully!");
        }

        private static async Task TestBiosSyncOperationsAsync()
        {
            string testBase = Path.Combine(AppContext.BaseDirectory, "TestBiosSyncTemp");
            if (Directory.Exists(testBase)) Directory.Delete(testBase, true);
            Directory.CreateDirectory(testBase);

            try
            {
                // Setup test central BIOS folder
                string centralRoot = BiosManager.GetCentralizedBiosRoot();
                string duckCentral = Path.Combine(centralRoot, "DuckStation", "PS1");
                Directory.CreateDirectory(duckCentral);

                string uniqueTestFileName = "scph_unique_test_5501.bin";
                string testBiosFile = Path.Combine(duckCentral, uniqueTestFileName);
                byte[] testBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
                File.WriteAllBytes(testBiosFile, testBytes);

                // Case 1: Uninstalled emulator
                var resNotInstalled = await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync("nonexistent_emu", null, CancellationToken.None);
                Assert(resNotInstalled.State == BiosSyncState.EmulatorNotInstalled, "Uninstalled emulator must return EmulatorNotInstalled state.");
                Assert(!resNotInstalled.IsInstalled, "IsInstalled must be false for uninstalled emulator.");

                // Case 2: Installed emulator with compatible BIOS sync & skipping identical files
                string emuDir = Path.Combine(AppContext.BaseDirectory, "Emulators", "DuckStation");
                Directory.CreateDirectory(emuDir);
                string exePath = Path.Combine(emuDir, "duckstation-qt.exe");
                if (!File.Exists(exePath)) File.WriteAllText(exePath, "dummy_exe");

                // Register emulator in EmulatorManager if missing
                var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, "duckstation", StringComparison.OrdinalIgnoreCase));
                if (emuItem == null)
                {
                    emuItem = new EmulatorItem { Id = "duckstation", Name = "DuckStation", Path = "Emulators/DuckStation/duckstation-qt.exe" };
                    EmulatorManager.Instance.Config.Emulators.Add(emuItem);
                }
                else
                {
                    emuItem.Path = "Emulators/DuckStation/duckstation-qt.exe";
                }

                // Ensure destination directory is clean before test
                string destBiosDir = Path.Combine(emuDir, "bios");
                if (Directory.Exists(destBiosDir)) Directory.Delete(destBiosDir, true);

                // First Sync
                var resSync1 = await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync("duckstation", null, CancellationToken.None);
                Assert(resSync1.State == BiosSyncState.SyncedSuccessfully, $"Installed emulator with central BIOS must return SyncedSuccessfully, but was {resSync1.State}. Error: {resSync1.ErrorMessage}");
                Assert(resSync1.CopiedCount >= 1, $"First sync must copy compatible BIOS file(s), but copied {resSync1.CopiedCount}, skipped {resSync1.SkippedCount}.");
                Assert(File.Exists(Path.Combine(resSync1.DestinationPath, uniqueTestFileName)), "Unique test BIOS file must be copied to destination.");

                int totalCount = resSync1.CopiedCount + resSync1.SkippedCount;

                // Second Sync -> copied = 0, skipped = totalCount (identical file skipping requirement)
                var resSync2 = await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync("duckstation", null, CancellationToken.None);
                Assert(resSync2.State == BiosSyncState.SyncedSuccessfully, "Second sync must return SyncedSuccessfully.");
                Assert(resSync2.CopiedCount == 0, "Second sync of identical files must copy 0 files.");
                Assert(resSync2.SkippedCount == totalCount, $"Second sync of identical files must skip all {totalCount} files.");

                // Case 3: Preserve user files in destination (never delete user files requirement)
                string userFile = Path.Combine(resSync1.DestinationPath, "user_custom_bios.bin");
                File.WriteAllText(userFile, "user_custom_data");

                await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync("duckstation", null, CancellationToken.None);
                Assert(File.Exists(userFile), "BIOS sync must never delete user BIOS files in destination folder.");

                // Case 4: Global Sync operation continuation on failure
                var globalResults = await BiosSynchronizationService.Instance.SyncAllEmulatorsBiosAsync(null, CancellationToken.None);
                Assert(globalResults.Any(), "Global sync must process registered emulators.");
                Assert(globalResults.Any(r => r.EmulatorId == "duckstation" && r.State == BiosSyncState.SyncedSuccessfully), "Global sync must process duckstation successfully.");

                RetroLogger.Log("BiosSynchronizationService unit tests passed.");
            }
            finally
            {
                try { if (Directory.Exists(testBase)) Directory.Delete(testBase, true); } catch { }
            }
        }
    }
}
