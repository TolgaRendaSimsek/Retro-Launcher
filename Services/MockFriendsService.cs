using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher.Services
{
    public class FriendsData
    {
        public List<Friend> Friends { get; set; } = new();
        public List<FriendRequest> Requests { get; set; } = new();
    }

    public class MockFriendsService : IFriendsService
    {
        private readonly string _profilePath;
        private readonly string _friendsPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public MockFriendsService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _profilePath = Path.Combine(baseDir, "profiles.json");
            _friendsPath = Path.Combine(baseDir, "friends.json");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            InitializeMockDataIfNeeded();
        }

        private void InitializeMockDataIfNeeded()
        {
            // Create directories if needed
            string? profileDir = Path.GetDirectoryName(_profilePath);
            if (!string.IsNullOrEmpty(profileDir) && !Directory.Exists(profileDir))
            {
                Directory.CreateDirectory(profileDir);
            }

            // Mock profile setup
            if (!File.Exists(_profilePath))
            {
                var profile = new UserProfile
                {
                    Username = "RetroPlayer",
                    FriendCode = "4820-9173",
                    Bio = "PlayStation and Retro Emulation enthusiast!",
                    FavoriteConsole = "Sony PlayStation 1",
                    FavoriteGames = new List<string> { "Chrono Cross", "Castlevania: Symphony of the Night" },
                    ThemeColor = "#6366F1",
                    Status = ActivityStatus.Online,
                    CurrentlyPlaying = "",
                    TotalPlayTimeMinutes = 120,
                    Activities = new List<ActivityLog>
                    {
                        new ActivityLog { Timestamp = DateTime.Now.AddDays(-2).ToString("g"), EventText = "Created RetroLauncher profile." },
                        new ActivityLog { Timestamp = DateTime.Now.AddDays(-1).ToString("g"), EventText = "Added Chrono Cross to library." }
                    }
                };
                SaveLocalProfile(profile);
            }

            // Mock friends setup
            if (!File.Exists(_friendsPath))
            {
                var data = new FriendsData();
                data.Friends.Add(new Friend
                {
                    Username = "Speedrunner99",
                    FriendCode = "1111-2222",
                    AvatarPath = "",
                    Status = ActivityStatus.Online,
                    CurrentlyPlaying = "Chrono Cross",
                    Blocked = false
                });
                data.Friends.Add(new Friend
                {
                    Username = "PixelArtist",
                    FriendCode = "3333-4444",
                    AvatarPath = "",
                    Status = ActivityStatus.Away,
                    CurrentlyPlaying = "",
                    Blocked = false
                });
                data.Friends.Add(new Friend
                {
                    Username = "NoobSlayer",
                    FriendCode = "5555-6666",
                    AvatarPath = "",
                    Status = ActivityStatus.Offline,
                    CurrentlyPlaying = "",
                    Blocked = false
                });
                data.Friends.Add(new Friend
                {
                    Username = "SpammerBot",
                    FriendCode = "9999-9999",
                    AvatarPath = "",
                    Status = ActivityStatus.Offline,
                    CurrentlyPlaying = "",
                    Blocked = true
                });

                // Pending requests
                data.Requests.Add(new FriendRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderName = "RetroKid",
                    SenderCode = "7777-8888",
                    Timestamp = DateTime.Now.AddHours(-1).ToString("g"),
                    Incoming = true
                });
                data.Requests.Add(new FriendRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderName = "TrophyHunter",
                    SenderCode = "2222-8888",
                    Timestamp = DateTime.Now.AddHours(-3).ToString("g"),
                    Incoming = false
                });

                SaveFriendsData(data);
            }
        }

        public UserProfile GetLocalProfile()
        {
            try
            {
                if (File.Exists(_profilePath))
                {
                    string json = File.ReadAllText(_profilePath);
                    return JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions) ?? new UserProfile();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading profile: {ex.Message}");
            }
            return new UserProfile();
        }

        public void SaveLocalProfile(UserProfile profile)
        {
            try
            {
                string json = JsonSerializer.Serialize(profile, _jsonOptions);
                File.WriteAllText(_profilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
            }
        }

        private FriendsData GetFriendsData()
        {
            try
            {
                if (File.Exists(_friendsPath))
                {
                    string json = File.ReadAllText(_friendsPath);
                    return JsonSerializer.Deserialize<FriendsData>(json, _jsonOptions) ?? new FriendsData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading friends list: {ex.Message}");
            }
            return new FriendsData();
        }

        private void SaveFriendsData(FriendsData data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(_friendsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving friends list: {ex.Message}");
            }
        }

        public List<Friend> GetFriends()
        {
            return GetFriendsData().Friends.Where(f => !f.Blocked).ToList();
        }

        public List<FriendRequest> GetPendingRequests()
        {
            return GetFriendsData().Requests;
        }

        public List<Friend> GetBlockedUsers()
        {
            return GetFriendsData().Friends.Where(f => f.Blocked).ToList();
        }

        public bool SendFriendRequest(string friendCodeOrName)
        {
            if (string.IsNullOrWhiteSpace(friendCodeOrName)) return false;

            var profile = GetLocalProfile();
            string trimmed = friendCodeOrName.Trim();

            // Self add validation
            if (trimmed == profile.Username || trimmed == profile.FriendCode)
                return false;

            var data = GetFriendsData();

            // Check if already friends
            if (data.Friends.Any(f => (f.Username.Equals(trimmed, StringComparison.OrdinalIgnoreCase) || f.FriendCode == trimmed) && !f.Blocked))
                return false;

            // Check if already blocked
            if (data.Friends.Any(f => (f.Username.Equals(trimmed, StringComparison.OrdinalIgnoreCase) || f.FriendCode == trimmed) && f.Blocked))
                return false;

            // Check if already in pending requests
            if (data.Requests.Any(r => r.SenderName.Equals(trimmed, StringComparison.OrdinalIgnoreCase) || r.SenderCode == trimmed))
                return false;

            // Generate request
            var newRequest = new FriendRequest
            {
                Id = Guid.NewGuid().ToString(),
                SenderName = trimmed.Contains("-") ? "User_" + trimmed.Substring(0, 4) : trimmed,
                SenderCode = trimmed.Contains("-") ? trimmed : "0000-0000",
                Timestamp = DateTime.Now.ToString("g"),
                Incoming = false
            };

            data.Requests.Add(newRequest);
            SaveFriendsData(data);

            LogActivity($"Sent friend request to {newRequest.SenderName}.");
            return true;
        }

        public bool AcceptFriendRequest(string requestId)
        {
            var data = GetFriendsData();
            var req = data.Requests.FirstOrDefault(r => r.Id == requestId);
            if (req == null) return false;

            // Remove request
            data.Requests.Remove(req);

            // Add friend
            var newFriend = new Friend
            {
                Username = req.SenderName,
                FriendCode = req.SenderCode,
                AvatarPath = "",
                Status = ActivityStatus.Online, // Mock status
                CurrentlyPlaying = "",
                Blocked = false
            };

            // Double check duplicate friend
            if (!data.Friends.Any(f => f.FriendCode == newFriend.FriendCode))
            {
                data.Friends.Add(newFriend);
            }

            SaveFriendsData(data);
            LogActivity($"Accepted friend request from {req.SenderName}.");
            return true;
        }

        public bool DeclineFriendRequest(string requestId)
        {
            var data = GetFriendsData();
            var req = data.Requests.FirstOrDefault(r => r.Id == requestId);
            if (req == null) return false;

            data.Requests.Remove(req);
            SaveFriendsData(data);
            LogActivity($"Declined friend request from {req.SenderName}.");
            return true;
        }

        public bool RemoveFriend(string friendCode)
        {
            var data = GetFriendsData();
            var friend = data.Friends.FirstOrDefault(f => f.FriendCode == friendCode && !f.Blocked);
            if (friend == null) return false;

            data.Friends.Remove(friend);
            SaveFriendsData(data);
            LogActivity($"Removed friend {friend.Username}.");
            return true;
        }

        public bool BlockUser(string friendCode)
        {
            var data = GetFriendsData();
            var friend = data.Friends.FirstOrDefault(f => f.FriendCode == friendCode);
            
            if (friend != null)
            {
                friend.Blocked = true;
                friend.Status = ActivityStatus.Offline;
                friend.CurrentlyPlaying = "";
            }
            else
            {
                data.Friends.Add(new Friend
                {
                    Username = "BlockedUser_" + friendCode.Substring(0, Math.Min(4, friendCode.Length)),
                    FriendCode = friendCode,
                    Blocked = true,
                    Status = ActivityStatus.Offline
                });
            }

            SaveFriendsData(data);
            LogActivity($"Blocked user {friendCode}.");
            return true;
        }

        public bool UnblockUser(string friendCode)
        {
            var data = GetFriendsData();
            var friend = data.Friends.FirstOrDefault(f => f.FriendCode == friendCode && f.Blocked);
            if (friend == null) return false;

            // Remove completely, or reset block status
            data.Friends.Remove(friend);
            SaveFriendsData(data);
            LogActivity($"Unblocked user {friendCode}.");
            return true;
        }

        public void UpdateMyStatus(ActivityStatus status, string currentlyPlaying)
        {
            var profile = GetLocalProfile();
            profile.Status = status;
            profile.CurrentlyPlaying = currentlyPlaying;
            SaveLocalProfile(profile);
        }

        public void LogActivity(string eventText)
        {
            var profile = GetLocalProfile();
            profile.Activities.Insert(0, new ActivityLog
            {
                Timestamp = DateTime.Now.ToString("g"),
                EventText = eventText
            });

            // Keep max 50 items
            if (profile.Activities.Count > 50)
            {
                profile.Activities = profile.Activities.Take(50).ToList();
            }

            SaveLocalProfile(profile);
        }
    }
}
