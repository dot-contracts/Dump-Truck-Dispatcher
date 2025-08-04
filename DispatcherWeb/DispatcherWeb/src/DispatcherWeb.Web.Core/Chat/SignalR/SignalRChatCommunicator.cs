using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Chat;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Friendships;
using DispatcherWeb.Friendships.Dto;
using DispatcherWeb.Web.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace DispatcherWeb.Web.Chat.SignalR
{
    public class SignalRChatCommunicator : IChatCommunicator, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IHubContext<SignalRHub> _chatHub;
        private readonly IChatMessageDriverDetailsEnricher _chatMessageDriverDetailsEnricher;
        private readonly IUserCache _userCache;

        public SignalRChatCommunicator(
            IHubContext<SignalRHub> chatHub,
            IChatMessageDriverDetailsEnricher chatMessageDriverDetailsEnricher,
            IUserCache userCache
        )
        {
            _chatHub = chatHub;
            _chatMessageDriverDetailsEnricher = chatMessageDriverDetailsEnricher;
            _userCache = userCache;
            Logger = NullLogger.Instance;
        }

        public async Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message)
        {
            var chatClients = GetClients(clients);
            if (chatClients != null)
            {
                await chatClients.SendAsync("getChatMessage", await GetChatMessageDto(message).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        public async Task SendFriendshipRequestToClient(IReadOnlyList<IOnlineClient> clients, Friendship friendship, bool isOwnRequest, bool isFriendOnline)
        {
            var chatClients = GetClients(clients);
            if (chatClients != null)
            {
                await chatClients.SendAsync("getFriendshipRequest", await GetFriendDto(friendship, isFriendOnline), isOwnRequest);
            }
        }

        public async Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected)
        {
            var chatClients = GetClients(clients);
            if (chatClients != null)
            {
                await chatClients.SendAsync("getUserConnectNotification", user, isConnected);
            }
        }

        public async Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState)
        {
            var chatClients = GetClients(clients);
            if (chatClients != null)
            {
                await chatClients.SendAsync("getUserStateChange", user, newState);
            }
        }

        public async Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            var chatClients = GetClients(clients);
            if (chatClients != null)
            {
                await chatClients.SendAsync("getallUnreadMessagesOfUserRead", user);
            }
        }

        public async Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            var chatClients = GetClients(clients);
            if (chatClients != null)
            {
                await chatClients.SendAsync("getReadStateChange", user);
            }
        }

        private IClientProxy GetClients(IReadOnlyList<IOnlineClient> clients)
        {
            return _chatHub.Clients.Clients(clients.Select(c => c.ConnectionId));
        }

        private async Task<ChatMessageDto> GetChatMessageDto(ChatMessage message)
        {
            return await _chatMessageDriverDetailsEnricher.EnrichDriverDetails(new ChatMessageDto
            {
                Id = message.Id,
                UserId = message.UserId,
                TenantId = message.TenantId,
                TargetUserId = message.TargetUserId,
                TargetTenantId = message.TargetTenantId,
                Side = message.Side,
                ReadState = message.ReadState,
                ReceiverReadState = message.ReceiverReadState,
                Message = message.Message,
                CreationTime = message.CreationTime,
                SharedMessageId = message.SharedMessageId?.ToString(),
                TargetDriverId = message.TargetDriverId,
                TargetTruckId = message.TargetTruckId,
            });
        }

        private async Task<FriendDto> GetFriendDto(Friendship friendship, bool isOnline)
        {
            var friendUser = await _userCache.GetUserAsync(friendship.ToFriendIdentifier());
            return new FriendDto
            {
                FriendUserId = friendship.FriendUserId,
                FriendTenantId = friendship.FriendTenantId,
                FriendUserName = friendship.FriendUserName,
                FriendTenancyName = friendship.FriendTenancyName,
                FriendProfilePictureId = friendship.FriendProfilePictureId,
                FriendFirstName = friendUser.FirstName,
                FriendLastName = friendUser.LastName,
                IsOnline = isOnline,
                State = friendship.State,
            };
        }
    }
}
