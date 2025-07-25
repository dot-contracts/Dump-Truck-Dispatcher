namespace DispatcherWeb.Web.Areas.App.Startup
{
    public static class AppPageTitles
    {
        public static string GetDetailName(string pageName, string entityName)
        {
            return $"{pageName} {entityName}";
        }

        public static class AuditLogs
        {
            public const string Index = "Audit Logs";
        }

        public static class CannedTexts
        {
            public const string Index = "Canned Texts";
        }

        public static class Customers
        {
            public const string Index = "Customers";
        }

        public static class Dashboard
        {
            public const string Index = "Dashboard";
        }

        public static class DriverAssignment
        {
            public const string Index = "Driver Assignments";
        }

        public static class Drivers
        {
            public const string Index = "Drivers";
        }

        public static class DriverMessages
        {
            public const string Index = "Messages";
        }

        public static class Editions
        {
            public const string Index = "Editions";
        }

        public static class HostSettings
        {
            public const string Index = "Settings";
        }

        public static class LeaseHaulers
        {
            public const string Index = "Lease Haulers";
        }

        public static class LeaseHaulerRequests
        {
            public const string AvailableTrucks = "Available Trucks";
            public const string TruckStatus = "TruckStatus";
        }

        public static class Maintenance
        {
            public const string Index = "Maintenance";
        }

        public static class Offices
        {
            public const string Index = "Offices";
        }

        public static class LeaseHaulerReconciliation
        {
            public const string Index = "Lease Hauler Reconciliation";
        }

        public static class Orders
        {
            public const string Index = "Orders";
            public const string Detail = "Order";
            public const string Receipts = "Receipts";
        }

        public static class OrganizationUnits
        {
            public const string Index = "Organization Units";
        }

        public static class OutOfSeriveTrucks
        {
            public const string Index = "Out of Service Trucks";
        }

        public static class PreventiveMaintenanceSchedule
        {
            public const string Index = "Preventive Maintenance Schedule";
        }

        public static class QuoteHistory
        {
            public const string Index = "Quote History";
        }

        public static class Quotes
        {
            public const string Index = "Quotes";
            public const string Detail = "Quote";
        }

        public static class Roles
        {
            public const string Index = "Roles";
        }

        public static class ScheduledReports
        {
            public const string Index = "Scheduled Reports";
        }

        public static class Schedule
        {
            public const string Index = "Schedule";
        }

        public static class Services
        {
            public const string Index = "Services";
        }

        public static class TenantSettings
        {
            public const string Index = "Settings";
        }

        public static class Locations
        {
            public const string Index = "Locations";
        }

        public static class Tenants
        {
            public const string Index = "Tenants";
        }

        public static class Tickets
        {
            public const string Index = "Tickets";
        }

        public static class Trucks
        {
            public const string Index = "Trucks";
        }

        public static class Users
        {
            public const string Index = "Users";
        }

        public static class VehicleIssues
        {
            public const string Index = "Issue List";
        }

        public static class VehicleServices
        {
            public const string Index = "Service List";
        }

        public static class ServiceType
        {
            public const string Index = "Service Types";
        }

        public static class WorkOrders
        {
            public const string Index = "Work Orders";
            public const string Detail = "Work Order";
        }


    }
}
