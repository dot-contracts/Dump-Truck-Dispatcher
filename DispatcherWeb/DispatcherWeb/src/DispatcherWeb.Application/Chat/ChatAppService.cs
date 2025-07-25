using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.Friendships.Dto;
using DispatcherWeb.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Chat
{
    [AbpAuthorize]
    public class ChatAppService : DispatcherWebAppServiceBase, IChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IAsyncOnlineClientManager _onlineClientManager;
        private readonly IChatMessageManager _chatMessageManager;
        private readonly IChatMessageDriverDetailsEnricher _chatMessageDriverDetailsEnricher;

        public ChatAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IUserFriendsCache userFriendsCache,
            IAsyncOnlineClientManager onlineClientManager,
            IChatMessageManager chatMessageManager,
            IChatMessageDriverDetailsEnricher chatMessageDriverDetailsEnricher)
        {
            _chatMessageRepository = chatMessageRepository;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
            _chatMessageManager = chatMessageManager;
            _chatMessageDriverDetailsEnricher = chatMessageDriverDetailsEnricher;
        }

        [DisableAuditing, MessagingMethod]
        public async Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings()
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            if (userIdentifier == null)
            {
                return new GetUserChatFriendsWithSettingsOutput();
            }

            var cacheItem = await _userFriendsCache.GetCacheItemAsync(userIdentifier);
            var friends = cacheItem.Friends
                .OrderByDescending(x => x.LastMessage?.CreationTime ?? DateTime.MinValue)
                .Select(x => new FriendDto
                {
                    FriendUserName = x.FriendUserName,
                    FriendFirstName = x.FriendFirstName,
                    FriendUserId = x.FriendUserId,
                    FriendTenantId = x.FriendTenantId,
                    FriendLastName = x.FriendLastName,
                    FriendTenancyName = x.FriendTenancyName,
                    FriendProfilePictureId = x.FriendProfilePictureId,
                    UnreadMessageCount = x.UnreadMessageCount,
                    LastMessageCreationTime = x.LastMessage?.CreationTime,
                    State = x.State,
                })
                .ToList();

            var onlineUsers = await _onlineClientManager.GetAllClientsAsync();
            foreach (var friend in friends)
            {
                friend.IsOnline = onlineUsers.Any(u => u.UserId == friend.FriendUserId);
            }

            return new GetUserChatFriendsWithSettingsOutput
            {
                Friends = friends,
                ServerTime = Clock.Now,
            };
        }

        [DisableAuditing]
        [MessagingMethod]
        public async Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input)
        {
            var userId = AbpSession.GetUserId();
            var messages = await (await _chatMessageRepository.GetQueryAsync())
                .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                .Select(x => new ChatMessageDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    TenantId = x.TenantId,
                    TargetUserId = x.TargetUserId,
                    TargetTenantId = x.TargetTenantId,
                    Side = x.Side,
                    ReadState = x.ReadState,
                    ReceiverReadState = x.ReceiverReadState,
                    Message = x.Message,
                    CreationTime = x.CreationTime,
                    SharedMessageId = x.SharedMessageId == null ? null : x.SharedMessageId.ToString(),
                    TargetDriverId = x.TargetDriverId,
                    TargetTruckId = x.TargetTruckId,
                })
                .OrderByDescending(m => m.CreationTime)
                .PageBy(input)
                .ToListAsync();

            await _chatMessageDriverDetailsEnricher.EnrichDriverDetails(messages);

            messages.Reverse();

            return new ListResultDto<ChatMessageDto>(messages);
        }

        [MessagingMethod]
        public async Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            await _chatMessageManager.MarkAsReadAsync(input.UserId);
        }
    }
}
