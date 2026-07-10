using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class MetadataManager
    {
        private readonly GameLibraryManager _libraryManager;

        private static MetadataManager? _instance;
        public static MetadataManager Instance => _instance ??= new MetadataManager();

        public MetadataManager()
        {
            _libraryManager = new GameLibraryManager();
        }

        public MetadataManager(GameLibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public GameMetadata? GetMetadata(string gameId)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return null;

            return new GameMetadata
            {
                Title = game.Title,
                Platform = game.Platform,
                ReleaseDate = game.ReleaseDate,
                ReleaseYear = game.ReleaseYear,
                Genre = game.Genre,
                Developer = game.Developer,
                Publisher = game.Publisher,
                Description = game.Description,
                PlayerCount = game.PlayerCount,
                Region = game.Region,
                FileFormat = game.FileFormat,
                GameId = game.GameId,
                Tags = game.Tags != null ? new List<string>(game.Tags) : new List<string>()
            };
        }

        public void UpdateMetadata(string gameId, GameMetadata metadata)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game != null)
            {
                game.Title = metadata.Title;
                game.Platform = metadata.Platform;
                game.ReleaseDate = metadata.ReleaseDate;
                game.ReleaseYear = metadata.ReleaseYear;
                game.Genre = metadata.Genre;
                game.Developer = metadata.Developer;
                game.Publisher = metadata.Publisher;
                game.Description = metadata.Description;
                game.PlayerCount = metadata.PlayerCount;
                game.Region = metadata.Region;
                game.FileFormat = metadata.FileFormat;
                game.GameId = metadata.GameId;
                game.Tags = metadata.Tags != null ? new List<string>(metadata.Tags) : new List<string>();

                _libraryManager.UpdateGame(game);
            }
        }

        public bool EditMetadataManually(string gameId)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return false;

            using (var form = new EditMetadataForm(game))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _libraryManager.UpdateGame(game);
                    return true;
                }
            }
            return false;
        }

        public bool ValidateMetadata(string gameId)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return false;

            if (string.IsNullOrWhiteSpace(game.Title)) return false;
            if (string.IsNullOrWhiteSpace(game.Platform)) return false;

            if (!string.IsNullOrEmpty(game.ReleaseYear) && game.ReleaseYear != "Unknown")
            {
                if (game.ReleaseYear.Length != 4 || !int.TryParse(game.ReleaseYear, out _))
                {
                    return false;
                }
            }

            return true;
        }

        public GameMetadata DetectBasicMetadataFromFileName(string romPath)
        {
            var meta = new GameMetadata();
            if (string.IsNullOrEmpty(romPath)) return meta;

            string fileName = Path.GetFileNameWithoutExtension(romPath);
            string extension = Path.GetExtension(romPath).TrimStart('.').ToUpper();
            meta.FileFormat = string.IsNullOrEmpty(extension) ? "Unknown" : extension;

            // Detect Serial/Game ID matching standard format (e.g. SLUS-01041, SCUS-94447)
            var serialMatch = Regex.Match(fileName, @"([A-Z]{4}-\d{5})");
            if (serialMatch.Success)
            {
                meta.GameId = serialMatch.Groups[1].Value;
                fileName = fileName.Replace(serialMatch.Value, "").Trim();
            }

            // Remove square brackets around game IDs if left empty
            fileName = Regex.Replace(fileName, @"\[\s*\]", "").Trim();

            // Detect Region inside parentheses
            var regionMatch = Regex.Match(fileName, @"\((USA|Europe|Japan|PAL|NTSC|France|Germany|Spain|Italy|UK)\)", RegexOptions.IgnoreCase);
            if (regionMatch.Success)
            {
                meta.Region = regionMatch.Groups[1].Value.ToUpper();
                fileName = fileName.Replace(regionMatch.Value, "").Trim();
            }
            else
            {
                meta.Region = "Unknown";
            }

            // Clean clean title
            string cleanTitle = Regex.Replace(fileName, @"\s+", " ").Trim();
            cleanTitle = Regex.Replace(cleanTitle, @"\s*[\(\[][^\)\]]*[\)\]]", "").Trim();

            meta.Title = string.IsNullOrEmpty(cleanTitle) ? Path.GetFileNameWithoutExtension(romPath) : cleanTitle;
            meta.Platform = "Unknown";
            meta.ReleaseDate = "Unknown";
            meta.ReleaseYear = "Unknown";
            meta.Genre = "General";
            meta.Developer = "Unknown";
            meta.Publisher = "Unknown";
            meta.Description = "Auto-detected game metadata.";
            meta.PlayerCount = "1 Player";

            return meta;
        }

        public void SaveMetadata()
        {
            _libraryManager.SaveGames();
        }

        public void LoadMetadata()
        {
            _libraryManager.LoadGames();
        }
    }
}
