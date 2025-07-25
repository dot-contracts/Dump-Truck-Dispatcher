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
    public class UserLoginEntityListCache : ListCacheBase<ListCacheTenantKey, UserLoginCacheItem, long, UserLogin>,
        IAsyncEventHandler<EntityChangingEventData<UserLogin>>,
        IUserLoginEntityListCache,
        ISingletonDependency
    {
        private readonly IRepository<UserLogin, long> _userLoginRepository;
        public override string CacheName => ListCacheNames.UserLoginEntity;

        public UserLoginEntityListCache(
            IRepository<UserLogin, long> userLoginRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _userLoginRepository = userLoginRepository;
        }

        protected override async Task<List<UserLoginCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _userLoginRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new UserLoginCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    UserId = x.UserId,
                    LoginProvider = x.LoginProvider,
                    ProviderKey = x.ProviderKey,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(UserLogin entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId ?? 0));
        }
    }
}
