using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using DispatcherWeb.CustomerNotifications.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.CustomerNotifications.Cache
{
    public class CustomerNotificationCache : ICustomerNotificationCache, ISingletonDependency
    {
        public const string IdsCacheName = "CustomerNotifications-Ids-Cache";
        public const string DetailsCacheName = "CustomerNotifications-Details-Cache";

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<CustomerNotification> _customerNotificationRepository;

        public CustomerNotificationCache(
            ICacheManager cacheManager,
            IRepository<CustomerNotification> customerNotificationRepository
        )
        {
            _cacheManager = cacheManager;
            _customerNotificationRepository = customerNotificationRepository;
        }

        public async Task<List<CustomerNotificationToShowDto>> StoreAndEnrichUserNotifications(DateTime date, long userId, List<int> customerNotificationIds)
        {
            await GetIdsCache()
                .SetAsync(GetKey(date, userId), customerNotificationIds);

            return await EnrichUserNotifications(customerNotificationIds);
        }

        public async Task<List<CustomerNotificationToShowDto>> GetFromCacheOrDefault(DateTime date, long userId)
        {
            var ids = await GetIdsCache()
                .GetOrDefaultAsync(GetKey(date, userId));

            if (ids == null)
            {
                return null;
            }

            return await EnrichUserNotifications(ids);
        }

        public async Task DismissCustomerNotification(DateTime date, long userId, int customerNotificationId)
        {
            var idsCache = GetIdsCache();
            var key = GetKey(date, userId);
            await idsCache.RemoveAsync(key);
        }

        public async Task InvalidateCache()
        {
            await GetDetailsCache().ClearAsync();
            await GetIdsCache().ClearAsync();
        }

        private async Task<List<CustomerNotificationToShowDto>> EnrichUserNotifications(List<int> ids)
        {
            var detailsCache = GetDetailsCache();

            var details = await detailsCache.GetSomeOrNoneAsync(ids);

            var missingIds = ids.Except(details.Select(x => x.Id)).ToList();
            if (missingIds.Any())
            {
                var missingDetails = await (await _customerNotificationRepository.GetQueryAsync())
                    .Where(x => missingIds.Contains(x.Id))
                    .Select(x => new CustomerNotificationToShowDto
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Body = x.Body,
                    }).ToListAsync();

                await detailsCache.SetAsync(missingDetails.ToKeyValuePairs(x => x.Id).ToArray());
                details.AddRange(missingDetails);
            }

            //reorder cachedDetails to match the order of received ids in case the order was significant
            return ids.Select(id => details.FirstOrDefault(d => d.Id == id)).Where(x => x != null).ToList();
        }

        private ITypedCache<string, List<int>> GetIdsCache()
        {
            return _cacheManager
                .GetCache(IdsCacheName)
                .AsTyped<string, List<int>>();
        }

        private ITypedCache<int, CustomerNotificationToShowDto> GetDetailsCache()
        {
            return _cacheManager
                .GetCache(DetailsCacheName)
                .AsTyped<int, CustomerNotificationToShowDto>();
        }

        private static string GetKey(DateTime date, long userId)
        {
            return $"{date:yyyy-MM-dd}-{userId}";
        }
    }
}
