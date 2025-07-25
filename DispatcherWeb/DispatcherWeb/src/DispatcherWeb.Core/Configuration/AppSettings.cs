using System;

namespace DispatcherWeb.Configuration
{
    /// <summary>
    /// Defines string constants for setting names in the application.
    /// See <see cref="AppSettingProvider"/> for setting definitions.
    /// </summary>
    public static class AppSettings
    {
        public static class General
        {
            public const string WebSiteRootAddress = "App.General.WebSiteRootAddress";
            public const string CompanyName = "App.General.CompanyName";
            public const string DefaultMapLocationAddress = "App.General.DefaultMapLocationAddress";
            public const string DefaultMapLocation = "App.General.DefaultMapLocation";
            public const string CurrencySymbol = "App.General.CurrencySymbol";
            public const string UserDefinedField1 = "App.General.UserDefinedField1";
            public const string ValidateDriverAndTruckOnTickets = "App.General.ValidateDriverAndTruckOnTickets";
            public const string ShowDriverNamesOnPrintedOrder = "App.General.ShowDriverNamesOnPrintedOrder";
            public const string ShowLoadAtOnPrintedOrder = "App.General.ShowLoadAtOnPrintedOrder";
            public const string AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles = "App.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles";
            public const string AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices = "App.General.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices";
            public const string SplitBillingByOffices = "App.General.SplitBillingByOffices";
            public const string ShowOfficeOnTicketsByDriver = "App.General.ShowOfficeOnTicketsByDriver";
            public const string ShowAggregateCost = "App.General.ShowAggregateCost";
            public const string PaymentProcessor = "App.General.PaymentProcessor";
            public const string UseShifts = "App.General.UseShifts";
            public const string ShiftName1 = "App.General.ShiftName1";
            public const string ShiftName2 = "App.General.ShiftName2";
            public const string ShiftName3 = "App.General.ShiftName3";
            public const string AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders = "App.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders";
        }

        public static class DriverApp
        {
            public const string LogLevel = "App.DriverApp.LogLevel";
            public const string LocationTimeout = "App.DriverApp.LocationTimeout"; //ms
            public const string LocationMaxAge = "App.DriverApp.LocationMaxAge"; //ms
            public const string EnableLocationHighAccuracy = "App.DriverApp.EnableLocationHighAccuracy";
            public const string LoggingLevel = "App.DriverApp.LoggingLevel";
            public const string SyncDataOnButtonClicks = "App.DriverApp.SyncDataOnButtonClicks";
            public const string PeriodicSyncCheckIntervalSeconds = "App.DriverApp.PeriodicSyncCheckIntervalSeconds";
            public const string IsDriverSyncRequestStoreEnabled = "App.DriverApp.IsDriverSyncRequestStoreEnabled";
        }

        public static class SyncRequests
        {
            public const string DefaultFilter = "App.SyncRequests.DefaultFilter";
        }

        public static class Logging
        {
            public const string UploadTruckPositionsToWialonLogLevel = "App.Logging.UploadTruckPositionsToWialonLogLevel";
        }

        public static class Trux
        {
            public const string AllowImportingTruxEarnings = "App.General.AllowImportingTruxEarnings";
            public const string TruxCustomerId = "App.Trux.TruxCustomerId";
            public const string UseForProductionPay = "App.Trux.UseForProductionPay";
        }

        public static class LuckStone
        {
            public const string AllowImportingLuckStoneEarnings = "App.General.AllowImportingLuckStoneEarnings";
            public const string LuckStoneCustomerId = "App.LuckStone.LuckStoneCustomerId";
            public const string HaulerRef = "App.LuckStone.HaulerRef";
            public const string UseForProductionPay = "App.LuckStone.UseForProductionPay";
        }

        public static class IronSheepdog
        {
            public const string AllowImportingIronSheepdogEarnings = "App.General.AllowImportingIronSheepdogEarnings";
            public const string IronSheepdogCustomerId = "App.IronSheepdog.IronSheepdogCustomerId";
            public const string UseForProductionPay = "App.IronSheepdog.UseForProductionPay";
        }

