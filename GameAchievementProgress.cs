using System;

namespace RetroLauncher
{
    public class GameAchievementProgress
    {
        public string GameId { get; set; } = "";
        public int UnlockedCount { get; set; } = 0;
        public int TotalCount { get; set; } = 0;
        public double CompletionPercentage { get; set; } = 0.0;
    }
}
