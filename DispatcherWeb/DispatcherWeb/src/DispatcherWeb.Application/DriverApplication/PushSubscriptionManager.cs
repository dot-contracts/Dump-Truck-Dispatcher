using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApplication
{
    public class PushSubscriptionManager : IPushSubscriptionManager, ITransientDependency
    {
        private readonly IRepository<DriverPushSubscription> _driverPushSubscriptionRepository;
        private readonly IRepository<PushSubscription> _pushSubscriptionRepository;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PushSubscriptionManager(
            IRepository<DriverPushSubscription> driverPushSubscriptionRepository,
            IRepository<PushSubscription> pushSubscriptionRepository,
            IDriverApplicationLogger driverApplicationLogger,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _driverPushSubscriptionRepository = driverPushSubscriptionRepository;
            _pushSubscriptionRepository = pushSubscriptionRepository;
            _driverApplicationLogger = driverApplicationLogger;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task AddDriverPushSubscription(AddDriverPushSubscriptionInput input)
        {
            if (input.PushSubscription == null)
            {
                return;
            }

            var existingSubscriptions = await (await _driverPushSubscriptionRepository.GetQueryAsync())
                .Include(x => x.PushSubscription)
                .Where(x => x.DriverId == input.DriverId)
                .ToListAsync();

            var driverSubscription = new DriverPushSubscription
            {
                DriverId = input.DriverId,
                DeviceId = input.DeviceId,
                PushSubscription = PushSubscription.FromDto(input.PushSubscription),
            };

            var existingMatchingSubscriptions = existingSubscriptions.Where(x => x.PushSubscription.Equals(driverSubscription.PushSubscription)).ToList();
            if (existingMatchingSubscriptions.Any())
            {
                var existingMatching = existingSubscriptions.First();
                existingMatching.LastModificationTime = Clock.Now;
                existingMatching.DeviceId = input.DeviceId;
                await _driverApplicationLogger.LogInfo(existingMatching.DriverId, $"Push subscription {existingMatching.Id:D7} was updated, deviceId {input.DeviceId}");

                return;
            }

            var existingSubscriptionForDeviceId = existingSubscriptions.Where(x => x.DeviceId == input.DeviceId && input.DeviceId.HasValue).ToList();
            foreach (var existingSub in existingSubscriptionForDeviceId)
            {
                await _driverApplicationLogger.LogInfo(existingSub.DriverId, $"Push subscription {existingSub.Id:D7} was deleted, deviceId {existingSub.DeviceId} (overwritten by another subscription)");
                await _driverPushSubscriptionRepository.DeleteAsync(existingSub);
                await _pushSubscriptionRepository.DeleteAsync(existingSub.PushSubscription);
            }

            var duplicateDriverSubscriptionsFromOtherDrivers = await (await _pushSubscriptionRepository.GetQueryAsync())
                .Include(x => x.DriverPushSubscriptions)
                .Where(x => x.Endpoint == driverSubscription.PushSubscription.Endpoint
                    && x.P256dh == driverSubscription.PushSubscription.P256dh
                    && x.Auth == driverSubscription.PushSubscription.Auth)
                .ToListAsync();

            if (duplicateDriverSubscriptionsFromOtherDrivers.Any())
            {
                foreach (var duplicateSubscription in duplicateDriverSubscriptionsFromOtherDrivers)
                {
                    foreach (var duplicateDriverSubscription in duplicateSubscription.DriverPushSubscriptions)
                    {
                        await _driverApplicationLogger.LogInfo(duplicateDriverSubscription.DriverId, $"Push subscription {duplicateDriverSubscription.Id:D7} was deleted, deviceId {duplicateDriverSubscription.DeviceId} (overwritten by another user)");
                        await _driverPushSubscriptionRepository.DeleteAsync(duplicateDriverSubscription);
                    }
                    await _pushSubscriptionRepository.DeleteAsync(duplicateSubscription);
                }
            }

            await _pushSubscriptionRepository.InsertAsync(driverSubscription.PushSubscription);
            await _driverPushSubscriptionRepository.InsertAsync(driverSubscription);
            await _unitOfWorkManager.Current.SaveChangesAsync();

            await _driverApplicationLogger.LogInfo(driverSubscription.DriverId, $"Push subscription {driverSubscription.Id:D7} was added, deviceId {input.DeviceId}");
        }

        public async Task RemoveDriverPushSubscription(RemoveDriverPushSubscriptionInput input)
        {
            if (input.PushSubscription == null)
            {
                return;
            }

            var subscriptions = await (await _driverPushSubscriptionRepository.GetQueryAsync())
                .Include(x => x.PushSubscription)
                .Where(x => x.DriverId == input.DriverId)
                .ToListAsync();

            var subscriptionToDelete = PushSubscription.FromDto(input.PushSubscription);

            foreach (var subscription in subscriptions)
            {
                if (subscription.PushSubscription.Equals(subscriptionToDelete))
                {
                    await _driverApplicationLogger.LogInfo(subscription.DriverId, $"Push subscription {subscription.Id:D7} was deleted, deviceId {subscription.DeviceId}");

                    await _driverPushSubscriptionRepository.DeleteAsync(subscription);
                    await _pushSubscriptionRepository.DeleteAsync(subscription.PushSubscription);
                }
            }
        }

        public async Task CleanupSubscriptions()
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {

                var subscriptionsToDelete = await (await _driverPushSubscriptionRepository.GetQueryAsync())
                    .Include(x => x.PushSubscription)
                    .Where(x => x.DeviceId == null)
                    .ToListAsync();

                foreach (var sub in subscriptionsToDelete)
                {
                    await _driverApplicationLogger.LogInfo(sub.DriverId, $"Push subscription {sub.Id:D7} was deleted, deviceId null (cleanup)");
                    await _driverPushSubscriptionRepository.DeleteAsync(sub);
                    await _pushSubscriptionRepository.DeleteAsync(sub.PushSubscription);
                }
                await _unitOfWorkManager.Current.SaveChangesAsync();


                var thresholdDate = Clock.Now.AddDays(-30);
                subscriptionsToDelete = await (await _driverPushSubscriptionRepository.GetQueryAsync())
                    .Include(x => x.PushSubscription)
                    .Where(x => x.CreationTime < thresholdDate && (x.LastModificationTime == null || x.LastModificationTime < thresholdDate))
                    .ToListAsync();

                foreach (var sub in subscriptionsToDelete)
                {
                    await _driverApplicationLogger.LogInfo(sub.DriverId, $"Push subscription {sub.Id:D7} was deleted, CreationTime {sub.CreationTime:s}, ModificationTime {sub.LastModificationTime:s}, deviceId {sub.DeviceId} (cleanup)");
                    await _driverPushSubscriptionRepository.DeleteAsync(sub);
                    await _pushSubscriptionRepository.DeleteAsync(sub.PushSubscription);
                }
                await _unitOfWorkManager.Current.SaveChangesAsync();


                var subscriptionGroups = await (await _driverPushSubscriptionRepository.GetQueryAsync())
                    .Include(x => x.PushSubscription)
                    .GroupBy(x => new { x.DriverId, x.DeviceId })
                    .Where(x => x.Count() > 1)
                    .ToListAsync();

                foreach (var subGroup in subscriptionGroups)
                {
                    var subDateToKeep = subGroup.Max(x => x.LastModificationTime ?? x.CreationTime);
                    var subToKeep = subGroup.First(x => (x.LastModificationTime ?? x.CreationTime) == subDateToKeep);
                    foreach (var sub in subGroup)
                    {
                        if (sub == subToKeep)
                        {
                            continue;
                        }
                        await _driverApplicationLogger.LogWarn(sub.DriverId, $"Push subscription {sub.Id:D7} was deleted, duplicate {subToKeep.Id:D7} exists for deviceId {sub.DeviceId} (cleanup)");
                        await _driverPushSubscriptionRepository.DeleteAsync(sub);
                        if (sub.PushSubscription.Id != subToKeep.PushSubscription.Id)
                        {
                            await _pushSubscriptionRepository.DeleteAsync(sub.PushSubscription);
                        }
                    }
                }
                await _unitOfWorkManager.Current.SaveChangesAsync();
            }
        }
    }
}