        public static class HostManagement
        {
            public const string BillingLegalName = "App.HostManagement.BillingLegalName";
            public const string BillingAddress = "App.HostManagement.BillingAddress";
            public const string NotificationsEmail = "App.HostManagement.NotificationsEmail";
            public const string DriverAppImageResolution = "App.HostManagement.DriverAppImageResolution";
            public const string LinkToResourceCenter = "App.HostManagement.LinkToResourceCenter";
            public const string TrainingMeetingRequestLink = "App.HostManagement.TrainingMeetingRequestLink";
            public const string SupportRequestLink = "App.HostManagement.SupportRequestLink";
            public const string DefaultDtdTrackerDeviceTypeId = "App.HostManagement.DefaultDtdTrackerDeviceTypeId";
            public const string DefaultDtdTrackerDeviceTypeName = "App.HostManagement.DefaultDtdTrackerDeviceTypeName";
            public const string DefaultDtdTrackerServerAddress = "App.HostManagement.DefaultDtdTrackerServerAddress";
            public const string MinimumNativeAppVersion = "App.HostManagement.MinimumNativeAppVersion";
            public const string RecommendedNativeAppVersion = "App.HostManagement.RecommendedNativeAppVersion";
            public const string GooglePlayUrl = "App.HostManagement.GooglePlayUrl";
            public const string AppleStoreUrl = "App.HostManagement.AppleStoreUrl";
            public const string InvoicePrintLimit = "App.HostManagement.InvoicePrintLimit";
            public const string TempFileExpirationTime = "App.HostManagement.TempFileExpirationTime";
        }

        public static class DashboardCustomization
        {
            public const string Configuration = "App.DashboardCustomization.Configuration";
        }

        public static class Dashboard
        {
            public const string TrucksRequestedToday = "App.Dashboard.ScheduledTruckCounters.TrucksRequestedToday";
            public const string TrucksScheduledToday = "App.Dashboard.ScheduledTruckCounters.TrucksScheduledToday";
            public const string TrucksRequestedTomorrow = "App.Dashboard.ScheduledTruckCounters.TrucksRequestedTomorrow";
            public const string TrucksScheduledTomorrow = "App.Dashboard.ScheduledTruckCounters.TrucksScheduledTomorrow";
            public const string TruckAvailabilityChart = "App.Dashboard.TruckAvailabilityChart";
            public const string ServiceStatusChart = "App.Dashboard.ServiceStatusChart";
            public const string LicensePlateStatusChart = "App.Dashboard.LicensePlateStatusChart";
            public const string DriverLicenseStatusChart = "App.Dashboard.DriverLicenseStatusChart";
            public const string PhysicalStatusChart = "App.Dashboard.PhysicalStatusChart";
            public const string DriverMvrStatusChart = "App.Dashboard.DriverMvrStatusChart";
            public const string Revenue = "App.Dashboard.RevenueCharts.Revenue";
            public const string RevenuePerTruck = "App.Dashboard.RevenueCharts.RevenuePerTruck";
            public const string FuelCost = "App.Dashboard.RevenueCharts.FuelCost";
            public const string AdjustedRevenue = "App.Dashboard.RevenueCharts.AdjustedRevenue";
            public const string AverageAdjustedRevenuePerTruck = "App.Dashboard.RevenueCharts.AverageAdjustedRevenuePerTruck";
            public const string AverageFuelCostPerMile = "App.Dashboard.RevenueCharts.AverageFuelCostPerMile";
            public const string AverageRevenuePerHour = "App.Dashboard.RevenueCharts.AverageRevenuePerHour";
            public const string AverageRevenuePerMile = "App.Dashboard.RevenueCharts.AverageRevenuePerMile";
            public const string RevenuePerTruckByDateGraph = "App.Dashboard.RevenuePerTruckByDateGraph";
            public const string RevenueByDateGraph = "App.Dashboard.RevenueByDateGraph";
            public const string TruckUtilizationGraph = "App.Dashboard.TruckUtilizationGraph";
        }

