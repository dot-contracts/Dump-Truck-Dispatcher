using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface IInsuranceListCache : IListCache<ListCacheTenantKey, InsuranceCacheItem>
    {
    }
}
