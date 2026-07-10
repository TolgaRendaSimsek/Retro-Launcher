using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher
{
    public class Badge
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconPath { get; set; } = "";
        public int XpReward { get; set; } = 100;
        public bool Unlocked { get; set; } = false;
        public string UnlockedAt { get; set; } = "";
        public string ConditionType { get; set; } = ""; // GameCount, Playtime, PlaytimeHours, AchievementsCount, CollectionCount, FavoritesCount
        public string ConditionValue { get; set; } = "";

        // camelCase aliases for JSON serialization and external tools compatibility
        public string id { get => Id; set => Id = value; }
        public string title { get => Title; set => Title = value; }
        public string description { get => Description; set => Description = value; }
        public string iconPath { get => IconPath; set => IconPath = value; }
        public int xpReward { get => XpReward; set => XpReward = value; }
        public bool unlocked { get => Unlocked; set => Unlocked = value; }
        public string unlockedAt { get => UnlockedAt; set => UnlockedAt = value; }
        public string conditionType { get => ConditionType; set => ConditionType = value; }
        public string conditionValue { get => ConditionValue; set => ConditionValue = value; }
    }

    public class BadgeConfig
    {
        public int XP { get; set; } = 0;
        public List<Badge> Badges { get; set; } = new();
    }

    public class LevelManager
    {
        private static LevelManager? _instance;
        public static LevelManager Instance => _instance ??= new LevelManager();

        public int XP { get; set; } = 0;

        public void AddXP(int amount)
        {
            XP += amount;
            BadgeManager.Instance.SaveBadges();
        }

        public int GetCurrentLevel()
        {
            return CalculateLevelFromXP(XP);
        }

        public int GetXPForNextLevel()
        {
            return 100 - (XP % 100);
        }

        public int CalculateLevelFromXP(int xp)
        {
            return (xp / 100) + 1;
        }
    }

    public class BadgeManager
    {
        private static readonly string BadgesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "badges.json");
        private static readonly object FileLock = new object();
        
        private List<Badge> _badges = new();

        private static BadgeManager? _instance;
        public static BadgeManager Instance => _instance ??= new BadgeManager();

        public BadgeManager()
        {
            LoadBadges();
        }

        public List<Badge> Badges => _badges;

        public void LoadBadges()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(BadgesPath))
                    {
                        string json = File.ReadAllText(BadgesPath);
                        var config = JsonSerializer.Deserialize<BadgeConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (config != null)
                        {
                            LevelManager.Instance.XP = config.XP;
                            _badges = config.Badges ?? new List<Badge>();
                        }
                    }
                    else
                    {
                        SeedDefaultBadges();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading badges: {ex.Message}");
                    SeedDefaultBadges();
                }
            }
        }

        public void SaveBadges()
        {
            lock (FileLock)
            {
                try
                {
                    var config = new BadgeConfig
                    {
                        XP = LevelManager.Instance.XP,
                        Badges = _badges
                    };
                    string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(BadgesPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving badges: {ex.Message}");
                }
            }
        }

        public void CheckBadgeUnlocks()
        {
            try
            {
                var libraryManager = new GameLibraryManager();
                var collectionManager = new CollectionManager(libraryManager);
                var achievementManager = new AchievementManager();

                int gameCount = libraryManager.Games.Count;
                int totalPlaytimeMinutes = libraryManager.Games.Sum(g => g.TotalPlaytimeMinutes);
                bool hasPlaytime = libraryManager.Games.Any(g => g.TotalPlaytimeMinutes > 0);
                bool hasAchievements = achievementManager.Achievements.Any(a => a.IsUnlocked);
                bool hasCollections = collectionManager.Collections.Any(c => !c.IsAutomatic);
                bool hasFavorites = libraryManager.Games.Any(g => g.IsFavorite);

                foreach (var badge in _badges.Where(b => !b.Unlocked))
                {
                    bool shouldUnlock = false;
                    int.TryParse(badge.ConditionValue, out int targetValue);

                    switch (badge.ConditionType)
                    {
                        case "GameCount":
                            shouldUnlock = gameCount >= targetValue;
                            break;
                        case "Playtime":
                            shouldUnlock = hasPlaytime;
                            break;
                        case "PlaytimeHours":
                            shouldUnlock = (totalPlaytimeMinutes / 60) >= targetValue;
                            break;
                        case "AchievementsCount":
                            shouldUnlock = hasAchievements;
                            break;
                        case "CollectionCount":
                            shouldUnlock = hasCollections;
                            break;
                        case "FavoritesCount":
                            shouldUnlock = hasFavorites;
                            break;
                    }

                    if (shouldUnlock)
                    {
                        UnlockBadge(badge.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking badge unlocks: {ex.Message}");
            }
        }

        public bool UnlockBadge(string badgeId)
        {
            var badge = _badges.FirstOrDefault(b => b.Id == badgeId);
            if (badge != null && !badge.Unlocked)
            {
                badge.Unlocked = true;
                badge.UnlockedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LevelManager.Instance.AddXP(badge.XpReward); // Award XP
                SaveBadges();
                return true;
            }
            return false;
        }

        public List<Badge> GetUnlockedBadges()
        {
            return _badges.Where(b => b.Unlocked).ToList();
        }

        public List<Badge> GetLockedBadges()
        {
            return _badges.Where(b => !b.Unlocked).ToList();
        }

        private void SeedDefaultBadges()
        {
            LevelManager.Instance.XP = 0;
            _badges = new List<Badge>
            {
                new Badge
                {
                    Id = "first_game_added",
                    Title = "Collector Recruit",
                    Description = "Added your first game to the library.",
                    IconPath = "media/badges/collector_recruit.png",
                    XpReward = 100,
                    ConditionType = "GameCount",
                    ConditionValue = "1"
                },
                new Badge
                {
                    Id = "first_game_played",
                    Title = "First Steps",
                    Description = "Started playing your first game.",
                    IconPath = "media/badges/first_steps.png",
                    XpReward = 100,
                    ConditionType = "Playtime",
                    ConditionValue = "1"
                },
                new Badge
                {
                    Id = "ten_games_added",
                    Title = "Library Hoarder",
                    Description = "Added 10 or more games to the library.",
                    IconPath = "media/badges/library_hoarder.png",
                    XpReward = 250,
                    ConditionType = "GameCount",
                    ConditionValue = "10"
                },
                new Badge
                {
                    Id = "hundred_playtime_hours",
                    Title = "Centurion Gamer",
                    Description = "Logged 100+ hours of game playtime.",
                    IconPath = "media/badges/centurion_gamer.png",
                    XpReward = 500,
                    ConditionType = "PlaytimeHours",
                    ConditionValue = "100"
                },
                new Badge
                {
                    Id = "first_achievement_unlocked",
                    Title = "Achiever",
                    Description = "Unlocked your first in-game achievement.",
                    IconPath = "media/badges/achiever.png",
                    XpReward = 150,
                    ConditionType = "AchievementsCount",
                    ConditionValue = "1"
                },
                new Badge
                {
                    Id = "collection_created",
                    Title = "Archivist",
                    Description = "Created your first custom collection.",
                    IconPath = "media/badges/archivist.png",
                    XpReward = 100,
                    ConditionType = "CollectionCount",
                    ConditionValue = "1"
                },
                new Badge
                {
                    Id = "favorite_game_selected",
                    Title = "Dedicated Fan",
                    Description = "Marked at least one game as a Favorite.",
                    IconPath = "media/badges/dedicated_fan.png",
                    XpReward = 100,
                    ConditionType = "FavoritesCount",
                    ConditionValue = "1"
                }
            };

            SaveBadges();
        }
    }
}