        public static class TenantManagement
        {
            public const string AllowSelfRegistration = "App.TenantManagement.AllowSelfRegistration";
            public const string IsNewRegisteredTenantActiveByDefault = "App.TenantManagement.IsNewRegisteredTenantActiveByDefault";
            public const string UseCaptchaOnRegistration = "App.TenantManagement.UseCaptchaOnRegistration";
            public const string DefaultEdition = "App.TenantManagement.DefaultEdition";
            public const string SubscriptionExpireNotifyDayCount = "App.TenantManagement.SubscriptionExpireNotifyDayCount";
            public const string BillingLegalName = "App.TenantManagement.BillingLegalName";
            public const string BillingAddress = "App.TenantManagement.BillingAddress";
            public const string BillingPhoneNumber = "App.TenantManagement.BillingPhoneNumber";
            public const string BillingTaxVatNo = "App.TenantManagement.BillingTaxVatNo";
        }

        public static class UserManagement
        {
            public static class TwoFactorLogin
            {
                public const string IsGoogleAuthenticatorEnabled = "App.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled";
            }

            public static class SessionTimeOut
            {
                public const string IsEnabled = "App.UserManagement.SessionTimeOut.IsEnabled";
                public const string TimeOutSecond = "App.UserManagement.SessionTimeOut.TimeOutSecond";
                public const string ShowTimeOutNotificationSecond = "App.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond";
                public const string ShowLockScreenWhenTimedOut = "App.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut";
            }

            public const string AllowSelfRegistration = "App.UserManagement.AllowSelfRegistration";
            public const string IsNewRegisteredUserActiveByDefault = "App.UserManagement.IsNewRegisteredUserActiveByDefault";
            public const string UseCaptchaOnRegistration = "App.UserManagement.UseCaptchaOnRegistration";
            public const string UseCaptchaOnLogin = "App.UserManagement.UseCaptchaOnLogin";
            public const string SmsVerificationEnabled = "App.UserManagement.SmsVerificationEnabled";
            public const string IsCookieConsentEnabled = "App.UserManagement.IsCookieConsentEnabled";
            public const string IsQuickThemeSelectEnabled = "App.UserManagement.IsQuickThemeSelectEnabled";
            public const string AllowOneConcurrentLoginPerUser = "App.UserManagement.AllowOneConcurrentLoginPerUser";
            public const string AllowUsingGravatarProfilePicture = "App.UserManagement.AllowUsingGravatarProfilePicture";
            public const string UseGravatarProfilePicture = "App.UserManagement.UseGravatarProfilePicture";
        }

        public static class Email
        {
            public const string UseHostDefaultEmailSettings = "App.Email.UseHostDefaultEmailSettings";
        }
        public static class Recaptcha
        {
            public const string SiteKey = "Recaptcha.SiteKey";
        }
        public static class TimeAndPay
        {
            public const string TimeTrackingDefaultTimeClassificationId = "App.General.TimeTrackingDefaultTimeClassificationId";
            public const string BasePayOnHourlyJobRate = "App.TimeAndPay.BasePayOnHourlyJobRate";
            public const string UseDriverSpecificHourlyJobRate = "App.TimeAndPay.UseDriverSpecificHourlyJobRate";
            public const string AllowProductionPay = "App.TimeAndPay.AllowProductionPay";
            public const string DefaultToProductionPay = "App.TimeAndPay.DefaultToProductionPay";
            public const string PreventProductionPayOnHourlyJobs = "App.TimeAndPay.PreventProductionPayOnHourlyJobs";
            public const string AllowDriverPayRateDifferentFromFreightRate = "App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate";
            public const string DriverIsPaidForLoadBasedOn = "App.TimeAndPay.DriverIsPaidForLoadBasedOn";
            public const string AllowLoadBasedRates = "App.TimeAndPay.AllowLoadBasedRates";
            public const string PayStatementReportOrientation = "App.TimeAndPay.PayStatementReportOrientation";
            public const string ShowFreightRateOnDriverPayStatementReport = "App.TimeAndPay.ShowFreightRateOnDriverPayStatementReport";
            public const string ShowDriverPayRateOnDriverPayStatementReport = "App.TimeAndPay.ShowDriverPayRateOnDriverPayStatementReport";
            public const string ShowQuantityOnDriverPayStatementReport = "App.TimeAndPay.ShowQuantityOnDriverPayStatementReport";
        }

