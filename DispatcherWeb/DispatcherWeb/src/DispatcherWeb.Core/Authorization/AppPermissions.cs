namespace DispatcherWeb.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        public static string[] ManualPermissionsList = new[]
        {
            AppPermissions.Pages_DriverApplication_ReactNativeDriverApp,
            AppPermissions.Pages_DriverApplication_WebBasedDriverApp,
        };

        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)
        public const string Pages = "Pages";

        // public const string Pages_DemoUiComponents= "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";

        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
        public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";
        public const string Pages_Administration_Users_Unlock = "Pages.Administration.Users.Unlock";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeDefaultLanguage = "Pages.Administration.Languages.ChangeDefaultLanguage";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";
        public const string Pages_Administration_AuditLogs_ViewAllTenants = "Pages.Administration.AuditLogs.ViewAllTenants";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";
        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree = "Pages.Administration.OrganizationUnits.ManageOrganizationTree";
        public const string Pages_Administration_OrganizationUnits_ManageMembers = "Pages.Administration.OrganizationUnits.ManageMembers";
        public const string Pages_Administration_OrganizationUnits_ManageRoles = "Pages.Administration.OrganizationUnits.ManageRoles";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_WebhookSubscription = "Pages.Administration.WebhookSubscription";
        public const string Pages_Administration_WebhookSubscription_Create = "Pages.Administration.WebhookSubscription.Create";
        public const string Pages_Administration_WebhookSubscription_Edit = "Pages.Administration.WebhookSubscription.Edit";
        public const string Pages_Administration_WebhookSubscription_ChangeActivity = "Pages.Administration.WebhookSubscription.ChangeActivity";
        public const string Pages_Administration_WebhookSubscription_Detail = "Pages.Administration.WebhookSubscription.Detail";
        public const string Pages_Administration_Webhook_ListSendAttempts = "Pages.Administration.Webhook.ListSendAttempts";
        public const string Pages_Administration_Webhook_ResendWebhook = "Pages.Administration.Webhook.ResendWebhook";

        public const string Pages_Administration_DynamicProperties = "Pages.Administration.DynamicProperties";
        public const string Pages_Administration_DynamicProperties_Create = "Pages.Administration.DynamicProperties.Create";
        public const string Pages_Administration_DynamicProperties_Edit = "Pages.Administration.DynamicProperties.Edit";
        public const string Pages_Administration_DynamicProperties_Delete = "Pages.Administration.DynamicProperties.Delete";

        public const string Pages_Administration_DynamicPropertyValue = "Pages.Administration.DynamicPropertyValue";
        public const string Pages_Administration_DynamicPropertyValue_Create = "Pages.Administration.DynamicPropertyValue.Create";
        public const string Pages_Administration_DynamicPropertyValue_Edit = "Pages.Administration.DynamicPropertyValue.Edit";
        public const string Pages_Administration_DynamicPropertyValue_Delete = "Pages.Administration.DynamicPropertyValue.Delete";

        public const string Pages_Administration_DynamicEntityProperties = "Pages.Administration.DynamicEntityProperties";
        public const string Pages_Administration_DynamicEntityProperties_Create = "Pages.Administration.DynamicEntityProperties.Create";
        public const string Pages_Administration_DynamicEntityProperties_Edit = "Pages.Administration.DynamicEntityProperties.Edit";
        public const string Pages_Administration_DynamicEntityProperties_Delete = "Pages.Administration.DynamicEntityProperties.Delete";

        public const string Pages_Administration_DynamicEntityPropertyValue = "Pages.Administration.DynamicEntityPropertyValue";
        public const string Pages_Administration_DynamicEntityPropertyValue_Create = "Pages.Administration.DynamicEntityPropertyValue.Create";
        public const string Pages_Administration_DynamicEntityPropertyValue_Edit = "Pages.Administration.DynamicEntityPropertyValue.Edit";
        public const string Pages_Administration_DynamicEntityPropertyValue_Delete = "Pages.Administration.DynamicEntityPropertyValue.Delete";

        //TENANT-SPECIFIC PERMISSIONS
        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";
        public const string Pages_Dashboard = "Pages.Dashboard";
        public const string Pages_Dashboard_Dispatching = "Pages.Dashboard.Dispatching";
        public const string Pages_Dashboard_DriverDotRequirements = "Pages.Dashboard.DriverDotRequirements";
        public const string Pages_Dashboard_TruckMaintenance = "Pages.Dashboard.TruckMaintenance";
        public const string Pages_Dashboard_Revenue = "Pages.Dashboard.Revenue";
        public const string Pages_Dashboard_TruckUtilization = "Pages.Dashboard.TruckUtilization";
        public const string Pages_Orders_View = "Pages.Orders.View"; //read only
        public const string Pages_Orders_Edit = "Pages.Orders.Edit"; //create, edit, delete
        public const string Pages_Orders_IdDropdown = "Pages.Orders.IdDropdown";
        public const string Pages_Orders_ViewJobSummary = "Pages.Orders.ViewJobSummary";
        public const string Pages_Orders_EditQuotedValues = "Pages.Orders.EditQuotedValues";
        public const string Pages_Schedule = "Pages.Schedule"; //full
        public const string Pages_PrintOrders = "Pages.PrintOrders";
        public const string Pages_SendOrdersToDrivers = "Pages.SendOrdersToDrivers";
        public const string Pages_DriverAssignment = "Pages.DriverAssignment";
        public const string Pages_LeaseHauler = "Pages.LeaseHauler"; //todo end with "s" for consistency
        public const string Pages_LeaseHaulers_SyncWithFulcrum = "Pages.LeaseHaulers.SyncWithFulcrum";
        public const string Pages_LeaseHaulers_Edit = "Pages.LeaseHaulers.Edit";
        public const string Pages_LeaseHaulers_SetHaulingCompanyTenantId = "Pages.LeaseHaulers.SetHaulingCompanyTenantId";
        public const string Pages_LeaseHaulerStatements = "Pages.LeaseHaulers.CreateStatements";
        public const string Pages_LeaseHaulerRequests = "Pages.LeaseHaulerRequests";
        public const string Pages_LeaseHaulerRequests_Edit = "Pages.LeaseHaulerRequests.Edit";
        public const string Pages_LeaseHaulerPerformance = "Pages.LeaseHaulerPerformance";
        public const string Pages_Trucks = "Pages.Trucks";
        public const string Pages_Trucks_SyncWithFulcrum = "Pages.Trucks.SyncWithFulcrum";
        public const string Pages_OutOfServiceHistory_Delete = "Pages.OutOfServiceHistory.Delete";
        public const string Pages_Customers = "Pages.Customers";
        public const string Pages_Customers_SyncWithFulcrum = "Pages.Customers.SyncWithFulcrum";
        public const string Pages_Customers_Merge = "Pages.Customers.Merge";
        public const string Pages_Items = "Pages.Items";
        public const string Pages_Items_HaulZones = "Pages.Items.HaulZones";
        public const string Pages_Items_Merge = "Pages.Items.Merge";
        public const string Pages_Items_PricingTiers = "Pages.Items.PricingTiers";
        public const string Pages_Items_PricingTiers_EditPricingTier = "Pages.Items.PricingTiers.EditPricingTier";
        public const string Pages_Items_TaxRates = "Pages.Items.TaxRates";
        public const string Pages_Items_TaxRates_SyncWithFulcrum = "Pages.Items.TaxRates.SyncWithFulcrum";
        public const string Pages_Items_TaxRates_Edit = "Pages.Items.TaxRates.Edit";
        public const string Pages_Items_SyncWithFulcrum = "Pages.Items.SyncWithFulcrum";
        public const string Pages_Drivers = "Pages.Drivers";
        public const string Pages_Drivers_SyncWithFulcrum = "Pages.Drivers.SyncWithFulcrum";
        public const string Pages_Locations = "Pages.Locations";
        public const string Pages_Locations_Merge = "Pages.Locations.Merge";
        public const string Pages_Quotes_View = "Pages.Quotes.View"; //read only (including read of Quote items)
        public const string Pages_Quotes_Edit = "Pages.Quotes.Edit"; //create, edit, delete quotes (inluding create, edit, delete Quote Items). Includes Pages_Quotes_Items_Create
        public const string Pages_Quotes_Items_Create = "Pages.Quotes.Items.Create"; //create quote items only
        public const string Pages_CannedText = "Pages.CannedText";
        public const string Pages_Charges = "Pages.Charges";
        public const string Pages_CounterSales = "Pages.CounterSales";
        public const string Pages_Offices = "Pages.Offices";
        public const string Pages_Tickets_View = "Pages.Tickets.View"; //read only
        public const string Pages_Tickets_Edit = "Pages.Tickets.Edit"; //create, edit, delete
        public const string Pages_Tickets_Export = "Pages.Tickets.Export";
        public const string Pages_Tickets_Download = "Pages.Tickets.Download";
        public const string Pages_TicketsByDriver = "Pages.TicketsByDriver";
        public const string Pages_TicketsByDriver_EditTicketsOnInvoicesOrPayStatements = "Pages.TicketsByDriver.EditTicketsOnInvoicesOrPayStatements";
        public const string Pages_Invoices = "Pages.Invoices";
        public const string Pages_Invoices_ApproveInvoices = "Pages.Invoices.ApproveInvoices";
        public const string DriverProductionPay = "DriverProductionPay";
        public const string CanBeSalesperson = "CanBeSalesperson";
        public const string VisibleToCustomersInChat = "VisibleToCustomersInChat";
        public const string ReceiveRnConflicts = "ReceiveRnConflicts";
        public const string DebugDriverApp = "DebugDriverApp";

        public const string Pages_VehicleService_View = "Pages.VehicleService.View";
        public const string Pages_VehicleService_Edit = "Pages.VehicleService.Edit";
        public const string Pages_PreventiveMaintenanceSchedule_View = "Pages.PreventiveMaintenanceSchedule.View";
        public const string Pages_PreventiveMaintenanceSchedule_Edit = "Pages.PreventiveMaintenanceSchedule.Edit";
        public const string Pages_WorkOrders_View = "Pages.WorkOrders.View";
        public const string Pages_WorkOrders_Edit = "Pages.WorkOrders.Edit";
        public const string Pages_WorkOrders_EditLimited = "Pages.WorkOrders.EditLimited";
        public const string Pages_FuelPurchases_View = "Pages.FuelPurchases.View";
        public const string Pages_FuelPurchases_Edit = "Pages.FuelPurchases.Edit";
        public const string Pages_VehicleUsages_View = "Pages.VehicleUsages.View";
        public const string Pages_VehicleUsages_Edit = "Pages.VehicleUsages.Edit";

        public const string Pages_Reports = "Pages.Reports";
        public const string Pages_Reports_ScheduledReports = "Pages.Reports.ScheduledReports";
        public const string Pages_Reports_OutOfServiceTrucks = "Pages.Reports.OutOfServiceTrucks";
        public const string Pages_Reports_RevenueBreakdown = "Pages.Reports.RevenueBreakdown";
        public const string Pages_Reports_RevenueBreakdownByTruck = "Pages.Reports.RevenueBreakdownByTruck";
        public const string Pages_Reports_Receipts = "Pages.Reports.Receipts";
        public const string Pages_Reports_PaymentReconciliation = "Pages.Reports.PaymentReconciliation";
        public const string Pages_Reports_DriverActivityDetail = "Pages.Reports.DriverActivityDetail";
        public const string Pages_Reports_RevenueAnalysis = "Pages.Reports.RevenueAnalysis";
        public const string Pages_Reports_VehicleWorkOrderCost = "Pages.Reports.VehicleWorkOrderCost";
        public const string Pages_Reports_JobsMissingTickets = "Pages.Reports.JobsMissingTickets";

        public const string Pages_ActiveReports = "Pages.ActiveReports";
        public const string Pages_ActiveReports_TenantStatisticsReport = "Pages.ActiveReports.TenantStatisticsReport";
        public const string Pages_ActiveReports_VehicleMaintenanceWorkOrderReport = "Pages.ActiveReports.VehicleMaintenanceWorkOrderReport";

        public const string Pages_Imports = "Pages.Imports";
        public const string Pages_Imports_FuelUsage = "Pages.Imports.FuelUsage";
        public const string Pages_Imports_VehicleUsage = "Pages.Imports.VehicleUsage";
        public const string Pages_Imports_Customers = "Pages.Imports.Customer";
        public const string Pages_Imports_Trucks = "Pages.Imports.Trucks";
        public const string Pages_Imports_Vendors = "Pages.Imports.Vendors";
        public const string Pages_Imports_Items = "Pages.Imports.Items";
        public const string Pages_Imports_Employees = "Pages.Imports.Employees";
        public const string Pages_Imports_Tickets_LuckStoneEarnings = "AllowImportingLuckStoneEarnings";
        public const string Pages_Imports_Tickets_TruxEarnings = "AllowImportingTruxEarnings";
        public const string Pages_Imports_Tickets_IronSheepdogEarnings = "AllowImportingIronSheepdogEarnings";

        public const string Pages_OfficeAccess_All = "Pages.OfficeAccess.All";
        public const string Pages_OfficeAccess_UserOnly = "Pages.OfficeAccess.UserOnly";

        public const string Pages_DriverMessages = "Pages.DriverMessages";
        public const string Pages_DriverApplication = "Pages.DriverApplication";
        public const string Pages_DriverApplication_WebBasedDriverApp = "Pages.DriverApplication.WebBasedDriverApp";
        public const string Pages_DriverApplication_ReactNativeDriverApp = "Pages.DriverApplication.ReactNativeDriverApp";
        public const string Pages_DriverApplication_Settings = "Pages.DriverApplication.Settings";
        public const string Pages_Dispatches = "Pages.Dispatches";
        public const string Pages_Dispatches_Edit = "Pages.Dispatches.Edit";
        public const string Pages_Dispatches_SendSyncRequest = "Pages.Dispatches.SendSyncRequest";
        public const string Pages_Dispatches_ShowRemoveDispatchesButton = "Pages.Dispatches.ShowRemoveDispatchesButton";

        public const string Pages_TimeEntry = "Pages.TimeEntry";
        public const string Pages_TimeEntry_EditAll = "Pages.TimeEntry.EditAll";
        public const string Pages_TimeEntry_EditPersonal = "Pages.TimeEntry.EditPersonal";
        public const string Pages_TimeEntry_ViewOnly = "Pages.TimeEntry.ViewOnly";
        public const string Pages_TimeEntry_EditTimeClassifications = "Pages.TimeEntry.EditTimeClassifications";

        public const string Pages_TimeOff = "Pages.TimeOff";

        public const string Pages_Backoffice_DriverPay = "Pages.Backoffice.DriverPay";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";
        public const string Pages_FuelSurchargeCalculations_Edit = "Pages.FuelSurchargeCalculations.Edit";
        public const string Pages_TempFiles = "Pages.TempFiles";

        public const string Pages_Misc = "Pages.Misc";
        public const string Pages_Misc_ReadItemPricing = "Pages.Misc.ReadItemPricing";
        public const string Pages_Misc_SelectLists = "Pages.Misc.SelectLists";
        public const string Pages_Misc_SelectLists_CannedTexts = "Pages.Misc.SelectLists.CannedTexts";
        public const string Pages_Misc_SelectLists_Customers = "Pages.Misc.SelectLists.Customers";
        public const string Pages_Misc_SelectLists_Drivers = "Pages.Misc.SelectLists.Drivers";
        public const string Pages_Misc_SelectLists_FuelSurchargeCalculations = "Pages.Misc.SelectLists.FuelSurchargeCalculations";
        public const string Pages_Misc_SelectLists_LeaseHaulers = "Pages.Misc.SelectLists.LeaseHaulers";
        public const string Pages_Misc_SelectLists_Locations = "Pages.Misc.SelectLists.Locations";
        public const string Pages_Misc_SelectLists_Locations_InlineCreation = "Pages.Misc.SelectLists.Locations.InlineCreation";
        public const string Pages_Misc_SelectLists_PricingTiers = "Pages.Misc.SelectLists.PricingTiers";
        public const string Pages_Misc_SelectLists_QuoteSalesreps = "Pages.Misc.SelectLists.QuoteSalesreps";
        public const string Pages_Misc_SelectLists_Items = "Pages.Misc.SelectLists.Items";
        public const string Pages_Misc_SelectLists_TimeClassifications = "Pages.Misc.SelectLists.TimeClassifications";
        public const string Pages_Misc_SelectLists_Trucks = "Pages.Misc.SelectLists.Trucks";
        public const string Pages_Misc_SelectLists_Users = "Pages.Misc.SelectLists.Users";
        public const string Pages_Misc_SelectLists_VehicleServices = "Pages.Misc.SelectLists.VehicleServices";

        public const string Pages_Administration_Tenant_SubscriptionManagement = "Pages.Administration.Tenant.SubscriptionManagement";

        public const string TimeClock = "TimeClock";

        //Customer Portal Specific Permissions

        public const string CustomerPortal = "CustomerPortal";
        public const string CustomerPortal_Invoices = "CustomerPortal.Invoices";
        public const string CustomerPortal_SelectLists = "CustomerPortal.SelectLists";
        public const string CustomerPortal_SelectLists_Users = "CustomerPortal.SelectLists.Users";
        public const string CustomerPortal_TicketList = "CustomerPortal.TicketList";
        public const string CustomerPortal_TicketList_Export = "CustomerPortal.TicketList.Export";
        public const string CustomerPortal_Orders_IdDropdown = "CustomerPortal.Orders.IdDropdown";

        //Lease Hauler Portal Specific Permissions

        public const string LeaseHaulerPortal = "LeaseHaulerPortal";
        public const string LeaseHaulerPortal_Jobs_Accept = "LeaseHaulerPortal.Jobs.Accept";
        public const string LeaseHaulerPortal_Jobs_Edit = "LeaseHaulerPortal.Jobs.Edit";
        public const string LeaseHaulerPortal_Jobs_Reject = "LeaseHaulerPortal.Jobs.Reject";
        public const string LeaseHaulerPortal_Jobs_View = "LeaseHaulerPortal.Jobs.View";
        public const string LeaseHaulerPortal_MyCompany = "LeaseHaulerPortal.MyCompany";
        public const string LeaseHaulerPortal_MyCompany_Contacts = "LeaseHaulerPortal.MyCompany.Contacts";
        public const string LeaseHaulerPortal_MyCompany_Drivers = "LeaseHaulerPortal.MyCompany.Drivers";
        public const string LeaseHaulerPortal_MyCompany_Insurance = "LeaseHaulerPortal.MyCompany.Insurance";
        public const string LeaseHaulerPortal_MyCompany_Profile = "LeaseHaulerPortal.MyCompany.Profile";
        public const string LeaseHaulerPortal_MyCompany_Trucks = "LeaseHaulerPortal.MyCompany.Trucks";
        public const string LeaseHaulerPortal_Schedule = "LeaseHaulerPortal.Schedule";
        public const string LeaseHaulerPortal_SelectLists = "LeaseHaulerPortal.SelectLists";
        public const string LeaseHaulerPortal_SelectLists_Drivers = "LeaseHaulerPortal.SelectLists.Drivers";
        public const string LeaseHaulerPortal_SelectLists_Trucks = "LeaseHaulerPortal.SelectLists.Trucks";
        public const string LeaseHaulerPortal_SelectLists_Users = "LeaseHaulerPortal.SelectLists.Users";
        public const string LeaseHaulerPortal_Tickets = "LeaseHaulerPortal.Tickets";
        public const string LeaseHaulerPortal_TicketsByDriver = "LeaseHaulerPortal.TicketsByDriver";
        public const string LeaseHaulerPortal_Truck_Request = "LeaseHaulerPortal.Truck.Request";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";
        public const string Pages_Editions_MoveTenantsToAnotherEdition = "Pages.Editions.MoveTenantsToAnotherEdition";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";
        public const string Pages_Tenants_AddMonthToDriver = "Pages.Tenants.AddMonthToDriver";
        public const string Pages_Tenants_AddDemoData = "Pages.Tenants.AddDemoData";
        public const string Pages_Tenants_DeleteDispatchData = "Pages.Tenants.DeleteDispatchData";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";

        public const string Pages_VehicleServiceTypes_View = "Pages.VehicleServiceTypes.View";
        public const string Pages_VehicleServiceTypes_Edit = "Pages.VehicleServiceTypes.Edit";

        public const string Pages_VehicleCategories = "Pages.VehicleCategories";

        public const string Pages_HostEmails = "Pages.HostEmails";
        public const string Pages_HostEmails_Send = "Pages.HostEmails.Send";

        public const string Pages_CustomerNotifications = "Pages.CustomerNotifications";
        public const string Pages_CustomerNotifications_Edit = "Pages.CustomerNotifications.Edit";

        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";

        public const string Pages_SwaggerAccess = "Pages.SwaggerAccess";
    }
}
