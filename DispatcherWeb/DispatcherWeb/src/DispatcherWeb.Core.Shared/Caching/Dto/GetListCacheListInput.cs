using System;

namespace DispatcherWeb.Caching.Dto
{
    public class GetListCacheListInput<TListKey>
        where TListKey : ListCacheKey
    {
        public GetListCacheListInput()
        {
        }

        public GetListCacheListInput(TListKey key)
        {
            Key = key;
        }

        public TListKey Key { get; set; }

        /// <summary>
        /// If specified, return only items changed after this date/time.
        /// </summary>
        public DateTime? AfterDateTime { get; set; }

        /// <summary>
        /// Backend Cache Item CreationDateTime. If specified, and is older than our CreationDateTime, ignore provided AfterDateTime parameter.
        /// </summary>
        public DateTime? CacheCreationDateTime { get; set; }

        /// <summary>
        /// Validates the remote this.CacheCreationDateTime against our cacheCreationDateTime and clears AfterDateTime and returns true when hard invalidation is detected
        /// </summary>
        /// <param name="cacheCreationDateTime">Current CacheCreationDateTime value on our (backend) side</param>
        public bool ValidateCacheCreationDateTime(DateTime? cacheCreationDateTime)
        {
            // If this.CacheCreationDateTime is provided, it is expected to be the same or newer that the locally stored cacheCreationDateTime, otherwise the remote state of cache should be ignored (afterDateTime should be ignored) and the full list should be returned

            if (AfterDateTime == null)
            {
                //CacheCreationDateTime doesn't matter when all data is requested
                return false;
            }

            var their = CacheCreationDateTime;
            var our = cacheCreationDateTime;

            if (their == null)
            {
                // If their value is not provided, it must be not a hard-invalidatable cache, so we will always honor afterDateTime value
                return false;
            }

            if (our == null)
            {
                // If our value is not provided, we must be using a database fallback and should always return all data, since last hard invalidation datetime is impossible to detect.
                AfterDateTime = null;
                return true;
            }

            if (our > their)
            {
                // If our value is newer than theirs, we should not use AfterDateTime, since their state might be invalid after our state was hard-invalidated.
                AfterDateTime = null;
                return true;
            }

            return false;
        }
    }
}
