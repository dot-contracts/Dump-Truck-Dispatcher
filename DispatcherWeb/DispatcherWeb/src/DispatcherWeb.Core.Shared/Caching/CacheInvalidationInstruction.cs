using System;

namespace DispatcherWeb.Caching
{
    public class CacheInvalidationInstruction
    {
        public string CacheName { get; set; }

        public string CacheKey { get; set; }

        public Guid Guid { get; set; }

        public DateTime CreationDateTime { get; set; }

        public bool HardInvalidate { get; set; }

        public string GetKey()
        {
            return GetKey(CacheName, CacheKey, HardInvalidate);
        }

        public static string GetKey(string cacheName, string cacheKey, bool hardInvalidate)
        {
            return cacheName + "-" + cacheKey + "-" + hardInvalidate;
        }

        public override bool Equals(object obj)
        {
            return obj is CacheInvalidationInstruction other
                && GetKey() == other.GetKey()
                && Guid == other.Guid
                && CreationDateTime == other.CreationDateTime
                && HardInvalidate == other.HardInvalidate;
        }

        public override int GetHashCode()
        {
            return GetKey().GetHashCode();
        }
    }
}
