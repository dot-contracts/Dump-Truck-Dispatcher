using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Authorization.Roles
{
    public static class DefaultRolePermissions
    {
        private static readonly Dictionary<string, string[]> _defaultRolePermissions = new Dictionary<string, string[]>
            {
                // Pages
                {
                    AppPermissions.Pages, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Driver,
                        StaticRoleNames.Tenants.Customer,
                        StaticRoleNames.Tenants.LeaseHaulerDriver,
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },
                // ActiveReports
                {
                    AppPermissions.Pages_ActiveReports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // ActiveReports VehicleMaintenanceWorkOrderReport
                {
                    AppPermissions.Pages_ActiveReports_VehicleMaintenanceWorkOrderReport, new[]
                    {

                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // Administration
                {
                    AppPermissions.Pages_Administration, new string[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_AuditLogs
                {
                    AppPermissions.Pages_Administration_AuditLogs, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },
                // Administration_Languages
                {
                    AppPermissions.Pages_Administration_Languages, new string[]
                    {
                        //StaticRoleNames.Tenants.Admin,
                    }
                },
                // Administration_OrganizationUnits
                {
                    AppPermissions.Pages_Administration_OrganizationUnits, new string[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_OrganizationUnits_ManageMembers
                {
                    AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers, new string[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Administration_Roles
                {
                    AppPermissions.Pages_Administration_Roles, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Roles_Create
                {
                    AppPermissions.Pages_Administration_Roles_Create, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Roles_Delete
                {
                    AppPermissions.Pages_Administration_Roles_Delete, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Roles_Edit
                {
                    AppPermissions.Pages_Administration_Roles_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Administration_Tenant_Settings
                {
                    AppPermissions.Pages_Administration_Tenant_Settings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Administration_Users
                {
                    AppPermissions.Pages_Administration_Users, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Users_ChangePermissions
                {
                    AppPermissions.Pages_Administration_Users_ChangePermissions, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Users_Create
                {
                    AppPermissions.Pages_Administration_Users_Create, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Users_Delete
                {
                    AppPermissions.Pages_Administration_Users_Delete, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Users_Edit
                {
                    AppPermissions.Pages_Administration_Users_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Users_Impersonation
                {
                    AppPermissions.Pages_Administration_Users_Impersonation, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Administration_Users_Unlock
                {
                    AppPermissions.Pages_Administration_Users_Unlock, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Can be salesperson
                {
                    AppPermissions.CanBeSalesperson, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // CannedText
                {
                    AppPermissions.Pages_CannedText, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Charges
                {
                    AppPermissions.Pages_Charges, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Customer Portal
                {
                    AppPermissions.CustomerPortal, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                // Customer Portal - Invoices
                {
                    AppPermissions.CustomerPortal_Invoices, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                // Customer Portal - OrderId Dropdown
                {
                    AppPermissions.CustomerPortal_Orders_IdDropdown, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                // Customer Portal - Select Lists
                {
                    AppPermissions.CustomerPortal_SelectLists, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                // Customer Portal - Select Lists - Users
                {
                    AppPermissions.CustomerPortal_SelectLists_Users, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                // Customer Portal - Ticket List
                {
                    AppPermissions.CustomerPortal_TicketList, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                // Customer Portal - Ticket List Export
                {
                    AppPermissions.CustomerPortal_TicketList_Export, new[]
                    {
                        StaticRoleNames.Tenants.Customer,
                    }
                },


                // Customers
                {
                    AppPermissions.Pages_Customers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Customers_Merge
                {
                    AppPermissions.Pages_Customers_Merge, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Dashboard
                {
                    AppPermissions.Pages_Dashboard, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // Dashboard RevenueGraph
                {
                    AppPermissions.Pages_Dashboard_Revenue, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Dashboard Dispatching
                {
                    AppPermissions.Pages_Dashboard_Dispatching, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Dashboard Driver DOT Requirements
                {
                    AppPermissions.Pages_Dashboard_DriverDotRequirements, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Dashboard Truck Maintenance
                {
                    AppPermissions.Pages_Dashboard_TruckMaintenance, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Dashboard Truck Utilization
                {
                    AppPermissions.Pages_Dashboard_TruckUtilization, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },

                // Dispatches
                {
                    AppPermissions.Pages_Dispatches, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Dispatches_Edit
                {
                    AppPermissions.Pages_Dispatches_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                //Dispatches_SendSyncRequest
                {
                    AppPermissions.Pages_Dispatches_SendSyncRequest, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // DriverApplication
                {
                    AppPermissions.Pages_DriverApplication, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Driver,
                        StaticRoleNames.Tenants.LeaseHaulerDriver,
                    }
                },

                // DriverApplication_Settings
                {
                    AppPermissions.Pages_DriverApplication_Settings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // DriverApplication_ReactNativeDriverApp
                {
                    AppPermissions.Pages_DriverApplication_ReactNativeDriverApp, new string[]
                    {
                        StaticRoleNames.Tenants.Driver,
                        StaticRoleNames.Tenants.LeaseHaulerDriver,
                    }
                },

                // DriverApplication_WebBasedDriverApp
                {
                    AppPermissions.Pages_DriverApplication_WebBasedDriverApp, new[]
                    {
                        StaticRoleNames.Tenants.Driver,
                        StaticRoleNames.Tenants.LeaseHaulerDriver,
                    }
                },

                // DriverAssignment
                {
                    AppPermissions.Pages_DriverAssignment, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // DriverProductionPay
                {
                    AppPermissions.DriverProductionPay, new string[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Backoffice_DriverPay
                {
                    AppPermissions.Pages_Backoffice_DriverPay, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Drivers
                {
                    AppPermissions.Pages_Drivers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // DriverMessages
                {
                    AppPermissions.Pages_DriverMessages, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                //Fuel View
                {
                    AppPermissions.Pages_FuelPurchases_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },
                //Fuel Edit
                {
                    AppPermissions.Pages_FuelPurchases_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Fuel Surcharge Calculations
                {
                    AppPermissions.Pages_FuelSurchargeCalculations_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Imports
                {
                    AppPermissions.Pages_Imports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_Customers
                {
                    AppPermissions.Pages_Imports_Customers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_Employees
                {
                    AppPermissions.Pages_Imports_Employees, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_FuelUsage
                {
                    AppPermissions.Pages_Imports_FuelUsage, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_IronSheepdogEarnings
                {
                    AppPermissions.Pages_Imports_Tickets_IronSheepdogEarnings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_Items
                {
                    AppPermissions.Pages_Imports_Items, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_LuckStoneEarnings
                {
                    AppPermissions.Pages_Imports_Tickets_LuckStoneEarnings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },
                // Imports_Trucks
                {
                    AppPermissions.Pages_Imports_Trucks, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_TruxEarnings
                {
                    AppPermissions.Pages_Imports_Tickets_TruxEarnings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_VehicleUsage
                {
                    AppPermissions.Pages_Imports_VehicleUsage, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_Vendors
                {
                    AppPermissions.Pages_Imports_Vendors, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // LeaseHaulerPortal
                {
                    AppPermissions.LeaseHaulerPortal, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Jobs_Accept
                {
                    AppPermissions.LeaseHaulerPortal_Jobs_Accept, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Jobs_Edit
                {
                    AppPermissions.LeaseHaulerPortal_Jobs_Edit, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Jobs_Reject
                {
                    AppPermissions.LeaseHaulerPortal_Jobs_Reject, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Jobs_View
                {
                    AppPermissions.LeaseHaulerPortal_Jobs_View, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_MyCompany
                {
                    AppPermissions.LeaseHaulerPortal_MyCompany, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_MyCompany_Contacts
                {
                    AppPermissions.LeaseHaulerPortal_MyCompany_Contacts, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_MyCompany_Drivers
                {
                    AppPermissions.LeaseHaulerPortal_MyCompany_Drivers, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_MyCompany_Insurance
                {
                    AppPermissions.LeaseHaulerPortal_MyCompany_Insurance, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_MyCompany_Profile
                {
                    AppPermissions.LeaseHaulerPortal_MyCompany_Profile, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_MyCompany_Trucks
                {
                    AppPermissions.LeaseHaulerPortal_MyCompany_Trucks, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Schedule
                {
                    AppPermissions.LeaseHaulerPortal_Schedule, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_SelectLists
                {
                    AppPermissions.LeaseHaulerPortal_SelectLists, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_SelectLists_Drivers
                {
                    AppPermissions.LeaseHaulerPortal_SelectLists_Drivers, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_SelectLists_Trucks
                {
                    AppPermissions.LeaseHaulerPortal_SelectLists_Trucks, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_SelectLists_Users
                {
                    AppPermissions.LeaseHaulerPortal_SelectLists_Users, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Tickets
                {
                    AppPermissions.LeaseHaulerPortal_Tickets, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_TicketsByDriver
                {
                    AppPermissions.LeaseHaulerPortal_TicketsByDriver, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },
                // LeaseHaulerPortal_Truck_Request
                {
                    AppPermissions.LeaseHaulerPortal_Truck_Request, new[]
                    {
                        StaticRoleNames.Tenants.LeaseHaulerAdministrator,
                        StaticRoleNames.Tenants.LeaseHaulerDispatcher,
                    }
                },

                // LeaseHaulerRequests
                {
                    AppPermissions.Pages_LeaseHaulerRequests, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },
                // LeaseHaulerRequests_Edit
                {
                    AppPermissions.Pages_LeaseHaulerRequests_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // LeaseHaulers
                {
                    AppPermissions.Pages_LeaseHauler, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // LeaseHaulers_Edit
                {
                    AppPermissions.Pages_LeaseHaulers_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // LeaseHaulerPerformance
                {
                    AppPermissions.Pages_LeaseHaulerPerformance, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // LeaseHaulerStatements
                {
                    AppPermissions.Pages_LeaseHaulerStatements, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                //Misc
                {
                    AppPermissions.Pages_Misc, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                //Misc - ReadItemPricing
                {
                    AppPermissions.Pages_Misc_ReadItemPricing, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists
                {
                    AppPermissions.Pages_Misc_SelectLists, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                //Misc - SelectLists - CannedTexts
                {
                    AppPermissions.Pages_Misc_SelectLists_CannedTexts, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - Customers
                {
                    AppPermissions.Pages_Misc_SelectLists_Customers, new[]
                    {
                        //everyone who has Orders, Quotes, Settings, or Invoices permissions
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - Drivers
                {
                    AppPermissions.Pages_Misc_SelectLists_Drivers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                //Misc - SelectLists - FuelSurchargeCalculations
                {
                    AppPermissions.Pages_Misc_SelectLists_FuelSurchargeCalculations, new[]
                    {
                        //everyone who has Orders or Quotes permissions
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - Items
                {
                    AppPermissions.Pages_Misc_SelectLists_Items, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                //Misc - SelectLists - LeaseHaulers
                {
                    AppPermissions.Pages_Misc_SelectLists_LeaseHaulers, new[]
                    {
                        //at least everyone who has TimeEntry permissions
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                //Misc - SelectLists - Locations
                {
                    AppPermissions.Pages_Misc_SelectLists_Locations, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                //Misc - SelectLists - Locations - InlineCreation
                {
                    AppPermissions.Pages_Misc_SelectLists_Locations_InlineCreation, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - PricingTiers
                {
                    AppPermissions.Pages_Misc_SelectLists_PricingTiers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - QuoteSalesreps
                {
                    AppPermissions.Pages_Misc_SelectLists_QuoteSalesreps, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - TimeClassifications
                {
                    AppPermissions.Pages_Misc_SelectLists_TimeClassifications, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                //Misc - SelectLists - Trucks
                {
                    AppPermissions.Pages_Misc_SelectLists_Trucks, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                        StaticRoleNames.Tenants.Customer,
                    }
                },

                //Misc - SelectLists - Users
                {
                    AppPermissions.Pages_Misc_SelectLists_Users, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },

                //Misc - SelectLists - VehicleServices
                {
                    AppPermissions.Pages_Misc_SelectLists_VehicleServices, new[]
                    {

                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // Offices
                {
                    AppPermissions.Pages_Offices, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // OfficeAccess All
                {
                    AppPermissions.Pages_OfficeAccess_All, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // OfficeAccess UserOnly
                {
                    AppPermissions.Pages_OfficeAccess_UserOnly, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // OrderId Dropdown
                {
                    AppPermissions.Pages_Orders_IdDropdown, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Orders View
                {
                    AppPermissions.Pages_Orders_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Orders Edit
                {
                    AppPermissions.Pages_Orders_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Orders JobSummary View
                {
                    AppPermissions.Pages_Orders_ViewJobSummary, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // PreventiveMaintenanceSchedule_View
                {
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // PreventiveMaintenanceSchedule_Edit
                {
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // PrintOrders
                {
                    AppPermissions.Pages_PrintOrders, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Products/Services
                {
                    AppPermissions.Pages_Items, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Products/Services_HaulZones
                {
                    AppPermissions.Pages_Items_HaulZones, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Products/Services_Merge
                {
                    AppPermissions.Pages_Items_Merge, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Products/Services_PricingTiers
                {
                    AppPermissions.Pages_Items_PricingTiers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Quotes_View
                {
                    AppPermissions.Pages_Quotes_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Quotes_Edit
                {
                    AppPermissions.Pages_Quotes_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Quotes_Items_Create
                {
                    AppPermissions.Pages_Quotes_Items_Create, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // ReceiveRnConflicts
                {
                    AppPermissions.ReceiveRnConflicts, new[]
                    {
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Reports
                {
                    AppPermissions.Pages_Reports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Reports_DriverActivityDetail
                {
                    AppPermissions.Pages_Reports_DriverActivityDetail, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Reports_OutOfServiceTrucks
                {
                    AppPermissions.Pages_Reports_OutOfServiceTrucks, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Reports_Receipts
                {
                    AppPermissions.Pages_Reports_Receipts, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Reports_RevenueBreakdown
                {
                    AppPermissions.Pages_Reports_RevenueBreakdown, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Reports_RevenueBreakdownByTruck
                {
                    AppPermissions.Pages_Reports_RevenueBreakdownByTruck, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Reports_ScheduledReports
                {
                    AppPermissions.Pages_Reports_ScheduledReports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_PaymentReconciliation
                {
                    AppPermissions.Pages_Reports_PaymentReconciliation, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },

                // Reports_RevenueAnalysis
                {
                    AppPermissions.Pages_Reports_RevenueAnalysis, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Reports_VehicleWorkOrderCost
                {
                    AppPermissions.Pages_Reports_VehicleWorkOrderCost, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Reports_JobsMissingTickets
                {
                    AppPermissions.Pages_Reports_JobsMissingTickets, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Schedule
                {
                    AppPermissions.Pages_Schedule, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // SendOrdersToDrivers
                {
                    AppPermissions.Pages_SendOrdersToDrivers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // SwaggerAccess
                {
                    AppPermissions.Pages_SwaggerAccess, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },

                // Locations
                {
                    AppPermissions.Pages_Locations, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Locations_Merge
                {
                    AppPermissions.Pages_Locations_Merge, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // TaxRates
                {
                    AppPermissions.Pages_Items_TaxRates, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // TaxRates_Edit
                {
                    AppPermissions.Pages_Items_TaxRates_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                //TempFiles
                {
                    AppPermissions.Pages_TempFiles, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Customer,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Tickets by Driver
                {
                    AppPermissions.Pages_TicketsByDriver, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Tickets_View
                {
                    AppPermissions.Pages_Tickets_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Tickets_Edit
                {
                    AppPermissions.Pages_Tickets_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Tickets_Export
                {
                    AppPermissions.Pages_Tickets_Export, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Tickets_Download
                {
                    AppPermissions.Pages_Tickets_Download, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },


                // TimeOff
                {
                    AppPermissions.Pages_TimeOff, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },
                // TimeEntry
                {
                    AppPermissions.Pages_TimeEntry, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // TimeEntry_EditAll
                {
                    AppPermissions.Pages_TimeEntry_EditAll, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // TimeEntry_EditTimeClassifications
                {
                    AppPermissions.Pages_TimeEntry_EditTimeClassifications, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // TimeEntry_EditPersonal
                {
                    AppPermissions.Pages_TimeEntry_EditPersonal, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // TimeEntry_ViewOnly
                {
                    AppPermissions.Pages_TimeEntry_ViewOnly, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                //TimeClock
                {
                    AppPermissions.TimeClock, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Invoices
                {
                    AppPermissions.Pages_Invoices, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        //StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        //StaticRoleNames.Tenants.Managers,
                    }
                },

                // Invoices_ApproveInvoices
                {
                    AppPermissions.Pages_Invoices_ApproveInvoices, new[]
                    {
                        StaticRoleNames.Tenants.Backoffice,
                    }
                },

                // Trucks
                {
                    AppPermissions.Pages_Trucks, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // Trucks
                {
                    AppPermissions.Pages_OutOfServiceHistory_Delete, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // VehicleService_View
                {
                    AppPermissions.Pages_VehicleService_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // VehicleService_Edit
                {
                    AppPermissions.Pages_VehicleService_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // VehicleUsage_View
                {
                    AppPermissions.Pages_VehicleUsages_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },
                // VehicleUsage_Edit
                {
                    AppPermissions.Pages_VehicleUsages_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },
                
                // VisibleToCustomersInChat
                {
                    AppPermissions.VisibleToCustomersInChat, new[]
                    {
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // WorkOrders_View
                {
                    AppPermissions.Pages_WorkOrders_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // WorkOrders_Edit
                {
                    AppPermissions.Pages_WorkOrders_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // WorkOrders_EditLimited
                {
                    AppPermissions.Pages_WorkOrders_EditLimited, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
            };

        public static string[] DefaultPermissions => _defaultRolePermissions.Keys.ToArray();
        public static bool IsPermissionsGrantedToRole(string roleName, string permissionName)
        {
            if (!_defaultRolePermissions.ContainsKey(permissionName))
            {
                return false;
            }
            return _defaultRolePermissions[permissionName].Contains(roleName);
        }

        public static IEnumerable<string> GetRolePermissions(string roleName)
        {
            foreach (string permission in DefaultPermissions)
            {
                if (IsPermissionsGrantedToRole(roleName, permission))
                {
                    yield return permission;
                }
            }
        }

        public static string[] GetRoleNamesHavingDefaultPermission(string permissionName)
        {
            if (_defaultRolePermissions.TryGetValue(permissionName, out var result))
            {
                return result;
            }
            return Array.Empty<string>();
        }
    }
}
