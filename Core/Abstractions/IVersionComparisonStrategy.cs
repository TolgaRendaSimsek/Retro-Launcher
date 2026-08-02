using System;

namespace RetroLauncher.Core.Abstractions
{
    public interface IVersionComparisonStrategy
    {
        bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime);
    }

    public class SemanticVersionComparisonStrategy : IVersionComparisonStrategy
    {
        private readonly SemanticVersionStrategy _inner = new();
        public bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime)
            => _inner.IsNewer(installed, available, installedTime, availableTime);
    }

    public class TimestampVersionComparisonStrategy : IVersionComparisonStrategy
    {
        private readonly ReleaseTimestampStrategy _inner = new();
        public bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime)
            => _inner.IsNewer(installed, available, installedTime, availableTime);
    }

    public class RollingBuildVersionComparisonStrategy : IVersionComparisonStrategy
    {
        private readonly RollingBuildStrategy _inner = new();
        public bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime)
            => _inner.IsNewer(installed, available, installedTime, availableTime);
    }
}
