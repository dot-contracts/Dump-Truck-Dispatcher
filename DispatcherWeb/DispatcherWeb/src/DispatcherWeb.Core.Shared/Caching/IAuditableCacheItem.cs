using System;

namespace DispatcherWeb.Caching
{
    public interface IAuditableCacheItem : IAuditableCacheItem<int>
    {
    }

    public interface IAuditableCacheItem<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        TKey Id { get; set; }

        bool IsDeleted { get; set; }

        DateTime? DeletionTime { get; set; }

        DateTime CreationTime { get; set; }

        DateTime? LastModificationTime { get; set; }

        DateTime? LastInteractionTime
        {
            get
            {
                var maxDate = CreationTime;

                if (LastModificationTime > maxDate)
                {
                    maxDate = LastModificationTime.Value;
                }

                if (DeletionTime > maxDate)
                {
                    maxDate = DeletionTime.Value;
                }

                return maxDate;
            }
        }
    }
}
