using System.Threading.Tasks;
using Abp;
using DispatcherWeb.Notifications.Dto;

namespace DispatcherWeb.Notifications.Cache
{
    public interface IUserNotificationCacheBase
    {
        Task<GetNotificationsOutput> GetFromCacheOrDefault(UserIdentifier userIdentifier);
        void RemoveFromCache(UserIdentifier userIdentifier);
        Task RemoveFromCacheAsync(UserIdentifier userIdentifier);
        Task StoreInCache(UserIdentifier userIdentifier, GetNotificationsOutput value);
    }
}
