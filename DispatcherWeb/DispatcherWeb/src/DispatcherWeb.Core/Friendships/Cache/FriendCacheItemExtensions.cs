using System.Collections.Generic;
using System.Linq;
using Abp;

namespace DispatcherWeb.Friendships.Cache
{
    public static class FriendCacheItemExtensions
    {
        public static bool ContainsFriend(this List<FriendCacheItem> items, FriendCacheItem item)
        {
            return items.Any(f => f.FriendTenantId == item.FriendTenantId && f.FriendUserId == item.FriendUserId);
        }

        public static UserIdentifier ToFriendIdentifier(this FriendCacheItem friendship)
        {
            return new UserIdentifier(friendship.FriendTenantId, friendship.FriendUserId);
        }
    }
}
