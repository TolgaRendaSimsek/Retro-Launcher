using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher.Services
{
    public class SaveBackupMetadata
    {
        public string GameId { get; set; } = "";
        public string BackupName { get; set; } = "";
        public long BackupSize { get; set; }
        public string CreatedAt { get; set; } = "";

        // camelCase support
        public string gameId { get => GameId; set => GameId = value; }
        public string backupName { get => BackupName; set => BackupName = value; }
        public long backupSize { get => BackupSize; set => BackupSize = value; }
        public string createdAt { get => CreatedAt; set => CreatedAt = value; }
    }

    public class StatisticsManager
    {
        private readonly GameLibraryManager _libraryManager;

        private static StatisticsManager? _instance;
        public static StatisticsManager Instance => _instance ??= new StatisticsManager();

        public StatisticsManager()
        {
            _libraryManager = new GameLibraryManager();
        }

        public StatisticsManager(GameLibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        // Direct JSON file deserializers for requirement compliance
        private Dictionary<string, GamePlaytimeRecord> LoadPlaytimeData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playtime.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<Dictionary<string, GamePlaytimeRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch { }
            }
            return new();
        }

        private List<Achievement> LoadAchievementsData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "achievements.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<List<Achievement>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch { }
            }
            return new();
        }

        private List<EmulatorItem> LoadEmulatorsData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    using (var doc = JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.TryGetProperty("Emulators", out var listProp))
                        {
                            return JsonSerializer.Deserialize<List<EmulatorItem>>(listProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                        }
                    }
                }
                catch { }
            }
            return new();
        }

        private List<ScreenshotMetadata> LoadScreenshotsData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<List<ScreenshotMetadata>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch { }
            }
            return new();
        }

        private List<VideoMetadata> LoadVideosData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var db = JsonSerializer.Deserialize<VideoDatabase>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return db?.Clips ?? new();
                }
                catch { }
            }
            return new();
        }

        private List<SaveBackupMetadata> LoadSaveBackupsData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save_backups.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<List<SaveBackupMetadata>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch { }
            }
            return new();
        }

        // Statistics query implementations
        public int GetTotalGames()
        {
            return _libraryManager.Games.Count;
        }

        public Dictionary<string, int> GetGamesByPlatform()
        {
            return _libraryManager.Games
                .GroupBy(g => g.Platform)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public int GetTotalPlaytime()
        {
            var playtimeRecords = LoadPlaytimeData();
            return playtimeRecords.Values.Sum(r => r.TotalPlaytimeMinutes);
        }

        public Dictionary<string, int> GetPlaytimeByPlatform()
        {
            var playtimeRecords = LoadPlaytimeData();
            var dict = new Dictionary<string, int>();
            
            foreach (var game in _libraryManager.Games)
            {
                if (playtimeRecords.TryGetValue(game.Id, out var rec))
                {
                    if (dict.ContainsKey(game.Platform))
                    {
                        dict[game.Platform] += rec.TotalPlaytimeMinutes;
                    }
                    else
                    {
                        dict[game.Platform] = rec.TotalPlaytimeMinutes;
                    }
                }
            }
            return dict;
        }

        public List<Game> GetMostPlayedGames()
        {
            var playtimeRecords = LoadPlaytimeData();
            return _libraryManager.Games
                .Select(g => new { Game = g, Playtime = playtimeRecords.TryGetValue(g.Id, out var r) ? r.TotalPlaytimeMinutes : 0 })
                .Where(x => x.Playtime > 0)
                .OrderByDescending(x => x.Playtime)
                .Select(x => x.Game)
                .Take(5)
                .ToList();
        }

        public List<Game> GetRecentlyPlayedGames()
        {
            var playtimeRecords = LoadPlaytimeData();
            return _libraryManager.Games
                .Select(g => new { Game = g, Last = playtimeRecords.TryGetValue(g.Id, out var r) ? r.LastPlayed : "" })
                .Where(x => !string.IsNullOrEmpty(x.Last) && x.Last != "Never")
                .OrderByDescending(x => x.Last)
                .Select(x => x.Game)
                .Take(5)
                .ToList();
        }

        public int GetCompletedGamesCount()
        {
            return _libraryManager.Games
                .Count(g => string.Equals(g.Status, "completed", StringComparison.OrdinalIgnoreCase) || 
                            string.Equals(g.Status, "perfect_completed", StringComparison.OrdinalIgnoreCase));
        }

        public int GetFavoritesCount()
        {
            return _libraryManager.Games.Count(g => g.IsFavorite);
        }

        public double GetAchievementCompletionRate()
        {
            var achievements = LoadAchievementsData();
            if (achievements.Count == 0) return 0.0;

            // Group achievements by game
            var groups = achievements.GroupBy(a => a.GameId).ToList();
            if (groups.Count == 0) return 0.0;

            double totalPercentage = 0.0;
            foreach (var group in groups)
            {
                int total = group.Count();
                int unlocked = group.Count(a => a.IsUnlocked);
                double pct = total == 0 ? 0.0 : ((double)unlocked / total) * 100.0;
                totalPercentage += pct;
            }

            return Math.Round(totalPercentage / groups.Count, 1);
        }

        public Dictionary<string, long> GetStorageUsage()
        {
            var usage = new Dictionary<string, long>();

            // 1. ROMs size (resolved from games.json paths)
            long romsSize = 0;
            foreach (var game in _libraryManager.Games)
            {
                if (!string.IsNullOrEmpty(game.RomPath))
                {
                    string resolved = ResolvePath(game.RomPath);
                    if (File.Exists(resolved))
                    {
                        romsSize += new FileInfo(resolved).Length;
                    }
                    else if (Directory.Exists(resolved))
                    {
                        romsSize += GetDirectorySize(resolved);
                    }
                }
            }
            usage["roms"] = romsSize;

            // 2. Media size (from execution directory)
            usage["media"] = GetDirectorySize("media");

            // 3. Emulators size (from emulators.json / installation paths)
            long emulatorsSize = GetDirectorySize("Emulators");
            var emulators = LoadEmulatorsData();
            foreach (var emu in emulators)
            {
                if (!string.IsNullOrEmpty(emu.InstallFolder))
                {
                    string resolvedEmu = ResolvePath(emu.InstallFolder);
                    if (Directory.Exists(resolvedEmu) && !resolvedEmu.Contains("Emulators"))
                    {
                        emulatorsSize += GetDirectorySize(resolvedEmu);
                    }
                }
            }
            usage["emulators"] = emulatorsSize;

            // 4. Screenshots size (from screenshots.json metadata paths)
            long screenshotSize = 0;
            var screenshots = LoadScreenshotsData();
            foreach (var shot in screenshots)
            {
                string resolved = ResolvePath(shot.FilePath);
                if (File.Exists(resolved))
                {
                    screenshotSize += new FileInfo(resolved).Length;
                }
            }
            if (screenshotSize == 0) screenshotSize = GetDirectorySize("screenshots");
            usage["screenshots"] = screenshotSize;

            // 5. Videos size (from videos.json metadata paths)
            long videosSize = 0;
            var videos = LoadVideosData();
            foreach (var vid in videos)
            {
                string resolved = ResolvePath(vid.FilePath);
                if (File.Exists(resolved))
                {
                    videosSize += new FileInfo(resolved).Length;
                }
            }
            if (videosSize == 0) videosSize = GetDirectorySize("videos");
            usage["videos"] = videosSize;

            // 6. Save backups size (from save_backups.json metadata)
            long backupSize = 0;
            var backups = LoadSaveBackupsData();
            foreach (var backup in backups)
            {
                backupSize += backup.BackupSize;
            }
            if (backupSize == 0) backupSize = GetDirectorySize("backups");
            usage["saves"] = backupSize;

            return usage;
        }

        private long GetDirectorySize(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            string resolved = ResolvePath(path);
            if (!Directory.Exists(resolved)) return 0;

            try
            {
                return Directory.GetFiles(resolved, "*.*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
            }
            catch
            {
                return 0;
            }
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }
    }
}
