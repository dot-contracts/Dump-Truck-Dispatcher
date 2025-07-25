using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching.Dto;

namespace DispatcherWeb.Caching
{
    [AbpAuthorize]
    public partial class CachingAppService
    {
        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
        public async Task<ListResultDto<CacheDto>> GetAllCaches()
        {
            await Task.CompletedTask;

            var caches = _cacheManager.GetAllCaches()
                .Select(cache =>
                {
                    var result = new CacheDto
                    {
                        Name = cache.Name,
                    };

                    if (cache is RedisInvalidatableInMemoryCache inMemoryCache)
                    {
                        var statistics = inMemoryCache.GetStatistics();
                        result.TotalHits = statistics?.TotalHits;
                        result.TotalMisses = statistics?.TotalMisses;
                        result.CurrentEntryCount = statistics?.CurrentEntryCount;
                        result.CurrentEstimatedSize = statistics?.CurrentEstimatedSize;
                    }

                    return result;
                })
                .ToList();

            return new ListResultDto<CacheDto>(caches);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
        public async Task ClearCache(EntityDto<string> input)
        {
            var cache = _cacheManager.GetCache(input.Id);
            await cache.ClearAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
        public async Task ClearAllCaches()
        {
            if (_cacheManager is RedisInvalidatableInMemoryCacheManager redisInvalidatableCacheManager)
            {
                await redisInvalidatableCacheManager.InvalidateAllCaches();
            }
            else
            {
                var caches = _cacheManager.GetAllCaches();
                foreach (var cache in caches)
                {
                    await cache.ClearAsync();
                }
            }
        }
    }
}
