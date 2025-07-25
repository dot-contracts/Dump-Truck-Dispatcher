using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Chat;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Chat.Exporting;
using DispatcherWeb.Dto;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Gdpr
{
    public class ChatUserCollectedDataProvider : IUserCollectedDataProvider, ITransientDependency
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IChatMessageListExcelExporter _chatMessageListExcelExporter;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<UserAccount, long> _userAccountRepository;
        private readonly IRepository<Tenant> _tenantRepository;

        public ChatUserCollectedDataProvider(
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatMessageListExcelExporter chatMessageListExcelExporter,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<UserAccount, long> userAccountRepository,
            IRepository<Tenant> tenantRepository)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatMessageListExcelExporter = chatMessageListExcelExporter;
            _unitOfWorkManager = unitOfWorkManager;
            _userAccountRepository = userAccountRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<List<FileDto>> GetFiles(UserIdentifier user)
        {
            var conversations = await GetUserChatMessages(user.TenantId, user.UserId);

            Dictionary<UserIdentifier, string> relatedUsernames;
            Dictionary<int, string> relatedTenancyNames;

            using (_unitOfWorkManager.Current.SetTenantId(null))
            {
                var tenantIds = conversations.Select(c => c.Key.TenantId);
                relatedTenancyNames = await (await _tenantRepository.GetQueryAsync())
                    .Where(t => tenantIds.Contains(t.Id))
                    .ToDictionaryAsync(t => t.Id, t => t.TenancyName);
                relatedUsernames = await GetFriendUsernames(conversations.Select(c => c.Key).ToList());
            }

            var chatMessageFiles = new List<FileDto>();

            foreach (var conversation in conversations)
            {
                foreach (var message in conversation.Value)
                {
                    message.TargetTenantName = message.TargetTenantId.HasValue
                        ? relatedTenancyNames[message.TargetTenantId.Value]
                        : ".";

                    message.TargetUserName = relatedUsernames[new UserIdentifier(message.TargetTenantId, message.TargetUserId)];
                }

                var messages = conversation.Value.OrderBy(m => m.CreationTime).ToList();
                chatMessageFiles.Add(await _chatMessageListExcelExporter.ExportToFileAsync(user, messages));
            }

            return chatMessageFiles;
        }

        private async Task<Dictionary<UserIdentifier, string>> GetFriendUsernames(List<UserIdentifier> users)
        {
            var predicate = PredicateBuilder.New<UserAccount>();

            foreach (var user in users)
            {
                predicate = predicate.Or(ua => ua.TenantId == user.TenantId && ua.UserId == user.UserId);
            }

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var userList = (await _userAccountRepository.GetAllListAsync(predicate))
                    .Select(ua => new
                    {
                        ua.TenantId,
                        ua.UserId,
                        ua.UserName,
                    }).Distinct();

                return userList.ToDictionary(ua => new UserIdentifier(ua.TenantId, ua.UserId), ua => ua.UserName);
            }
        }

        private async Task<Dictionary<UserIdentifier, List<ChatMessageExportDto>>> GetUserChatMessages(int? tenantId, long userId)
        {
            var conversations = (await (await _chatMessageRepository.GetQueryAsync())
                    .Where(message => message.UserId == userId && message.TenantId == tenantId)
                    .ToListAsync()
                )
                .GroupBy(message => new
                {
                    message.TargetTenantId,
                    message.TargetUserId,
                })
                .Select(messageGrouped => new
                {
                    TargetTenantId = messageGrouped.Key.TargetTenantId,
                    TargetUserId = messageGrouped.Key.TargetUserId,
                    Messages = messageGrouped.Select(m => new ChatMessageExportDto
                    {
                        TargetTenantId = m.TargetTenantId,
                        TargetUserId = m.TargetUserId,
                        Side = m.Side,
                        ReadState = m.ReadState,
                        ReceiverReadState = m.ReceiverReadState,
                        Message = m.Message,
                        CreationTime = m.CreationTime,
                    }).ToList(),
                }).ToList();

            return conversations.ToDictionary(c => new UserIdentifier(c.TargetTenantId, c.TargetUserId), c => c.Messages);
        }
    }
}
