using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class ListExtensions
    {
        //Now a part of .NET 6
        //public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        //{
        //    var pos = 0;
        //    while (source.Skip(pos).Any())
        //    {
        //        yield return source.Skip(pos).Take(chunksize);
        //        pos += chunksize;
        //    }
        //}

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyValuePairs<TKey, TValue>(this IEnumerable<TValue> collection, Func<TValue, TKey> keySelector)
        {
            return collection.ToKeyValuePairs(keySelector, x => x);
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyValuePairs<TKey, TValue, TCollection>(this IEnumerable<TCollection> collection, Func<TCollection, TKey> keySelector, Func<TCollection, TValue> valueSelector)
        {
            return collection.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), valueSelector(x)));
        }

        public static async Task<List<TValue>> GetSomeOrNoneAsync<TKey, TValue>(this IAbpCache<TKey, TValue> cache, List<TKey> keys)
        {
            var values = await cache.TryGetValuesAsync(keys.ToArray());
            return values.Where(x => x.HasValue).Select(x => x.Value).ToList();
        }

        public static async Task<bool> AllAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
        {
            foreach (var item in source)
            {
                if (!await predicate(item))
                {
                    return false;
                }
            }

            return true;
        }

        public static async Task<bool> AnyAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
        {
            foreach (var item in source)
            {
                if (await predicate(item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
