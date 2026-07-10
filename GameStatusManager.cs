using System;
using System.Collections.Generic;
using System.Linq;

namespace RetroLauncher
{
    public class GameStatusManager
    {
        private readonly GameLibraryManager _libraryManager;

        private static GameStatusManager? _instance;
        public static GameStatusManager Instance => _instance ??= new GameStatusManager();

        public GameStatusManager()
        {
            _libraryManager = new GameLibraryManager();
        }

        public GameStatusManager(GameLibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public bool SetGameStatus(string gameId, string status)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return false;

            string normalizedStatus = status.ToLower().Trim();
            if (normalizedStatus != "wishlist" &&
                normalizedStatus != "playing" &&
                normalizedStatus != "completed" &&
                normalizedStatus != "backlog" &&
                normalizedStatus != "dropped" &&
                normalizedStatus != "perfect_completed")
            {
                return false;
            }

            game.Status = normalizedStatus;

            if (normalizedStatus == "wishlist")
            {
                game.AddedToWishlistAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (normalizedStatus == "completed" || normalizedStatus == "perfect_completed")
            {
                game.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            SaveStatusData();
            return true;
        }

        public List<Game> GetGamesByStatus(string status)
        {
            string normalizedStatus = status.ToLower().Trim();
            return _libraryManager.Games
                .Where(g => string.Equals(g.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public bool AddToWishlist(string gameId)
        {
            return SetGameStatus(gameId, "wishlist");
        }

        public bool RemoveFromWishlist(string gameId)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return false;

            if (string.Equals(game.Status, "wishlist", StringComparison.OrdinalIgnoreCase))
            {
                game.Status = "backlog";
                SaveStatusData();
                return true;
            }
            return false;
        }

        public bool MarkAsPlaying(string gameId)
        {
            return SetGameStatus(gameId, "playing");
        }

        public bool MarkAsCompleted(string gameId)
        {
            return SetGameStatus(gameId, "completed");
        }

        public bool MarkAsDropped(string gameId)
        {
            return SetGameStatus(gameId, "dropped");
        }

        public bool SetUserRating(string gameId, int rating)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return false;

            game.UserRating = rating;
            SaveStatusData();
            return true;
        }

        public void SaveStatusData()
        {
            _libraryManager.SaveGames();
        }
    }
}
