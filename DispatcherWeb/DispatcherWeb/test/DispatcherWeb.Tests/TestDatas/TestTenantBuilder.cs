using System;
using System.Linq;
using System.Reflection;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Editions;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Offices;
using Microsoft.EntityFrameworkCore;
using Tenant = DispatcherWeb.MultiTenancy.Tenant;

namespace DispatcherWeb.Tests.TestDatas
{
    public class TestTenantBuilder
    {
        private readonly DispatcherWebDbContext _context;

        public TestTenantBuilder(DispatcherWebDbContext context)
        {
            _context = context;
        }

        public int Create()
        {
            var tenantId = CreateDefaultTenant();

            return tenantId;
        }

        private int CreateDefaultTenant()
        {
            //Default tenant

            var defaultTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == Tenant.DefaultTenantName);
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant(AbpTenantBase.DefaultTenantName, AbpTenantBase.DefaultTenantName);

                var defaultEdition = _context.Editions.IgnoreQueryFilters().OfType<SubscribableEdition>().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    defaultTenant.EditionId = defaultEdition.Id;
                }

                _context.Tenants.Add(defaultTenant);
                _context.SaveChanges();

                var tenantRoles = typeof(StaticRoleNames.Tenants)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                    .Select(fi => fi.GetValue(null).ToString())
                    .ToList();

                foreach (var roleName in tenantRoles)
                {
                    if (!_context.Roles.IgnoreQueryFilters().Any(r => r.TenantId == defaultTenant.Id && r.Name == roleName))
                    {
                        _context.Roles.Add(new Role(defaultTenant.Id, roleName, roleName) { IsStatic = true });
                    }
                }
                _context.SaveChanges();

                var office = new Office
                {
                    TenantId = defaultTenant.Id,
                    Name = "Main",
                    TruckColor = "#6251d7",
                };
                _context.Offices.Add(office);
                _context.SaveChanges();

                var adminFirstName = "admin";
                var adminLastName = "admin";
                var adminEmailAddress = "testadmin@example.com";
                var adminUser = User.CreateTenantAdminUser(defaultTenant.Id, adminFirstName, adminLastName, adminEmailAddress);
                adminUser.ShouldChangePasswordOnNextLogin = false;
                adminUser.IsActive = true;
                adminUser.Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw=="; //123qwe
                adminUser.OfficeId = office.Id;

                adminUser.SetNormalizedNames();

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == defaultTenant.Id && r.Name == StaticRoleNames.Tenants.Admin);
                if (adminRole == null)
                {
                    throw new Exception("There is no admin role!");
                }
                _context.UserRoles.Add(new UserRole(defaultTenant.Id, adminUser.Id, adminRole.Id));
                _context.SaveChanges();

                _context.UserAccounts.Add(new UserAccount
                {
                    TenantId = defaultTenant.Id,
                    UserId = adminUser.Id,
                    UserName = adminUser.UserName,
                    EmailAddress = adminUser.EmailAddress,
                });
                _context.SaveChanges();
            }

            return defaultTenant.Id;
        }
    }
}