        public static class Fuel
        {
            public const string ShowFuelSurcharge = "App.Fuel.ShowFuelSurcharge";
            public const string ShowFuelSurchargeOnInvoice = "App.Fuel.ShowFuelSurchargeOnInvoice";
            public const string ItemIdToUseForFuelSurchargeOnInvoice = "App.Fuel.ItemIdToUseForFuelSurchargeOnInvoice";
            public const string DefaultFuelSurchargeCalculationId = "App.Fuel.DefaultFuelSurchargeCalculationId";
            public const string OnlyEvenIncrements = "App.Fuel.OnlyEvenIncrements";
        }

        public static class HaulZone
        {
            public static class HaulRateCalculation
            {
                public const string BaseUomIdForCod = "HaulZone.HaulRateCalculation.BaseUomIdForCod";
                public const string BaseUomId = "HaulZone.HaulRateCalculation.BaseUomId";
            }
        }

        public static class EmailTemplate
        {
            public const string UserEmailSubjectTemplate = "App.EmailTemplate.UserEmailSubjectTemplate";
            public const string UserEmailBodyTemplate = "App.EmailTemplate.UserEmailBodyTemplate";
            public const string DriverEmailSubjectTemplate = "App.EmailTemplate.DriverEmailSubjectTemplate";
            public const string DriverEmailBodyTemplate = "App.EmailTemplate.DriverEmailBodyTemplate";
            public const string LeaseHaulerInviteEmailSubjectTemplate = "App.EmailTemplate.LeaseHaulerInviteEmailSubjectTemplate";
            public const string LeaseHaulerInviteEmailBodyTemplate = "App.EmailTemplate.LeaseHaulerInviteEmailBodyTemplate";
            public const string LeaseHaulerDriverEmailSubjectTemplate = "App.EmailTemplate.LeaseHaulerDriverEmailSubjectTemplate";
            public const string LeaseHaulerDriverEmailBodyTemplate = "App.EmailTemplate.LeaseHaulerDriverEmailBodyTemplate";
            public const string LeaseHaulerJobRequestEmailSubjectTemplate = "App.EmailTemplate.LeaseHaulerJobRequestEmailSubjectTemplate";
            public const string LeaseHaulerJobRequestEmailBodyTemplate = "App.EmailTemplate.LeaseHaulerJobRequestEmailBodyTemplate";
            public const string CustomerPortalEmailSubjectTemplate = "App.EmailTemplate.CustomerPortalEmailSubjectTemplate";
            public const string CustomerPortalEmailBodyTemplate = "App.EmailTemplate.CustomerPortalEmailBodyTemplate";
        }

        public static class Tickets
        {
            public const string PrintPdfTicket = "App.PrintTickets.PrintPdfTickets";
        }

