using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RetroLauncher
{
    public class SocialManager
    {
        private static readonly string FriendsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "friends.json");
        private static readonly string ActivitiesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "activities.json");
        private static readonly object FileLock = new object();

        private List<Friend> _friends = new();
        private List<FriendRequest> _requests = new();
        private List<ActivityItem> _activities = new();

        private static SocialManager? _instance;
        public static SocialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SocialManager();
                }
                return _instance;
            }
        }

        private SocialManager()
        {
            LoadFriends();
            LoadActivities();
        }

        public List<Friend> Friends => _friends;
        public List<FriendRequest> Requests => _requests;
        public List<ActivityItem> Activities => _activities;

        public void LoadFriends()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(FriendsPath))
                    {
                        string json = File.ReadAllText(FriendsPath);
                        var data = JsonSerializer.Deserialize<FriendsData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (data != null)
                        {
                            _friends = data.Friends ?? new List<Friend>();
                            _requests = data.Requests ?? new List<FriendRequest>();
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading friends: {ex.Message}");
                }
                _friends = new List<Friend>();
                _requests = new List<FriendRequest>();
            }
        }

        public void SaveFriends()
        {
            lock (FileLock)
            {
                try
                {
                    var data = new FriendsData { Friends = _friends, Requests = _requests };
                    string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(FriendsPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving friends: {ex.Message}");
                }
            }
        }

        public void LoadActivities()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ActivitiesPath))
                    {
                        string json = File.ReadAllText(ActivitiesPath);
                        _activities = JsonSerializer.Deserialize<List<ActivityItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ActivityItem>();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading activities: {ex.Message}");
                }
                _activities = new List<ActivityItem>();
            }
        }

        public void SaveActivities()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_activities, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ActivitiesPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving activities: {ex.Message}");
                }
            }
        }

        public void AddActivity(string userId, string type, string message, string relatedGameId)
        {
            var item = new ActivityItem
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = type,
                Message = message,
                RelatedGameId = relatedGameId,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            lock (FileLock)
            {
                _activities.Insert(0, item);
                SaveActivities();
            }
        }
    }
}
