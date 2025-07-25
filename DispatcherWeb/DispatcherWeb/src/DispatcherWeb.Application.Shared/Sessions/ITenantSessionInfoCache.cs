using System.Threading.Tasks;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Sessions
{
    public interface ITenantSessionInfoCache
    {
        Task<TenantLoginInfoDto> GetTenantSessionInfoFromCacheOrSource(int tenantId);
        Task InvalidateCache();
        Task InvalidateCache(int tenantId);
    }
}
