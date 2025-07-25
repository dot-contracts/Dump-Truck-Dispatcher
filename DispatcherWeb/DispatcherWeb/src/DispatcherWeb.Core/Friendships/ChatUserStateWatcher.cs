using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.RealTime;
using DispatcherWeb.Chat;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Friendships
{
    public class ChatUserStateWatcher : ISingletonDependency
    {
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IAsyncOnlineClientManager _onlineClientManager;

        public ChatUserStateWatcher(
            IChatCommunicator chatCommunicator,
            IUserFriendsCache userFriendsCache,
            IAsyncOnlineClientManager onlineClientManager)
        {
            _chatCommunicator = chatCommunicator;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
        }

        public void Initialize()
        {
            _onlineClientManager.UserConnected += OnlineClientManager_UserConnected;
            _onlineClientManager.UserDisconnected += OnlineClientManager_UserDisconnected;
        }

        private async void OnlineClientManager_UserConnected(object sender, OnlineUserEventArgs e)
        {
            await NotifyUserConnectionStateChangeAsync(e.User, true);
        }

        private async void OnlineClientManager_UserDisconnected(object sender, OnlineUserEventArgs e)
        {
            await NotifyUserConnectionStateChangeAsync(e.User, false);
        }

        private async Task NotifyUserConnectionStateChangeAsync(UserIdentifier user, bool isConnected)
        {
            var cacheItem = await _userFriendsCache.GetCacheItemAsync(user);
            var onlineClients = await _onlineClientManager.GetAllClientsAsync();
            foreach (var friend in cacheItem.Friends)
            {
                var friendUserClients = onlineClients.FilterBy(new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)).ToList();
                if (!friendUserClients.Any())
                {
                    continue;
                }

                await _chatCommunicator.SendUserConnectionChangeToClients(friendUserClients, user, isConnected);
            }
        }
    }
}
