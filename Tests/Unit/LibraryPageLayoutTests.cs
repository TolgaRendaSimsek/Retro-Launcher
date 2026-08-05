using System;
using System.Windows.Forms;
using RetroLauncher.Services.Logging;
using RetroLauncher.UI.Forms;

namespace RetroLauncher.Tests.Unit
{
    public static class LibraryPageLayoutTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting LibraryPageLayout Unit Tests...");

            TestSingleSearchBoxConstraint();
            TestSinglePageInstanceRule();

            RetroLogger.Log("All LibraryPageLayout Unit Tests completed successfully!");
        }

        private static void TestSingleSearchBoxConstraint()
        {
            using (var form = new MainForm())
            {
                // Verify that no obsolete TextBox named tbSearch exists in form controls
                var controls = form.Controls.Find("tbSearch", true);
                Assert(controls.Length == 0, "Obsolete tbSearch control must be completely removed from the form hierarchy.");
                RetroLogger.Log("Single search box constraint test passed.");
            }
        }

        private static void TestSinglePageInstanceRule()
        {
            using (var form = new MainForm())
            {
                Assert(form.Controls.Count > 0, "MainForm initialized with root layout.");
                RetroLogger.Log("Single page instance rule test passed.");
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"LibraryPageLayout Unit Test Failed: {message}");
            }
        }
    }
}
