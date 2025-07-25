using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.MultiTenancy;
using Abp.Zero.Configuration;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace DispatcherWeb.Tests.MultiTenancy
{
    public class TenantAppService_Tests : AppTestBase
    {
        private readonly ITenantAppService _tenantAppService;

        public TenantAppService_Tests()
        {
            LoginAsHostAdmin();
            _tenantAppService = Resolve<ITenantAppService>();
        }

        [MultiTenantFact]
        public async Task GetTenants_Test()
        {
            //Act
            var output = await _tenantAppService.GetTenants(new GetTenantsInput());

            //Assert
            output.TotalCount.ShouldBe(1);
            output.Items.Count.ShouldBe(1);
            output.Items[0].TenancyName.ShouldBe(Tenant.DefaultTenantName);
        }

        [MultiTenantFact]
        public async Task Create_Update_And_Delete_Tenant_Test()
        {
            //CREATE --------------------------------

            //Act
            await _tenantAppService.CreateTenant(
                new CreateTenantInput
                {
                    CompanyName = "Tenant for test purpose",
                    AdminFirstName = "Test",
                    AdminLastName = "Test",
                    AdminEmailAddress = "admin@testtenant.com",
                    AdminPassword = "123qwe",
                    IsActive = true,
                });

            //Assert
            var tenant = await GetTenantOrNullAsync("Tenant-for-test-purpose");
            tenant.ShouldNotBe(null);
            tenant.Name.ShouldBe("Tenant for test purpose");
            tenant.IsActive.ShouldBe(true);

            await UsingDbContextAsync(tenant.Id, async context =>
            {
                //Check static roles
                var staticRoleNames = Resolve<IRoleManagementConfig>().StaticRoles.Where(r => r.Side == MultiTenancySides.Tenant).Select(role => role.RoleName).ToList();
                foreach (var staticRoleName in staticRoleNames)
                {
                    (await context.Roles.CountAsync(r => r.TenantId == tenant.Id && r.Name == staticRoleName)).ShouldBe(1);
                }

                //Check default admin user
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.UserName == User.AdminUserName);
                adminUser.ShouldNotBeNull();

                //Check notification registration
                //(await context.NotificationSubscriptions.FirstOrDefaultAsync(ns => ns.UserId == adminUser.Id && ns.NotificationName == AppNotificationNames.NewUserRegistered)).ShouldNotBeNull();
            });

            //GET FOR EDIT -----------------------------

            //Act
            var editDto = await _tenantAppService.GetTenantForEdit(new EntityDto(tenant.Id));

            //Assert
            editDto.TenancyName.ShouldBe("Tenant-for-test-purpose");
            editDto.Name.ShouldBe("Tenant for test purpose");
            editDto.IsActive.ShouldBe(true);

            // UPDATE ----------------------------------

            editDto.Name = "edited tenant name";
            editDto.IsActive = false;
            await _tenantAppService.UpdateTenant(editDto);

            //Assert
            tenant = await GetTenantAsync("Tenant-for-test-purpose");
            tenant.Name.ShouldBe("edited tenant name");
            tenant.IsActive.ShouldBe(false);

            // DELETE ----------------------------------

            //Act
            await _tenantAppService.DeleteTenant(new EntityDto((await GetTenantAsync("Tenant-for-test-purpose")).Id));

            //Assert
            (await GetTenantOrNullAsync("Tenant-for-test-purpose")).IsDeleted.ShouldBe(true);
        }

        [MultiTenantFact]
        public async Task Create_Tenant_Should_Have_Standard_Edition_Test()
        {
            //CREATE --------------------------------

            //Act
            await _tenantAppService.CreateTenant(
                new CreateTenantInput
                {
                    CompanyName = "Tenant for test purpose",
                    AdminFirstName = "Test",
                    AdminLastName = "Test",
                    AdminEmailAddress = "admin@testtenant.com",
                    AdminPassword = "123qwe",
                    IsActive = true,
                    EditionId = 1,
                });

            //Assert
            var tenant = await GetTenantOrNullAsync("Tenant-for-test-purpose");
            tenant.ShouldNotBe(null);
            tenant.Name.ShouldBe("Tenant for test purpose");
            tenant.IsActive.ShouldBe(true);
            tenant.EditionId.ShouldBe(1);
        }

        [MultiTenantFact(Skip = "#14648 Historically failing")]
        public async Task CreateTenant_should_seed_database_Test()
        {
            //Act
            await _tenantAppService.CreateTenant(
                new CreateTenantInput
                {
                    AdminFirstName = "Test",
                    AdminLastName = "Test",
                    CompanyName = "Tenant for test purpose",
                    AdminEmailAddress = "admin@testtenant.com",
                    AdminPassword = "123qwe",
                    IsActive = true,
                });

            //Assert
            var tenant = await GetTenantOrNullAsync("Tenant-for-test-purpose");
            tenant.ShouldNotBe(null);
            tenant.Name.ShouldBe("Tenant for test purpose");
            tenant.IsActive.ShouldBe(true);

            await UsingDbContextAsync(tenant.Id, async context =>
            {
                //Check static roles
                var staticRoleNames = Resolve<IRoleManagementConfig>().StaticRoles.Where(r => r.Side == MultiTenancySides.Tenant).Select(role => role.RoleName).ToList();
                foreach (var staticRoleName in staticRoleNames)
                {
                    (await context.Roles.CountAsync(r => r.TenantId == tenant.Id && r.Name == staticRoleName)).ShouldBe(1);
                }
                (await context.Roles.CountAsync(r => r.TenantId == tenant.Id)).ShouldBe(11);

                // Check user role is default
                (await context.Roles.FirstAsync(r => r.Name == StaticRoleNames.Tenants.User)).IsDefault.ShouldBeTrue();

                // Check default permissions granted to roles
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Admin))
                    .ShouldBe(59);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Administrative))
                    .ShouldBe(27);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Backoffice))
                    .ShouldBe(11);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Dispatching))
                    .ShouldBe(23);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.LimitedQuoting))
                    .ShouldBe(4);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Maintenance))
                    .ShouldBe(8);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.MaintenanceSupervisor))
                    .ShouldBe(12);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Managers))
                    .ShouldBe(31);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Quoting))
                    .ShouldBe(5);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.User))
                    .ShouldBe(0);
                (await GetPermissionsCountForRole(context, tenant.Id, StaticRoleNames.Tenants.Driver))
                    .ShouldBe(2);

                //Check default admin user
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.UserName == User.AdminUserName);
                adminUser.ShouldNotBeNull();

                // Check LocationCategory created
                (await context.LocationCategories.CountAsync(sc => sc.TenantId == tenant.Id)).ShouldBe(8);

                // Check Service created
                (await context.Items.CountAsync(sc => sc.TenantId == tenant.Id)).ShouldBe(2);

                // Check UnitOfMeasure created
                (await context.UnitsOfMeasure.CountAsync(sc => sc.TenantId == tenant.Id)).ShouldBe(4);
            });

        }

        private async Task<int> GetPermissionsCountForRole(DispatcherWebDbContext context, int tenantId, string roleName)
        {
            int roleId = await context.Roles.Where(r => r.TenantId == tenantId && r.Name == roleName).Select(r => r.Id).FirstOrDefaultAsync();
            return await context.RolePermissions.CountAsync(rp => rp.IsGranted && rp.RoleId == roleId && rp.TenantId == tenantId);
        }

    }
}