        public static class Invoice
        {
            public const string RemitToInformation = "App.Invoice.RemitToInformation";
            public const string TaxCalculationType = "App.Invoice.TaxCalculationType";
            public const string DefaultTaxRate = "App.Invoice.DefaultTaxRate";
            public const string AutopopulateDefaultTaxRate = "App.Invoice.AutopopulateDefaultTaxRate";
            public const string TermsAndConditions = "App.Invoice.TermsAndConditions";
            public const string EmailSubjectTemplate = "App.Invoice.EmailSubjectTemplate";
            public const string EmailBodyTemplate = "App.Invoice.EmailBodyTemplate";
            public const string DefaultMessageOnInvoice = "App.Invoice.DefaultInvoiceComments";
            public const string InvoiceTemplate = "App.Invoice.InvoiceTemplate";
            public const string AllowInvoiceApprovalFlow = "App.Invoice.AllowInvoiceApprovalFlow";
            public const string HideLoadAtAndDeliverToOnHourlyInvoices = "App.Invoice.HideLoadAtAndDeliverToOnHourlyInvoices";
            public const string CalculateMinimumFreightAmount = "App.Invoice.CalculateMinimumFreightAmount";
            public const string MinimumFreightAmountForTons = "App.Invoice.MinimumFreightAmountForTons";
            public const string MinimumFreightAmountForHours = "App.Invoice.MinimumFreightAmountForHours";

            public static class Quickbooks
            {
                public const string IntegrationKind = "App.Invoice.Quickbooks.IntegrationKind";
                public const string InvoiceNumberPrefix = "App.Invoice.Quickbooks.InvoiceNumberPrefix";
                public const string IsConnected = "App.Invoice.Quickbooks.IsConnected";
                public const string AccessToken = "App.Invoice.Quickbooks.AccessToken";
                public const string RefreshToken = "App.Invoice.Quickbooks.RefreshToken";
                public const string AccessTokenExpirationDate = "App.Invoice.Quickbooks.AccessTokenExpirationDate";
                public const string RefreshTokenExpirationDate = "App.Invoice.Quickbooks.RefreshTokenExpirationDate";
                public const string RealmId = "App.Invoice.Quickbooks.RealmId";
                public const string CsrfToken = "App.Invoice.Quickbooks.CsrfToken";
                public const string DefaultIncomeAccountId = "App.Invoice.Quickbooks.IncomeAccountForServicesId";
                public const string DefaultIncomeAccountName = "App.Invoice.Quickbooks.IncomeAccountForServicesName";
            }

            public static class QuickbooksDesktop
            {
                public const string TaxAgencyVendorName = "App.Invoice.QuickbooksDesktop.TaxAgencyVendorName";
                public const string DefaultIncomeAccountName = "App.Invoice.QuickbooksDesktop.IncomeAccountNameForServices";
                public const string DefaultIncomeAccountType = "App.Invoice.QuickbooksDesktop.IncomeAccountTypeForServices";
                public const string AccountsReceivableAccountName = "App.Invoice.QuickbooksDesktop.AccountsReceivableAccountName";
                public const string TaxAccountName = "App.Invoice.QuickbooksDesktop.TaxAccountName";
            }
        }

        public static class UserOptions
        {
            public const string DontShowZeroQuantityWarning = "App.UserOptions.DontShowZeroQuantityWarning";
            public const string PlaySoundForNotifications = "App.UserOptions.PlaySoundForNotifications";
            public const string HostEmailPreference = "App.UserOptions.HostEmailPreference";
            public const string DoNotShowWaitingForTicketDownload = "App.UserOptions.DoNotShowWaitingForTicketDownload";
        }

        public static class Quote
        {
            public const string PromptForDisplayingQuarryInfoOnQuotes = "App.General.PromptForDisplayingQuarryInfoOnQuotes";
            public const string DefaultNotes = "App.Quote.DefaultNotes";
            public const string EmailSubjectTemplate = "App.Quote.EmailSubjectTemplate";
            public const string EmailBodyTemplate = "App.Quote.EmailBodyTemplate";
            public const string GeneralTermsAndConditions = "App.Quote.GeneralTermsAndConditions";

            public static class ChangedNotificationEmail
            {
                public const string SubjectTemplate = "App.Quote.ChangedNotificationEmail.SubjectTemplate";
                public const string BodyTemplate = "App.Quote.ChangedNotificationEmail.BodyTemplate";
            }

        }

