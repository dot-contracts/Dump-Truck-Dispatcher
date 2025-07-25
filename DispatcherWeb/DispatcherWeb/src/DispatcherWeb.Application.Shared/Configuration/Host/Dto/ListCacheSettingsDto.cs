using System.Collections.Generic;

namespace DispatcherWeb.Configuration.Host.Dto
{
    public class ListCacheSettingsDto
    {
        public List<CacheDto> Caches { get; set; }
        public int GlobalCacheVersion { get; set; }

        public class CacheDto
        {
            public string CacheName { get; set; }
            public CacheSideDto Backend { get; set; }
            public CacheSideDto Frontend { get; set; }
        }

        public class CacheSideDto
        {
            public bool IsEnabled { get; set; }
            public int SlidingExpirationTimeMinutes { get; set; }
        }
    }
}
