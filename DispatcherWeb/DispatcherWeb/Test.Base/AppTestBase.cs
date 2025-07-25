using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.EntityFrameworkCore.Extensions;
using Abp.Events.Bus;
using Abp.Events.Bus.Entities;
using Abp.Modules;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.TestBase;
using Abp.TestBase.Runtime.Session;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Test.Base.Sessions;
using DispatcherWeb.Test.Base.TestData;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Test.Base
{
    /// <summary>
    /// This is base class for all our test classes.
    /// It prepares ABP system, modules and a fake, in-memory database.
    /// Seeds database with initial data.
    /// Provides methods to easily work with <see cref="DispatcherWebDbContext"/>.
    /// </summary>
    public abstract class AppTestBase<T> : AbpIntegratedTestBase<T> where T : AbpModule
    {
        [Obsolete("Do not use TestAbpSession, use ExtendedTestAbpSession instead", error: true)]
        protected new TestAbpSession? AbpSession { get; set; }

        protected ExtendedTestAbpSession Session { get; set; }

        protected AppTestBase()
        {
            Session = Resolve<ExtendedTestAbpSession>();
            SeedTestData();
            LoginAsDefaultTenantAdmin();
        }

        private void SeedTestData()
        {
            void NormalizeDbContext(DispatcherWebDbContext context)
            {
                context.EntityChangeEventHelper = NullEntityChangeEventHelper.Instance;
                context.EventBus = NullEventBus.Instance;
                context.SuppressAutoSetTenantId = true;
            }

            //Seed initial data for default tenant
            Session.TenantId = 1;
            Session.OfficeId = 1;

            UsingDbContext(context =>
            {
                NormalizeDbContext(context);
                new TestDataBuilder(context, 1).Create();
            });
        }

        protected IDisposable UsingTenantId(int? tenantId)
        {
            var previousTenantId = Session.TenantId;
            Session.TenantId = tenantId;
            return new DisposeAction(() => Session.TenantId = previousTenantId);
        }

        protected void UsingDbContext(Action<DispatcherWebDbContext> action)
        {
            UsingDbContext(Session.TenantId, action);
        }

        protected Task UsingDbContextAsync(Func<DispatcherWebDbContext, Task> action)
        {
            return UsingDbContextAsync(Session.TenantId, action);
        }

        protected TResult UsingDbContext<TResult>(Func<DispatcherWebDbContext, TResult> func)
        {
            return UsingDbContext(Session.TenantId, func);
        }

        protected Task<TResult> UsingDbContextAsync<TResult>(Func<DispatcherWebDbContext, Task<TResult>> func)
        {
            return UsingDbContextAsync(Session.TenantId, func);
        }

        protected void UsingDbContext(int? tenantId, Action<DispatcherWebDbContext> action)
        {
            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    action(context);
                    context.SaveChanges();
                }
            }
        }

        protected async Task UsingDbContextAsync(int? tenantId, Func<DispatcherWebDbContext, Task> action)
        {
            using (UsingTenantId(tenantId))
            {
                await using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    await action(context);
                    await context.SaveChangesAsync();
                }
            }
        }

        protected TResult UsingDbContext<TResult>(int? tenantId, Func<DispatcherWebDbContext, TResult> func)
        {
            TResult result;

            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    result = func(context);
                    context.SaveChanges();
                }
            }

            return result;
        }

        protected async Task<TResult> UsingDbContextAsync<TResult>(int? tenantId, Func<DispatcherWebDbContext, Task<TResult>> func)
        {
            TResult result;

            using (UsingTenantId(tenantId))
            {
                await using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    result = await func(context);
                    await context.SaveChangesAsync();
                }
            }

            return result;
        }

        #region Login

        protected void LoginAsHostAdmin()
        {
            LoginAsHost(AbpUserBase.AdminUserName);
        }

        protected void LoginAsDefaultTenantAdmin()
        {
            LoginAsTenant(AbpTenantBase.DefaultTenantName, AbpUserBase.AdminUserName);
        }

        protected void LoginAsHost(string userName)
        {
            Session.TenantId = null;

            var user = UsingDbContext(context => context.Users.FirstOrDefault(u => u.TenantId == Session.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for host.");
            }

            Session.UserId = user.Id;
        }

        protected void LoginAsTenant(string tenancyName, string userName)
        {
            Session.TenantId = null;

            var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
            if (tenant == null)
            {
                throw new Exception("There is no tenant: " + tenancyName);
            }

            Session.TenantId = tenant.Id;

            var user = UsingDbContext(context => context.Users.FirstOrDefault(u => u.TenantId == Session.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for tenant: " + tenancyName);
            }

            Session.UserId = user.Id;
        }

        #endregion

        #region GetCurrentUser

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected User GetCurrentUser()
        {
            var userId = Session.GetUserId();
            return UsingDbContext(context => context.Users.Single(u => u.Id == userId));
        }

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected async Task<User> GetCurrentUserAsync()
        {
            var userId = Session.GetUserId();
            return await UsingDbContext(context => context.Users.SingleAsync(u => u.Id == userId));
        }

        #endregion

        #region GetCurrentTenant

        /// <summary>
        /// Gets current tenant if <see cref="IAbpSession.GetTenantIdOrNullAsync"/> is not null.
        /// Throws exception if there is no current tenant.
        /// </summary>
        protected async Task<Tenant> GetCurrentTenantAsync()
        {
            var tenantId = await Session.GetTenantIdAsync();
            return await UsingDbContextAsync(null, async context => await context.Tenants.SingleAsync(t => t.Id == tenantId));
        }

        #endregion

        #region GetTenant / GetTenantOrNull

        protected Tenant GetTenant(string tenancyName)
        {
            return UsingDbContext(null, context => context.Tenants.Single(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant> GetTenantAsync(string tenancyName)
        {
            return await UsingDbContext(null, async context => await context.Tenants.SingleAsync(t => t.TenancyName == tenancyName));
        }

        protected Tenant? GetTenantOrNull(string tenancyName)
        {
            return UsingDbContext(null, context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant?> GetTenantOrNullAsync(string tenancyName)
        {
            return await UsingDbContext(null, async context => await context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == tenancyName));
        }

        #endregion

        #region GetRole

        protected Role GetRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Single(r => r.Name == roleName && r.TenantId == Session.TenantId));
        }

        protected async Task<Role> GetRoleAsync(string roleName)
        {
            return await UsingDbContext(async context => await context.Roles.SingleAsync(r => r.Name == roleName && r.TenantId == Session.TenantId));
        }

        #endregion

        #region GetUserByUserName

        protected User GetUserByUserName(string userName)
        {
            var user = GetUserByUserNameOrNull(userName);
            if (user == null)
            {
                throw new Exception("Can not find a user with username: " + userName);
            }

            return user;
        }

        protected async Task<User> GetUserByUserNameAsync(string userName)
        {
            var user = await GetUserByUserNameOrNullAsync(userName);
            if (user == null)
            {
                throw new Exception("Can not find a user with username: " + userName);
            }

            return user;
        }

        protected User? GetUserByUserNameOrNull(string userName)
        {
            return UsingDbContext(context =>
                context.Users.FirstOrDefault(u =>
                    u.UserName == userName
                    && u.TenantId == Session.TenantId
                    ));
        }

        protected async Task<User?> GetUserByUserNameOrNullAsync(string userName, bool includeRoles = false)
        {
            return await UsingDbContextAsync(async context =>
                await context.Users
                    .IncludeIf(includeRoles, u => u.Roles)
                    .FirstOrDefaultAsync(u =>
                            u.UserName == userName
                            && u.TenantId == Session.TenantId
                    ));
        }

        #endregion
    }
}
