using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Organizations;
using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Caching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Cache
{
    public class OrganizationUnitRoleEntityListCache : ListCacheBase<ListCacheTenantKey, OrganizationUnitRoleCacheItem, long, OrganizationUnitRole>,
        IAsyncEventHandler<EntityChangingEventData<OrganizationUnitRole>>,
        IOrganizationUnitRoleEntityListCache,
        ISingletonDependency
    {
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        public override string CacheName => ListCacheNames.OrganizationUnitRoleEntity;

        public OrganizationUnitRoleEntityListCache(
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
        }

        protected override async Task<List<OrganizationUnitRoleCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _organizationUnitRoleRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new OrganizationUnitRoleCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    OrganizationUnitId = x.OrganizationUnitId,
                    RoleId = x.RoleId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(OrganizationUnitRole entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId ?? 0));
        }
    }
}
