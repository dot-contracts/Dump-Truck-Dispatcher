using System;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Drivers.Dto
{
    public class DriverCacheItem : AuditableCacheItem
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOfHire { get; set; }

        public bool IsExternal { get; set; }

        public bool IsInactive { get; set; }

        public long? UserId { get; set; }
    }
}
