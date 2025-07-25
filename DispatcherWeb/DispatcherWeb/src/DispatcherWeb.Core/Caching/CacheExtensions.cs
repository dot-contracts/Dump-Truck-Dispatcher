using System;
using System.Threading.Tasks;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Caching
{
    public static class CacheExtensions
    {
        public static T UseListCacheLogic<T>(this T cache)
            where T : IAbpCache
        {
            if (cache is RedisInvalidatableInMemoryCache inMemoryCache)
            {
                inMemoryCache.UseStaleListLogic = true;
            }

            return cache;
        }

        public static T SuppressInvalidationSyncCalls<T>(this T cache)
            where T : IAbpCache
        {
            if (cache is RedisInvalidatableInMemoryCache inMemoryCache)
            {
                inMemoryCache.SuppressInvalidationSyncCalls = true;
            }

            return cache;
        }

        public static async Task SendCacheInvalidationInstructionAsync<TKey, TValue>(this ITypedCache<TKey, TValue> typedCache, string key, bool hardInvalidate = false)
        {
            var cache = typedCache.InternalCache;
            if (cache is not RedisInvalidatableInMemoryCache inMemoryCache)
            {
                throw new InvalidOperationException("This method can only be used on RedisInvalidatableInMemoryCache classes.");
            }

            await inMemoryCache.SendCacheInvalidationInstructionAsync(key, hardInvalidate);
        }

        public static async Task<TValue> SetWithLockAsync<TKey, TValue>(
            this ITypedCache<TKey, TValue> typedCache,
            TKey key,
            TValue newValue,
            Func<TValue, bool> decider
        )
        {
            var cache = typedCache.InternalCache;
            if (cache is not RedisInvalidatableInMemoryCache inMemoryCache)
            {
                throw new InvalidOperationException("This method can only be used on RedisInvalidatableInMemoryCache classes.");
            }

            return (TValue)await inMemoryCache.SetWithLockAsync(
                key.ToString(),
                newValue,
                (currentValue) => decider((TValue)currentValue)
            );
        }

        public static async Task<TValue> WithLockAsync<TKey, TValue>(
            this ITypedCache<TKey, TValue> typedCache,
            TKey key,
            Func<Task<TValue>> func
        )
        {
            var cache = typedCache.InternalCache;
            if (cache is not RedisInvalidatableInMemoryCache inMemoryCache)
            {
                throw new InvalidOperationException("This method can only be used on RedisInvalidatableInMemoryCache classes.");
            }
            return (TValue)await inMemoryCache.WithLockAsync(
                key.ToString(),
                async () => await func()
            );
        }
    }
}
