using DispatcherWeb.Caching;

namespace DispatcherWeb.FuelSurchargeCalculations.Dto
{
    public class FuelSurchargeCalculationCacheItem : AuditableCacheItem
    {
        public string Name { get; set; }
        public FuelSurchargeCalculationType Type { get; set; }
        public decimal BaseFuelCost { get; set; }
        public bool CanChangeBaseFuelCost { get; set; }
        public decimal FreightRatePercent { get; set; }
    }
}
