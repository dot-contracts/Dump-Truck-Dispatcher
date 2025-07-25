using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Abp.Data;
using Abp.Runtime.Caching.Redis;
using Abp.Threading.Extensions;

namespace DispatcherWeb.Caching
{
    public class RedisCache : AbpRedisCache
    {
        protected readonly ConcurrentDictionary<string, SemaphoreSlim> CacheLocks = new();

        public RedisCache(
            string name,
            IAbpRedisCacheDatabaseProvider redisCacheDatabaseProvider,
            IRedisCacheSerializer redisCacheSerializer)
            : base(name, redisCacheDatabaseProvider, redisCacheSerializer)
        {
        }

        public override object Get(string key, Func<string, object> factory)
        {
            if (TryGetValue(key, out object value))
            {
                return value;
            }

            using (LockCache(key))
            {
                if (TryGetValue(key, out value))
                {
                    return value;
                }

                var generatedValue = factory(key);
                if (!IsDefaultValue(generatedValue))
                {
                    try
                    {
                        Set(key, generatedValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString(), ex);
                    }
                }
                return generatedValue;
            }
        }

        public override async Task<object> GetAsync(string key, Func<string, Task<object>> factory)
        {
            ConditionalValue<object> result = default;

            try
            {
                result = await TryGetValueAsync(key);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), ex);
            }

            if (result.HasValue)
            {
                return result.Value;
            }

            using (await LockCacheAsync(key))
            {
                try
                {
                    result = await TryGetValueAsync(key);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString(), ex);
                }

                if (result.HasValue)
                {
                    return result.Value;
                }

                var generatedValue = await factory(key);
                if (IsDefaultValue(generatedValue))
                {
                    return generatedValue;
                }

                try
                {
                    await SetAsync(key, generatedValue);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString(), ex);
                }
                return generatedValue;
            }
        }

        protected IDisposable LockCache(string key)
        {
            return CacheLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1)).Lock();
        }

        protected async Task<IDisposable> LockCacheAsync(string key)
        {
            return await CacheLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1)).LockAsync();
        }
    }
}
