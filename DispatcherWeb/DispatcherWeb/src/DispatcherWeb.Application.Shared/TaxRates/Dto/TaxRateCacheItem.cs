using DispatcherWeb.Caching;

namespace DispatcherWeb.TaxRates.Dto
{
    public class TaxRateCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public decimal Rate { get; set; }
    }
}
