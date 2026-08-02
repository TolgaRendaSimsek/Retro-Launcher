using System;
using System.Collections.Generic;

namespace RetroLauncher.Core.Models
{
    public enum ActivityStatus
    {
        Offline,
        Online,
        Away,
        Busy
    }

    public class UserProfile
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = "RetroPlayer";
        public string FriendCode { get; set; } = "4820-9173";
        public string AvatarPath { get; set; } = "";
        public string BannerPath { get; set; } = "";
        public string Bio { get; set; } = "Emulation enthusiast.";
        public string FavoriteConsole { get; set; } = "Sony PlayStation 1";
        public List<string> FavoriteGames { get; set; } = new();
        public List<string> FavoriteGameIds { get; set; } = new();
        public string ThemeColor { get; set; } = "#6366F1";
        public ActivityStatus Status { get; set; } = ActivityStatus.Online;
        public string CurrentlyPlaying { get; set; } = "";
        public int TotalPlayTimeMinutes { get; set; } = 0;
        public List<ActivityLog> Activities { get; set; } = new();
        public List<string> ShowcaseAchievementIds { get; set; } = new();
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class ActivityLog
    {
        public string Timestamp { get; set; } = "";
        public string EventText { get; set; } = "";
    }

    public class Friend
    {
        public string userId { get; set; } = "";
        public string username { get; set; } = "";
        public string avatarPath { get; set; } = "";
        public string status { get; set; } = "Offline";
        public string currentlyPlayingGameId { get; set; } = "";
        public string lastOnline { get; set; } = "";

        // Legacy compatibility properties
        public string UserId { get => userId; set => userId = value; }
        public string Username { get => username; set => username = value; }
        public string AvatarPath { get => avatarPath; set => avatarPath = value; }
        public ActivityStatus Status
        {
            get
            {
                if (Enum.TryParse<ActivityStatus>(status, true, out var result))
                {
                    return result;
                }
                return ActivityStatus.Offline;
            }
            set => status = value.ToString();
        }
        public string CurrentlyPlayingGameId { get => currentlyPlayingGameId; set => currentlyPlayingGameId = value; }
        public string LastOnline { get => lastOnline; set => lastOnline = value; }
        
        public string FriendCode { get; set; } = "";
        public string CurrentlyPlaying { get; set; } = "";
        public bool Blocked { get; set; } = false;
    }

    public class FriendRequest
    {
        public string id { get; set; } = "";
        public string fromUserId { get; set; } = "";
        public string toUserId { get; set; } = "";
        public string status { get; set; } = "Pending";
        public string createdAt { get; set; } = "";

        // Legacy compatibility properties
        public string Id { get => id; set => id = value; }
        public string FromUserId { get => fromUserId; set => fromUserId = value; }
        public string ToUserId { get => toUserId; set => toUserId = value; }
        public string Status { get => status; set => status = value; }
        public string CreatedAt { get => createdAt; set => createdAt = value; }

        public string SenderName { get; set; } = "";
        public string SenderCode { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public bool Incoming { get; set; } = true;
    }

    public class ActivityItem
    {
        public string id { get; set; } = "";
        public string userId { get; set; } = "";
        public string type { get; set; } = "";
        public string message { get; set; } = "";
        public string relatedGameId { get; set; } = "";
        public string createdAt { get; set; } = "";

        // Legacy compatibility properties
        public string Id { get => id; set => id = value; }
        public string UserId { get => userId; set => userId = value; }
        public string Type { get => type; set => type = value; }
        public string Message { get => message; set => message = value; }
        public string RelatedGameId { get => relatedGameId; set => relatedGameId = value; }
        public string CreatedAt { get => createdAt; set => createdAt = value; }
    }
}
