using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Localization;
using Abp.Notifications;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.Notifications
{
    public interface IAppNotifier
    {
        Task WelcomeToTheApplicationAsync(User user);

        Task NewTenantRegisteredAsync(Tenant tenant);

        Task GdprDataPrepared(UserIdentifier user, Guid binaryObjectId);
        Task QuoteEmailDeliveryFailed(UserIdentifier user, Quotes.Quote quote, Emailing.TrackableEmailReceiver emailReceiver);
        Task OrderEmailDeliveryFailed(UserIdentifier user, Orders.Order order, Emailing.TrackableEmailReceiver emailReceiver);
        Task InvoiceEmailDeliveryFailed(UserIdentifier user, int invoiceId, Emailing.TrackableEmailReceiver emailReceiver);

        Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info);

        Task SendMessageAsync(UserIdentifier[] users, string message, NotificationSeverity severity = NotificationSeverity.Info);

        Task SendMessageAsync(UserIdentifier user, LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null, NotificationSeverity severity = NotificationSeverity.Info);

        Task SendPriorityNotification(SendPriorityNotificationInput input);
        Task TenantsMovedToEdition(UserIdentifier user, string sourceEditionName, string targetEditionName);

        Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName);
        Task SendNotificationAsync(SendNotificationInput input);
        Task SendNotificationImmediatelyAsync(SendNotificationInput input);
        Task SendPriorityNotificationImmediately(SendPriorityNotificationInput input);
    }
}
