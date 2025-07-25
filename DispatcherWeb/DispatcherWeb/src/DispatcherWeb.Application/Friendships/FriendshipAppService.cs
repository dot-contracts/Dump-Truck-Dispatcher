using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Chat;
using DispatcherWeb.Friendships.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Friendships
{
    [AbpAuthorize]
    public class FriendshipAppService : DispatcherWebAppServiceBase, IFriendshipAppService
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IAsyncOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly ITenantCache _tenantCache;
        private readonly IUserCache _userCache;
        private readonly IChatFeatureChecker _chatFeatureChecker;

        public FriendshipAppService(
            IFriendshipManager friendshipManager,
            IAsyncOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            ITenantCache tenantCache,
            IUserCache userCache,
            IChatFeatureChecker chatFeatureChecker)
        {
            _friendshipManager = friendshipManager;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _tenantCache = tenantCache;
            _userCache = userCache;
            _chatFeatureChecker = chatFeatureChecker;
        }

        [MessagingMethod]
        public async Task<FriendDto> CreateFriendshipRequest(CreateFriendshipRequestInput input)
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var probableFriend = new UserIdentifier(input.TenantId, input.UserId);

            await _chatFeatureChecker.CheckChatFeaturesAsync(userIdentifier.TenantId, probableFriend.TenantId);

            if (await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend) != null)
            {
                throw new UserFriendlyException(L("YouAlreadySentAFriendshipRequestToThisUser"));
            }

            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

            User probableFriendUser;
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                probableFriendUser = await UserManager.FindByIdAsync(input.UserId.ToString());
            }

            var friendTenancyName = await GetTenancyNameAsync(probableFriend.TenantId);
            var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName, probableFriendUser.UserName, probableFriendUser.ProfilePictureId, FriendshipState.Accepted);
            await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

            var userTenancyName = await GetTenancyNameAsync(user.TenantId);
            var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName, user.UserName, user.ProfilePictureId, FriendshipState.Accepted);
            await _friendshipManager.CreateFriendshipAsync(targetFriendship);

            var onlineClients = await _onlineClientManager.GetAllClientsAsync();

            var clientsOfProbableFriend = onlineClients.FilterBy(probableFriend).ToList();
            if (clientsOfProbableFriend.Any())
            {
                var isFriendOnline = onlineClients.Any(x => x.UserId == sourceFriendship.UserId && x.TenantId == sourceFriendship.TenantId);
                await _chatCommunicator.SendFriendshipRequestToClient(clientsOfProbableFriend, targetFriendship, false, isFriendOnline);
            }

            var senderClients = onlineClients.FilterBy(userIdentifier).ToList();
            if (senderClients.Any())
            {
                var isFriendOnline = onlineClients.Any(x => x.UserId == targetFriendship.UserId && x.TenantId == targetFriendship.TenantId);
                await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true, isFriendOnline);
            }

            var friendUser = await _userCache.GetUserAsync(probableFriend);

            var sourceFriendshipRequest = new FriendDto
            {
                FriendUserId = sourceFriendship.FriendUserId,
                FriendTenantId = sourceFriendship.FriendTenantId,
                FriendUserName = sourceFriendship.FriendUserName,
                FriendTenancyName = sourceFriendship.FriendTenancyName,
                FriendProfilePictureId = sourceFriendship.FriendProfilePictureId,
                FriendFirstName = friendUser.FirstName,
                FriendLastName = friendUser.LastName,
                State = sourceFriendship.State,
                IsOnline = onlineClients.Any(x => x.UserId == sourceFriendship.UserId && x.TenantId == sourceFriendship.TenantId),
            };

            return sourceFriendshipRequest;
        }

        private async Task<string> GetTenancyNameAsync(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var tenant = await _tenantCache.GetAsync(tenantId.Value);
                return tenant.TenancyName;
            }

            return null;
        }

        [MessagingMethod]
        public async Task<FriendDto> CreateFriendshipRequestByUserName(CreateFriendshipRequestByUserNameInput input)
        {
            var probableFriend = await GetUserIdentifier(input.TenancyName, input.UserName);
            return await CreateFriendshipRequest(new CreateFriendshipRequestInput
            {
                TenantId = probableFriend.TenantId,
                UserId = probableFriend.UserId,
            });
        }

        [MessagingMethod]
        public async Task BlockUser(BlockUserInput input)
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var friendIdentifier = new UserIdentifier(input.TenantId, input.UserId);
            await _friendshipManager.BanFriendAsync(userIdentifier, friendIdentifier);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(clients, friendIdentifier, FriendshipState.Blocked);
            }
        }

        [MessagingMethod]
        public async Task UnblockUser(UnblockUserInput input)
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var friendIdentifier = new UserIdentifier(input.TenantId, input.UserId);
            await _friendshipManager.AcceptFriendshipRequestAsync(userIdentifier, friendIdentifier);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(clients, friendIdentifier, FriendshipState.Accepted);
            }
        }

        [MessagingMethod]
        public async Task AcceptFriendshipRequest(AcceptFriendshipRequestInput input)
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var friendIdentifier = new UserIdentifier(input.TenantId, input.UserId);
            await _friendshipManager.AcceptFriendshipRequestAsync(userIdentifier, friendIdentifier);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(clients, friendIdentifier, FriendshipState.Blocked);
            }
        }

        private async Task<UserIdentifier> GetUserIdentifier(string tenancyName, string userName)
        {
            int? tenantId = null;
            if (!tenancyName.Equals("."))
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    var tenant = await TenantManager.FindByTenancyNameAsync(tenancyName);
                    if (tenant == null)
                    {
                        throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                    }

                    tenantId = tenant.Id;
                }
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await UserManager.FindByNameOrEmailAsync(userName);
                if (user == null)
                {
                    throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                }

                return user.ToUserIdentifier();
            }
        }
    }
}
