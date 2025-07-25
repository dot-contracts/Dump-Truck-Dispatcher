using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.Infrastructure.EventBus.Events;
using DispatcherWeb.Infrastructure.General;
using DispatcherWeb.Infrastructure.Notifications;
using DispatcherWeb.Notifications;
using DispatcherWeb.Url;

namespace DispatcherWeb.Infrastructure.EventBus.EventHandlers
{
    public class ImportCompletedNotificationPublisher : IAsyncEventHandler<ImportCompletedEventData>, ITransientDependency
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IWebUrlService _webUrlService;
        private readonly IAbpSession _session;
        private readonly INotAuthorizedUserAppService _notAuthorizedUserService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ImportCompletedNotificationPublisher(
            INotificationPublisher notificationPublisher,
            IWebUrlService webUrlService,
            IAbpSession session,
            INotAuthorizedUserAppService notAuthorizedUserService,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _notificationPublisher = notificationPublisher;
            _webUrlService = webUrlService;
            _session = session;
            _notAuthorizedUserService = notAuthorizedUserService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [UnitOfWork]
        public async Task HandleEventAsync(ImportCompletedEventData eventData)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_session.Use(eventData.Args.RequestorUser.TenantId, eventData.Args.RequestorUser.UserId))
                {
                    await PublishNotificationAsync(eventData.Args.RequestorUser, eventData.Args.File);
                }
            });
        }
        private async Task PublishNotificationAsync(UserIdentifier user, string file)
        {
            var tenancyName = await _notAuthorizedUserService.GetTenancyNameOrNullAsync(user.TenantId);
            string fileLink = $"{_webUrlService.GetSiteRootAddress(tenancyName)}app/ImportResults/{file}";
            await _notificationPublisher.PublishAsync(AppNotificationNames.ImportCompleted,
                new ImportCompletedNotificationData(fileLink),
                userIds: new[] { user },
                severity: NotificationSeverity.Success);
        }

    }
}
