using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Caching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Cache
{
    public class UserOrganizationUnitEntityListCache : ListCacheBase<ListCacheTenantKey, UserOrganizationUnitCacheItem, long, UserOrganizationUnit>,
        IAsyncEventHandler<EntityChangingEventData<UserOrganizationUnit>>,
        IUserOrganizationUnitEntityListCache,
        ISingletonDependency
    {
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        public override string CacheName => ListCacheNames.UserOrganizationUnitEntity;

        public UserOrganizationUnitEntityListCache(
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
        }

        protected override async Task<List<UserOrganizationUnitCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _userOrganizationUnitRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new UserOrganizationUnitCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    OrganizationUnitId = x.OrganizationUnitId,
                    UserId = x.UserId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(UserOrganizationUnit entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId ?? 0));
        }
    }
}
