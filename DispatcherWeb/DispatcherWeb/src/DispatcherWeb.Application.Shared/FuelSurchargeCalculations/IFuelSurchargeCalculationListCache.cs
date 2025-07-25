using DispatcherWeb.Caching;
using DispatcherWeb.FuelSurchargeCalculations.Dto;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    public interface IFuelSurchargeCalculationListCache : IListCache<ListCacheTenantKey, FuelSurchargeCalculationCacheItem>
    {
    }
}