        public static class Job
        {
            public const string HideBedConstruction = "App.Job.HideBedConstruction";
            public const string HideSubContractorRate = "App.Job.HideSubContractorRate";
            public const string HideTaxInformation = "App.Job.HideTaxInformation";
            public const string HideChargeTo = "App.Job.HideChargeTo";
        }

        public static class Order
        {
            public const string EmailSubjectTemplate = "App.Order.EmailSubjectTemplate";
            public const string EmailBodyTemplate = "App.Order.EmailBodyTemplate";
        }

        public static class Receipt
        {
            public const string EmailSubjectTemplate = "App.Receipt.EmailSubjectTemplate";
            public const string EmailBodyTemplate = "App.Receipt.EmailBodyTemplate";
        }

        public static class Sms
        {
            public const string AccountSid = "App.Sms.AccountSid";
            public const string AuthToken = "App.Sms.AuthToken";
            public const string PhoneNumber = "App.Sms.PhoneNumber";
        }

        public static class DriverOrderNotification
        {
            public const string EmailTitle = "App.DriverOrderNotification.EmailTitle";
            public const string EmailBody = "App.DriverOrderNotification.EmailBody";
            public const string Sms = "App.DriverOrderNotification.Sms";
        }

        public static class GpsIntegration
        {
            public const string Platform = "App.GpsIntegration.GpsPlatform";
            public static class DtdTracker
            {
                public const string EnableDriverAppGps = "App.GpsIntegration.DtdTracker.EnableDriverAppGps";
                public const string AccountName = "App.GpsIntegration.DtdTracker.AccountName";
                public const string AccountId = "App.GpsIntegration.DtdTracker.AccountId";
                public const string UserId = "App.GpsIntegration.DtdTracker.UserId";
                public const string LastUploadedTruckPositionId = "App.GpsIntegration.DtdTracker.LastUploadedTruckPositionId";
                public const string LastUploadedTruckPositionTimestamp = "App.GpsIntegration.DtdTracker.LastUploadedTruckPositionTimestamp";
            }
            public static class Geotab
            {
                public const string Server = "App.GpsIntegration.Geotab.Server";
                public const string Database = "App.GpsIntegration.Geotab.Database";
                public const string MapBaseUrl = "App.GpsIntegration.Geotab.MapBaseUrl";
                public const string User = "App.GpsIntegration.Geotab.User";
                public const string Password = "App.GpsIntegration.Geotab.Password";
            }
            public static class IntelliShift
            {
                public const string ApiAuthUrl = "App.GpsIntegration.IntelliShift.ApiAuthUrl";
                public const string BaseUrl = "App.GpsIntegration.IntelliShift.BaseUrl";

                public const string User = "App.GpsIntegration.IntelliShift.User";
                public const string Password = "App.GpsIntegration.IntelliShift.Password";
            }

            public static class Samsara
            {
                public const string ApiToken = "App.GpsIntegration.Samsara.ApiToken";
                public const string BaseUrl = "App.GpsIntegration.Samsara.BaseUrl";
            }
        }

        public static class ExternalLoginProvider
        {
            public const string OpenIdConnectMappedClaims = "ExternalLoginProvider.OpenIdConnect.MappedClaims";
            public const string WsFederationMappedClaims = "ExternalLoginProvider.WsFederation.MappedClaims";

            public static class Host
            {
                public const string Facebook = "ExternalLoginProvider.Facebook";
                public const string Google = "ExternalLoginProvider.Google";
                public const string Twitter = "ExternalLoginProvider.Twitter";
                public const string Microsoft = "ExternalLoginProvider.Microsoft";
                public const string OpenIdConnect = "ExternalLoginProvider.OpenIdConnect";
                public const string WsFederation = "ExternalLoginProvider.WsFederation";
            }

