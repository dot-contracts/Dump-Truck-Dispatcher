using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Dispatching.Dto
{
    public class LoadCacheItem : AuditableCacheItem
    {
        public int DispatchId { get; set; }
        public DateTime? SourceDateTime { get; set; }
        public DateTime? DestinationDateTime { get; set; }
    }
}
