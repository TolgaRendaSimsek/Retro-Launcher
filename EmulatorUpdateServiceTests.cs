using System;

namespace RetroLauncher
{
    public static class EmulatorUpdateServiceTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting EmulatorUpdateService Version Strategy Unit Tests...");

            // 1. Semantic Version Strategy
            var semVer = new SemanticVersionStrategy();
            
            Assert(semVer.IsNewer("1.0.0", "1.1.0", null, null), "Semantic: 1.1.0 > 1.0.0");
            Assert(!semVer.IsNewer("1.1.0", "1.0.0", null, null), "Semantic: 1.0.0 is not > 1.1.0");
            Assert(!semVer.IsNewer("1.0.0", "1.0.0", null, null), "Semantic: 1.0.0 is not > 1.0.0");
            Assert(semVer.IsNewer("v1.2.3", "v1.2.4", null, null), "Semantic: v1.2.4 > v1.2.3");
            Assert(semVer.IsNewer("1.2.3-beta", "1.2.4", null, null), "Semantic: 1.2.4 > 1.2.3-beta");

            // 2. Release Timestamp Strategy
            var timeVer = new ReleaseTimestampStrategy();
            var t1 = DateTime.UtcNow.AddDays(-1);
            var t2 = DateTime.UtcNow;
            Assert(timeVer.IsNewer("", "", t1, t2), "Timestamp: t2 > t1");
            Assert(!timeVer.IsNewer("", "", t2, t1), "Timestamp: t1 is not > t2");

            // 3. Rolling Build Strategy
            var rollingVer = new RollingBuildStrategy();
            
            Assert(rollingVer.IsNewer("0.0.32-16628-98e578c7", "0.0.32-16650-abcdef12", null, null), "Rolling: build 16650 > 16628");
            Assert(!rollingVer.IsNewer("0.0.32-16650-abcdef12", "0.0.32-16628-98e578c7", null, null), "Rolling: build 16628 is not > 16650");
            Assert(rollingVer.IsNewer("unparseable", "another_unparseable", t1, t2), "Rolling fallback: timestamp t2 > t1");

            RetroLogger.Log("All EmulatorUpdateService Version Strategy Unit Tests completed successfully!");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Test assertion failed: {message}");
            }
            RetroLogger.Log($"Test Case passed: {message}");
        }
    }
}
