using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderCacheItem : AuditableCacheItem
    {
        public DateTime DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
        public OrderPriority Priority { get; set; }
        public int OfficeId { get; set; }
        public int CustomerId { get; set; }
        public string PoNumber { get; set; }
        public string SpectrumNumber { get; set; }
        public string ChargeTo { get; set; }
        public string Directions { get; set; }
        public bool IsPending { get; set; }
        public decimal? SalesTaxRate { get; set; }
        public decimal? SalesTax { get; set; }
        public decimal? FreightTotal { get; set; }
        public decimal? MaterialTotal { get; set; }
        public decimal? CodTotal { get; set; }
    }
}
