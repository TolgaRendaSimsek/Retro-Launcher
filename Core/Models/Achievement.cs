using System;

namespace RetroLauncher.Core.Models
{
    public class Achievement
    {
        public string Id { get; set; } = "";
        public string GameId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconPath { get; set; } = "";
        public bool IsUnlocked { get; set; } = false;
        public string UnlockedAt { get; set; } = ""; // "yyyy-MM-dd HH:mm" or empty if locked
        public string Rarity { get; set; } = "Common"; // e.g. "Common (85%)", "Rare (12%)"
        public int Points { get; set; } = 0;
    }
}
