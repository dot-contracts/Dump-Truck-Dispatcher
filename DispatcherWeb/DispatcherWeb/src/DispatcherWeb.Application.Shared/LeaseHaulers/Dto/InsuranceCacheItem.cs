using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class InsuranceCacheItem : AuditableCacheItem
    {
        public int LeaseHaulerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
