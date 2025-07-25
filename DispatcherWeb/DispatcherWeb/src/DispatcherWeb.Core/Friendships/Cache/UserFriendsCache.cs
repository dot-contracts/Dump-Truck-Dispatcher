using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.Threading.Extensions;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Chat;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Friendships.Cache
{
    public class UserFriendsCache : IUserFriendsCache, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly ITenantCache _tenantCache;
        private readonly IUserCache _userCache;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _friendCacheLocks = new();

        public UserFriendsCache(
            ICacheManager cacheManager,
            IRepository<Friendship, long> friendshipRepository,
            IRepository<ChatMessage, long> chatMessageRepository,
            ITenantCache tenantCache,
            IUserCache userCache,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _cacheManager = cacheManager;
            _friendshipRepository = friendshipRepository;
            _chatMessageRepository = chatMessageRepository;
            _tenantCache = tenantCache;
            _userCache = userCache;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public virtual async Task<UserWithFriendsCacheItem> GetCacheItemAsync(UserIdentifier userIdentifier)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var cacheItem = await GetFriendCache()
                    .GetAsync(GetKey(userIdentifier),
                        async _ => await GetUserFriendsCacheItemInternalAsync(userIdentifier));

                await PopulateFriendNamesFromCache(cacheItem.Friends);

                return cacheItem;
            });
        }

        public virtual async Task<UserWithFriendsCacheItem> GetCacheItemOrNullAsync(UserIdentifier userIdentifier)
        {
            var cacheItem = await GetFriendCache()
                .GetOrDefaultAsync(GetKey(userIdentifier));

            if (cacheItem != null)
            {
                await PopulateFriendNamesFromCache(cacheItem.Friends);
            }

            return cacheItem;
        }

        public virtual async Task ResetUnreadMessageCountAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier)
        {
            using (await LockFriendCacheAsync(userIdentifier))
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    var user = await GetCacheItemOrNullAsync(userIdentifier);
                    if (user == null)
                    {
                        return;
                    }

                    var friend = user.Friends.FirstOrDefault(
                        f => f.FriendUserId == friendIdentifier.UserId
                             && f.FriendTenantId == friendIdentifier.TenantId
                    );

                    if (friend == null)
                    {
                        return;
                    }

                    friend.UnreadMessageCount = 0;
                    await UpdateUserOnCacheAsync(userIdentifier, user);
                });
            }
        }

        public virtual async Task UpdateLastMessageAsync(UserIdentifier userIdentifier, UserIdentifier friendIdentifier, FriendMessageCacheItem lastMessage)
        {
            using (await LockFriendCacheAsync(userIdentifier))
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    var user = await GetCacheItemOrNullAsync(userIdentifier);
                    if (user == null)
                    {
                        return;
                    }

                    var friend = user.Friends.FirstOrDefault(
                        f => f.FriendUserId == friendIdentifier.UserId
                             && f.FriendTenantId == friendIdentifier.TenantId
                    );

                    if (friend == null)
                    {
                        return;
                    }

                    if (lastMessage?.ReadState == ChatMessageReadState.Unread
                        && lastMessage.Side == ChatSide.Receiver)
                    {
                        friend.UnreadMessageCount += 1;
                    }
                    friend.LastMessage = lastMessage;

                    await UpdateUserOnCacheAsync(userIdentifier, user);
                });
            }
        }

        public async Task AddFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            using (await LockFriendCacheAsync(userIdentifier))
            {
                var user = await GetCacheItemOrNullAsync(userIdentifier);
                if (user == null)
                {
                    return;
                }

                if (!user.Friends.ContainsFriend(friend))
                {
                    user.Friends.Add(friend);
                    await PopulateLastMessagesFromDb(userIdentifier, friend);
                    await UpdateUserOnCacheAsync(userIdentifier, user);
                }
            }
        }

        public async Task RemoveFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            using (await LockFriendCacheAsync(userIdentifier))
            {
                var user = await GetCacheItemOrNullAsync(userIdentifier);
                if (user == null)
                {
                    return;
                }

                if (user.Friends.ContainsFriend(friend))
                {
                    user.Friends.Remove(friend);
                    await UpdateUserOnCacheAsync(userIdentifier, user);
                }
            }
        }

        public async Task UpdateFriendAsync(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            using (await LockFriendCacheAsync(userIdentifier))
            {
                var user = await GetCacheItemOrNullAsync(userIdentifier);
                if (user == null)
                {
                    return;
                }

                var existingFriend = user.Friends.FirstOrDefault(
                    f => f.FriendUserId == friend.FriendUserId
                         && f.FriendTenantId == friend.FriendTenantId
                );
                if (existingFriend != null)
                {
                    existingFriend.FriendUserName = friend.FriendUserName;
                    existingFriend.FriendTenancyName = friend.FriendTenancyName;
                    existingFriend.FriendProfilePictureId = friend.FriendProfilePictureId;
                    existingFriend.State = friend.State;
                }
                else
                {
                    user.Friends.Add(friend);
                    await PopulateLastMessagesFromDb(userIdentifier, friend);
                }
                await UpdateUserOnCacheAsync(userIdentifier, user);
            }
        }

        public async Task<FriendCacheItem> GetFriendAsync(UserIdentifier userId, UserIdentifier friendId)
        {
            var receiverCacheItem = await GetCacheItemAsync(userId);
            var senderAsFriend = receiverCacheItem?.Friends.FirstOrDefault(f => f.FriendTenantId == friendId.TenantId && f.FriendUserId == friendId.UserId);
            return senderAsFriend;
        }

        protected virtual async Task<UserWithFriendsCacheItem> GetUserFriendsCacheItemInternalAsync(UserIdentifier userIdentifier)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenancyName = userIdentifier.TenantId.HasValue
                    ? (await _tenantCache.GetOrNullAsync(userIdentifier.TenantId.Value))?.TenancyName
                    : null;

                using (_unitOfWorkManager.Current.SetTenantId(userIdentifier.TenantId))
                {
                    var friendCacheItems = await (await _friendshipRepository.GetQueryAsync())
                        .Where(friendship => friendship.UserId == userIdentifier.UserId)
                        .Select(friendship => new FriendCacheItem
                        {
                            FriendUserId = friendship.FriendUserId,
                            FriendTenantId = friendship.FriendTenantId,
                            State = friendship.State,
                            FriendUserName = friendship.FriendUserName,
                            FriendTenancyName = friendship.FriendTenancyName,
                            FriendProfilePictureId = friendship.FriendProfilePictureId,
                        }).ToListAsync();

                    await PopulateLastMessagesFromDb(userIdentifier, friendCacheItems);

                    await PopulateFriendNamesFromCache(friendCacheItems);

                    var user = await _userCache.GetUserAsync(userIdentifier);

                    return new UserWithFriendsCacheItem
                    {
                        TenantId = userIdentifier.TenantId,
                        UserId = userIdentifier.UserId,
                        TenancyName = tenancyName,
                        UserName = user.UserName,
                        ProfilePictureId = user.ProfilePictureId,
                        Friends = friendCacheItems,
                    };
                }
            });
        }

        private async Task UpdateUserOnCacheAsync(UserIdentifier userIdentifier, UserWithFriendsCacheItem user)
        {
            await GetFriendCache().SetAsync(GetKey(userIdentifier), user);
        }

        private ITypedCache<string, UserWithFriendsCacheItem> GetFriendCache()
        {
            return _cacheManager
                .GetCache(FriendCacheItem.CacheName)
                .AsTyped<string, UserWithFriendsCacheItem>();
        }

        private static string GetKey(UserIdentifier userIdentifier)
        {
            return userIdentifier.ToUserIdentifierString();
        }

        private async Task<IDisposable> LockFriendCacheAsync(UserIdentifier userIdentifier)
        {
            return await _friendCacheLocks.GetOrAdd(GetKey(userIdentifier), _ => new SemaphoreSlim(1, 1)).LockAsync();
        }

        private async Task PopulateFriendNamesFromCache(List<FriendCacheItem> friends)
        {
            var userIds = friends.Select(x => x.ToFriendIdentifier()).ToList();
            var users = await _userCache.GetUsersAsync(userIds);
            foreach (var friend in friends)
            {
                var friendUser = users.FirstOrDefault(x => x.ToUserIdentifier() == friend.ToFriendIdentifier());
                if (friendUser == null)
                {
                    friend.IsMissing = true;
                }
                else
                {
                    friend.FriendFirstName = friendUser.FirstName;
                    friend.FriendLastName = friendUser.LastName;
                }
            }
        }

        private async Task PopulateLastMessagesFromDb(UserIdentifier userIdentifier, FriendCacheItem friend)
        {
            await PopulateLastMessagesFromDb(userIdentifier, new List<FriendCacheItem> { friend });
        }

        private async Task PopulateLastMessagesFromDb(UserIdentifier userIdentifier, List<FriendCacheItem> friends)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var friendIds = friends.Select(x => x.FriendUserId).ToList();

                var messages = await (await _chatMessageRepository.GetQueryAsync())
                    .Where(m => m.UserId == userIdentifier.UserId && friendIds.Contains(m.TargetUserId))
                    .GroupBy(x => new { x.TargetUserId })
                    .Select(x => new
                    {
                        FriendUserId = x.Key.TargetUserId,
                        LastMessage = x
                            .OrderByDescending(m => m.CreationTime)
                            .Select(m => new FriendMessageCacheItem
                            {
                                Id = m.Id,
                                Message = m.Message,
                                CreationTime = m.CreationTime,
                                Side = m.Side,
                                ReadState = m.ReadState,
                                ReceiverReadState = m.ReceiverReadState,
                            })
                            .First(),
                        UnreadMessageCount = x.Count(m =>
                            m.ReadState == ChatMessageReadState.Unread
                            && m.Side == ChatSide.Receiver
                        ),
                    })
                    .ToListAsync();

                foreach (var friend in friends)
                {
                    var lastMessage = messages.FirstOrDefault(x => x.FriendUserId == friend.FriendUserId);
                    friend.LastMessage = lastMessage?.LastMessage;
                    friend.UnreadMessageCount = lastMessage?.UnreadMessageCount ?? 0;
                }
            });
        }
    }
}
