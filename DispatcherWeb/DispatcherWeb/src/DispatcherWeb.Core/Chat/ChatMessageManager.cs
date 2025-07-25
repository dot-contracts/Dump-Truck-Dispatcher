using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Friendships;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SignalR;
using DispatcherWeb.SyncRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Chat
{
    [AbpAuthorize]
    public class ChatMessageManager : DispatcherWebDomainServiceBase, IChatMessageManager
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IAsyncOnlineClientManager _onlineClientManager;
        private readonly UserManager _userManager;
        private readonly ITenantCache _tenantCache;
        private readonly IUserCache _userCache;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IUserEmailer _userEmailer;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ChatMessageManager(
            IFriendshipManager friendshipManager,
            IChatCommunicator chatCommunicator,
            IAsyncOnlineClientManager onlineClientManager,
            UserManager userManager,
            ITenantCache tenantCache,
            IUserCache userCache,
            IUserFriendsCache userFriendsCache,
            IUserEmailer userEmailer,
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatFeatureChecker chatFeatureChecker,
            ISyncRequestSender syncRequestSender,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _friendshipManager = friendshipManager;
            _chatCommunicator = chatCommunicator;
            _onlineClientManager = onlineClientManager;
            _userManager = userManager;
            _tenantCache = tenantCache;
            _userCache = userCache;
            _userFriendsCache = userFriendsCache;
            _userEmailer = userEmailer;
            _chatMessageRepository = chatMessageRepository;
            _chatFeatureChecker = chatFeatureChecker;
            _syncRequestSender = syncRequestSender;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<ChatMessage> SendMessageAsync(SendMessageInput input)
        {
            input.SenderIdentifier ??= await Session.ToUserIdentifierAsync();

            var sender = await _userCache.GetUserAsync(input.SenderIdentifier.UserId);
            var senderId = sender.ToUserIdentifier();
            var receiver = await _userCache.GetUserAsync(input.TargetUserId);
            if (receiver == null)
            {
                throw new UserFriendlyException(L("TargetUserNotFoundProbablyDeleted"));
            }
            var receiverId = receiver.ToUserIdentifier();

            await _chatFeatureChecker.CheckChatFeaturesAsync(sender.TenantId, receiver.TenantId);

            var senderFriendshipState = (await _userFriendsCache.GetFriendAsync(senderId, receiverId))?.State;
            if (senderFriendshipState == FriendshipState.Blocked)
            {
                throw new UserFriendlyException(L("UserIsBlocked"));
            }
            var receiverFriendshipState = (await _userFriendsCache.GetFriendAsync(receiverId, senderId))?.State;
            if (receiverFriendshipState == FriendshipState.Blocked)
            {
                throw new UserFriendlyException(L("UserIsBlocked"));
            }

            var sharedMessageId = Guid.NewGuid();

            var sentMessage = await HandleSenderToReceiverAsync(senderId, receiverId, input, sharedMessageId);
            await HandleReceiverToSenderAsync(senderId, receiverId, input, sharedMessageId);
            await HandleSenderUserInfoChangeAsync(sender, receiver);

            return sentMessage;
        }

        [UnitOfWork]
        public virtual async Task<long> SaveAsync(ChatMessage message)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(message.TenantId))
                {
                    return await _chatMessageRepository.InsertAndGetIdAsync(message);
                }
            });
        }

        public virtual async Task<int> GetUnreadMessageCountAsync(UserIdentifier sender, UserIdentifier receiver)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(receiver.TenantId))
                {
                    return await _chatMessageRepository.CountAsync(cm => cm.UserId == receiver.UserId
                        && cm.TargetUserId == sender.UserId
                        && cm.TargetTenantId == sender.TenantId
                        && cm.ReadState == ChatMessageReadState.Unread);
                }
            });
        }

        public async Task<ChatMessage> FindMessageAsync(int id, long userId)
        {
            return await _chatMessageRepository.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }

        private async Task<ChatMessage> HandleSenderToReceiverAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, SendMessageInput input, Guid sharedMessageId)
        {
            var friendshipState = (await _userFriendsCache.GetFriendAsync(senderIdentifier, receiverIdentifier))?.State;
            if (friendshipState == null)
            {
                friendshipState = FriendshipState.Accepted;

                var receiverTenancyName = await GetTenancyNameOrNull(receiverIdentifier.TenantId);

                var receiverUser = await _userManager.GetUserAsync(receiverIdentifier);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        senderIdentifier,
                        receiverIdentifier,
                        receiverTenancyName,
                        receiverUser.UserName,
                        receiverUser.ProfilePictureId,
                        friendshipState.Value)
                );
            }

            if (friendshipState.Value == FriendshipState.Blocked)
            {
                //Do not send message if sender banned the receiver
                return null;
            }

            var sentMessage = new ChatMessage(
                senderIdentifier,
                receiverIdentifier,
                ChatSide.Sender,
                input.Message,
                ChatMessageReadState.Read,
                sharedMessageId,
                ChatMessageReadState.Unread
            );

            await SaveAsync(sentMessage);

            await _chatCommunicator.SendMessageToClient(
                await _onlineClientManager.GetAllByUserIdAsync(senderIdentifier),
                sentMessage
                );

            return sentMessage;
        }

        private async Task<ChatMessage> HandleReceiverToSenderAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, SendMessageInput input, Guid sharedMessageId)
        {
            var friendshipState = (await _userFriendsCache.GetFriendAsync(receiverIdentifier, senderIdentifier))?.State;

            if (friendshipState == null)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                var senderUser = await _userManager.GetUserAsync(senderIdentifier);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        receiverIdentifier,
                        senderIdentifier,
                        senderTenancyName,
                        senderUser.UserName,
                        senderUser.ProfilePictureId,
                        FriendshipState.Accepted
                    )
                );
            }

            if (friendshipState == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
                throw new UserFriendlyException(L("UserIsBlocked"));
            }

            var sentMessage = new ChatMessage(
                receiverIdentifier,
                senderIdentifier,
                ChatSide.Receiver,
                input.Message,
                ChatMessageReadState.Unread,
                sharedMessageId,
                ChatMessageReadState.Read
            )
            {
                TargetTruckId = input.SourceTruckId,
                TargetTrailerId = input.SourceTrailerId,
                TargetDriverId = input.SourceDriverId,
            };

            await SaveAsync(sentMessage);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(receiverIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendMessageToClient(clients, sentMessage);
            }
            else
            {
                if (await GetUnreadMessageCountAsync(senderIdentifier, receiverIdentifier) == 1)
                {
                    var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                    await _userEmailer.TryToSendChatMessageMail(
                          await _userManager.GetUserAsync(receiverIdentifier),
                          (await _userManager.GetUserAsync(senderIdentifier)).UserName,
                          senderTenancyName,
                          sentMessage
                      );
                }

                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.SetTenantId(sentMessage.TenantId))
                    {
                        await _syncRequestSender.SendSyncRequest(new SyncRequest()
                            .AddChange(EntityEnum.ChatMessage, sentMessage.ToChangedEntity()));
                    }
                });
            }

            return sentMessage;
        }

        private async Task HandleSenderUserInfoChangeAsync(UserCacheItem sender, UserCacheItem receiver)
        {
            var senderTenant = sender.TenantId.HasValue ? await _tenantCache.GetAsync(sender.TenantId.Value) : null;
            var senderId = sender.ToUserIdentifier();
            var receiverId = receiver.ToUserIdentifier();

            var senderAsFriend = await _userFriendsCache.GetFriendAsync(receiverId, senderId);
            if (senderAsFriend == null)
            {
                return;
            }

            if (senderAsFriend.FriendTenancyName == senderTenant?.TenancyName
                && senderAsFriend.FriendUserName == sender.UserName
                && senderAsFriend.FriendProfilePictureId == sender.ProfilePictureId)
            {
                return;
            }

            var friendship = await _friendshipManager.GetFriendshipOrNullAsync(receiverId, senderId);
            if (friendship == null)
            {
                return;
            }

            friendship.FriendTenancyName = senderTenant?.TenancyName;
            friendship.FriendUserName = sender.UserName;
            friendship.FriendProfilePictureId = sender.ProfilePictureId;

            await _friendshipManager.UpdateFriendshipAsync(friendship);
        }

        private async Task<string> GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var tenant = await _tenantCache.GetAsync(tenantId.Value);
                return tenant.TenancyName;
            }

            return null;
        }

        public async Task MarkAsReadAsync(long targetUserId)
        {
            var currentUserId = Session.GetUserId();
            var currentTenantId = await Session.GetTenantIdOrNullAsync();

            var targetUser = await _userCache.GetUserAsync(targetUserId);

            // receiver messages
            var messages = await (await _chatMessageRepository.GetQueryAsync())
                 .Where(m =>
                        m.UserId == currentUserId
                        && m.TargetTenantId == targetUser.TenantId
                        && m.TargetUserId == targetUser.Id
                        && m.ReadState == ChatMessageReadState.Unread)
                 .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
            }

            // sender messages
            using (CurrentUnitOfWork.SetTenantId(targetUser.TenantId))
            {
                var reverseMessages = await (await _chatMessageRepository.GetQueryAsync())
                    .Where(m => m.UserId == targetUserId && m.TargetTenantId == currentTenantId && m.TargetUserId == currentUserId)
                    .ToListAsync();

                if (!reverseMessages.Any())
                {
                    return;
                }

                foreach (var message in reverseMessages)
                {
                    message.ChangeReceiverReadState(ChatMessageReadState.Read);
                }
            }

            var userIdentifier = await Session.ToUserIdentifierAsync();
            var friendIdentifier = targetUser.ToUserIdentifier();

            await _userFriendsCache.ResetUnreadMessageCountAsync(userIdentifier, friendIdentifier);

            var onlineClients = await _onlineClientManager.GetAllClientsAsync();

            var onlineUserClients = onlineClients.FilterBy(userIdentifier).ToList();
            if (onlineUserClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineUserClients, friendIdentifier);
            }

            var onlineFriendClients = onlineClients.FilterBy(friendIdentifier).ToList();
            if (onlineFriendClients.Any())
            {
                await _chatCommunicator.SendReadStateChangeToClients(onlineFriendClients, userIdentifier);
            }
        }
    }
}
