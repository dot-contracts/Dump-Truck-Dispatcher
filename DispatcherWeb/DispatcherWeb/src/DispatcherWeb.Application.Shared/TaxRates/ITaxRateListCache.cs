using DispatcherWeb.Caching;
using DispatcherWeb.TaxRates.Dto;

namespace DispatcherWeb.TaxRates
{
    public interface ITaxRateListCache : IListCache<ListCacheTenantKey, TaxRateCacheItem>
    {
    }
}
