using Abp.Dependency;
using DispatcherWeb.Authorization.Cache;

namespace DispatcherWeb.Caching
{
    public class EntityListCacheCollection : ISingletonDependency
    {
        public IUserEntityListCache User { get; }
        public IUserLoginEntityListCache UserLogin { get; }
        public IRoleEntityListCache Role { get; }
        public IUserRoleEntityListCache UserRole { get; }
        public IUserOrganizationUnitEntityListCache UserOrganizationUnit { get; }
        public IOrganizationUnitRoleEntityListCache OrganizationUnitRole { get; }

        public EntityListCacheCollection(
            IUserEntityListCache user,
            IUserLoginEntityListCache userLogin,
            IRoleEntityListCache role,
            IUserRoleEntityListCache userRole,
            IUserOrganizationUnitEntityListCache userOrganizationUnit,
            IOrganizationUnitRoleEntityListCache organizationUnitRole
        )
        {
            User = user;
            UserLogin = userLogin;
            Role = role;
            UserRole = userRole;
            UserOrganizationUnit = userOrganizationUnit;
            OrganizationUnitRole = organizationUnitRole;
        }
    }
}
