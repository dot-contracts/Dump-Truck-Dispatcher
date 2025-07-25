using System.Threading.Tasks;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Sessions
{
    public interface ISessionInfoCache
    {
        Task<GetCurrentLoginInformationsOutput> GetSessionInfoFromCacheOrSource();
        Task InvalidateCache();
        Task InvalidateCacheForUser(long userId);
        Task InvalidateCacheForTenant(int tenantId);
    }
}
