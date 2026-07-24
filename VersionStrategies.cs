using System;
using System.Text.RegularExpressions;

namespace RetroLauncher
{
    public class SemanticVersionStrategy : IEmulatorVersionStrategy
    {
        public bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime)
        {
            if (string.IsNullOrEmpty(installed) || string.IsNullOrEmpty(available)) return false;

            string cleanInst = CleanVersion(installed);
            string cleanAvail = CleanVersion(available);

            if (Version.TryParse(cleanInst, out Version? vInst) && Version.TryParse(cleanAvail, out Version? vAvail))
            {
                return vAvail > vInst;
            }

            return false;
        }

        private string CleanVersion(string version)
        {
            version = version.Trim().ToLower().TrimStart('v').TrimStart('r');
            int dashIndex = version.IndexOf('-');
            if (dashIndex > 0) version = version.Substring(0, dashIndex);
            return version;
        }
    }

    public class ReleaseTimestampStrategy : IEmulatorVersionStrategy
    {
        public bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime)
        {
            if (installedTime.HasValue && availableTime.HasValue)
            {
                return availableTime.Value > installedTime.Value;
            }
            return false;
        }
    }

    public class RollingBuildStrategy : IEmulatorVersionStrategy
    {
        public bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime)
        {
            if (string.IsNullOrEmpty(installed) || string.IsNullOrEmpty(available)) return false;

            int instBuild = ParseBuildNumber(installed);
            int availBuild = ParseBuildNumber(available);

            if (instBuild > 0 && availBuild > 0)
            {
                return availBuild > instBuild;
            }

            // Fallback to timestamp comparison
            if (installedTime.HasValue && availableTime.HasValue)
            {
                return availableTime.Value > installedTime.Value;
            }

            return false;
        }

        private int ParseBuildNumber(string version)
        {
            if (string.IsNullOrEmpty(version)) return 0;
            var match = Regex.Match(version, @"-(\d+)(?:-|$)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int buildNum))
            {
                return buildNum;
            }
            return 0;
        }
    }
}
