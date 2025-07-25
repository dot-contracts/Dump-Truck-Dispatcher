using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Castle.Core.Logging;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;
using WebPushLib = WebPush;

namespace DispatcherWeb.DriverApplication
{
    public class DriverApplicationPushSender : IDriverApplicationPushSender, ITransientDependency
    {
        private readonly IExtendedAbpSession _session;
        private readonly IRepository<DriverPushSubscription> _driverPushSubscriptionRepository;
        private readonly IRepository<PushSubscription> _pushSubscriptionRepository;
        private readonly IWebPushSender _webPushSender;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger _logger;

        public DriverApplicationPushSender(
            IExtendedAbpSession session,
            IRepository<DriverPushSubscription> driverPushSubscriptionRepository,
            IRepository<PushSubscription> pushSubscriptionRepository,
            IWebPushSender webPushSender,
            IDriverApplicationLogger driverApplicationLogger,
            IAppNotifier appNotifier,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger logger
            )
        {
            _session = session;
            _driverPushSubscriptionRepository = driverPushSubscriptionRepository;
            _pushSubscriptionRepository = pushSubscriptionRepository;
            _webPushSender = webPushSender;
            _driverApplicationLogger = driverApplicationLogger;
            _appNotifier = appNotifier;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        private async Task<List<SendPushMessageToDriversSubscriptionDto>> GetPushMessageSubscriptions(List<int> driverIds, int? ignoreForDeviceId)
        {
            var subscriptions = await (await _driverPushSubscriptionRepository.GetQueryAsync())
                .Where(x => driverIds.Contains(x.DriverId))
                .WhereIf(ignoreForDeviceId.HasValue, x => x.DeviceId != ignoreForDeviceId)
                .Select(x => new SendPushMessageToDriversSubscriptionDto
                {
                    DriverId = x.DriverId,
                    PushSubscriptionId = x.PushSubscriptionId,
                    PushSubscription = new PushSubscriptionDto
                    {
                        Endpoint = x.PushSubscription.Endpoint,
                        Keys = new PushSubscriptionDto.PushSubscriptionKeys
                        {
                            P256dh = x.PushSubscription.P256dh,
                            Auth = x.PushSubscription.Auth,
                        },
                    },
                })
                .ToListAsync();

            return subscriptions;
        }

        public async Task SendPushMessageToDriversImmediately(DriverApplicationPushSenderBackgroundJobArgs input)
        {
            try
            {
                var syncRequest = input.GetSyncRequest();

                if (syncRequest.IgnoreForDeviceId.HasValue)
                {
                    //not all PWA Push Subscriptions have DeviceId filled for some reason, so we can't reliably skip specific PWA apps yet
                    return;
                }

                var driverIds = syncRequest.GetDriverIds();

                if (!driverIds.Any())
                {
                    return;
                }

                var changedDispatches = syncRequest.GetChangedEntitiesOfType<ChangedDispatch>();
                var logMessage = syncRequest.LogMessage;
                if (changedDispatches.Any())
                {
                    logMessage += "; DispatchIds: " + string.Join(", ", changedDispatches.Select(x => x.Id).Distinct());
                }

                var subscriptions = await _unitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
                {
                    using (_session.Use(input.RequestorUser.TenantId, input.RequestorUser.UserId))
                    using (_unitOfWorkManager.Current.SetTenantId(input.RequestorUser.TenantId))
                    {
                        return await GetPushMessageSubscriptions(driverIds, syncRequest.IgnoreForDeviceId);
                    }
                });


                var processedDrivers = new List<int>();
                var results = new List<(SendPushMessageToDriversSubscriptionDto Subscription, Guid Guid, Exception Exception)>();

                foreach (var pushSub in subscriptions)
                {
                    var guid = Guid.NewGuid();
                    try
                    {
                        await _webPushSender.SendAsync(pushSub.PushSubscription, new PwaPushMessage //TODO update to send a SyncRequestPushMessage with an additional Changes field
                        {
                            Action = DriverApplicationPushAction.SilentSync,
                            Guid = guid,
                        });
                        results.Add((pushSub, guid, null));
                    }
                    catch (Exception exception)
                    {
                        results.Add((pushSub, guid, exception));
                    }
                }

                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_session.Use(input.RequestorUser.TenantId, input.RequestorUser.UserId))
                    using (_unitOfWorkManager.Current.SetTenantId(input.RequestorUser.TenantId))
                    {
                        foreach (var (pushSub, guid, unknownException) in results)
                        {
                            if (unknownException == null)
                            {
                                await _driverApplicationLogger.LogInfo(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscriptionId:D7}; {logMessage}");
                                processedDrivers.Add(pushSub.DriverId); //for these drivers we won't show a "no push" log message
                            }
                            else if (unknownException is WebPushLib.WebPushException exception)
                            {
                                switch (exception.StatusCode)
                                {
                                    case HttpStatusCode.Gone:
                                    case HttpStatusCode.NotFound:
                                        _logger.Warn($"DriverApplicationPushSender: subscription gone, http status code {exception.StatusCode}, message {exception.Message}", exception);
                                        _logger.Warn($"DriverApplicationPushSender: removing subscription {pushSub.PushSubscriptionId}");
                                        await _driverApplicationLogger.LogWarn(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscriptionId:D7} failed, subscription gone");
                                        await _driverPushSubscriptionRepository.DeleteAsync(x => x.PushSubscriptionId == pushSub.PushSubscriptionId);
                                        await _pushSubscriptionRepository.DeleteAsync(pushSub.PushSubscriptionId);
                                        break;
                                    default:
                                        _logger.Error($"DriverApplicationPushSender: error, http status code {exception.StatusCode}, message {exception.Message}", exception);
                                        await _driverApplicationLogger.LogError(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscriptionId:D7} failed, error: {exception.Message}; status code {exception.StatusCode}");
                                        break;
                                }
                            }
                            else
                            {
                                _logger.Error($"DriverApplicationPushSender: unexpected exception, message {unknownException.Message}", unknownException);
                                await _driverApplicationLogger.LogError(pushSub.DriverId, $"[Dispatcher] Sending push {guid};{pushSub.PushSubscriptionId:D7} failed, error: {unknownException.Message}; unexpected exception");
                            }
                        }

                        foreach (var driverId in driverIds)
                        {
                            if (!processedDrivers.Contains(driverId))
                            {
                                await _driverApplicationLogger.LogWarn(driverId, $"[Dispatcher] No push subscription found. {logMessage}");
                            }
                        }
                    }
                });
            }
            catch (Exception exception)
            {
                _logger.Error("Unexpected error in DriverApplicationPushSender.SendPushMessageToDriversImmediately: " + exception.Message, exception);
                await _appNotifier.SendMessageAsync(input.RequestorUser, "Sending sync request failed", NotificationSeverity.Error);
            }
        }
    }
}
