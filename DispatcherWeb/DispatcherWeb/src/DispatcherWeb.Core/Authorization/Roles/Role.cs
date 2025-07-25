using Abp.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Roles
{
    /// <summary>
    /// Represents a role in the system.
    /// </summary>
    public class Role : AbpRole<User>, IAuditableCacheItem
    {
        //Can add application specific role properties here

        public Role()
        {

        }

        public Role(int? tenantId, string displayName)
            : base(tenantId, displayName)
        {

        }

        public Role(int? tenantId, string name, string displayName)
            : base(tenantId, name, displayName)
        {

        }

        public Role Clone()
        {
            return new Role
            {
                Id = Id,
                TenantId = TenantId,
                Name = Name,
                DisplayName = DisplayName,
                IsStatic = IsStatic,
                IsDefault = IsDefault,
                NormalizedName = NormalizedName,
                ConcurrencyStamp = ConcurrencyStamp,
                CreationTime = CreationTime,
                LastModificationTime = LastModificationTime,
                DeletionTime = DeletionTime,
                IsDeleted = IsDeleted,
                CreatorUserId = CreatorUserId,
                DeleterUserId = DeleterUserId,
                LastModifierUserId = LastModifierUserId,
                // Not collections or navigation properties:
                //Claims = Claims,
                //Permissions = Permissions,
                //LastModifierUser = LastModifierUser,
                //CreatorUser = CreatorUser,
                //DeleterUser = DeleterUser,
            };
        }
    }
}
