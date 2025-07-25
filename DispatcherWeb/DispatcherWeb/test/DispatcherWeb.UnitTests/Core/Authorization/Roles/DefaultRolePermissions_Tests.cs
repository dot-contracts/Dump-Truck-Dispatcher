using System.Linq;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Core.Authorization.Roles
{
    public class DefaultRolePermissions_Tests
    {
        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Admin_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Admin, new[]
            {
                AppPermissions.Pages,

                AppPermissions.Pages_Administration,
                AppPermissions.Pages_Administration_AuditLogs,
                AppPermissions.Pages_Administration_Languages,
                AppPermissions.Pages_Administration_OrganizationUnits,
                AppPermissions.Pages_Administration_Roles,
                AppPermissions.Pages_Administration_Roles_Create,
                AppPermissions.Pages_Administration_Roles_Delete,
                AppPermissions.Pages_Administration_Roles_Edit,
                AppPermissions.Pages_Administration_Tenant_Settings,
                AppPermissions.Pages_Administration_Users,
                AppPermissions.Pages_Administration_Users_ChangePermissions,
                AppPermissions.Pages_Administration_Users_Create,
                AppPermissions.Pages_Administration_Users_Delete,
                AppPermissions.Pages_Administration_Users_Edit,
                AppPermissions.Pages_Administration_Users_Impersonation,

                AppPermissions.Pages_CannedText,
                AppPermissions.Pages_Customers,
                AppPermissions.Pages_Customers_Merge,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_Dashboard_Revenue,
                AppPermissions.Pages_Dispatches,
                AppPermissions.Pages_Dispatches_Edit,
                AppPermissions.Pages_DriverApplication,
                AppPermissions.Pages_DriverAssignment,
                AppPermissions.Pages_DriverMessages,
                AppPermissions.Pages_Drivers,
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_LeaseHaulerRequests,
                AppPermissions.Pages_LeaseHaulerRequests_Edit,
                AppPermissions.Pages_OfficeAccess_All,
                AppPermissions.Pages_OfficeAccess_UserOnly,
                AppPermissions.Pages_Offices,
                AppPermissions.Pages_Orders_View,
                AppPermissions.Pages_Orders_Edit,
                AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
                AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit,
                AppPermissions.Pages_PrintOrders,
                AppPermissions.Pages_Quotes_View,
                AppPermissions.Pages_Quotes_Edit,
                AppPermissions.Pages_Quotes_Items_Create,
                AppPermissions.Pages_Reports,
                AppPermissions.Pages_Reports_OutOfServiceTrucks,
                AppPermissions.Pages_Reports_RevenueBreakdown,
                AppPermissions.Pages_Reports_RevenueBreakdownByTruck,
                AppPermissions.Pages_Reports_ScheduledReports,
                AppPermissions.Pages_Reports_Receipts,
                AppPermissions.Pages_Reports_PaymentReconciliation,
                AppPermissions.Pages_Items,
                AppPermissions.Pages_Items_Merge,
                AppPermissions.Pages_Schedule,
                AppPermissions.Pages_SendOrdersToDrivers,
                AppPermissions.Pages_Locations,
                AppPermissions.Pages_Locations_Merge,
                AppPermissions.Pages_Tickets_View,
                AppPermissions.Pages_Tickets_Edit,
                AppPermissions.Pages_Trucks,
                AppPermissions.Pages_Items_TaxRates,
                AppPermissions.Pages_OutOfServiceHistory_Delete,
                AppPermissions.Pages_VehicleService_View,
                AppPermissions.Pages_VehicleService_Edit,
                AppPermissions.Pages_WorkOrders_View,
                AppPermissions.Pages_WorkOrders_Edit,
                AppPermissions.Pages_WorkOrders_EditLimited,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Dispatching_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Dispatching, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Customers,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_DriverAssignment,
                AppPermissions.Pages_DriverMessages,
                AppPermissions.Pages_Drivers,
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_LeaseHaulerRequests,
                AppPermissions.Pages_LeaseHaulerRequests_Edit,
                AppPermissions.Pages_OfficeAccess_UserOnly,
                AppPermissions.Pages_Orders_View,
                AppPermissions.Pages_Orders_Edit,
                AppPermissions.Pages_PrintOrders,
                AppPermissions.Pages_Items,
                AppPermissions.Pages_Schedule,
                AppPermissions.Pages_SendOrdersToDrivers,
                AppPermissions.Pages_Locations,
                AppPermissions.Pages_Tickets_View,
                AppPermissions.Pages_Tickets_Edit,
                AppPermissions.Pages_Trucks,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_LimitedQuoting_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.LimitedQuoting, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Quotes_View,
                AppPermissions.Pages_Quotes_Edit,
                AppPermissions.Pages_Quotes_Items_Create,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Quoting_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Quoting, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Quotes_View,
                AppPermissions.Pages_Quotes_Edit,
                AppPermissions.Pages_Quotes_Items_Create,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Backoffice_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Backoffice, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_Orders_View,
                AppPermissions.Pages_Orders_Edit,
                AppPermissions.Pages_PrintOrders,
                AppPermissions.Pages_Tickets_View,
                AppPermissions.Pages_Tickets_Edit,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Administrative_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Administrative, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Administration,
                AppPermissions.Pages_Administration_Roles,
                AppPermissions.Pages_Administration_Roles_Create,
                AppPermissions.Pages_Administration_Roles_Delete,
                AppPermissions.Pages_Administration_Roles_Edit,
                AppPermissions.Pages_Administration_Tenant_Settings,
                AppPermissions.Pages_Administration_Users,
                AppPermissions.Pages_Administration_Users_ChangePermissions,
                AppPermissions.Pages_Administration_Users_Create,
                AppPermissions.Pages_Administration_Users_Delete,
                AppPermissions.Pages_Administration_Users_Edit,
                AppPermissions.Pages_Administration_Users_Impersonation,

                AppPermissions.Pages_CannedText,
                AppPermissions.Pages_Customers,
                AppPermissions.Pages_Customers_Merge,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_Drivers,
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_Offices,
                AppPermissions.Pages_Items,
                AppPermissions.Pages_Items_Merge,
                AppPermissions.Pages_Locations,
                AppPermissions.Pages_Locations_Merge,
                AppPermissions.Pages_Trucks,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Managers_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Managers, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_CannedText,
                AppPermissions.Pages_Customers,
                AppPermissions.Pages_Customers_Merge,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_DriverAssignment,
                AppPermissions.Pages_DriverMessages,
                AppPermissions.Pages_Drivers,
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.Pages_OfficeAccess_UserOnly,
                AppPermissions.Pages_Orders_View,
                AppPermissions.Pages_Orders_Edit,
                AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
                AppPermissions.Pages_PrintOrders,
                AppPermissions.Pages_Items,
                AppPermissions.Pages_Items_Merge,
                AppPermissions.Pages_Quotes_View,
                AppPermissions.Pages_Quotes_Edit,
                AppPermissions.Pages_Quotes_Items_Create,
                AppPermissions.Pages_Schedule,
                AppPermissions.Pages_SendOrdersToDrivers,
                AppPermissions.Pages_Locations,
                AppPermissions.Pages_Locations_Merge,
                AppPermissions.Pages_Tickets_View,
                AppPermissions.Pages_Tickets_Edit,
                AppPermissions.Pages_Trucks,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Maintenance_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Maintenance, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_OfficeAccess_All,
                AppPermissions.Pages_OfficeAccess_UserOnly,
                AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
                AppPermissions.Pages_VehicleService_View,
                AppPermissions.Pages_WorkOrders_View,
                AppPermissions.Pages_WorkOrders_EditLimited,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_MaintenanceSupervisor_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.MaintenanceSupervisor, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_Dashboard,
                AppPermissions.Pages_OfficeAccess_All,
                AppPermissions.Pages_OfficeAccess_UserOnly,
                AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
                AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit,
                AppPermissions.Pages_Trucks,
                AppPermissions.Pages_VehicleService_View,
                AppPermissions.Pages_VehicleService_Edit,
                AppPermissions.Pages_WorkOrders_View,
                AppPermissions.Pages_WorkOrders_Edit,
                AppPermissions.Pages_WorkOrders_EditLimited,
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_Driver_role_has_all_default_permissions()
        {
            AssertRoleHasPermissions(StaticRoleNames.Tenants.Driver, new[]
            {
                AppPermissions.Pages,
                AppPermissions.Pages_DriverApplication,
            });
        }

        private void AssertRoleHasPermissions(string roleName, string[] permissions)
        {
            string[] allPermissions = permissions.Concat(DefaultRolePermissions.DefaultPermissions).Distinct().ToArray();
            foreach (string permissionName in allPermissions)
            {
                bool isPermissionsGranted = DefaultRolePermissions.IsPermissionsGrantedToRole(roleName, permissionName);
                bool isPermissionShouldBeGranted = permissions.Contains(permissionName);
                isPermissionsGranted.ShouldBe(isPermissionShouldBeGranted,
                    $"Permission {permissionName} is{(isPermissionsGranted ? "" : "n't")} granted, but it should{(isPermissionShouldBeGranted ? "" : "n't")} be."
                );
            }
        }

    }
}
