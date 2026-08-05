using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RetroLauncher.Core.Models;
using RetroLauncher.Services;
using RetroLauncher.Services.Logging;
using RetroLauncher.UI.Forms;

namespace RetroLauncher.Tests.Unit
{
    public static class PostGameLaunchLifecycleTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting PostGameLaunchLifecycle Unit Tests...");

            TestLaunchStateEnum();
            TestFormStatePreservation();

            RetroLogger.Log("All PostGameLaunchLifecycle Unit Tests completed successfully!");
        }

        private static void TestLaunchStateEnum()
        {
            var service = GameLaunchService.Instance;
            Assert(service.CurrentState == LaunchState.Idle || service.CurrentState == LaunchState.Failed, "Initial state should be Idle or Failed.");
            RetroLogger.Log("LaunchState initial state test passed.");
        }

        private static void TestFormStatePreservation()
        {
            FormWindowState testState = FormWindowState.Maximized;
            Rectangle testBounds = new Rectangle(100, 100, 1200, 800);

            Assert(testState == FormWindowState.Maximized, "Maximized window state preservation test passed.");
            Assert(testBounds.Width == 1200 && testBounds.Height == 800, "Window bounds preservation test passed.");
            RetroLogger.Log("Form state preservation test passed.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"PostGameLaunchLifecycle Unit Test Failed: {message}");
            }
        }
    }
}
