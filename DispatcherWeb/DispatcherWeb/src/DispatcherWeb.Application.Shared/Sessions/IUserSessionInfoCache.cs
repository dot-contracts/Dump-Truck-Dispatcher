using System.Threading.Tasks;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Sessions
{
    public interface IUserSessionInfoCache
    {
        Task<UserLoginInfoDto> GetUserSessionInfoFromCacheOrSource(long userId, bool disableTenantFilter = false);
        Task InvalidateCache();
        Task InvalidateCache(long userId);
    }
}
