using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Localization;
using Abp.RealTime;
using Abp.Timing;
using Abp.UI;
using Castle.Windsor;
using DispatcherWeb.Caching;
using DispatcherWeb.Chat;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SignalR;
using DispatcherWeb.SignalR.Dto;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.Web.Chat.SignalR;

namespace DispatcherWeb.Web.SignalR
{
    public class SignalRHub : AsyncOnlineClientHubBase
    {
        private readonly IChatMessageManager _chatMessageManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IWindsorContainer _windsorContainer;
        private readonly IDriverListCache _driverListCache;
        private readonly IDriverSyncRequestStore _driverSyncRequestStore;
        private bool _isCallByRelease;

        public SignalRHub(
            IChatMessageManager chatMessageManager,
            ILocalizationManager localizationManager,
            IWindsorContainer windsorContainer,
            IDriverListCache driverListCache,
            IDriverSyncRequestStore driverSyncRequestStore,
            IAsyncOnlineClientManager asyncOnlineClientManager,
            IOnlineClientManager onlineClientManager,
            IOnlineClientInfoProvider clientInfoProvider
        ) : base(
            asyncOnlineClientManager,
            onlineClientManager,
            clientInfoProvider
        )
        {
            _chatMessageManager = chatMessageManager;
            _localizationManager = localizationManager;
            _windsorContainer = windsorContainer;
            _driverListCache = driverListCache;
            _driverSyncRequestStore = driverSyncRequestStore;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isCallByRelease)
            {
                return;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                _isCallByRelease = true;
                _windsorContainer.Release(this);
            }
        }

        public void Register()
        {
            Logger.Debug("A client is registered: " + Context.ConnectionId);
        }

        private UserIdentifier GetUserIdentifierOrThrow()
        {
            var userIdentifier = Context.ToUserIdentifier();
            if (userIdentifier == null)
            {
                Logger.Warn("Unauthorized user called on SignalR method");
                throw new AbpAuthorizationException("Unauthorized");
            }
            return userIdentifier;
        }

        public async Task<string> SendMessage(SendChatMessageInput input)
        {
            var sender = GetUserIdentifierOrThrow();

            try
            {
#pragma warning disable CS0618 // Type or member is obsolete - The usage is correct, we want to set the session values for the ChatMessageManager to use, not read the values from the obsolete session
                using (AbpSession.Use(sender.TenantId, sender.UserId))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    await _chatMessageManager.SendMessageAsync(new SendMessageInput
                    {
                        SenderIdentifier = sender,
                        TargetUserId = input.TargetUserId,
                        Message = input.Message,
                        SourceDriverId = input.SourceDriverId,
                        SourceTruckId = input.SourceTruckId,
                        SourceTrailerId = input.SourceTrailerId,
                    });
                }
                return string.Empty;
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + input.TargetUserId);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            //other exceptions will be simply thrown
            //catch (Exception ex)
            //{
            //    Logger.Warn("Could not send chat message to user: " + input.TargetUserId);
            //    Logger.Warn(ex.ToString(), ex);
            //    return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            //}
        }

        private async Task<IOnlineClient> GetCurrentClientAsync()
        {
            return await OnlineClientManager.GetByConnectionIdOrNullAsync(Context.ConnectionId)
                ?? CreateClientForCurrentConnection();
        }

        private async Task<SyncRequestFilterDto> GetDefaultFilterAsync()
        {
            var defaultFilter = (SyncRequestDefaultFilter)await SettingManager.GetSettingValueAsync<int>(AppSettings.SyncRequests.DefaultFilter);
            switch (defaultFilter)
            {
                case SyncRequestDefaultFilter.None:
                    return null;

                case SyncRequestDefaultFilter.ByDriverIds:
                    var client = await GetCurrentClientAsync();
                    var sender = Context.ToUserIdentifier();
                    if (sender == null && client.UserId == null)
                    {
                        Logger.Warn("Unauthorized user called on SignalR method SubscribeToSyncRequests");
                        throw new AbpAuthorizationException("Unauthorized");
                    }
                    if (sender == null)
                    {
                        Logger.Warn("Context.ToUserIdentifier() returned null, using client.UserId instead for user " + client.UserId);
                    }
                    var userId = sender?.UserId ?? client.UserId;
                    var tenantId = sender?.TenantId ?? client.TenantId;

                    if (tenantId == null)
                    {
                        return null;
                    }

                    var drivers = await _driverListCache.GetList(new ListCacheTenantKey(tenantId.Value));
                    var driverIds = drivers.Items
                        .Where(x => x.UserId == userId)
                        .Select(x => x.Id)
                        .ToList();

                    return new SyncRequestFilterDto
                    {
                        DriverIds = driverIds,
                    };

                default:
                    Logger.Warn($"SignalRHub.GetDefaultFilter: Unexpected value of SyncRequestDefaultFilter ({defaultFilter.ToIntString()})");
                    return null;
            }
        }

        public async Task SubscribeToSyncRequests()
        {
            await SubscribeToSyncRequestsWithFilter(await GetDefaultFilterAsync());
        }

        public async Task SubscribeToSyncRequestsWithFilter(SyncRequestFilterDto input)
        {
            var client = await GetCurrentClientAsync();

            client.SetIsSubscribedToSyncRequests();
            client.SetSyncRequestFilter(input);

            await OnlineClientManager.UpdateAsync(client);
        }

        public async Task<Dictionary<string, DateTime>> GetSyncRequestSummary(int driverId)
        {
            var sender = GetUserIdentifierOrThrow();
            return await _driverSyncRequestStore.GetAsync(driverId, sender.TenantId);
        }

        public async Task SubscribeToListCacheSyncRequests()
        {
            var sender = GetUserIdentifierOrThrow();
            await Groups.AddToGroupAsync(Context.ConnectionId,
                DispatcherWebConsts.SignalRGroups.ListCacheSyncRequest.AllTenants);
            await Groups.AddToGroupAsync(Context.ConnectionId,
                DispatcherWebConsts.SignalRGroups.ListCacheSyncRequest.Tenant(sender.TenantId));
        }

        public async Task Heartbeat()
        {
            var client = await GetCurrentClientAsync();

            client.SetLastHeartbeatDateTime(Clock.Now);

            await OnlineClientManager.UpdateAsync(client);
        }
    }
}
