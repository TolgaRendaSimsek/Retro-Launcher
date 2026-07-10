using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RetroLauncher
{
    public class ProfileManager
    {
        private static readonly string ProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.json");
        private static readonly object FileLock = new object();
        private UserProfile _profile = new();

        private static ProfileManager? _instance;
        public static ProfileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProfileManager();
                }
                return _instance;
            }
        }

        private ProfileManager()
        {
            LoadProfile();
        }

        public UserProfile Profile => _profile;

        public void LoadProfile()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ProfilePath))
                    {
                        string json = File.ReadAllText(ProfilePath);
                        var prof = JsonSerializer.Deserialize<UserProfile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (prof != null)
                        {
                            _profile = prof;
                            // Set defaults if empty
                            if (string.IsNullOrEmpty(_profile.UserId))
                            {
                                _profile.UserId = Guid.NewGuid().ToString();
                            }
                            if (string.IsNullOrEmpty(_profile.CreatedAt))
                            {
                                _profile.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
                }

                // If loading fails, start a new default profile
                _profile = new UserProfile
                {
                    UserId = Guid.NewGuid().ToString(),
                    Username = "RetroPlayer",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }
        }

        public void SaveProfile()
        {
            lock (FileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_profile, options);
                    File.WriteAllText(ProfilePath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving user profile: {ex.Message}");
                }
            }
        }

        public void UpdateAvatar(string path)
        {
            _profile.AvatarPath = path;
            SaveProfile();
        }

        public void UpdateBanner(string path)
        {
            _profile.BannerPath = path;
            SaveProfile();
        }

        public void UpdateUsername(string name)
        {
            _profile.Username = name;
            SaveProfile();
        }

        public void UpdateBio(string text)
        {
            _profile.Bio = text;
            SaveProfile();
        }

        public Dictionary<string, string> GetProfileStats()
        {
            var stats = new Dictionary<string, string>();
            var lib = new GameLibraryManager();
            stats["TotalGames"] = lib.Games.Count.ToString();
            stats["TotalPlaytimeMinutes"] = _profile.TotalPlayTimeMinutes.ToString();
            stats["FavoriteConsole"] = _profile.FavoriteConsole;
            stats["CreatedAt"] = _profile.CreatedAt;
            return stats;
        }
    }
}
