using System.Threading.Tasks;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerCache
    {
        Task<LeaseHaulerCacheItem> GetLeaseHaulerFromCacheOrDefault(int leaseHaulerId);
        Task InvalidateCache();
        Task InvalidateCache(int leaseHaulerId);
    }
}
