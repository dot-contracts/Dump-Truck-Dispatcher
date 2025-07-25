using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public class UserListCache : ListCacheBase<ListCacheTenantKey, UserCacheItem, long, User>,
        IUserListCache,
        ISingletonDependency
    {
        private readonly IRepository<User, long> _userRepository;
        public override string CacheName => ListCacheNames.User;

        public UserListCache(
            IRepository<User, long> userRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _userRepository = userRepository;
        }

        protected override async Task<List<UserCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _userRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new UserCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    TenantId = x.TenantId,
                    UserName = x.UserName,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    Email = x.EmailAddress,
                    ProfilePictureId = x.ProfilePictureId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(User entity)
        {
            if (entity.TenantId == null)
            {
                return Task.FromResult<ListCacheTenantKey>(null);
            }
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId.Value));
        }
    }
}
