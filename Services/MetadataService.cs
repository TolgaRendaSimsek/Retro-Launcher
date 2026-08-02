using System;
using System.Collections.Generic;

namespace RetroLauncher.Services
{
    public interface IGameMetadataProvider
    {
        List<MetadataSearchResult> SearchGame(string title, string platform);
        GameMetadata GetGameDetails(string gameId);
        GameImages GetImages(string gameId);
    }

    public class MetadataSearchResult
    {
        public string GameId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Platform { get; set; } = "";
        public string ReleaseYear { get; set; } = "";
    }

    public class GameImages
    {
        public string CoverUrl { get; set; } = "";
        public string HeroUrl { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public List<string> ScreenshotUrls { get; set; } = new();
    }

    public class GameMetadata
    {
        public string Title { get; set; } = "";
        public string Platform { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public string ReleaseYear { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Developer { get; set; } = "";
        public string Publisher { get; set; } = "";
        public string Description { get; set; } = "";
        public string PlayerCount { get; set; } = "";
        public string Region { get; set; } = "";
        public string FileFormat { get; set; } = "";
        public string GameId { get; set; } = "";
        public List<string> Tags { get; set; } = new();
    }

    public class LocalMetadataProvider : IGameMetadataProvider
    {
        public List<MetadataSearchResult> SearchGame(string title, string platform)
        {
            var results = new List<MetadataSearchResult>();
            string lowerTitle = title.ToLower();

            // Mock database of known games
            var mockDatabase = new List<MetadataSearchResult>
            {
                new MetadataSearchResult { GameId = "chrono_cross_slus", Title = "Chrono Cross", Platform = "Sony PlayStation 1", ReleaseYear = "1999" },
                new MetadataSearchResult { GameId = "sotn_slus", Title = "Castlevania: Symphony of the Night", Platform = "Sony PlayStation 1", ReleaseYear = "1997" },
                new MetadataSearchResult { GameId = "demons_souls_blus", Title = "Demon's Souls", Platform = "Sony PlayStation 3", ReleaseYear = "2009" }
            };

            foreach (var item in mockDatabase)
            {
                if (item.Title.ToLower().Contains(lowerTitle))
                {
                    results.Add(item);
                }
            }

            // Fallback result
            if (results.Count == 0)
            {
                results.Add(new MetadataSearchResult
                {
                    GameId = "generic_" + Guid.NewGuid().ToString().Substring(0, 8),
                    Title = title,
                    Platform = platform,
                    ReleaseYear = DateTime.Now.Year.ToString()
                });
            }

            return results;
        }

        public GameMetadata GetGameDetails(string gameId)
        {
            if (gameId.Contains("chrono_cross"))
            {
                return new GameMetadata
                {
                    Title = "Chrono Cross",
                    Platform = "Sony PlayStation 1",
                    ReleaseDate = "1999-11-18",
                    ReleaseYear = "1999",
                    Genre = "Role-Playing (RPG)",
                    Developer = "Square",
                    Publisher = "Square EA",
                    Description = "Chrono Cross is a role-playing video game developed and published by Square for the PlayStation video game console. It is the sequel to the 1995 game Chrono Trigger.",
                    PlayerCount = "1 Player",
                    Region = "NTSC-U",
                    FileFormat = "BIN/CUE",
                    GameId = "SLUS-01041",
                    Tags = new List<string> { "RPG", "Adventure", "Turn-Based", "Classic" }
                };
            }
            else if (gameId.Contains("sotn"))
            {
                return new GameMetadata
                {
                    Title = "Castlevania: Symphony of the Night",
                    Platform = "Sony PlayStation 1",
                    ReleaseDate = "1997-03-20",
                    ReleaseYear = "1997",
                    Genre = "Action-Adventure / Metroidvania",
                    Developer = "Konami Computer Entertainment Tokyo",
                    Publisher = "Konami",
                    Description = "Castlevania: Symphony of the Night is an action-adventure role-playing game developed and published by Konami in 1997 for the PlayStation.",
                    PlayerCount = "1 Player",
                    Region = "NTSC-U",
                    FileFormat = "CHD",
                    GameId = "SLUS-00067",
                    Tags = new List<string> { "Action", "Metroidvania", "Platformer", "Gothic" }
                };
            }
            else if (gameId.Contains("demons_souls"))
            {
                return new GameMetadata
                {
                    Title = "Demon's Souls",
                    Platform = "Sony PlayStation 3",
                    ReleaseDate = "2009-02-05",
                    ReleaseYear = "2009",
                    Genre = "Action Role-Playing",
                    Developer = "FromSoftware",
                    Publisher = "Sony Computer Entertainment",
                    Description = "Demon's Souls is an action role-playing game developed by FromSoftware for the PlayStation 3 under the direction of Hidetaka Miyazaki.",
                    PlayerCount = "1 Player",
                    Region = "NTSC-U",
                    FileFormat = "ISO",
                    GameId = "BLUS-30443",
                    Tags = new List<string> { "Souls-like", "RPG", "Dark Fantasy", "Action" }
                };
            }

            // Fallback generic details
            return new GameMetadata
            {
                Title = "Auto-detected Game",
                Platform = "Sony PlayStation 1",
                ReleaseDate = DateTime.Now.ToString("yyyy-MM-dd"),
                ReleaseYear = DateTime.Now.Year.ToString(),
                Genre = "General",
                Developer = "Unknown",
                Publisher = "Unknown",
                Description = "A retro game in your library.",
                PlayerCount = "1 Player",
                Region = "NTSC",
                FileFormat = "ISO",
                GameId = "SLUS-" + new Random().Next(10000, 99999),
                Tags = new List<string> { "Retro" }
            };
        }

        public GameImages GetImages(string gameId)
        {
            return new GameImages
            {
                CoverUrl = "",
                HeroUrl = "",
                LogoUrl = "",
                ScreenshotUrls = new List<string>()
            };
        }
    }
}
