using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RetroLauncher.Services
{
    public class GameFilters
    {
        public string Platform { get; set; } = "All Platforms";
        public string Genre { get; set; } = "All Genres";
        public bool FavoritesOnly { get; set; }
        public bool InstalledOnly { get; set; }
        public bool NeverPlayedOnly { get; set; }
        public bool ShortPlaytimeOnly { get; set; } // Playtime < 60 minutes
    }

    public class RandomGameManager
    {
        private readonly GameLibraryManager _libraryManager;
        private readonly Random _random = new Random();

        private static RandomGameManager? _instance;
        public static RandomGameManager Instance => _instance ??= new RandomGameManager();

        public RandomGameManager()
        {
            _libraryManager = new GameLibraryManager();
        }

        public RandomGameManager(GameLibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public Game? GetRandomGame()
        {
            var games = _libraryManager.Games;
            if (games.Count == 0) return null;
            return games[_random.Next(games.Count)];
        }

        public Game? GetRandomGameByPlatform(string platform)
        {
            if (string.IsNullOrEmpty(platform) || platform == "All Platforms")
            {
                return GetRandomGame();
            }

            var matching = _libraryManager.Games
                .Where(g => string.Equals(g.Platform, platform, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matching.Count == 0) return null;
            return matching[_random.Next(matching.Count)];
        }

        public Game? GetRandomGameByGenre(string genre)
        {
            if (string.IsNullOrEmpty(genre) || genre == "All Genres")
            {
                return GetRandomGame();
            }

            var matching = _libraryManager.Games
                .Where(g => string.Equals(g.Genre, genre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matching.Count == 0) return null;
            return matching[_random.Next(matching.Count)];
        }

        public Game? GetRandomFavoriteGame()
        {
            var matching = _libraryManager.Games
                .Where(g => g.IsFavorite)
                .ToList();

            if (matching.Count == 0) return null;
            return matching[_random.Next(matching.Count)];
        }

        public Game? GetRandomUnplayedGame()
        {
            var matching = _libraryManager.Games
                .Where(g => g.TotalPlaytimeMinutes == 0)
                .ToList();

            if (matching.Count == 0) return null;
            return matching[_random.Next(matching.Count)];
        }

        public Game? GetRandomInstalledGame()
        {
            var matching = _libraryManager.Games
                .Where(IsRomInstalled)
                .ToList();

            if (matching.Count == 0) return null;
            return matching[_random.Next(matching.Count)];
        }

        public Game? ApplyRandomFilters(GameFilters filters)
        {
            if (filters == null) return GetRandomGame();

            var query = _libraryManager.Games.AsEnumerable();

            // Filter platform
            if (!string.IsNullOrEmpty(filters.Platform) && filters.Platform != "All Platforms")
            {
                query = query.Where(g => string.Equals(g.Platform, filters.Platform, StringComparison.OrdinalIgnoreCase));
            }

            // Filter genre
            if (!string.IsNullOrEmpty(filters.Genre) && filters.Genre != "All Genres")
            {
                query = query.Where(g => string.Equals(g.Genre, filters.Genre, StringComparison.OrdinalIgnoreCase));
            }

            // Filter favorites
            if (filters.FavoritesOnly)
            {
                query = query.Where(g => g.IsFavorite);
            }

            // Filter installed
            if (filters.InstalledOnly)
            {
                query = query.Where(IsRomInstalled);
            }

            // Filter never played
            if (filters.NeverPlayedOnly)
            {
                query = query.Where(g => g.TotalPlaytimeMinutes == 0);
            }

            // Filter short playtime (< 60 minutes)
            if (filters.ShortPlaytimeOnly)
            {
                query = query.Where(g => g.TotalPlaytimeMinutes < 60);
            }

            var matching = query.ToList();
            if (matching.Count == 0) return null;

            return matching[_random.Next(matching.Count)];
        }

        private bool IsRomInstalled(Game game)
        {
            if (string.IsNullOrEmpty(game.RomPath)) return false;
            string resolved = ResolvePath(game.RomPath);
            return File.Exists(resolved) || Directory.Exists(resolved);
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }
    }
}
