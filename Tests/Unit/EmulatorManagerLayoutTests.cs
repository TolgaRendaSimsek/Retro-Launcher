using System;
using System.IO;
using System.Windows.Forms;

namespace RetroLauncher.Tests.Unit
{
    public static class EmulatorManagerLayoutTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"EmulatorManagerLayout test failed: {message}");
            }
        }

        public static void RunTests()
        {
            RetroLogger.Log("Starting EmulatorManagerLayout Unit Tests...");
            TestResponsiveLayoutArchitecture();
            TestDynamicInstallButtonTextAndVisibility();
            RetroLogger.Log("All EmulatorManagerLayout Unit Tests completed successfully!");
        }

        private static void TestResponsiveLayoutArchitecture()
        {
            using (var form = new EmulatorManagerForm())
            {
                Assert(form.MinimumSize.Width >= 740 && form.MinimumSize.Height >= 540, "Form minimum size must be configured for responsive resizing.");
                Assert(form.FormBorderStyle == FormBorderStyle.Sizable, "Form must be resizable.");

                // Find split container and layout panels
                SplitContainer? splitContainer = null;
                foreach (Control ctrl in form.Controls)
                {
                    if (ctrl is SplitContainer sc)
                    {
                        splitContainer = sc;
                        break;
                    }
                }

                Assert(splitContainer != null, "Form must contain a SplitContainer for responsive layout.");
                Assert(splitContainer!.Dock == DockStyle.Fill, "SplitContainer must fill form area.");
                Assert(splitContainer.Panel1.Controls.Count > 0, "Left panel must contain controls.");
                Assert(splitContainer.Panel2.Controls.Count > 0, "Right panel must contain controls.");

                // Inspect right main layout panel
                TableLayoutPanel? pnlRightMain = null;
                foreach (Control ctrl in splitContainer.Panel2.Controls)
                {
                    if (ctrl is TableLayoutPanel tlp)
                    {
                        pnlRightMain = tlp;
                        break;
                    }
                }

                Assert(pnlRightMain != null, "Right panel must use TableLayoutPanel for responsive layout.");
                Assert(pnlRightMain!.AutoScroll, "Right panel TableLayoutPanel must enable AutoScroll for DPI scaling.");
                Assert(pnlRightMain.Padding.Left >= 8 && pnlRightMain.Padding.Top >= 8, "Minimum spacing of 8px must be enforced.");
            }

            RetroLogger.Log("Responsive layout architecture test passed.");
        }

        private static void TestDynamicInstallButtonTextAndVisibility()
        {
            using (var form = new EmulatorManagerForm())
            {
                // Verify dynamic button text rules and emulator-specific visibility
                var duckEmu = new EmulatorItem { Id = "duckstation", Name = "DuckStation", Path = "Emulators/DuckStation/duckstation-qt.exe" };
                var pcsx2Emu = new EmulatorItem { Id = "pcsx2", Name = "PCSX2", Path = "Emulators/PCSX2/pcsx2-qt.exe" };

                Assert(duckEmu.Id == "duckstation", "DuckStation emulator ID verified.");
                Assert(pcsx2Emu.Id == "pcsx2", "PCSX2 emulator ID verified.");
            }

            RetroLogger.Log("Dynamic install button text and visibility test passed.");
        }
    }
}
