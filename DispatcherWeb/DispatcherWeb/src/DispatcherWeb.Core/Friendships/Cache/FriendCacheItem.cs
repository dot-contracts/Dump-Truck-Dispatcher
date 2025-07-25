using System;

namespace DispatcherWeb.Friendships.Cache
{
    public class FriendCacheItem
    {
        public const string CacheName = "AppUserFriendCache";

        public long FriendUserId { get; set; }

        public int? FriendTenantId { get; set; }

        public string FriendUserName { get; set; }

        public string FriendTenancyName { get; set; }

        public string FriendFirstName { get; set; }

        public string FriendLastName { get; set; }

        public Guid? FriendProfilePictureId { get; set; }

        public int UnreadMessageCount { get; set; }

        public FriendshipState State { get; set; }

        public bool IsMissing { get; internal set; }

        public FriendMessageCacheItem LastMessage { get; set; }
    }
}
