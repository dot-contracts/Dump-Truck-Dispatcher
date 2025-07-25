using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverApp.Friends.Dto;
using DispatcherWeb.Friendships.Cache;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.DriverApp.Friends
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class FriendAppService : DispatcherWebDriverAppAppServiceBase, IFriendAppService
    {
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IAsyncOnlineClientManager _onlineClientManager;

        public FriendAppService(
            IUserFriendsCache userFriendsCache,
            IAsyncOnlineClientManager onlineClientManager
        )
        {
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
        }

        public async Task<IPagedResult<FriendDto>> Get(GetInput input)
        {
            var cacheItem = await _userFriendsCache.GetCacheItemAsync(await Session.ToUserIdentifierAsync());
            var allFriends = cacheItem.Friends.Select(x => new FriendDto
            {
                User = new Users.Dto.UserDto
                {
                    Id = x.FriendUserId,
                    FirstName = x.FriendFirstName,
                    LastName = x.FriendLastName,
                    //Commented out for #13244. We'll need to send a smaller image to the driver app later, and make sure the driver app caches it.
                    //ProfilePictureId = x.FriendProfilePictureId
                },
                UnreadMessageCount = x.UnreadMessageCount,
                LastMessage = x.LastMessage == null ? null : new Messages.Dto.MessageDto
                {
                    Id = x.LastMessage.Id,
                    Message = x.LastMessage.Message,
                    CreationTime = x.LastMessage.CreationTime,
                    Side = x.LastMessage.Side,
                    ReadState = x.LastMessage.ReadState,
                    ReceiverReadState = x.LastMessage.ReceiverReadState,
                },
            }).ToList();

            var totalFriendCount = allFriends.Count;

            var onlineUsers = await _onlineClientManager.GetAllClientsAsync();
            foreach (var friend in allFriends)
            {
                friend.IsOnline = onlineUsers.Any(x => x.UserId == friend.User.Id);
            }

            allFriends = allFriends
                .OrderByDescending(x => x.LastMessage?.CreationTime ?? DateTime.MinValue)
                .ToList();

            var friends = allFriends
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<FriendDto>(
                totalFriendCount,
                friends);
        }
    }
}
