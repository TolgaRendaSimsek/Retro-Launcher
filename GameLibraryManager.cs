using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class GameLibraryManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games.json");
        public List<Game> Games { get; private set; } = new();
        private readonly JsonSerializerOptions _jsonOptions;

        public GameLibraryManager()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            LoadGames();
        }

        public void LoadGames()
        {
            try
            {
                string pathToUse = ResolvePath(ConfigPath);
                
                if (!File.Exists(pathToUse))
                {
                    Games = CreateDefaultGames();
                    SaveGames();
                    return;
                }

                string json = File.ReadAllText(pathToUse);
                
                // Attempt to deserialize new Game model with legacy fallback migration
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        JsonElement gamesArray;
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("Games", out var gamesProp))
                        {
                            gamesArray = gamesProp;
                        }
                        else
                        {
                            gamesArray = doc.RootElement;
                        }

                        if (gamesArray.ValueKind == JsonValueKind.Array)
                        {
                            Games = new List<Game>();
                            foreach (var element in gamesArray.EnumerateArray())
                            {
                                var game = JsonSerializer.Deserialize<Game>(element.GetRawText(), _jsonOptions) ?? new Game();
                                
                                // Migrate legacy fields if they exist and target properties are empty
                                if (string.IsNullOrEmpty(game.Title) && element.TryGetProperty("Name", out var nameProp))
                                {
                                    game.Title = nameProp.GetString() ?? "";
                                }
                                if (string.IsNullOrEmpty(game.Platform) && element.TryGetProperty("Console", out var consoleProp))
                                {
                                    game.Platform = consoleProp.GetString() ?? "";
                                }
                                if (string.IsNullOrEmpty(game.EmulatorId) && element.TryGetProperty("EmulatorPath", out var emuProp))
                                {
                                    game.EmulatorId = emuProp.GetString() ?? "";
                                }
                                
                                Games.Add(game);
                            }
                        }
                        else
                        {
                            Games = CreateDefaultGames();
                            SaveGames();
                        }
                    }
                }
                catch
                {
                    Games = CreateDefaultGames();
                    SaveGames();
                }

                // Verify installation status for all games on load and sync playtime stats
                foreach (var game in Games)
                {
                    string resolvedRom = ResolvePath(game.RomPath);
                    game.IsInstalled = !string.IsNullOrEmpty(game.RomPath) && (File.Exists(resolvedRom) || Directory.Exists(resolvedRom));
                    
                    var record = PlaytimeManager.Instance.GetOrCreateRecord(game.Id);
                    if (record.TotalPlaytimeMinutes > 0)
                    {
                        game.TotalPlaytimeMinutes = record.TotalPlaytimeMinutes;
                    }
                    if (!string.IsNullOrEmpty(record.LastPlayed))
                    {
                        game.LastPlayed = record.LastPlayed;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading games library: {ex.Message}");
                Games = CreateDefaultGames();
                SaveGames();
            }
        }

        public void SaveGames()
        {
            try
            {
                string pathToUse = ConfigPath;
                string? dir = Path.GetDirectoryName(pathToUse);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Save in a wrapper object to match standard settings format: { "Games": [...] }
                var wrapper = new { Games = Games };
                string json = JsonSerializer.Serialize(wrapper, _jsonOptions);
                File.WriteAllText(pathToUse, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save game database.\n\nError: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddGame(Game game)
        {
            if (game == null) return;
            string resolvedRom = ResolvePath(game.RomPath);
            game.IsInstalled = !string.IsNullOrEmpty(game.RomPath) && (File.Exists(resolvedRom) || Directory.Exists(resolvedRom));
            Games.Add(game);
            SaveGames();
        }

        public void RemoveGame(Game game)
        {
            if (game == null) return;
            var match = Games.FirstOrDefault(g => g.Id == game.Id);
            if (match != null)
            {
                Games.Remove(match);
                SaveGames();
            }
        }

        public void UpdateGame(Game game)
        {
            if (game == null) return;
            var match = Games.FirstOrDefault(g => g.Id == game.Id);
            if (match != null)
            {
                // Update properties
                match.Title = game.Title;
                match.Platform = game.Platform;
                match.RomPath = game.RomPath;
                match.EmulatorId = game.EmulatorId;
                match.CoverImagePath = game.CoverImagePath;
                match.HeroImagePath = game.HeroImagePath;
                match.LogoImagePath = game.LogoImagePath;
                match.IconImagePath = game.IconImagePath;
                match.ScreenshotPaths = game.ScreenshotPaths;
                match.TrailerVideoPath = game.TrailerVideoPath;
                match.IsFavorite = game.IsFavorite;
                match.Genre = game.Genre;
                match.Description = game.Description;
                match.Developer = game.Developer;
                match.Publisher = game.Publisher;
                match.ReleaseYear = game.ReleaseYear;
                match.TotalPlaytimeMinutes = game.TotalPlaytimeMinutes;
                match.LastPlayed = game.LastPlayed;

                string resolvedRom = ResolvePath(match.RomPath);
                match.IsInstalled = !string.IsNullOrEmpty(match.RomPath) && (File.Exists(resolvedRom) || Directory.Exists(resolvedRom));

                SaveGames();
            }
        }

        public List<Game> SearchGames(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Games;
            string lowerQuery = query.Trim().ToLower();
            return Games.Where(g => 
                g.Title.ToLower().Contains(lowerQuery) || 
                g.Platform.ToLower().Contains(lowerQuery) || 
                g.Genre.ToLower().Contains(lowerQuery) ||
                g.Description.ToLower().Contains(lowerQuery)
            ).ToList();
        }

        public List<Game> FilterGames(List<Game> list, string platform, string filterBy)
        {
            var result = list;

            // 1. Sidebar Platform Filter
            if (!string.IsNullOrEmpty(platform) && !platform.Equals("All Games", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(g => g.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // 2. Toolbar Status Filter
            if (!string.IsNullOrEmpty(filterBy))
            {
                if (filterBy.Equals("Favorites Only", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Where(g => g.IsFavorite).ToList();
                }
                else if (filterBy.Equals("Installed Only", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Where(g => g.IsInstalled).ToList();
                }
                else if (filterBy.Equals("Missing ROMs", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Where(g => !g.IsInstalled).ToList();
                }
            }

            return result;
        }

        public List<Game> SortGames(List<Game> list, string sortBy)
        {
            if (string.IsNullOrEmpty(sortBy)) return list;

            if (sortBy.Equals("Title A-Z", StringComparison.OrdinalIgnoreCase))
            {
                return list.OrderBy(g => g.Title).ToList();
            }
            else if (sortBy.Equals("Last Played", StringComparison.OrdinalIgnoreCase))
            {
                return list.OrderByDescending(g => 
                {
                    if (string.IsNullOrEmpty(g.LastPlayed) || g.LastPlayed.Equals("Never", StringComparison.OrdinalIgnoreCase))
                    {
                        return DateTime.MinValue;
                    }
                    return DateTime.TryParse(g.LastPlayed, out DateTime dt) ? dt : DateTime.MinValue;
                }).ToList();
            }
            else if (sortBy.Equals("Most Played", StringComparison.OrdinalIgnoreCase))
            {
                return list.OrderByDescending(g => g.TotalPlaytimeMinutes).ToList();
            }
            else if (sortBy.Equals("Recently Added", StringComparison.OrdinalIgnoreCase))
            {
                return list.OrderByDescending(g => 
                {
                    if (string.IsNullOrEmpty(g.DateAdded)) return DateTime.MinValue;
                    return DateTime.TryParse(g.DateAdded, out DateTime dt) ? dt : DateTime.MinValue;
                }).ToList();
            }

            return list;
        }

        private List<Game> CreateDefaultGames()
        {
            return new List<Game>
            {
                new Game
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Chrono Cross",
                    Platform = "Sony PlayStation 1",
                    RomPath = "Games/PS1/Chrono Cross.chd",
                    EmulatorId = "Emulators/PS1/duckstation.exe",
                    CoverImagePath = "Assets/Covers/chrono_cross.jpg",
                    HeroImagePath = "",
                    LogoImagePath = "",
                    IconImagePath = "",
                    IsFavorite = true,
                    TotalPlaytimeMinutes = 120,
                    LastPlayed = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm"),
                    DateAdded = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd"),
                    Genre = "Role-Playing",
                    Description = "A role-playing video game developed and published by Square for the PlayStation video game console. It is the sequel to the 1995 game Chrono Trigger.",
                    Developer = "Square",
                    Publisher = "Square",
                    ReleaseYear = "1999"
                },
                new Game
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Shadow of the Colossus",
                    Platform = "Sony PlayStation 2",
                    RomPath = "Games/PS2/Shadow of the Colossus.chd",
                    EmulatorId = "Emulators/PS2/pcsx2.exe",
                    CoverImagePath = "Assets/Covers/sotc.jpg",
                    HeroImagePath = "",
                    LogoImagePath = "",
                    IconImagePath = "",
                    IsFavorite = false,
                    TotalPlaytimeMinutes = 45,
                    LastPlayed = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd HH:mm"),
                    DateAdded = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd"),
                    Genre = "Action-Adventure",
                    Description = "An action-adventure game developed by SCE Japan Studio and Team Ico, and published by Sony Computer Entertainment for the PlayStation 2.",
                    Developer = "SCE Japan Studio / Team Ico",
                    Publisher = "Sony Computer Entertainment",
                    ReleaseYear = "2005"
                },
                new Game
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Demon's Souls",
                    Platform = "Sony PlayStation 3",
                    RomPath = "Games/PS3/GAMES/BLES00932",
                    EmulatorId = "Emulators/PS3/rpcs3.exe",
                    CoverImagePath = "Assets/Covers/demons_souls.jpg",
                    HeroImagePath = "",
                    LogoImagePath = "",
                    IconImagePath = "",
                    IsFavorite = true,
                    TotalPlaytimeMinutes = 600,
                    LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    DateAdded = DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd"),
                    Genre = "Action Role-Playing",
                    Description = "An action role-playing game developed by FromSoftware for the PlayStation 3. It was published in Japan by Sony Computer Entertainment.",
                    Developer = "FromSoftware",
                    Publisher = "Sony Computer Entertainment",
                    ReleaseYear = "2009"
                }
            };
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string testPath1 = Path.Combine(baseDir, path);
            if (File.Exists(testPath1) || Directory.Exists(testPath1)) return testPath1;

            string testPath2 = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(testPath2) || Directory.Exists(testPath2)) return testPath2;

            return testPath1;
        }
    }
}