            public static class Tenant
            {
                public const string Facebook = "ExternalLoginProvider.Facebook.Tenant";
                public const string Facebook_IsDeactivated = "ExternalLoginProvider.Facebook.IsDeactivated";
                public const string Google = "ExternalLoginProvider.Google.Tenant";
                public const string Google_IsDeactivated = "ExternalLoginProvider.Google.IsDeactivated";
                public const string Twitter = "ExternalLoginProvider.Twitter.Tenant";
                public const string Twitter_IsDeactivated = "ExternalLoginProvider.Twitter.IsDeactivated";
                public const string Microsoft = "ExternalLoginProvider.Microsoft.Tenant";
                public const string Microsoft_IsDeactivated = "ExternalLoginProvider.Microsoft.IsDeactivated";
                public const string OpenIdConnect = "ExternalLoginProvider.OpenIdConnect.Tenant";
                public const string OpenIdConnect_IsDeactivated = "ExternalLoginProvider.OpenIdConnect.IsDeactivated";
                public const string WsFederation = "ExternalLoginProvider.WsFederation.Tenant";
                public const string WsFederation_IsDeactivated = "ExternalLoginProvider.WsFederation.IsDeactivated";
            }
        }



        public static class Heartland
        {
            public const string PublicKey = "App.Heartland.PublicKey";
            public const string SecretKey = "App.Heartland.SecretKey";
        }

        public static class DispatchingAndMessaging
        {
            public const string DispatchVia = "App.DispatchingAndMessaging.DispatchVia";
            public const string AllowSmsMessages = "App.DispatchingAndMessaging.AllowSmsMessages";
            public const string SendSmsOnDispatchingObsoleteBool = "App.DispatchingAndMessaging.SendSmsOnDispatching";
            public const string SendSmsOnDispatching = "App.DispatchingAndMessaging.SendSmsOnDispatchingEnum";
            public const string SmsPhoneNumber = "App.DispatchingAndMessaging.SmsPhoneNumber";
            public const string DriverDispatchSmsTemplate = "App.DispatchingAndMessaging.DriverDispatchSmsTemplate";
            public const string DriverStartTimeTemplate = "App.DispatchingAndMessaging.DriverStartTime";
            public const string HideTicketControlsInDriverApp = "App.DispatchingAndMessaging.HideTicketControlsInDriverApp";
            [Obsolete("Use RequiredTicketEntry instead")]
            public const string RequireDriversToEnterTickets = "App.DispatchingAndMessaging.RequireDriversToEnterTickets";
            public const string RequiredTicketEntry = "App.DispatchingAndMessaging.RequiredTicketEntry";
            public const string RequireSignature = "App.DispatchingAndMessaging.RequireSignature";
            public const string RequireTicketPhoto = "App.DispatchingAndMessaging.RequireTicketPhoto";
            public const string DispatchesLockedToTruck = "App.DispatchingAndMessaging.DispatchesLockedToTruck";
            public const string DefaultStartTime = "App.DispatchingAndMessaging.DefaultStartTime";
            public const string TextForSignatureView = "App.DispatchingAndMessaging.TextForSignatureView";
            public const string ShowTrailersOnSchedule = "App.DispatchingAndMessaging.ShowTrailersOnSchedule";
            public const string ShowStaggerTimes = "App.DispatchingAndMessaging.ShowStaggerTimes";
            public const string ValidateUtilization = "App.DispatchingAndMessaging.ValidateUtilization";
            public const string AllowSchedulingTrucksWithoutDrivers = "App.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers";
            public const string AllowCounterSalesForTenant = "App.DispatchingAndMessaging.AllowCounterSalesForTenant";
            public const string AllowCounterSalesForUser = "App.DispatchingAndMessaging.AllowCounterSalesForUser";
            public const string DefaultLoadAtLocationId = "App.DispatchingAndMessaging.DefaultLoadAtLocationId";
            public const string DefaultDesignationToMaterialOnly = "App.DispatchingAndMessaging.DefaultDesignationToMaterialOnly";
            public const string DefaultMaterialItemId = "App.DispatchingAndMessaging.DefaultMaterialItemId";
            public const string DefaultMaterialUomId = "App.DispatchingAndMessaging.DefaultMaterialUomId";
            public const string DefaultAutoGenerateTicketNumber = "App.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber";
            public const string CCMeOnInvoices = "App.DispatchingAndMessaging.CCMeOnInvoices";
            public const string AutoGenerateTicketNumbers = "App.DispatchingAndMessaging.AutoGenerateTicketNumbers";
            public const string DisableTicketNumberOnDriverApp = "App.DispatchingAndMessaging.DisableTicketNumberOnDriverApp";
            public const string AllowLoadCountOnHourlyJobs = "App.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs";
            public const string AllowEditingTimeOnHourlyJobs = "App.DispatchingAndMessaging.AllowEditingTimeOnHourlyJobs";
            public const string AllowMultipleDispatchesToBeInProgressAtTheSameTime = "App.DispatchingAndMessaging.AllowMultipleDispatchesToBeInProgressAtTheSameTime";
            public const string HideDriverAppTimeScreen = "App.DispatchingAndMessaging.HideDriverAppTimeScreen";
        }

