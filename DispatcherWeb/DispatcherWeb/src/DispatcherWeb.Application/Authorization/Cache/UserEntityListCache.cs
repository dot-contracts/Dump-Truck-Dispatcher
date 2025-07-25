using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Cache
{
    public class UserEntityListCache : ListCacheBase<ListCacheTenantKey, User, long, User>,
        IAsyncEventHandler<EntityChangingEventData<User>>,
        IUserEntityListCache,
        ISingletonDependency
    {
        private readonly IRepository<User, long> _userRepository;
        public override string CacheName => ListCacheNames.UserEntity;

        public UserEntityListCache(
            IRepository<User, long> userRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _userRepository = userRepository;
        }

        protected override async Task<List<User>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _userRepository.GetQueryAsync(), afterDateTime)
                .AsNoTracking()
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(User entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId ?? 0));
        }
    }
}
