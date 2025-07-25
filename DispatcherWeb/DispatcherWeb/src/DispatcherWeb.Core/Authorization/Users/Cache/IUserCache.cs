using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public interface IUserCache
    {
        Task<UserCacheItem> GetUserAsync(UserIdentifier userIdentifier);
        Task<UserCacheItem> GetUserAsync(long userId);
        Task<List<UserCacheItem>> GetUsersAsync(List<UserIdentifier> userIdentifiers);
        Task<List<UserCacheItem>> GetUsersAsync(List<long> userIds);
    }
}
