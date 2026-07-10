using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher
{
    public class Collection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string CoverImagePath { get; set; } = "";
        public List<string> GameIds { get; set; } = new();
        public bool IsAutomatic { get; set; } = false;
        public string AutoRule { get; set; } = ""; // e.g., "favorites", "recently_played", "platform:Sony PlayStation 1", "genre:RPG", "developer:Square"
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string UpdatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // camelCase aliases for JSON serialization and external tools compatibility
        public string id { get => Id; set => Id = value; }
        public string name { get => Name; set => Name = value; }
        public string description { get => Description; set => Description = value; }
        public string coverImagePath { get => CoverImagePath; set => CoverImagePath = value; }
        public List<string> gameIds { get => GameIds; set => GameIds = value; }
        public bool isAutomatic { get => IsAutomatic; set => IsAutomatic = value; }
        public string autoRule { get => AutoRule; set => AutoRule = value; }
        public string createdAt { get => CreatedAt; set => CreatedAt = value; }
        public string updatedAt { get => UpdatedAt; set => UpdatedAt = value; }
    }

    public class CollectionManager
    {
        private static readonly string CollectionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collections.json");
        private static readonly object FileLock = new object();
        
        private List<Collection> _collections = new();
        private readonly GameLibraryManager _libraryManager;

        private static CollectionManager? _instance;
        public static CollectionManager Instance => _instance ??= new CollectionManager();

        public CollectionManager()
        {
            _libraryManager = new GameLibraryManager();
            LoadCollections();
        }

        public CollectionManager(GameLibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
            LoadCollections();
        }

        public List<Collection> Collections => _collections;

        public void LoadCollections()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(CollectionsPath))
                    {
                        string json = File.ReadAllText(CollectionsPath);
                        _collections = JsonSerializer.Deserialize<List<Collection>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Collection>();
                    }
                    else
                    {
                        GenerateAutomaticCollections();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading collections: {ex.Message}");
                    GenerateAutomaticCollections();
                }
            }
        }

        public void SaveCollections()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_collections, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(CollectionsPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving collections: {ex.Message}");
                }
            }
        }

        public Collection CreateCollection(string name, string description = "", string coverImagePath = "")
        {
            var col = new Collection
            {
                Name = name,
                Description = description,
                CoverImagePath = coverImagePath,
                IsAutomatic = false,
                GameIds = new List<string>()
            };

            _collections.Add(col);
            SaveCollections();
            return col;
        }

        public bool RenameCollection(string collectionId, string newName)
        {
            var col = _collections.FirstOrDefault(c => c.Id == collectionId);
            if (col != null)
            {
                col.Name = newName;
                col.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                SaveCollections();
                return true;
            }
            return false;
        }

        public bool DeleteCollection(string collectionId)
        {
            var col = _collections.FirstOrDefault(c => c.Id == collectionId);
            if (col != null)
            {
                _collections.Remove(col);
                SaveCollections();
                return true;
            }
            return false;
        }

        public bool AddGameToCollection(string collectionId, string gameId)
        {
            var col = _collections.FirstOrDefault(c => c.Id == collectionId);
            if (col != null && !col.IsAutomatic)
            {
                if (!col.GameIds.Contains(gameId))
                {
                    col.GameIds.Add(gameId);
                    col.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SaveCollections();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveGameFromCollection(string collectionId, string gameId)
        {
            var col = _collections.FirstOrDefault(c => c.Id == collectionId);
            if (col != null && !col.IsAutomatic)
            {
                if (col.GameIds.Contains(gameId))
                {
                    col.GameIds.Remove(gameId);
                    col.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SaveCollections();
                    return true;
                }
            }
            return false;
        }

        public List<Game> GetGamesInCollection(string collectionId)
        {
            var col = _collections.FirstOrDefault(c => c.Id == collectionId);
            if (col == null) return new List<Game>();

            if (!col.IsAutomatic)
            {
                return _libraryManager.Games.Where(g => col.GameIds.Contains(g.Id)).ToList();
            }

            // Evaluate automatic criteria
            string rule = col.AutoRule.ToLower().Trim();
            if (rule == "favorites")
            {
                return _libraryManager.Games.Where(g => g.IsFavorite).ToList();
            }
            else if (rule == "recently_played")
            {
                return _libraryManager.Games
                    .Where(g => g.LastPlayed != "Never")
                    .OrderByDescending(g => g.LastPlayed)
                    .Take(10)
                    .ToList();
            }
            else if (rule.StartsWith("platform:"))
            {
                string platformVal = col.AutoRule.Substring("platform:".Length).Trim();
                return _libraryManager.Games.Where(g => string.Equals(g.Platform, platformVal, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else if (rule.StartsWith("genre:"))
            {
                string genreVal = col.AutoRule.Substring("genre:".Length).Trim();
                return _libraryManager.Games.Where(g => string.Equals(g.Genre, genreVal, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else if (rule.StartsWith("developer:"))
            {
                string devVal = col.AutoRule.Substring("developer:".Length).Trim();
                return _libraryManager.Games.Where(g => string.Equals(g.Developer, devVal, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return new List<Game>();
        }

        public void GenerateAutomaticCollections()
        {
            // Clear or seed default automatic collections if missing
            var defaults = new List<Collection>
            {
                new Collection { Name = "Favorite Games", Description = "Your favorited games in RetroLauncher", IsAutomatic = true, AutoRule = "favorites" },
                new Collection { Name = "Recently Played", Description = "Games you played recently", IsAutomatic = true, AutoRule = "recently_played" },
                new Collection { Name = "PlayStation 1 Library", Description = "Sony PlayStation 1 collection", IsAutomatic = true, AutoRule = "platform:Sony PlayStation 1" },
                new Collection { Name = "PlayStation 2 Library", Description = "Sony PlayStation 2 collection", IsAutomatic = true, AutoRule = "platform:Sony PlayStation 2" },
                new Collection { Name = "PlayStation 3 Library", Description = "Sony PlayStation 3 collection", IsAutomatic = true, AutoRule = "platform:Sony PlayStation 3" }
            };

            foreach (var col in defaults)
            {
                if (!_collections.Any(c => c.IsAutomatic && c.AutoRule == col.AutoRule))
                {
                    _collections.Add(col);
                }
            }

            SaveCollections();
        }
    }
}
