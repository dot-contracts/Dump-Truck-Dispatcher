using System;

namespace DispatcherWeb.Caching
{
    public class ListCacheStaleState
    {
        public string Key { get; set; }
        public DateTime? StaleSinceDateTime { get; set; }
        public bool IsStale => StaleSinceDateTime.HasValue;
    }
}
