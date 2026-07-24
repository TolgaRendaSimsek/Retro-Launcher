using System;
using System.Collections.Generic;

namespace RetroLauncher
{
    public class HealthCheckItem
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string TechnicalDetail { get; set; } = "";
        public HealthStatus Status { get; set; } = HealthStatus.Unknown;
        public string RelatedEmulatorId { get; set; } = "";
        public string RelatedGameId { get; set; } = "";
        public string SuggestedFix { get; set; } = "";
        public HealthFixAction FixAction { get; set; } = HealthFixAction.None;
    }

    public class HealthCheckResult
    {
        public List<HealthCheckItem> Items { get; set; } = new();
        public int HealthyCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int UnknownCount { get; set; }
        public DateTime RunAt { get; set; } = DateTime.UtcNow;
    }
}
