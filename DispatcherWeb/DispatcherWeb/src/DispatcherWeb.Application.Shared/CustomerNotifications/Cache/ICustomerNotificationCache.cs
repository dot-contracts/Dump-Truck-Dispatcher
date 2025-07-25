using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.CustomerNotifications.Dto;

namespace DispatcherWeb.CustomerNotifications.Cache
{
    public interface ICustomerNotificationCache
    {
        Task DismissCustomerNotification(DateTime date, long userId, int customerNotificationId);
        Task<List<CustomerNotificationToShowDto>> GetFromCacheOrDefault(DateTime date, long userId);
        Task InvalidateCache();
        Task<List<CustomerNotificationToShowDto>> StoreAndEnrichUserNotifications(DateTime date, long userId, List<int> customerNotificationIds);
    }
}
