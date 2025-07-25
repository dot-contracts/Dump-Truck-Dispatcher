using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Notifications;
using DispatcherWeb.Fulcrum;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class FulcrumDispatchDtdTicketJob : DispatcherWebAsyncBackgroundJobBase<FulcrumDispatchDtdTicketJobArgs>, ITransientDependency
    {
        private readonly IFulcrumAppService _fulcrumAppService;
        private readonly INotificationPublisher _notificationPublisher;

        public FulcrumDispatchDtdTicketJob(
            IFulcrumAppService fulcrumAppService,
            INotificationPublisher notificationPublisher,
            IExtendedAbpSession session
        ) : base(session)
        {
            _fulcrumAppService = fulcrumAppService;
            _notificationPublisher = notificationPublisher;
        }

        public override async Task ExecuteAsync(FulcrumDispatchDtdTicketJobArgs args)
        {
            try
            {
                using (Session.Use(args.RequestorUser))
                {
                    switch (args.Action)
                    {
                        case FulcrumDtdTicketAction.Create:
                            await _fulcrumAppService.CreateDtdTicketToToFulcrum(args.DispatchId);
                            break;
                        case FulcrumDtdTicketAction.Delete:
                            await _fulcrumAppService.DeleteDtdTicketFromFulcrum(args.DispatchId);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error when updating fulcrum: {e}");
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.FulcrumSyncError,
                    new MessageNotificationData("Fulcrum ticket update failed."),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[] { args.RequestorUser }
                );
                throw;
            }
        }
    }
}
