using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Chat;
using DispatcherWeb.Notifications.Cache;
using DispatcherWeb.Notifications.Dto;

namespace DispatcherWeb.Notifications
{
    [AbpAuthorize]
    public class NotificationAppService : DispatcherWebAppServiceBase, INotificationAppService
    {
        private readonly INotificationDefinitionManager _notificationDefinitionManager;
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly DispatcherWebNotificationStore _notificationStore;
        private readonly ITop3UserNotificationCache _top3UserNotificationCache;
        private readonly IPriorityUserNotificationCache _priorityUserNotificationCache;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;

        public NotificationAppService(
            INotificationDefinitionManager notificationDefinitionManager,
            IUserNotificationManager userNotificationManager,
            DispatcherWebNotificationStore notificationStore,
            ITop3UserNotificationCache top3UserNotificationCache,
            IPriorityUserNotificationCache priorityUserNotificationCache,
            INotificationSubscriptionManager notificationSubscriptionManager)
        {
            _notificationDefinitionManager = notificationDefinitionManager;
            _userNotificationManager = userNotificationManager;
            _notificationStore = notificationStore;
            _top3UserNotificationCache = top3UserNotificationCache;
            _priorityUserNotificationCache = priorityUserNotificationCache;
            _notificationSubscriptionManager = notificationSubscriptionManager;
        }

        [MessagingMethod]
        [DisableAuditing]
        public async Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input)
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var useTop3Cache = input.SkipCount == 0
                && input.MaxResultCount == 3
                && input.StartDate == null
                && input.EndDate == null
                && input.State == null;
            if (useTop3Cache)
            {
                var cacheValue = await _top3UserNotificationCache.GetFromCacheOrDefault(userIdentifier);
                if (cacheValue != null)
                {
                    return cacheValue;
                }
            }

            var totalCount = await _userNotificationManager.GetUserNotificationCountAsync(
                userIdentifier, input.State, input.StartDate, input.EndDate
                );

            var unreadCount = await _userNotificationManager.GetUserNotificationCountAsync(
                userIdentifier, UserNotificationState.Unread, input.StartDate, input.EndDate
                );

            var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                userIdentifier, input.State, input.SkipCount, input.MaxResultCount, input.StartDate, input.EndDate
                );

            var result = new GetNotificationsOutput(totalCount, unreadCount, notifications);

            if (useTop3Cache)
            {
                await _top3UserNotificationCache.StoreInCache(userIdentifier, result);
            }

            return result;
        }

        [MessagingMethod]
        [DisableAuditing]
        public async Task<GetNotificationsOutput> GetUnreadPriorityNotifications()
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var cacheValue = await _priorityUserNotificationCache.GetFromCacheOrDefault(userIdentifier);
            if (cacheValue != null)
            {
                return cacheValue;
            }
            var notifications = await _notificationStore.GetUserNotificationsWithNotificationsAsync(userIdentifier, UserNotificationState.Unread, notificationName: AppNotificationNames.PriorityNotification);
            var userNotifications = notifications.Select(x => x.ToUserNotification()).ToList();
            var result = new GetNotificationsOutput(notifications.Count, notifications.Count, userNotifications);
            await _priorityUserNotificationCache.StoreInCache(userIdentifier, result);
            return result;
        }

        [MessagingMethod]
        public async Task SetAllNotificationsAsRead()
        {
            await _userNotificationManager.UpdateAllUserNotificationStatesAsync(await AbpSession.ToUserIdentifierAsync(), UserNotificationState.Read);
        }

        [MessagingMethod]
        public async Task SetNotificationAsRead(EntityDto<Guid> input)
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            var userNotification = await _userNotificationManager.GetUserNotificationAsync(tenantId, input.Id);
            if (userNotification == null)
            {
                return;
            }

            if (userNotification.UserId != AbpSession.GetUserId())
            {
                throw new Exception(string.Format("Given user notification id ({0}) is not belong to the current user ({1})", input.Id, AbpSession.GetUserId()));
            }

            await _userNotificationManager.UpdateUserNotificationStateAsync(
                tenantId,
                input.Id,
                userNotification.State.Equals(UserNotificationState.Read) ? UserNotificationState.Unread : UserNotificationState.Read);
        }

        public async Task<GetNotificationSettingsOutput> GetNotificationSettings()
        {
            var output = new GetNotificationSettingsOutput();
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();

            output.ReceiveNotifications = await SettingManager.GetSettingValueAsync<bool>(NotificationSettingNames.ReceiveNotifications);

            //Get general notifications, not entity related notifications.
            var notificationDefinitions = (await _notificationDefinitionManager.GetAllAvailableAsync(userIdentifier))
                .Where(nd => nd.EntityType == null)
                .Select(x => new NotificationSubscriptionWithDisplayNameDto
                {
                    DisplayName = L(x.DisplayName),
                    Description = L(x.Description),
                    Name = x.Name,
                }).ToList();

            output.Notifications = notificationDefinitions;

            var subscribedNotifications = (await _notificationSubscriptionManager
                .GetSubscribedNotificationsAsync(userIdentifier))
                .Select(ns => ns.NotificationName)
                .ToList();

            output.Notifications.ForEach(n => n.IsSubscribed = subscribedNotifications.Contains(n.Name));

            return output;
        }

        public async Task UpdateNotificationSettings(UpdateNotificationSettingsInput input)
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            await SettingManager.ChangeSettingForUserAsync(userIdentifier, NotificationSettingNames.ReceiveNotifications, input.ReceiveNotifications.ToString());

            foreach (var notification in input.Notifications)
            {
                if (notification.IsSubscribed)
                {
                    await _notificationSubscriptionManager.SubscribeAsync(userIdentifier, notification.Name);
                }
                else
                {
                    await _notificationSubscriptionManager.UnsubscribeAsync(userIdentifier, notification.Name);
                }
            }
        }

        [MessagingMethod]
        public async Task DeleteNotification(EntityDto<Guid> input)
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            var notification = await _userNotificationManager.GetUserNotificationAsync(tenantId, input.Id);
            if (notification == null)
            {
                return;
            }

            if (notification.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("ThisNotificationDoesntBelongToYou"));
            }

            await _userNotificationManager.DeleteUserNotificationAsync(tenantId, input.Id);
        }

        [MessagingMethod]
        public async Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input)
        {
            await _userNotificationManager.DeleteAllUserNotificationsAsync(
                await AbpSession.ToUserIdentifierAsync(),
                input.State,
                input.StartDate,
                input.EndDate);
        }
    }
}
