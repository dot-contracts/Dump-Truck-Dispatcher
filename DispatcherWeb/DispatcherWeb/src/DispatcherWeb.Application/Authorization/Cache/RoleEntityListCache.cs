using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Caching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Cache
{
    public class RoleEntityListCache : ListCacheBase<ListCacheTenantKey, Role, Role>,
        IAsyncEventHandler<EntityChangingEventData<Role>>,
        IRoleEntityListCache,
        ISingletonDependency
    {
        private readonly IRepository<Role> _roleRepository;
        public override string CacheName => ListCacheNames.RoleEntity;

        public RoleEntityListCache(
            IRepository<Role> roleRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _roleRepository = roleRepository;
        }

        protected override async Task<List<Role>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _roleRepository.GetQueryAsync(), afterDateTime)
                .AsNoTracking()
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Role entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId ?? 0));
        }
    }
}
