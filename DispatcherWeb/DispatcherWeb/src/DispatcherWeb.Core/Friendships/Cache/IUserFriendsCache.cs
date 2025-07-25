using System.Threading.Tasks;
using Abp;

namespace DispatcherWeb.Friendships.Cache
{
    public interface IUserFriendsCache
    {
        Task<UserWithFriendsCacheItem> GetCacheItemAsync(UserIdentifier userIdentifier);

        Task<UserWithFriendsCacheItem> GetCacheItemOrNullAsync(UserIdentifier userIdentifier);

        Task ResetUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier);

        Task UpdateLastMessageAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier, FriendMessageCacheItem lastMessage);

        Task AddFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend);

        Task RemoveFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend);

        Task UpdateFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend);

        Task<FriendCacheItem> GetFriendAsync(UserIdentifier userId, UserIdentifier friendId);
    }
}
