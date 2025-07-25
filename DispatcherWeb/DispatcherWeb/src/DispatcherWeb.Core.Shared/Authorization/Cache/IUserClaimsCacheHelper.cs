using System.Threading.Tasks;
using DispatcherWeb.Authorization.Cache.Dto;

namespace DispatcherWeb.Authorization.Cache
{
    public interface IUserClaimsCacheHelper
    {
        public Task<UserClaimsCacheItem> GetUserClaimsAsync(long userId);
    }
}
