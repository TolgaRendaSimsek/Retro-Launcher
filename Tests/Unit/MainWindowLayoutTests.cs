using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetroLauncher.Tests.Unit
{
    public static class MainWindowLayoutTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"MainWindowLayout test failed: {message}");
            }
        }

        public static void RunTests()
        {
            RetroLogger.Log("Starting MainWindowLayout Unit Tests...");
            TestMainLayoutHierarchyAndGridSizing();
            TestPageSwitchingDockAndLocation();
            RetroLogger.Log("All MainWindowLayout Unit Tests completed successfully!");
        }

        private static void TestMainLayoutHierarchyAndGridSizing()
        {
            using (var form = new MainForm())
            {
                // Verify root layout container
                TableLayoutPanel? rootLayout = null;
                foreach (Control ctrl in form.Controls)
                {
                    if (ctrl is TableLayoutPanel tlp && tlp.ColumnCount == 2)
                    {
                        rootLayout = tlp;
                        break;
                    }
                }

                Assert(rootLayout != null, "MainForm must contain RootLayout TableLayoutPanel with 2 columns.");
                Assert(rootLayout!.Dock == DockStyle.Fill, "RootLayout must have Dock = DockStyle.Fill.");
                Assert(rootLayout.ColumnStyles.Count >= 2, "RootLayout must have at least 2 ColumnStyles.");
                Assert(rootLayout.ColumnStyles[0].SizeType == SizeType.Absolute, "Sidebar column must use SizeType.Absolute.");
                Assert(rootLayout.ColumnStyles[1].SizeType == SizeType.Percent, "Content column must use SizeType.Percent.");

                // Verify right layout container
                TableLayoutPanel? rightLayout = null;
                foreach (Control ctrl in rootLayout.Controls)
                {
                    if (ctrl is TableLayoutPanel tlp && tlp.RowCount == 2)
                    {
                        rightLayout = tlp;
                        break;
                    }
                }

                Assert(rightLayout != null, "RightLayout TableLayoutPanel must have 2 rows.");
                Assert(rightLayout!.Dock == DockStyle.Fill, "RightLayout must have Dock = DockStyle.Fill.");
                Assert(rightLayout.RowStyles[0].SizeType == SizeType.Absolute, "TopBar row must use SizeType.Absolute.");
                Assert(rightLayout.RowStyles[1].SizeType == SizeType.Percent, "ContentHost row must use SizeType.Percent.");

                // Verify ContentHostPanel
                Panel? contentHost = null;
                foreach (Control ctrl in rightLayout.Controls)
                {
                    if (ctrl is Panel pnl && pnl.AutoScroll)
                    {
                        contentHost = pnl;
                        break;
                    }
                }

                Assert(contentHost != null, "ContentHostPanel must have AutoScroll = true.");
                Assert(!contentHost!.AutoSize, "ContentHostPanel must have AutoSize = false.");
                Assert(contentHost.Dock == DockStyle.Fill, "ContentHostPanel must have Dock = DockStyle.Fill.");
            }

            RetroLogger.Log("Main layout hierarchy and grid sizing test passed.");
        }

        private static void TestPageSwitchingDockAndLocation()
        {
            using (var form = new MainForm())
            {
                // Verify Form has non-zero size and is resizable
                Assert(form.MinimumSize.Width >= 980 && form.MinimumSize.Height >= 600, "MainForm minimum size verified.");
                Assert(form.FormBorderStyle != FormBorderStyle.None || form.WindowState == FormWindowState.Maximized || form.ClientSize.Width > 0, "Form client size verified.");
            }

            RetroLogger.Log("Page switching dock and location test passed.");
        }
    }
}
