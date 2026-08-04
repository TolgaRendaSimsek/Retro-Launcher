using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RetroLauncher.Core.Enums;
using RetroLauncher.Core.Models;
using RetroLauncher.Emulators.Adapters;
using RetroLauncher.Services.Controllers;
using RetroLauncher.Services.Logging;
using RetroLauncher.UI.Controls;

namespace RetroLauncher.Tests.Unit
{
    public static class MasterControllerKeyboardPresetTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting MasterControllerKeyboardPreset Unit Tests...");

            TestPresetCatalog();
            TestKeyFormatDisplay();
            TestPlayerConfigMappingConversion();
            TestAdapterKeyboardTranslations();

            RetroLogger.Log("All MasterControllerKeyboardPreset Unit Tests completed successfully!");
        }

        private static void TestPresetCatalog()
        {
            var presets = KeyboardPresetCatalog.GetPresetNames();
            Assert(presets.Contains(KeyboardPresetCatalog.ModernWASD), "Preset catalog must contain Modern WASD.");
            Assert(presets.Contains(KeyboardPresetCatalog.ArrowKeys), "Preset catalog must contain Arrow Keys.");
            Assert(presets.Contains(KeyboardPresetCatalog.Custom), "Preset catalog must contain Custom.");

            var wasd = KeyboardPresetCatalog.GetModernWASDPreset();
            Assert(wasd.Count >= 20, "Modern WASD preset must contain at least 20 actions.");

            var faceSouth = wasd.FirstOrDefault(m => m.Action == VirtualControllerAction.FaceSouth);
            Assert(faceSouth != null && faceSouth.Key == Keys.Space, "Modern WASD FaceSouth must be Space.");

            var dpadUp = wasd.FirstOrDefault(m => m.Action == VirtualControllerAction.DPadUp);
            Assert(dpadUp != null && dpadUp.Key == Keys.W, "Modern WASD DPadUp must be W.");

            var arrowPreset = KeyboardPresetCatalog.GetArrowKeysPreset();
            var arrowUp = arrowPreset.FirstOrDefault(m => m.Action == VirtualControllerAction.DPadUp);
            Assert(arrowUp != null && arrowUp.Key == Keys.Up, "Arrow Keys DPadUp must be Up.");

            RetroLogger.Log("Preset catalog tests passed.");
        }

        private static void TestKeyFormatDisplay()
        {
            Assert(KeyCaptureControl.FormatKeyDisplay(Keys.Space) == "Space", "Space key formatting test failed.");
            Assert(KeyCaptureControl.FormatKeyDisplay(Keys.Return) == "Enter", "Return key formatting test failed.");
            Assert(KeyCaptureControl.FormatKeyDisplay(Keys.Escape) == "Escape", "Escape key formatting test failed.");
            Assert(KeyCaptureControl.FormatKeyDisplay(Keys.LShiftKey) == "Left Shift", "LShiftKey formatting test failed.");
            Assert(KeyCaptureControl.FormatKeyDisplay(Keys.Up) == "Up Arrow", "Up arrow formatting test failed.");
            Assert(KeyCaptureControl.FormatKeyDisplay(Keys.F1) == "F1", "F1 key formatting test failed.");
            Assert(KeyCaptureControl.FormatKeyDisplay(null) == "[ Unassigned ]", "Null key formatting test failed.");

            RetroLogger.Log("Key display formatting tests passed.");
        }

        private static void TestPlayerConfigMappingConversion()
        {
            var player = new PlayerControllerConfig { PlayerIndex = 1, ControllerType = "Keyboard" };
            var wasdPreset = KeyboardPresetCatalog.GetModernWASDPreset();
            player.SetKeyboardMappings(wasdPreset);

            var retrieved = player.GetKeyboardMappings();
            Assert(retrieved.Count >= 20, "Retrieved keyboard mappings must contain at least 20 items.");

            var faceSouth = retrieved.FirstOrDefault(m => m.Action == VirtualControllerAction.FaceSouth);
            Assert(faceSouth != null && faceSouth.Key == Keys.Space, "Retrieved FaceSouth key must be Space.");

            RetroLogger.Log("PlayerConfig mapping conversion tests passed.");
        }

        private static void TestAdapterKeyboardTranslations()
        {
            var globalConfig = new GlobalControllerConfig();
            var p1 = globalConfig.Players[0];
            p1.ControllerType = "Keyboard";
            p1.SetKeyboardMappings(KeyboardPresetCatalog.GetModernWASDPreset());

            var duckAdapter = new DuckStationAdapter();
            bool duckSuccess = duckAdapter.ApplyGlobalControllerConfiguration(globalConfig);
            Assert(duckSuccess, "DuckStation keyboard configuration export failed.");

            var pcsx2Adapter = new PCSX2Adapter();
            bool pcsx2Success = pcsx2Adapter.ApplyGlobalControllerConfiguration(globalConfig);
            Assert(pcsx2Success, "PCSX2 keyboard configuration export failed.");

            var rpcs3Adapter = new RPCS3Adapter();
            bool rpcs3Success = rpcs3Adapter.ApplyGlobalControllerConfiguration(globalConfig);
            Assert(rpcs3Success, "RPCS3 keyboard configuration export failed.");

            RetroLogger.Log("Adapter keyboard translation export tests passed for DuckStation, PCSX2, and RPCS3.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"MasterControllerKeyboardPreset Unit Test Failed: {message}");
            }
        }
    }
}
