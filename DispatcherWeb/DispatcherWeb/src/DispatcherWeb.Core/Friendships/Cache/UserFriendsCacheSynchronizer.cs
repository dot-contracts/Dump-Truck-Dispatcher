using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.Chat;

namespace DispatcherWeb.Friendships.Cache
{
    public class UserFriendsCacheSynchronizer :
        IAsyncEventHandler<EntityCreatedEventData<Friendship>>,
        IAsyncEventHandler<EntityDeletedEventData<Friendship>>,
        IAsyncEventHandler<EntityUpdatedEventData<Friendship>>,
        IAsyncEventHandler<EntityCreatedEventData<ChatMessage>>,
        ITransientDependency
    {
        private readonly IUserFriendsCache _userFriendsCache;

        public UserFriendsCacheSynchronizer(
            IUserFriendsCache userFriendsCache)
        {
            _userFriendsCache = userFriendsCache;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<Friendship> eventData)
        {
            await _userFriendsCache.AddFriendAsync(eventData.Entity.ToUserIdentifier(), GetFriendCacheItem(eventData.Entity));
        }

        public async Task HandleEventAsync(EntityDeletedEventData<Friendship> eventData)
        {
            await _userFriendsCache.RemoveFriendAsync(eventData.Entity.ToUserIdentifier(), GetFriendCacheItem(eventData.Entity));
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Friendship> eventData)
        {
            await _userFriendsCache.UpdateFriendAsync(eventData.Entity.ToUserIdentifier(), GetFriendCacheItem(eventData.Entity));
        }

        private FriendCacheItem GetFriendCacheItem(Friendship entity)
        {
            return new FriendCacheItem
            {
                FriendUserId = entity.FriendUserId,
                FriendTenantId = entity.FriendTenantId,
                FriendUserName = entity.FriendUserName,
                FriendTenancyName = entity.FriendTenancyName,
                FriendProfilePictureId = entity.FriendProfilePictureId,
                State = entity.State,
            };
        }

        public async Task HandleEventAsync(EntityCreatedEventData<ChatMessage> eventData)
        {
            var message = eventData.Entity;
            await _userFriendsCache.UpdateLastMessageAsync(
                new UserIdentifier(message.TenantId, message.UserId),
                new UserIdentifier(message.TargetTenantId, message.TargetUserId),
                new FriendMessageCacheItem
                {
                    Id = message.Id,
                    Message = message.Message,
                    CreationTime = message.CreationTime,
                    Side = message.Side,
                    ReadState = message.ReadState,
                    ReceiverReadState = message.ReceiverReadState,
                });
        }
    }
}
