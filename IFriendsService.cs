using System.Collections.Generic;

namespace RetroLauncher
{
    public interface IFriendsService
    {
        UserProfile GetLocalProfile();
        void SaveLocalProfile(UserProfile profile);
        
        List<Friend> GetFriends();
        List<FriendRequest> GetPendingRequests();
        List<Friend> GetBlockedUsers();
        
        bool SendFriendRequest(string friendCodeOrName);
        bool AcceptFriendRequest(string requestId);
        bool DeclineFriendRequest(string requestId);
        bool RemoveFriend(string friendCode);
        bool BlockUser(string friendCode);
        bool UnblockUser(string friendCode);
        
        void UpdateMyStatus(ActivityStatus status, string currentlyPlaying);
        void LogActivity(string eventText);
    }
}
