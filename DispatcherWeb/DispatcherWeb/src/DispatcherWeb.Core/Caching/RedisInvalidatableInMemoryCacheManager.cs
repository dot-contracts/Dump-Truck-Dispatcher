using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Runtime.Caching;
using Abp.Runtime.Caching.Configuration;
using Abp.Runtime.Caching.Memory;
using DispatcherWeb.Authorization.Impersonation;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Friendships.Cache;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Caching
{
    public class RedisInvalidatableInMemoryCacheManager : AbpMemoryCacheManager
    {
        private readonly IIocManager _iocManager;
        private readonly IConfigurationRoot _configuration;
        private readonly ICacheInvalidationService _cacheInvalidationService;

        public RedisInvalidatableInMemoryCacheManager(
            IIocManager iocManager,
            IAppConfigurationAccessor configurationAccessor,
            ICacheInvalidationService cacheInvalidationService,
            ICachingConfiguration configuration
            ) : base(configuration)
        {
            _iocManager = iocManager;
            _configuration = configurationAccessor.Configuration;
            _cacheInvalidationService = cacheInvalidationService;
            _iocManager.RegisterIfNot<RedisCache>(DependencyLifeStyle.Transient);
        }

        private readonly string[] _redisCacheNames = new[]
        {
            ImpersonationCacheItem.CacheName,
            SwitchToLinkedAccountCacheItem.CacheName,
            FriendCacheItem.CacheName,
        };

        private bool IsRedisCacheEnabled => !string.IsNullOrEmpty(_configuration["Abp:RedisCache:ConnectionString"]);

        protected override ICache CreateCacheImplementation(string name)
        {
            if (_redisCacheNames.Contains(name) && IsRedisCacheEnabled)
            {
                return _iocManager.Resolve<RedisCache>(new { name });
            }
            else
            {
                return new RedisInvalidatableInMemoryCache(name, _cacheInvalidationService)
                {
                    Logger = Logger,
                };
            }
        }

        public async Task InvalidateCache(CacheInvalidationInstruction instruction)
        {
            if (string.IsNullOrEmpty(instruction.CacheName))
            {
                await InvalidateAllCaches(false);
                return;
            }

            foreach (var cache in Caches.Values
                         .OfType<RedisInvalidatableInMemoryCache>()
                         .Where(x => x.Name == instruction.CacheName)
                         .ToList())
            {
                if (cache.UseStaleListLogic)
                {
                    var listCache = FindListCache(cache.Name);
                    if (listCache != null
                        && !instruction.HardInvalidate
                        && !string.IsNullOrEmpty(instruction.CacheKey))
                    {
                        //if (string.IsNullOrEmpty(instruction.CacheKey))
                        //{
                        //    await listCache.InvalidateCache();
                        //}
                        //else
                        //{
                        //    if (instruction.HardInvalidate)
                        //    {
                        //        await listCache.HardInvalidateCache(instruction.CacheKey);
                        //    }
                        //    else
                        //    {

                        await listCache.MarkAsStaleInternal(instruction.CacheKey);

                        //    }
                        //}
                        continue;
                    }
                }
                if (string.IsNullOrEmpty(instruction.CacheKey))
                {
                    await cache.ClearAsync(false);
                }
                else
                {
                    await cache.RemoveAsync(instruction.CacheKey, false);
                }
            }
        }

        public async Task InvalidateAllCaches(bool sendCacheInvalidationInstruction = true)
        {
            if (sendCacheInvalidationInstruction)
            {
                await _cacheInvalidationService.SendCacheInvalidationInstructionAsync(null);
            }

            foreach (var cache in Caches.Values
                         .OfType<RedisInvalidatableInMemoryCache>()
                         .ToList())
            {
                //if (cache.UseStaleListLogic)
                //{
                //    var listCache = FindListCache(cache.Name);
                //    if (listCache != null)
                //    {
                //        await listCache.InvalidateCache();
                //        continue;
                //    }
                //}

                await cache.ClearAsync(false);
            }

            foreach (var cache in Caches.Values
                         .OfType<RedisCache>()
                         .ToList())
            {
                await cache.ClearAsync();
            }
        }


        protected override void DisposeCaches()
        {
            foreach (var cache in Caches.Values)
            {
                if (cache is RedisCache redisCache)
                {
                    _iocManager.Release(redisCache);
                }
                else
                {
                    cache.Dispose();
                }
            }
        }

        private readonly List<IListCache> _listCaches = new List<IListCache>();

        public void RegisterListCache(IListCache listCache)
        {
            _listCaches.Add(listCache);
        }

        public IReadOnlyCollection<IListCache> ListCaches => _listCaches;

        public IListCache FindListCache(string name)
        {
            return ListCaches.FirstOrDefault(x => x.CacheName == name);
        }
    }
}
