using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SyncRequests.Entities;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.SyncRequests
{
    public class SyncRequestSender : ISyncRequestSender, ITransientDependency
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public SyncRequestSender(
            IExtendedAbpSession session,
            IAppConfigurationAccessor configurationAccessor,
            IUnitOfWorkManager unitOfWorkManager,
            IBackgroundJobManager backgroundJobManager
            )
        {
            Session = session;
            _configuration = configurationAccessor.Configuration;
            _unitOfWorkManager = unitOfWorkManager;
            _backgroundJobManager = backgroundJobManager;
        }

        public IExtendedAbpSession Session { get; }

        public async Task SendSyncRequest(SyncRequest syncRequest)
        {
            var requestorUser = await Session.ToUserIdentifierAsync();
            if (_unitOfWorkManager.Current == null)
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    await SendSyncRequestImmediately(syncRequest, requestorUser);
                });
            }
            else
            {
                _unitOfWorkManager.Current.Completed += async (sender, args) =>
                {
                    await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                    {
                        await SendSyncRequestImmediately(syncRequest, requestorUser);
                    });
                };
            }
        }

        private async Task SendSyncRequestImmediately(SyncRequest syncRequest, UserIdentifier requestorUser)
        {
            syncRequest.UpdateChangesFromReferences();
            if (!syncRequest.Changes.Any())
            {
                return;
            }

            int delay = 0;
            if (syncRequest.HasChangesWithEntityType<ChangedSettings>())
            {
                delay += _configuration.GetCombinedCacheInvalidationDelay();
            }
            var delayTimeSpan = delay > 0 ? TimeSpan.FromMilliseconds(delay) : (TimeSpan?)null;

            //SignalR
            await _backgroundJobManager.EnqueueAsync<SignalRSyncRequestSenderBackgroundJob, SignalRSyncRequestSenderBackgroundJobArgs>(new SignalRSyncRequestSenderBackgroundJobArgs
            {
                RequestorUser = requestorUser,
            }.SetSyncRequestString(syncRequest), delay: delayTimeSpan);

            //RN
            await _backgroundJobManager.EnqueueAsync<DriverAppSyncRequestSenderBackgroundJob, DriverAppSyncRequestSenderBackgroundJobArgs>(new DriverAppSyncRequestSenderBackgroundJobArgs
            {
                RequestorUser = requestorUser,
            }.SetSyncRequestString(syncRequest), delay: delayTimeSpan);

            //not all PWA Push Subscriptions have DeviceId filled for some reason, so we can't reliably skip specific PWA apps yet
            if (syncRequest.IgnoreForDeviceId == null)
            {
                //PWA
                await _backgroundJobManager.EnqueueAsync<DriverApplicationPushSenderBackgroundJob, DriverApplicationPushSenderBackgroundJobArgs>(new DriverApplicationPushSenderBackgroundJobArgs
                {
                    RequestorUser = requestorUser,
                }.SetSyncRequestString(syncRequest), delay: delayTimeSpan);
            }
        }
    }
}
