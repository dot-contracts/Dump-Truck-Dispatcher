using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Notifications
{
    public class AppNotifier : DispatcherWebDomainServiceBase, IAppNotifier
    {
        private readonly DispatcherWebNotificationPublisher _notificationPublisher;
        private readonly IAsyncOnlineClientManager _onlineClientManager;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        public AppNotifier(
            DispatcherWebNotificationPublisher notificationPublisher,
            IAsyncOnlineClientManager onlineClientManager,
            IBackgroundJobManager backgroundJobManager,
            UserManager userManager,
            RoleManager roleManager
        )
        {
            _notificationPublisher = notificationPublisher;
            _onlineClientManager = onlineClientManager;
            _backgroundJobManager = backgroundJobManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task WelcomeToTheApplicationAsync(User user)
        {
            await _notificationPublisher.PublishAsync(
                AppNotificationNames.WelcomeToTheApplication,
                new MessageNotificationData(L("WelcomeToTheApplicationNotificationMessage")),
                severity: NotificationSeverity.Success,
                userIds: new[] { user.ToUserIdentifier() }
                );
        }

        public async Task NewTenantRegisteredAsync(Tenant tenant)
        {
            var notificationData = new LocalizableMessageNotificationData(
                new LocalizableString(
                    "NewTenantRegisteredNotificationMessage",
                    DispatcherWebConsts.LocalizationSourceName
                    )
                );

            notificationData["tenancyName"] = tenant.TenancyName;
            await _notificationPublisher.PublishAsync(AppNotificationNames.NewTenantRegistered, notificationData);
        }

        public Task GdprDataPrepared(UserIdentifier user, Guid binaryObjectId)
        {
            return Task.CompletedTask;
            //var notificationData = new LocalizableMessageNotificationData(
            //    new LocalizableString(
            //        "GdprDataPreparedNotificationMessage",
            //        DispatcherWebConsts.LocalizationSourceName
            //    )
            //);
            //
            //notificationData["binaryObjectId"] = binaryObjectId;
            //
            //await _notificationPublisher.PublishAsync(AppNotificationNames.GdprDataPrepared, notificationData, userIds: new[] { user });
        }

        public async Task QuoteEmailDeliveryFailed(UserIdentifier user, Quotes.Quote quote, Emailing.TrackableEmailReceiver emailReceiver)
        {
            var notificationData = new MessageNotificationData(
                $"Email delivery to {emailReceiver.Email} for quote #{quote.Id} has failed."
            )
            {
                ["quoteId"] = quote.Id,
            };

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.QuoteEmailDeliveryFailed,
                notificationData,
                severity: NotificationSeverity.Error,
                userIds: new[] { user }
            );
        }

        public async Task InvoiceEmailDeliveryFailed(UserIdentifier user, int invoiceId, Emailing.TrackableEmailReceiver emailReceiver)
        {
            var notificationData = new MessageNotificationData(
                $"Email delivery to {emailReceiver.Email} for invoice #{invoiceId} has failed."
            )
            {
                ["invoiceId"] = invoiceId,
            };

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.InvoiceEmailDeliveryFailed,
                notificationData,
                severity: NotificationSeverity.Error,
                userIds: new[] { user }
            );
        }

        public async Task OrderEmailDeliveryFailed(UserIdentifier user, Orders.Order order, Emailing.TrackableEmailReceiver emailReceiver)
        {
            var notificationData = new MessageNotificationData(
                $"Email delivery to {emailReceiver.Email} for order #{order.Id} has failed."
            )
            {
                ["orderId"] = order.Id,
            };

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.OrderEmailDeliveryFailed,
                notificationData,
                severity: NotificationSeverity.Error,
                userIds: new[] { user }
            );
        }

        public async Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await SendMessageAsync(new[] { user }, message, severity);
        }

        public async Task SendMessageAsync(UserIdentifier[] users, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await _notificationPublisher.PublishAsync(
                AppNotificationNames.SimpleMessage,
                new MessageNotificationData(message),
                severity: severity,
                userIds: users
            );
        }

        public Task SendMessageAsync(UserIdentifier user,
            LocalizableString localizableMessage,
            IDictionary<string, object> localizableMessageData = null,
            NotificationSeverity severity = NotificationSeverity.Info)
        {
            return SendNotificationAsync(AppNotificationNames.SimpleMessage, user, localizableMessage,
                localizableMessageData, severity);
        }

        public async Task SendNotificationAsync(SendNotificationInput input)
        {
            await _backgroundJobManager.EnqueueAsync<AppNotifierNotificationSenderBackgroundJob, AppNotifierNotificationSenderBackgroundJobArgs>(new AppNotifierNotificationSenderBackgroundJobArgs
            {
                RequestorUser = await Session.ToUserIdentifierAsync(),
                SendNotificationInput = input,
            });
        }

        public async Task SendNotificationImmediatelyAsync(SendNotificationInput input)
        {
            var users = new List<User>();

            if (input.IncludeLocalUsers)
            {
                users.AddRange(
                    await (await _userManager.GetQueryAsync())
                        .AsNoTracking()
                        .WhereIf(input.OfficeIdFilter?.Any() == true, x => x.OfficeId.HasValue && input.OfficeIdFilter.Contains(x.OfficeId.Value))
                        .ToListAsync()
                );
            }

            if (input.IncludeHostUsers)
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    users.AddRange(
                        await (await _userManager.GetQueryAsync())
                            .AsNoTracking()
                            .Where(x => x.TenantId == null)
                            .ToListAsync()
                    );
                }
            }

            if (input.RoleFilter?.Any() == true)
            {
                foreach (var user in users.ToList())
                {
                    bool isInAnyRole = false;
                    foreach (var role in input.RoleFilter)
                    {
                        if (await _userManager.IsInRoleAsync(user, role))
                        {
                            isInAnyRole = true;
                            break;
                        }
                    }
                    if (!isInAnyRole)
                    {
                        users.Remove(user);
                    }
                }
            }

            if (!string.IsNullOrEmpty(input.PermissionFilter))
            {
                var usersWithPermission = await (await _userManager
                    .GetUsersWithGrantedPermission(_roleManager, input.PermissionFilter))
                    .Select(x => x.Id)
                    .ToListAsync();

                users.RemoveAll(u => !usersWithPermission.Contains(u.Id));
            }

            if (input.OnlineFilter.HasValue)
            {
                var onlineClients = await _onlineClientManager.GetAllClientsAsync();
                users = users
                    .Where(user => onlineClients.Any(x => x.UserId == user.Id) == input.OnlineFilter)
                    .ToList();
            }

            await _notificationPublisher.PublishImmediatelyAsync(
                input.NotificationName,
                new MessageNotificationData(input.Message),
                severity: input.Severity,
                userIds: users.Select(x => x.ToUserIdentifier()).ToArray()
                );
        }

        public async Task SendPriorityNotification(SendPriorityNotificationInput input)
        {
            await _backgroundJobManager.EnqueueAsync<AppNotifierPriorityNotificationSenderBackgroundJob, AppNotifierPriorityNotificationSenderBackgroundJobArgs>(new AppNotifierPriorityNotificationSenderBackgroundJobArgs
            {
                RequestorUser = await Session.ToUserIdentifierAsync(),
                SendPriorityNotificationInput = input,
            });
        }

        public async Task SendPriorityNotificationImmediately(SendPriorityNotificationInput input)
        {
            var users = await (await _userManager.GetQueryAsync())
                .WhereIf(input.OfficeIdFilter?.Any() == true, x => x.OfficeId.HasValue && input.OfficeIdFilter.Contains(x.OfficeId.Value))
                .WhereIf(input.UserIdFilter?.Any() == true, x => input.UserIdFilter.Contains(x.Id))
                .ToListAsync();

            if (input.RoleFilter?.Any() == true)
            {
                foreach (var user in users.ToList())
                {
                    bool isInAnyRole = false;
                    foreach (var role in input.RoleFilter)
                    {
                        if (await _userManager.IsInRoleAsync(user, role))
                        {
                            isInAnyRole = true;
                            break;
                        }
                    }
                    if (!isInAnyRole)
                    {
                        users.Remove(user);
                    }
                }
            }

            if (input.OnlineFilter.HasValue)
            {
                var onlineClients = await _onlineClientManager.GetAllClientsAsync();
                users = users
                    .Where(user => onlineClients.Any(x => x.UserId == user.Id) == input.OnlineFilter)
                    .ToList();
            }

            await _notificationPublisher.PublishImmediatelyAsync(
                AppNotificationNames.PriorityNotification,
                new MessageNotificationData(input.Message),
                severity: input.Severity,
                userIds: users.Select(x => x.ToUserIdentifier()).ToArray()
                );
        }
        protected async Task SendNotificationAsync(string notificationName, UserIdentifier user,
            LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null,
            NotificationSeverity severity = NotificationSeverity.Info)
        {
            var notificationData = new LocalizableMessageNotificationData(localizableMessage);
            if (localizableMessageData != null)
            {
                foreach (var pair in localizableMessageData)
                {
                    notificationData[pair.Key] = pair.Value;
                }
            }

            await _notificationPublisher.PublishAsync(notificationName, notificationData, severity: severity,
                userIds: new[] { user });
        }

        public Task TenantsMovedToEdition(UserIdentifier user, string sourceEditionName, string targetEditionName)
        {
            return SendNotificationAsync(AppNotificationNames.TenantsMovedToEdition, user,
                new LocalizableString(
                    "TenantsMovedToEditionNotificationMessage",
                    DispatcherWebConsts.LocalizationSourceName
                ),
                new Dictionary<string, object>
                {
                    {"sourceEditionName", sourceEditionName},
                    {"targetEditionName", targetEditionName},
                });
        }

        public Task<TResult> TenantsMovedToEdition<TResult>(UserIdentifier argsUser, int sourceEditionId,
            int targetEditionId)
        {
            throw new NotImplementedException();
        }

        public Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName)
        {
            return SendNotificationAsync(AppNotificationNames.DownloadInvalidImportUsers, user,
                new LocalizableString(
                    "ClickToSeeInvalidUsers",
                    DispatcherWebConsts.LocalizationSourceName
                ),
                new Dictionary<string, object>
                {
                    { "fileToken", fileToken },
                    { "fileType", fileType },
                    { "fileName", fileName },
                });
        }
    }
}