        public static class LeaseHaulers
        {
            public const string ShowLeaseHaulerRateOnQuote = "App.LeaseHaulers.ShowLeaseHaulerRateOnQuote";
            public const string ShowLeaseHaulerRateOnOrder = "App.LeaseHaulers.ShowLeaseHaulerRateOnOrder";
            public const string AllowSubcontractorsToDriveCompanyOwnedTrucks = "App.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks";
            public const string AllowLeaseHaulerRequestProcess = "App.LeaseHaulers.AllowLeaseHaulerRequestProcess";
            public const string BrokerFee = "App.LeaseHaulers.BrokerFee";
            public const string ThankYouForTrucksTemplate = "App.LeaseHaulers.ThankYouForTrucksTemplate";
            public const string NotAllowSchedulingLeaseHaulersWithExpiredInsurance = "App.LeaseHaulers.NotAllowSchedulingLeaseHaulersWithExpiredInsurance";
        }

        public static class GettingStarted
        {
            public const string ShowGettingStarted = "App.GettingStarted.ShowGettingStarted";
            public const string UsersChecked = "App.GettingStarted.UsersChecked";
            public const string DriversChecked = "App.GettingStarted.DriversChecked";
            public const string TrucksChecked = "App.GettingStarted.TrucksChecked";
            public const string CustomersChecked = "App.GettingStarted.CustomersChecked";
            public const string ServicesChecked = "App.GettingStarted.ServicesChecked";
            public const string LocationsChecked = "App.GettingStarted.LocationsChecked";
            public const string LeaseHaulersChecked = "App.GettingStarted.LeaseHaulersChecked";
            public const string LogoChecked = "App.GettingStarted.LogoChecked";
        }

        public static class FulcrumIntegration
        {
            public const string IsEnabled = "App.FulcrumIntegration.IsEnabled";
            public const string CustomerNumber = "App.FulcrumIntegration.CustomerNumber";
            public const string UserName = "App.FulcrumIntegration.UserName";
            public const string Password = "App.FulcrumIntegration.Password";
        }

        public static class ListCaches
        {
            public static string IsEnabled(string cacheName, ListCacheSide cacheSide)
            {
                return $"App.ListCaches.{cacheName}.{cacheSide}.IsEnabled";
            }

            public static string SlidingExpirationTimeMinutes(string cacheName, ListCacheSide cacheSide)
            {
                return $"App.ListCaches.{cacheName}.{cacheSide}.SlidingExpirationTimeMinutes";
            }

            public const string GlobalCacheVersion = "App.ListCaches.GlobalCacheVersion";
            public const string SyncRequestDebounceDelayMs = "App.ListCaches.SyncRequestDebounceDelayMs";

            public static class DateKeyLookup
            {
                public const string IsEnabled = "App.ListCaches.DateKeyLookup.IsEnabled";
                public const string SlidingExpirationTimeMinutes = "App.ListCaches.DateKeyLookup.SlidingExpirationTimeMinutes";
            }
        }
    }
}
