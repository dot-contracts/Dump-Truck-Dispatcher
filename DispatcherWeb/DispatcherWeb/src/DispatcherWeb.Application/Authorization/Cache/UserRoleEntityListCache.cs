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
    public class UserRoleEntityListCache : ListCacheBase<ListCacheTenantKey, UserRoleCacheItem, long, UserRole>,
        IAsyncEventHandler<EntityChangingEventData<UserRole>>,
        IUserRoleEntityListCache,
        ISingletonDependency
    {
        private readonly IRepository<UserRole, long> _userRoleRepository;
        public override string CacheName => ListCacheNames.UserRoleEntity;

        public UserRoleEntityListCache(
            IRepository<UserRole, long> userRoleRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _userRoleRepository = userRoleRepository;
        }

        protected override async Task<List<UserRoleCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _userRoleRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new UserRoleCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    UserId = x.UserId,
                    RoleId = x.RoleId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(UserRole entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId ?? 0));
        }
    }
}
