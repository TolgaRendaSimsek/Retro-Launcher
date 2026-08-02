using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher.Services
{
    public class AchievementManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "achievements.json");
        public List<Achievement> Achievements { get; private set; } = new();
        private readonly JsonSerializerOptions _jsonOptions;

        public AchievementManager()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            LoadAchievements();
        }

        public void LoadAchievements()
        {
            try
            {
                string pathToUse = ResolvePath(ConfigPath);

                if (!File.Exists(pathToUse))
                {
                    Achievements = CreateDefaultAchievements();
                    SaveAchievements();
                    return;
                }

                string json = File.ReadAllText(pathToUse);
                Achievements = JsonSerializer.Deserialize<List<Achievement>>(json, _jsonOptions) ?? new List<Achievement>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading achievements: {ex.Message}");
                Achievements = CreateDefaultAchievements();
                SaveAchievements();
            }
        }

        public void SaveAchievements()
        {
            try
            {
                string pathToUse = ConfigPath;
                string? dir = Path.GetDirectoryName(pathToUse);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(Achievements, _jsonOptions);
                File.WriteAllText(pathToUse, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save achievements: {ex.Message}");
            }
        }

        public void UnlockAchievement(string gameId, string achievementId)
        {
            var achievement = Achievements.FirstOrDefault(a => a.GameId == gameId && a.Id == achievementId);
            if (achievement != null && !achievement.IsUnlocked)
            {
                achievement.IsUnlocked = true;
                achievement.UnlockedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                SaveAchievements();

                try
                {
                    var lib = new GameLibraryManager();
                    var game = lib.Games.FirstOrDefault(g => g.Id == gameId);
                    string gameTitle = game != null ? game.Title : gameId;
                    var fs = new MockFriendsService();
                    fs.LogActivity($"Unlocked achievement in {gameTitle}: {achievement.Title}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error logging unlocked achievement: {ex.Message}");
                }
            }
        }

        public void LockAchievement(string gameId, string achievementId)
        {
            var achievement = Achievements.FirstOrDefault(a => a.GameId == gameId && a.Id == achievementId);
            if (achievement != null && achievement.IsUnlocked)
            {
                achievement.IsUnlocked = false;
                achievement.UnlockedAt = "";
                SaveAchievements();
            }
        }

        public List<Achievement> GetAchievementsByGame(string gameId)
        {
            return Achievements.Where(a => a.GameId == gameId).ToList();
        }

        public double CalculateCompletionPercentage(string gameId)
        {
            var gameAchievements = GetAchievementsByGame(gameId);
            if (gameAchievements.Count == 0) return 0.0;

            int unlockedCount = gameAchievements.Count(a => a.IsUnlocked);
            return Math.Round(((double)unlockedCount / gameAchievements.Count) * 100.0, 1);
        }

        public GameAchievementProgress GetProgress(string gameId)
        {
            var gameAchievements = GetAchievementsByGame(gameId);
            int total = gameAchievements.Count;
            int unlocked = gameAchievements.Count(a => a.IsUnlocked);
            double percentage = total == 0 ? 0.0 : Math.Round(((double)unlocked / total) * 100.0, 1);

            return new GameAchievementProgress
            {
                GameId = gameId,
                UnlockedCount = unlocked,
                TotalCount = total,
                CompletionPercentage = percentage
            };
        }

        private List<Achievement> CreateDefaultAchievements()
        {
            var defaults = new List<Achievement>();

            // Chrono Cross default achievements (gameId: chrono_cross)
            defaults.Add(new Achievement
            {
                Id = "cc_another_world",
                GameId = "chrono_cross",
                Title = "Another World",
                Description = "Cross over to the alternate dimension for the first time.",
                IconPath = "media/chrono_cross/achievements/dimension.png",
                IsUnlocked = true,
                UnlockedAt = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd HH:mm"),
                Rarity = "Common (85.2%)",
                Points = 10
            });
            defaults.Add(new Achievement
            {
                Id = "cc_recruit_kid",
                GameId = "chrono_cross",
                Title = "Partner In Crime",
                Description = "Recruit Kid into your party at Termina.",
                IconPath = "media/chrono_cross/achievements/recruit_kid.png",
                IsUnlocked = true,
                UnlockedAt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm"),
                Rarity = "Common (70.5%)",
                Points = 15
            });
            defaults.Add(new Achievement
            {
                Id = "cc_defeat_lynx",
                GameId = "chrono_cross",
                Title = "Feline Nemesis",
                Description = "Defeat Lynx at Fort Dragonia.",
                IconPath = "media/chrono_cross/achievements/defeat_lynx.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Rare (12.4%)",
                Points = 30
            });
            defaults.Add(new Achievement
            {
                Id = "cc_frozen_flame",
                GameId = "chrono_cross",
                Title = "The Frozen Flame",
                Description = "Reach Chronopolis and locate the Frozen Flame.",
                IconPath = "media/chrono_cross/achievements/frozen_flame.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Very Rare (4.5%)",
                Points = 50
            });
            defaults.Add(new Achievement
            {
                Id = "cc_chrono_cross_unlocked",
                GameId = "chrono_cross",
                Title = "Chrono Cross",
                Description = "Acquire the element 'Chrono Cross' and restore the dimensions.",
                IconPath = "media/chrono_cross/achievements/chrono_cross.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Ultra Rare (1.8%)",
                Points = 100
            });

            // Shadow of the Colossus default achievements (gameId: sotc)
            defaults.Add(new Achievement
            {
                Id = "sotc_first_colossus",
                GameId = "sotc",
                Title = "Valley Gazer",
                Description = "Defeat the 1st Colossus (Valus).",
                IconPath = "media/sotc/achievements/colossus_1.png",
                IsUnlocked = true,
                UnlockedAt = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm"),
                Rarity = "Common (95.0%)",
                Points = 10
            });
            defaults.Add(new Achievement
            {
                Id = "sotc_agro_stunt",
                GameId = "sotc",
                Title = "Agro Acrobat",
                Description = "Perform a stunt while riding Agro.",
                IconPath = "media/sotc/achievements/agro.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Rare (25.3%)",
                Points = 20
            });
            defaults.Add(new Achievement
            {
                Id = "sotc_eighth_colossus",
                GameId = "sotc",
                Title = "Scaler of the Wall",
                Description = "Defeat the 8th Colossus (Kuromori).",
                IconPath = "media/sotc/achievements/colossus_8.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Rare (18.1%)",
                Points = 30
            });
            defaults.Add(new Achievement
            {
                Id = "sotc_sixteenth_colossus",
                GameId = "sotc",
                Title = "The Last Colossus",
                Description = "Defeat the 16th Colossus (Malus).",
                IconPath = "media/sotc/achievements/colossus_16.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Very Rare (8.0%)",
                Points = 60
            });

            // Demon's Souls default achievements (gameId: demons_souls)
            defaults.Add(new Achievement
            {
                Id = "ds_phalanx",
                GameId = "demons_souls",
                Title = "Slayer of Phalanx",
                Description = "Defeat the Demon 'Phalanx' in Gates of Boletaria.",
                IconPath = "media/demons_souls/achievements/phalanx.png",
                IsUnlocked = true,
                UnlockedAt = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm"),
                Rarity = "Common (90.2%)",
                Points = 10
            });
            defaults.Add(new Achievement
            {
                Id = "ds_tower_knight",
                GameId = "demons_souls",
                Title = "Slayer of Tower Knight",
                Description = "Defeat the Demon 'Tower Knight' without killing archers.",
                IconPath = "media/demons_souls/achievements/tower_knight.png",
                IsUnlocked = true,
                UnlockedAt = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm"),
                Rarity = "Rare (28.4%)",
                Points = 25
            });
            defaults.Add(new Achievement
            {
                Id = "ds_flamelurker",
                GameId = "demons_souls",
                Title = "Slayer of Flamelurker",
                Description = "Defeat the Demon 'Flamelurker' in Stonefang Tunnel.",
                IconPath = "media/demons_souls/achievements/flamelurker.png",
                IsUnlocked = true,
                UnlockedAt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm"),
                Rarity = "Rare (15.5%)",
                Points = 40
            });
            defaults.Add(new Achievement
            {
                Id = "ds_false_king",
                GameId = "demons_souls",
                Title = "Slayer of False King",
                Description = "Defeat the Demon 'False King Allant'.",
                IconPath = "media/demons_souls/achievements/false_king.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Very Rare (6.8%)",
                Points = 50
            });
            defaults.Add(new Achievement
            {
                Id = "ds_platinum",
                GameId = "demons_souls",
                Title = "Trophy Master",
                Description = "Acquire all other achievements in Boletaria.",
                IconPath = "media/demons_souls/achievements/platinum.png",
                IsUnlocked = false,
                UnlockedAt = "",
                Rarity = "Ultra Rare (0.9%)",
                Points = 150
            });

            return defaults;
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string testPath1 = Path.Combine(baseDir, path);
            if (File.Exists(testPath1)) return testPath1;

            string testPath2 = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(testPath2)) return testPath2;

            return testPath1;
        }
    }
}
