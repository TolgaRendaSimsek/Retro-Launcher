using System;
using System.IO;
using System.Threading.Tasks;
using RetroLauncher.Emulators.Adapters;
using RetroLauncher.Services;
using RetroLauncher.Services.Logging;

namespace RetroLauncher.Tests.Unit
{
    public static class GameLaunchServiceTests
    {
        public static async Task RunTestsAsync()
        {
            RetroLogger.Log("Starting GameLaunchService Unit Tests...");

            await TestNullGameLaunchAsync();
            await TestMissingRomLaunchAsync();
            TestAdapterBuildCommandValidation();

            RetroLogger.Log("All GameLaunchService Unit Tests completed successfully!");
        }

        private static async Task TestNullGameLaunchAsync()
        {
            try
            {
                await GameLaunchService.Instance.LaunchGameAsync(null!);
                throw new Exception("LaunchGameAsync(null) should have thrown ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
                RetroLogger.Log("Null game launch validation test passed.");
            }
        }

        private static async Task TestMissingRomLaunchAsync()
        {
            var fakeGame = new Game
            {
                Id = "test_game_missing_rom",
                Title = "Missing ROM Game",
                Platform = "Sony PlayStation 1",
                RomPath = @"C:\NonExistentFolder\MissingRom.bin"
            };

            try
            {
                await GameLaunchService.Instance.LaunchGameAsync(fakeGame);
                throw new Exception("LaunchGameAsync with missing ROM should have thrown FileNotFoundException.");
            }
            catch (FileNotFoundException ex)
            {
                Assert(ex.Message.Contains("ROM file or folder not found"), "FileNotFoundException message should detail missing ROM.");
                RetroLogger.Log("Missing ROM launch validation test passed.");
            }
        }

        private static void TestAdapterBuildCommandValidation()
        {
            var duckGame = new Game { Id = "ps1_test", Title = "Tekken 3", Platform = "Sony PlayStation 1", RomPath = "tekken3.cue" };
            var pcsx2Game = new Game { Id = "ps2_test", Title = "Gran Turismo 4", Platform = "Sony PlayStation 2", RomPath = "gt4.iso" };
            var rpcs3Game = new Game { Id = "ps3_test", Title = "Demon's Souls", Platform = "Sony PlayStation 3", RomPath = "demons_souls.iso" };

            var duckAdapter = new DuckStationAdapter();
            var pcsx2Adapter = new PCSX2Adapter();
            var rpcs3Adapter = new RPCS3Adapter();

            var duckPsi = duckAdapter.BuildLaunchCommand(duckGame);
            Assert(!string.IsNullOrEmpty(duckPsi.FileName), "DuckStation FileName must not be empty.");
            Assert(!string.IsNullOrEmpty(duckPsi.WorkingDirectory), "DuckStation WorkingDirectory must not be empty.");
            Assert(duckPsi.UseShellExecute == false, "DuckStation UseShellExecute must be false.");

            var pcsx2Psi = pcsx2Adapter.BuildLaunchCommand(pcsx2Game);
            Assert(!string.IsNullOrEmpty(pcsx2Psi.FileName), "PCSX2 FileName must not be empty.");
            Assert(!string.IsNullOrEmpty(pcsx2Psi.WorkingDirectory), "PCSX2 WorkingDirectory must not be empty.");
            Assert(pcsx2Psi.UseShellExecute == false, "PCSX2 UseShellExecute must be false.");

            var rpcs3Psi = rpcs3Adapter.BuildLaunchCommand(rpcs3Game);
            Assert(!string.IsNullOrEmpty(rpcs3Psi.FileName), "RPCS3 FileName must not be empty.");
            Assert(!string.IsNullOrEmpty(rpcs3Psi.WorkingDirectory), "RPCS3 WorkingDirectory must not be empty.");
            Assert(rpcs3Psi.UseShellExecute == false, "RPCS3 UseShellExecute must be false.");

            RetroLogger.Log("Adapter launch command building & WorkingDirectory validation test passed for DuckStation, PCSX2, and RPCS3.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"GameLaunchService test failed: {message}");
            }
        }
    }
}
