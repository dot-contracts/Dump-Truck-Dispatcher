using System;

namespace DispatcherWeb.Caching
{
    public class AuditableCacheItem : AuditableCacheItem<int>
    {
    }

    public class AuditableCacheItem<TKey> : IAuditableCacheItem<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        public TKey Id { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletionTime { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }
    }
}
