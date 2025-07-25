using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Dependency;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SignalR;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Dto;
using DispatcherWeb.SyncRequests.Entities;

namespace DispatcherWeb.BackgroundJobs
{
    public class SignalRSyncRequestSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<SignalRSyncRequestSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IDriverSyncRequestStore _driverSyncRequestStore;
        private readonly IAsyncOnlineClientManager _onlineClientManager;
        private readonly ISignalRCommunicator _signalRCommunicator;

        public SignalRSyncRequestSenderBackgroundJob(
            IExtendedAbpSession session,
            IDriverSyncRequestStore driverSyncRequestStore,
            IAsyncOnlineClientManager onlineClientManager,
            ISignalRCommunicator signalRCommunicator
            ) : base(session)
        {
            _driverSyncRequestStore = driverSyncRequestStore;
            _onlineClientManager = onlineClientManager;
            _signalRCommunicator = signalRCommunicator;
        }

        public override async Task ExecuteAsync(SignalRSyncRequestSenderBackgroundJobArgs args)
        {
            try
            {
                using (Session.Use(args.RequestorUser))
                {
                    var syncRequest = args.GetSyncRequest();

                    var driverIds = syncRequest.GetDriverIds();
                    var affectsAllDrivers = syncRequest.GetDriverRelatedChanges().Any(x => x.AffectsAllDrivers);
                    var tenantId = await Session.GetTenantIdOrNullAsync();

                    await _driverSyncRequestStore.SetAsync(
                        syncRequest.Changes
                            .OfType<ISyncRequestChangeDetail>()
                            .Where(x => x.Entity is IChangedDriverAppEntity)
                            .SelectMany(x => driverIds.Select(driverId => new UpdateDriverSyncRequestTimestampInput
                            {
                                DriverId = ((IChangedDriverAppEntity)x.Entity).AffectsAllDrivers ? null : driverId,
                                TenantId = syncRequest.SuppressTenantFilter ? null : tenantId,
                                EntityType = x.EntityType.ToString(),
                            }))
                            .ToList()
                    );

                    if (!syncRequest.Changes.Any(c => c.EntityType != EntityEnum.ChatMessage))
                    {
                        return;
                    }

                    var allClients = await _onlineClientManager.GetAllClientsAsync();
                    var clients = allClients
                        .WhereIf(!syncRequest.SuppressTenantFilter, x => x.TenantId == tenantId)
                        .Where(x => x.IsSubscribedToSyncRequests())
                        .Where(x =>
                        {
                            var syncRequestFilter = x.GetSyncRequestFilter();
                            return syncRequestFilter?.DriverIds == null
                                || affectsAllDrivers
                                || driverIds.Intersect(syncRequestFilter.DriverIds).Any();
                        })
                        .WhereIf(syncRequest.IgnoreForCurrentUser, x => x.UserId != Session.UserId)
                        .ToList();

                    if (!clients.Any())
                    {
                        return;
                    }

                    await _signalRCommunicator.SendSyncRequest(clients, syncRequest);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected error in SignalRSyncRequestSenderBackgroundJob: " + e.Message, e);
            }
        }
    }
}
