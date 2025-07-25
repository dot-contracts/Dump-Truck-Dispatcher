using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Notifications;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.BackgroundJobs
{
    public class UpdateMileageJob : AsyncBackgroundJob<UpdateMileageJobArgs>, ITransientDependency
    {
        private readonly ITruckTelematicsAppService _truckTelematicsAppService;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IExtendedAbpSession _session;

        public UpdateMileageJob(
            ITruckTelematicsAppService truckTelematicsAppService,
            INotificationPublisher notificationPublisher,
            IExtendedAbpSession session
        )
        {
            _truckTelematicsAppService = truckTelematicsAppService;
            _notificationPublisher = notificationPublisher;
            _session = session;
        }

        public override async Task ExecuteAsync(UpdateMileageJobArgs args)
        {
            try
            {
                using (_session.Use(args.RequestorUser))
                {
                    await _notificationPublisher.PublishAsync(
                        AppNotificationNames.MileageUpdateCompleted,
                        new MessageNotificationData("We are updating your truck mileages. This is a slow process, so we will notify you when it is done."),
                        null,
                        NotificationSeverity.Info,
                        userIds: new[] { args.RequestorUser }
                    );
                    var result = await _truckTelematicsAppService.UpdateMileageForCurrentTenantAsync();
                    string ignoredTrucksMessage = result.trucksIgnored != 0 ? $"Ignored (don't exist in the DB) {result.trucksIgnored} trucks." : "";
                    await _notificationPublisher.PublishAsync(
                        AppNotificationNames.MileageUpdateCompleted,
                        new MessageNotificationData($"Updating mileage has finished successfully. Updated {result.trucksUpdated} trucks. {ignoredTrucksMessage}"),
                        null,
                        NotificationSeverity.Success,
                        userIds: new[] { args.RequestorUser }
                    );
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error when updating mileage: {e}");
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.MileageUpdateError,
                    new MessageNotificationData("Updating mileage failed."),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[] { args.RequestorUser }
                );
            }
        }
    }
}
