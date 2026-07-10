using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RetroLauncher
{
    public class Game
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Platform { get; set; } = "";
        public string RomPath { get; set; } = "";
        public string EmulatorId { get; set; } = "";
        
        // Media Assets
        public string CoverImagePath { get; set; } = "";
        public string HeroImagePath { get; set; } = "";
        public string LogoImagePath { get; set; } = "";
        public string IconImagePath { get; set; } = "";
        public List<string> ScreenshotPaths { get; set; } = new();
        public string TrailerVideoPath { get; set; } = "";

        // Status / Statistics
        public bool IsFavorite { get; set; } = false;
        public bool IsInstalled { get; set; } = false;
        public string LastPlayed { get; set; } = "Never";
        public int TotalPlaytimeMinutes { get; set; } = 0;
        public string DateAdded { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        
        // Metadata
        public string Genre { get; set; } = "Unknown";
        public string Description { get; set; } = "No description available.";
        public string Developer { get; set; } = "Unknown";
        public string Publisher { get; set; } = "Unknown";
        public string ReleaseYear { get; set; } = "Unknown";
        public string ReleaseDate { get; set; } = "Unknown";
        public string PlayerCount { get; set; } = "1 Player";
        public string Region { get; set; } = "Unknown";
        public string FileFormat { get; set; } = "Unknown";
        public string GameId { get; set; } = "";
        public List<string> Tags { get; set; } = new();

        // Wishlist & Game Status Tracking
        public string Status { get; set; } = "backlog"; // wishlist, playing, completed, backlog, dropped, perfect_completed
        public string AddedToWishlistAt { get; set; } = "";
        public string CompletedAt { get; set; } = "";
        public int UserRating { get; set; } = 0;
        public string UserNotes { get; set; } = "";
        public string Priority { get; set; } = "medium";

        // camelCase aliases for JSON serialization and external tools compatibility
        [JsonIgnore]
        public string title { get => Title; set => Title = value; }
        [JsonIgnore]
        public string platform { get => Platform; set => Platform = value; }
        [JsonIgnore]
        public string releaseDate { get => ReleaseDate; set => ReleaseDate = value; }
        [JsonIgnore]
        public string releaseYear { get => ReleaseYear; set => ReleaseYear = value; }
        [JsonIgnore]
        public string genre { get => Genre; set => Genre = value; }
        [JsonIgnore]
        public string developer { get => Developer; set => Developer = value; }
        [JsonIgnore]
        public string publisher { get => Publisher; set => Publisher = value; }
        [JsonIgnore]
        public string description { get => Description; set => Description = value; }
        [JsonIgnore]
        public string playerCount { get => PlayerCount; set => PlayerCount = value; }
        [JsonIgnore]
        public string region { get => Region; set => Region = value; }
        [JsonIgnore]
        public string fileFormat { get => FileFormat; set => FileFormat = value; }
        [JsonIgnore]
        public string gameId { get => GameId; set => GameId = value; }
        [JsonIgnore]
        public List<string> tags { get => Tags; set => Tags = value; }
        [JsonIgnore]
        public string status { get => Status; set => Status = value; }
        [JsonIgnore]
        public string addedToWishlistAt { get => AddedToWishlistAt; set => AddedToWishlistAt = value; }
        [JsonIgnore]
        public string completedAt { get => CompletedAt; set => CompletedAt = value; }
        [JsonIgnore]
        public int userRating { get => UserRating; set => UserRating = value; }
        [JsonIgnore]
        public string userNotes { get => UserNotes; set => UserNotes = value; }
        [JsonIgnore]
        public string priority { get => Priority; set => Priority = value; }

        public override string ToString()
        {
            return Title;
        }
    }
}
