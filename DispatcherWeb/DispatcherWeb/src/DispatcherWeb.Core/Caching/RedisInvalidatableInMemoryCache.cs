using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Abp.Data;
using Abp.Runtime.Caching.Memory;
using Abp.Threading.Extensions;

namespace DispatcherWeb.Caching
{
    public class RedisInvalidatableInMemoryCache : AbpMemoryCache
    {
        private ICacheInvalidationService _cacheInvalidationService;

        protected readonly ConcurrentDictionary<string, SemaphoreSlim> CacheLocks = new();

        public bool UseStaleListLogic { get; set; }

        public bool SuppressInvalidationSyncCalls { get; set; }

        public RedisInvalidatableInMemoryCache(
            string name,
            ICacheInvalidationService cacheInvalidationService
            ) : base(name)
        {
            _cacheInvalidationService = cacheInvalidationService;
        }

        protected override bool TrackStatistics => true;

        protected override long? SizeLimit => long.MaxValue;

        public override void Remove(string key)
        {
            Remove(key, true);
        }

        public override async Task RemoveAsync(string key)
        {
            await RemoveAsync(key, true);
        }

        public void Remove(string key, bool sendCacheInvalidationInstruction)
        {
            if (sendCacheInvalidationInstruction && !SuppressInvalidationSyncCalls)
            {
                var hardInvalidate = UseStaleListLogic;
                _cacheInvalidationService.SendCacheInvalidationInstruction(Name, key, hardInvalidate);
            }

            base.Remove(key);
        }

        public async Task RemoveAsync(string key, bool sendCacheInvalidationInstruction)
        {
            if (sendCacheInvalidationInstruction && !SuppressInvalidationSyncCalls)
            {
                var hardInvalidate = UseStaleListLogic;
                await _cacheInvalidationService.SendCacheInvalidationInstructionAsync(Name, key, hardInvalidate);
            }

            base.Remove(key); //not base.RemoveAsync because it will call this.Remove which is overridden to call this method. See implementation for more details.
        }

        public override void Clear()
        {
            Clear(true);
        }

        public override async Task ClearAsync()
        {
            await ClearAsync(true);
        }

        public void Clear(bool sendCacheInvalidationInstruction)
        {
            if (sendCacheInvalidationInstruction && !SuppressInvalidationSyncCalls)
            {
                _cacheInvalidationService.SendCacheInvalidationInstruction(Name);
            }

            base.Clear();
        }

        public async Task ClearAsync(bool sendCacheInvalidationInstruction)
        {
            if (sendCacheInvalidationInstruction && !SuppressInvalidationSyncCalls)
            {
                await _cacheInvalidationService.SendCacheInvalidationInstructionAsync(Name);
            }

            base.Clear(); //not base.ClearAsync because it will call this.Clear which is overridden to call this method. See implementation for more details.
        }

        public override void Dispose()
        {
            _cacheInvalidationService = null;
            base.Dispose();
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
                Logger.Error($"Failed to get cached value for key '{key}': {ex.Message}", ex);
                // Clear the problematic cache entry
                try
                {
                    base.Remove(key);
                }
                catch (Exception clearEx)
                {
                    Logger.Error($"Failed to clear problematic cache entry '{key}': {clearEx.Message}", clearEx);
                }
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
                    Logger.Error($"Failed to get cached value for key '{key}' in lock: {ex.Message}", ex);
                    // Clear the problematic cache entry
                    try
                    {
                        base.Remove(key);
                    }
                    catch (Exception clearEx)
                    {
                        Logger.Error($"Failed to clear problematic cache entry '{key}' in lock: {clearEx.Message}", clearEx);
                    }
                }

                if (result.HasValue)
                {
                    return result.Value;
                }

                try
                {
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
                        Logger.Error($"Failed to set cached value for key '{key}': {ex.Message}", ex);
                    }
                    return generatedValue;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to generate value for key '{key}': {ex.Message}", ex);
                    // Return null or default value to prevent application crash
                    return null;
                }
            }
        }

        public async Task SendCacheInvalidationInstructionAsync(string key, bool hardInvalidate)
        {
            await _cacheInvalidationService.SendCacheInvalidationInstructionAsync(Name, key, hardInvalidate);
        }

        /// <param name="key">Key</param>
        /// <param name="newValue">New value</param>
        /// <param name="decider">Function that takes currentValue, newValue as an arguments and returns true if we should proceed to update the value</param>
        public async Task<object> SetWithLockAsync(string key, object newValue, Func<object, bool> decider)
        {
            using (await LockCacheAsync(key))
            {
                var currentValueResult = await TryGetValueAsync(key);
                var currentValue = currentValueResult.HasValue ? currentValueResult.Value : null;
                if (decider(currentValue))
                {
                    await SetAsync(key, newValue);

                    return newValue;
                }

                return currentValue;
            }
        }

        public async Task<object> WithLockAsync(string key, Func<Task<object>> func)
        {
            using (await LockCacheAsync(key))
            {
                return await func();
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
