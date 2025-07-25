namespace DispatcherWeb.Features
{
    public static class AppFeatures
    {
        public const string MaxUserCount = "App.MaxUserCount";
        public const string ChatFeature = "App.ChatFeature";
        public const string TenantToTenantChatFeature = "App.ChatFeature.TenantToTenant";
        public const string TenantToHostChatFeature = "App.ChatFeature.TenantToHost";

        public const string AllowMultiOfficeFeature = "App.AllowMultiOfficeFeature";
        public const string NumberOfTrucksFeature = "App.NumberOfTrucksFeature";
        public const string AllowPaymentProcessingFeature = "App.AllowPaymentProcessingFeature";
        public const string AllowLeaseHaulersFeature = "App.AllowLeaseHaulersFeature";
        public const string AllowInvoicingFeature = "App.AllowInvoicingFeature";
        public const string AllowInvoiceApprovalFlow = "App.AllowInvoiceApprovalFlow";
        public const string AllowImportingTruxEarnings = "App.AllowImportingTruxEarnings";
        public const string AllowImportingLuckStoneEarnings = "App.AllowImportingLuckStoneEarnings";
        public const string AllowImportingIronSheepdogEarnings = "App.AllowImportingIronSheepdogEarnings";
        public const string AllowSendingOrdersToDifferentTenant = "App.AllowSendingOrdersToDifferentTenant";
        public const string DriverProductionPayFeature = "App.DriverProductionPayFeature";

        public const string GpsIntegrationFeature = "App.GpsIntegrationFeature";
        public const string FulcrumIntegration = "App.FulcrumIntegration";

        public const string SmsIntegrationFeature = "App.SmsIntegrationFeature";
        public const string DispatchingFeature = "App.DispatchingFeature";
        public const string QuickbooksFeature = "App.Quickbooks";
        public const string QuickbooksImportFeature = "App.QuickbooksImportFeature";

        public const string WebBasedDriverApp = "DriverApp.WebBasedDriverApp";
        public const string ReactNativeDriverApp = "DriverApp.ReactNativeDriverApp";
        public const string AllowGpsTracking = "DriverApp.ReactNativeDriverApp.AllowGpsTracking";
        public const string SendRnConflictsToUsers = "DriverApp.ReactNativeDriverApp.SendRnConflictsToUsers";

        public const string FreeFunctionality = "App.FreeFunctionalityFeature";
        public const string PaidFunctionality = "App.PaidFunctionalityFeature";

        public const string PricingTiers = "App.PricingTiersFeature";

        public const string TicketsFeature = "App.Tickets";
        public const string ConvertReceivedPdfTicketImagesToJpgBeforeStoring = "App.Tickets.ConvertReceivedPdfTicketImagesToJpgBeforeStoring";
        public const string PrintAlreadyUploadedPdfTicketImages = "App.Tickets.PrintAlreadyUploadedPdfTicketImages";
        public const string MaximumNumberOfTicketsPerDownload = "App.Tickets.MaximumNumberOfTicketsPerDownload";

        public const string CustomerPortal = "App.CustomerPortalFeature";
        public const string JobSummary = "App.JobSummary";
        public const string LeaseHaulerPortal = "App.LeaseHaulerPortalFeature";
        public const string LeaseHaulerPortalJobBasedLeaseHaulerRequest = "App.LeaseHaulerPortalJobBasedLeaseHaulerRequest";
        public const string LeaseHaulerPortalTicketsByDriver = "App.LeaseHaulerPortalTicketsByDriver";
        public const string LeaseHaulerPortalTruckRequest = "App.LeaseHaulerPortalTruckRequest";
        public const string LeaseHaulerPortalContacts = "App.LeaseHaulerPortal.Contacts";

        public const string PrivateLabel = "App.PrivateLabelFeature";
        public const string SeparateMaterialAndFreightItems = "App.SeparateMaterialAndFreightItems";
        public const string HaulZone = "App.HaulZone";

        public const string Charges = "App.Charges";
        public const string UseMaterialQuantity = "App.Charges.UseMaterialQuantity";

        public const string IncludeTravelTime = "App.IncludeTravelTime";
    }
}
