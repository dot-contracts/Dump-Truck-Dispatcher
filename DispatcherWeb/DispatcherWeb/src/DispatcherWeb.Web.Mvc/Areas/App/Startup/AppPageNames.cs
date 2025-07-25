namespace DispatcherWeb.Web.Areas.App.Startup
{
    public class AppPageNames
    {
        public static class Common
        {
            public const string Administration = "Administration";
            public const string Roles = "Administration.Roles";
            public const string Users = "Administration.Users";
            public const string AuditLogs = "Administration.AuditLogs";
            public const string OrganizationUnits = "Administration.OrganizationUnits";
            public const string Languages = "Administration.Languages";
        }

        public static class Host
        {
            public const string Tenants = "Tenants";
            public const string Editions = "Editions";
            public const string VehicleCategories = "VehicleCategories";
            public const string HostEmails = "HostEmails";
            public const string CustomerNotifications = "CustomerNotifications";
            public const string Maintenance = "Administration.Maintenance";
            public const string Settings = "Administration.Settings.Host";
            public const string Dashboard = "Dashboard";
            public const string ServiceType = "VehicleMaintenance.ServiceType";
            public const string DemoUiComponents = "DemoUiComponents";
        }

        public static class Tenant
        {
            public const string Dashboard = "Dashboard.Tenant";
            public const string Settings = "Administration.Settings.Tenant";
            public const string SubscriptionManagement = "Administration.SubscriptionManagement.Tenant";

            public const string CannedTexts = "Administration.CannedTexts";
            public const string Offices = "Administration.Offices";
            public const string Trucks = "Administration.Trucks";
            public const string ProductsOrServices = "Administration.ProductsOrServices";
            public const string Items = "Administration.Services";
            public const string PricingTiers = "Administration.PricingTiers";
            public const string TaxRates = "TaxRates";
            public const string HaulZones = "HaulZones";
            public const string Locations = "Administration.Locations";
            public const string Customers = "Administration.Customers";
            public const string Drivers = "Administration.Drivers";

            public const string Quotes = "Quotes";
            public const string QuoteHistory = "Quotes.QuoteHistory";

            public const string LeaseHauler = "LeaseHauler";
            public const string LeaseHaulers = "LeaseHauler.LeaseHaulers";
            public const string LeaseHaulerRequests = "LeaseHauler.LeaseHaulerRequests";
            public const string LeaseHaulerAvailableTrucks = "LeaseHauler.AvailableTrucks";
            public const string LeaseHaulerPerformance = "LeaseHauler.LeaseHaulerPerformance";

            public const string VehicleMaintenance = "VehicleMaintenance";
            public const string VehicleServices = "VehicleMaintenance.VehicleServices";
            public const string PreventiveMaintenanceSchedule = "VehicleMaintenance.PreventiveMaintenanceSchedule";
            public const string WorkOrders = "VehicleMaintenance.WorkOrders";
            public const string FuelPurchases = "VehicleMaintenance.FuelPurchases";
            public const string VehicleUsages = "VehicleMaintenance.VehicleUsages";

            public const string Time = "Time";
            public const string TimeEntry = "BackOffice.TimeEntry";
            public const string TimeOff = "BackOffice.TimeOff";
            public const string TimeClassifications = "BackOffice.TimeClassifications";

            public const string BackOffice = "BackOffice";
            public const string Receipts = "BackOffice.Receipts";
            public const string PaymentReconciliation = "BackOffice.PaymentReconciliation";
            public const string LeaseHaulerReconciliation = "BackOffice.LeaseHaulerReconciliation";
            public const string DriverPayStatements = "BackOffice.DriverPayStatements";
            public const string LeaseHaulerStatements = "BackOffice.LeaseHaulerStatements";

            public const string TicketsAndLoads = "TicketsAndLoads";
            public const string Tickets = "Orders.Tickets";
            public const string Dispatches = "Orders.Dispatches";

            public const string Orders = "Orders";
            public const string AddOrders = "Orders.AddOrders";
            public const string ViewOrders = "Orders.ViewOrders";
            public const string TicketsByDriver = "Orders.TicketsByDriver";
            public const string Invoices = "Orders.Invoices";
            public const string Schedule = "Orders.Schedule";
            public const string DriverAssignment = "Orders.DriverAssignment";
            public const string PrintOrders = "Orders.PrintOrders";
            public const string SendOrdersToDrivers = "Orders.SendOrdersToDrivers";
            public const string TruckDispatchList = "Orders.TruckDispatchList";
            public const string DriverMessages = "Orders.DriverMessages";

            public const string Reports = "Reports";
            public const string OutOfSeriveTrucks = "Reports.OutOfSeriveTrucks";
            public const string ScheduledReports = "Reports.ScheduledReports";
            public const string RevenueBreakdown = "Reports.RevenueBreakdown";
            public const string RevenueBreakdownByTruck = "Reports.RevenueBreakdownByTruck";
            public const string DriverActivityDetailReport = "Reports.DriverActivityDetail";
            public const string RevenueAnalysis = "Reports.RevenueAnalysis";
            public const string VehicleWorkOrderCost = "Reports.VehicleWorkOrderCost";
            public const string JobsMissingTickets = "Reports.JobsMissingTickets";

            public const string Imports = "Imports";
            public const string ImportFuelUsage = "Imports.FuelUsage";
            public const string ImportVehicleUsage = "Imports.VehicleUsage";
            public const string ImportCustomers = "Imports.Customers";
            public const string ImportTrucks = "Imports.Trucks";
            public const string ImportTrux = "Imports.Trux";
            public const string ImportLuckStone = "Imports.LuckStone";
            public const string ImportVendors = "Imports.Vendors";
            public const string ImportItems = "Imports.Items";
            public const string ImportEmployees = "Imports.Employees";

            public const string LeaseHaulerMyCompany = "LeaseHauler.Portal.MyCompany";
            public const string LeaseHaulerTruckRequest = "LeaseHauler.Truck.Request";
        }
    }
}
