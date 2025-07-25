using System.Collections.Generic;
using System.Linq;
using Abp.Configuration;
using Abp.Encryption;
using Abp.Extensions;
using Abp.Json;
using Abp.Net.Mail;
using Abp.Zero.Configuration;
using DispatcherWeb.Authentication;
using DispatcherWeb.Caching;
using DispatcherWeb.Dashboard;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Configuration
{
    /// <summary>
    /// Defines settings for the application.
    /// See <see cref="AppSettings"/> for setting names.
    /// </summary>
    public class AppSettingProvider : SettingProvider
    {
        private readonly IConfigurationRoot _appConfiguration;
        private VisibleSettingClientVisibilityProvider _visibleSettingClientVisibilityProvider;
        private readonly IEncryptionService _encryptionService;

        public AppSettingProvider(
            IAppConfigurationAccessor configurationAccessor,
            IEncryptionService encryptionService
            )
        {
            _appConfiguration = configurationAccessor.Configuration;
            _visibleSettingClientVisibilityProvider = new VisibleSettingClientVisibilityProvider();
            _encryptionService = encryptionService;
        }

        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            // Disable TwoFactorLogin by default (can be enabled by UI)
            context.Manager.GetSettingDefinition(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled)
                .DefaultValue = false.ToString().ToLowerInvariant();

            // Change scope of Email settings
            ChangeEmailSettingScopes(context);

            return GetHostSettings()
                .Union(GetTenantSettings())
                .Union(GetSharedSettings())
                .Union(GetListCacheSettings())
                .Union(GetDashboardSettings())
                .Union(GetExternalLoginProviderSettings());
        }

        private void ChangeEmailSettingScopes(SettingDefinitionProviderContext context)
        {
            if (!DispatcherWebConsts.AllowTenantsToChangeEmailSettings)
            {
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Host).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Port).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.UserName).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Password).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Domain).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.EnableSsl).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.UseDefaultCredentials).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.DefaultFromAddress).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.DefaultFromDisplayName).Scopes = SettingScopes.Application;
            }
        }

        private IEnumerable<SettingDefinition> GetHostSettings()
        {
            return new[]
            {
                new SettingDefinition(AppSettings.TenantManagement.AllowSelfRegistration,
                    GetFromAppSettings(AppSettings.TenantManagement.AllowSelfRegistration, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault,
                    GetFromAppSettings(AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault, "false")),
                new SettingDefinition(AppSettings.TenantManagement.UseCaptchaOnRegistration,
                    GetFromAppSettings(AppSettings.TenantManagement.UseCaptchaOnRegistration, "true"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.TenantManagement.DefaultEdition,
                    GetFromAppSettings(AppSettings.TenantManagement.DefaultEdition, "")),
                new SettingDefinition(AppSettings.UserManagement.SmsVerificationEnabled,
                    GetFromAppSettings(AppSettings.UserManagement.SmsVerificationEnabled, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount,
                    GetFromAppSettings(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount, "7"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.HostManagement.BillingLegalName,
                    GetFromAppSettings(AppSettings.HostManagement.BillingLegalName, "")),
                new SettingDefinition(AppSettings.HostManagement.BillingAddress,
                    GetFromAppSettings(AppSettings.HostManagement.BillingAddress, "")),
                new SettingDefinition(AppSettings.HostManagement.NotificationsEmail,
                    GetFromAppSettings(AppSettings.HostManagement.NotificationsEmail, "joe@dumptruckdispatcher.com")),
                new SettingDefinition(AppSettings.HostManagement.LinkToResourceCenter,
                    GetFromAppSettings(AppSettings.HostManagement.LinkToResourceCenter, "https://dumptruckdispatcher.com/doc_category/getting-started/")),
                new SettingDefinition(AppSettings.HostManagement.TrainingMeetingRequestLink,
                    GetFromAppSettings(AppSettings.HostManagement.TrainingMeetingRequestLink, "https://lp.dumptruckdispatcher.com/meetings/dump-truck-dispatcher/training")),
                new SettingDefinition(AppSettings.HostManagement.SupportRequestLink,
                    GetFromAppSettings(AppSettings.HostManagement.SupportRequestLink, "https://share.hsforms.com/19V6UQm7NSBKuynKytvmSGQe1lp2"), isVisibleToClients: true),
                new SettingDefinition(AppSettings.HostManagement.DriverAppImageResolution,
                    GetFromAppSettings(AppSettings.HostManagement.DriverAppImageResolution, DriverAppImageResolutionEnum.Medium.ToIntString()), scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.HostManagement.DefaultDtdTrackerDeviceTypeId,
                    GetFromAppSettings(AppSettings.HostManagement.DefaultDtdTrackerDeviceTypeId, "400000192"), isVisibleToClients: true, scopes: SettingScopes.Application),
                new SettingDefinition(AppSettings.HostManagement.DefaultDtdTrackerDeviceTypeName,
                    GetFromAppSettings(AppSettings.HostManagement.DefaultDtdTrackerDeviceTypeName, "WiaTag"), isVisibleToClients: true, scopes: SettingScopes.Application),
                new SettingDefinition(AppSettings.HostManagement.DefaultDtdTrackerServerAddress,
                    GetFromAppSettings(AppSettings.HostManagement.DefaultDtdTrackerServerAddress, "us.gpsgsm.org:20963"), isVisibleToClients: true, scopes: SettingScopes.Application),
                new SettingDefinition(AppSettings.Recaptcha.SiteKey, GetFromSettings("Recaptcha:SiteKey"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.General.WebSiteRootAddress, "http://localhost:5000/"),
                new SettingDefinition(AppSettings.Sms.AccountSid, GetFromAppSettings(AppSettings.Sms.AccountSid, ""), isEncrypted: true),
                new SettingDefinition(AppSettings.Sms.AuthToken, GetFromAppSettings(AppSettings.Sms.AuthToken, ""), isEncrypted: true),
                new SettingDefinition(AppSettings.Sms.PhoneNumber, GetFromAppSettings(AppSettings.Sms.PhoneNumber, "")),
                new SettingDefinition(AppSettings.Logging.UploadTruckPositionsToWialonLogLevel, GetFromAppSettings(AppSettings.Logging.UploadTruckPositionsToWialonLogLevel, LogLevel.Warning.ToIntString())),
                new SettingDefinition(AppSettings.HostManagement.MinimumNativeAppVersion, GetFromAppSettings(AppSettings.HostManagement.MinimumNativeAppVersion, "1.0.1.300")),
                new SettingDefinition(AppSettings.HostManagement.RecommendedNativeAppVersion, GetFromAppSettings(AppSettings.HostManagement.RecommendedNativeAppVersion, "1.0.1.300")),
                new SettingDefinition(AppSettings.HostManagement.GooglePlayUrl, GetFromAppSettings(AppSettings.HostManagement.GooglePlayUrl, "")),
                new SettingDefinition(AppSettings.HostManagement.AppleStoreUrl, GetFromAppSettings(AppSettings.HostManagement.AppleStoreUrl, "")),
                new SettingDefinition(AppSettings.HostManagement.InvoicePrintLimit, GetFromAppSettings(AppSettings.HostManagement.InvoicePrintLimit, "100")),
                new SettingDefinition(AppSettings.HostManagement.TempFileExpirationTime, GetFromAppSettings(AppSettings.HostManagement.TempFileExpirationTime, "2880")),
            };
        }

        private IEnumerable<SettingDefinition> GetTenantSettings()
        {
            return new[]
            {
                new SettingDefinition(AppSettings.UserManagement.AllowSelfRegistration, GetFromAppSettings(AppSettings.UserManagement.AllowSelfRegistration, "true"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault, GetFromAppSettings(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault, "false"), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.UseCaptchaOnRegistration, GetFromAppSettings(AppSettings.UserManagement.UseCaptchaOnRegistration, "true"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TenantManagement.BillingLegalName, GetFromAppSettings(AppSettings.TenantManagement.BillingLegalName, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BillingAddress, GetFromAppSettings(AppSettings.TenantManagement.BillingAddress, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BillingPhoneNumber, GetFromAppSettings(AppSettings.TenantManagement.BillingPhoneNumber, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.RemitToInformation, GetFromAppSettings(AppSettings.Invoice.RemitToInformation, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.DefaultMessageOnInvoice, GetFromAppSettings(AppSettings.Invoice.DefaultMessageOnInvoice, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BillingTaxVatNo, GetFromAppSettings(AppSettings.TenantManagement.BillingTaxVatNo, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Email.UseHostDefaultEmailSettings, GetFromAppSettings(AppSettings.Email.UseHostDefaultEmailSettings, "true"), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.TaxCalculationType, GetFromAppSettings(AppSettings.Invoice.TaxCalculationType, ((int)TaxCalculationType.MaterialLineItemsTotal).ToString()), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.AutopopulateDefaultTaxRate, GetFromAppSettings(AppSettings.Invoice.AutopopulateDefaultTaxRate, "false"), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.TermsAndConditions, GetFromAppSettings(AppSettings.Invoice.TermsAndConditions, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.DefaultTaxRate, GetFromAppSettings(AppSettings.Invoice.DefaultTaxRate, "0"), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.InvoiceTemplate, GetFromAppSettings(AppSettings.Invoice.InvoiceTemplate, ((int)InvoiceTemplateEnum.Invoice1).ToString()), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.AllowInvoiceApprovalFlow, GetFromAppSettings(AppSettings.Invoice.AllowInvoiceApprovalFlow, "false"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.HideLoadAtAndDeliverToOnHourlyInvoices, GetFromAppSettings(AppSettings.Invoice.HideLoadAtAndDeliverToOnHourlyInvoices, "false"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.CalculateMinimumFreightAmount, GetFromAppSettings(AppSettings.Invoice.CalculateMinimumFreightAmount, "false"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.MinimumFreightAmountForTons, GetFromAppSettings(AppSettings.Invoice.MinimumFreightAmountForTons, "0"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.MinimumFreightAmountForHours, GetFromAppSettings(AppSettings.Invoice.MinimumFreightAmountForHours, "0"), scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.Invoice.EmailSubjectTemplate, "Invoice from {CompanyName}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.EmailBodyTemplate, "Attached is a pdf with your invoice. We appreciate your prompt payment.\n\nThank You,\n{User.FirstName} {User.LastName}", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.Invoice.Quickbooks.IntegrationKind, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.IsConnected, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.AccessToken, "", scopes: SettingScopes.Tenant, isEncrypted: true),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.RefreshToken, "", scopes: SettingScopes.Tenant, isEncrypted: true),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.AccessTokenExpirationDate, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.RefreshTokenExpirationDate, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.RealmId, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.CsrfToken, "", scopes: SettingScopes.Tenant, isEncrypted: true),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.DefaultIncomeAccountId, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.Quickbooks.DefaultIncomeAccountName, "", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName, "TaxAgencyVendor", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName, "Services Income", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType, "INC", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName, "Accounts Receivable", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Invoice.QuickbooksDesktop.TaxAccountName, "Sales Tax Payable", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.Quote.DefaultNotes, DispatcherWebConsts.DefaultSettings.Quote.DefaultNotes, scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Quote.EmailSubjectTemplate, "Quote", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Quote.EmailBodyTemplate, "Attached is a quote for your review. Please let me know if you have any questions.\r\n\r\nBest Regards\r\n{User.FirstName} {User.LastName}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate, "Quote #{Quote.Id} changed", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Quote.ChangedNotificationEmail.BodyTemplate, "Quote #{Quote.Id} for {Customer.Name} was changed by {ChangedByUser.FirstName} {ChangedByUser.LastName}. You may want to review it to see if you need to send a followup quote. Here is a shortcut to the quote {Quote.Url}\r\n\r\nYou can see the changes here: {QuoteHistory.Url}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Quote.PromptForDisplayingQuarryInfoOnQuotes, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Quote.GeneralTermsAndConditions, DispatcherWebConsts.DefaultSettings.Quote.GeneralTermsAndConditions, scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.Job.HideBedConstruction, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Job.HideSubContractorRate, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Job.HideTaxInformation, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Job.HideChargeTo, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.Order.EmailSubjectTemplate, "Thank you for your order", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Order.EmailBodyTemplate, "Attached is a copy of your order dated {DeliveryDate} from {CompanyName}.", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Receipt.EmailSubjectTemplate, "Thank you for your order", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Receipt.EmailBodyTemplate, "Attached is the receipt for your order dated {DeliveryDate} from {CompanyName}.", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.General.CompanyName, "Company", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.General.DefaultMapLocationAddress, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.General.DefaultMapLocation, "", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.CurrencySymbol, "$", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.UserDefinedField1, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.General.ValidateDriverAndTruckOnTickets, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShowDriverNamesOnPrintedOrder, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShowLoadAtOnPrintedOrder, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.SplitBillingByOffices, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShowOfficeOnTicketsByDriver, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShowAggregateCost, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.PaymentProcessor, "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.DriverOrderNotification.EmailTitle, "Orders for {DeliveryDate}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.DriverOrderNotification.EmailBody, "Attached is a pdf with your orders for {DeliveryDate}.", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.DriverOrderNotification.Sms, "Following are your orders for {DeliveryDate}. [order]Order {OrderNumber} at {TimeOnJob}, Comments: {Comments}, [item]{MaterialQuantity} {MaterialUom} of {Item} from {LoadAt} to {DeliverTo}[/item][/order]", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.GpsIntegration.Platform, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.GpsIntegration.Geotab.Server, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.Geotab.Database, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.Geotab.MapBaseUrl, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.Geotab.User, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.Geotab.Password, "", scopes: SettingScopes.Tenant, isEncrypted: true),

                new SettingDefinition(AppSettings.GpsIntegration.Samsara.ApiToken, "", scopes: SettingScopes.Tenant, isEncrypted: true),
                new SettingDefinition(AppSettings.GpsIntegration.Samsara.BaseUrl, "https://api.samsara.com/", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.GpsIntegration.IntelliShift.ApiAuthUrl, "https://auth.intellishift.com/oauth/token", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.IntelliShift.BaseUrl, "https://connect.intellishift.com/api", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.IntelliShift.User, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.IntelliShift.Password, "", scopes: SettingScopes.Tenant, isEncrypted: true),

                new SettingDefinition(AppSettings.GpsIntegration.DtdTracker.EnableDriverAppGps, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.GpsIntegration.DtdTracker.AccountName, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.DtdTracker.AccountId, "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.DtdTracker.UserId, "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionId, "0", scopes: SettingScopes.Application),
                new SettingDefinition(AppSettings.GpsIntegration.DtdTracker.LastUploadedTruckPositionTimestamp, "2023-01-01T00:00:00", scopes: SettingScopes.Application),

                new SettingDefinition(AppSettings.FulcrumIntegration.IsEnabled, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.FulcrumIntegration.CustomerNumber, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.FulcrumIntegration.UserName, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.FulcrumIntegration.Password, "", scopes: SettingScopes.Tenant, isEncrypted: true),

                new SettingDefinition(AppSettings.Heartland.PublicKey, GetFromAppSettings(AppSettings.Heartland.PublicKey, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Heartland.SecretKey, GetFromAppSettings(AppSettings.Heartland.SecretKey, ""), scopes: SettingScopes.Tenant, isEncrypted: true),

                new SettingDefinition(AppSettings.DispatchingAndMessaging.DispatchVia, DispatchVia.DriverApplication.ToIntString(), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowSmsMessages, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.SendSmsOnDispatchingObsoleteBool, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.SendSmsOnDispatching, "1", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.RequiredTicketEntry, RequiredTicketEntryEnum.None.ToIntString(), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.RequireSignature, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.RequireTicketPhoto, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.TextForSignatureView, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.SmsPhoneNumber, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate, "Truck Code: {TruckCode} | Start time: {DeliveryDate} {TimeOnJob} | Customer: {Customer} | From: {LoadAt} | Product: {Quantity} {UOM} {Item} | To: {DeliverTo} | Notes: {Note} | ChargeTo: {ChargeTo}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate, "Your start time for {StartDate} is {StartTime}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DefaultStartTime, "2000-01-01T12:00:00", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.ShowStaggerTimes, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.ValidateUtilization, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowCounterSalesForUser, "false", scopes: SettingScopes.User, isVisibleToClients: true), //the value of this setting shouldn't be inherited from AllowCounterSalesForTenant, so these are two different settings
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly, "false", scopes: SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId, "0", scopes: SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DefaultMaterialItemId, "0", scopes: SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DefaultMaterialUomId, "0", scopes: SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber, "false", scopes: SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.CCMeOnInvoices, "true", scopes: SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AutoGenerateTicketNumbers, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.DisableTicketNumberOnDriverApp, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowEditingTimeOnHourlyJobs, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.AllowMultipleDispatchesToBeInProgressAtTheSameTime, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DispatchingAndMessaging.HideDriverAppTimeScreen, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Tickets.PrintPdfTicket, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DriverApp.LoggingLevel, LogLevel.Error.ToIntString(), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DriverApp.SyncDataOnButtonClicks, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DriverApp.PeriodicSyncCheckIntervalSeconds, "120", scopes: SettingScopes.Application | SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.DriverApp.IsDriverSyncRequestStoreEnabled, "true", scopes: SettingScopes.Application, isVisibleToClients: true),

                new SettingDefinition(AppSettings.SyncRequests.DefaultFilter, SyncRequestDefaultFilter.ByDriverIds.ToIntString(), scopes: SettingScopes.Application | SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnQuote, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnOrder, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LeaseHaulers.AllowLeaseHaulerRequestProcess, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LeaseHaulers.BrokerFee, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LeaseHaulers.ThankYouForTrucksTemplate, "Thank you for making {NumberOfTrucks} truck(s) available to us. If we already received trucks from others, we'll let you know shortly. If we still need your trucks, you should hear from us by 3 PM with the order information.", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.LeaseHaulers.NotAllowSchedulingLeaseHaulersWithExpiredInsurance, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.General.UseShifts, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShiftName1, "", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShiftName2, "", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.General.ShiftName3, "", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.BasePayOnHourlyJobRate, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.AllowProductionPay, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.DefaultToProductionPay, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.DriverIsPaidForLoadBasedOn, DriverIsPaidForLoadBasedOnEnum.TicketDate.ToIntString(), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.AllowLoadBasedRates, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.PayStatementReportOrientation, PayStatementReportOrientation.Portrait.ToIntString(), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.ShowFreightRateOnDriverPayStatementReport, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.ShowDriverPayRateOnDriverPayStatementReport, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TimeAndPay.ShowQuantityOnDriverPayStatementReport, "true", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Trux.AllowImportingTruxEarnings, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Trux.TruxCustomerId, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Trux.UseForProductionPay, "true", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.LuckStone.AllowImportingLuckStoneEarnings, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LuckStone.LuckStoneCustomerId, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.LuckStone.HaulerRef, "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.LuckStone.UseForProductionPay, "true", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.IronSheepdog.AllowImportingIronSheepdogEarnings, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.IronSheepdog.IronSheepdogCustomerId, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.IronSheepdog.UseForProductionPay, "true", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.UserOptions.DontShowZeroQuantityWarning, "false", scopes: SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.UserOptions.PlaySoundForNotifications, "false", scopes: SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.UserOptions.HostEmailPreference, "15", scopes: SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettings.UserOptions.DoNotShowWaitingForTicketDownload, "false", scopes: SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),

                new SettingDefinition(AppSettings.GettingStarted.ShowGettingStarted, "true", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.UsersChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.DriversChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.TrucksChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.CustomersChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.ServicesChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.LocationsChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.LeaseHaulersChecked, "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.GettingStarted.LogoChecked, "false", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.Fuel.ShowFuelSurcharge, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Fuel.ShowFuelSurchargeOnInvoice, ShowFuelSurchargeOnInvoiceEnum.SingleLineItemAtTheBottom.ToIntString(), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Fuel.DefaultFuelSurchargeCalculationId, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.Fuel.OnlyEvenIncrements, "false", scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.HaulZone.HaulRateCalculation.BaseUomIdForCod, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.HaulZone.HaulRateCalculation.BaseUomId, "0", scopes: SettingScopes.Tenant, isVisibleToClients: true),

                new SettingDefinition(AppSettings.EmailTemplate.UserEmailSubjectTemplate, "Dump Truck Dispatcher account email activation", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.UserEmailBodyTemplate, "Click “Verify email” to start experiencing the benefits of Dump Truck Dispatcher.\r\n\r\nTenancy name: {TenancyName}\r\nUser name: {UserName}\r\nPassword: {Password}\r\n\r\nPlease click the link below to verify your email address:\r\n{VerifyEmailButton}\r\n\r\nButton not working? click the link below\r\n{VerifyEmailLink}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.DriverEmailSubjectTemplate, "Dump Truck Dispatcher account email activation", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.DriverEmailBodyTemplate, "Click “Verify email” to start experiencing the benefits of Dump Truck Dispatcher.\r\n\r\nTenancy name: {TenancyName}\r\nUser name: {UserName}\r\nPassword: {Password}\r\n\r\nPlease click the link below to verify your email address:\r\n{VerifyEmailButton}\r\n\r\nButton not working? click the link below\r\n{VerifyEmailLink}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.LeaseHaulerInviteEmailSubjectTemplate, "Invitation to access {CompanyName} portal on Dump Truck Dispatcher", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.LeaseHaulerInviteEmailBodyTemplate, "{CompanyName} has invited you to become their Lease Hauler.\r\n{CompanyName} uses Dump Truck Dispatcher for managing orders, dispatches, and tickets. Accept the invitation to start collaborating and keep track with delivery requests from them.\r\n\r\n{AcceptEmailButton}\r\n\r\nUse {dumptruckdispatcher.com} to easily manage your dispatching business. Automate your daily processes and make your process more efficient than ever.\r\n\r\nButton not working? click the link below\r\n{AcceptEmailLink}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.LeaseHaulerDriverEmailSubjectTemplate, "Dump Truck Dispatcher account email activation", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.LeaseHaulerDriverEmailBodyTemplate, "Click “Verify email” to start experiencing the benefits of Dump Truck Dispatcher.\r\n\r\nTenancy name: {TenancyName}\r\nUser name: {UserName}\r\nPassword: {Password}\r\n\r\nPlease click the link below to verify your email address:\r\n{VerifyEmailButton}\r\n\r\nButton not working? click the link below\r\n{VerifyEmailLink}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailSubjectTemplate, "Truck Request from {CompanyName}", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailBodyTemplate, "{CompanyName} is requesting {NumberOfTrucks} {TruckPluralization} for {OrderDate}. Please visit this link for more information {LinkToSchedulePage}.", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.CustomerPortalEmailSubjectTemplate, "Dump Truck Dispatcher account email activation", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.EmailTemplate.CustomerPortalEmailBodyTemplate, "You are being invited to Dump Truck Dispatcher by {CompanyName} so you can see your tickets and invoices. Instead of dealing with trying to reconcile all the paper tickets, you'll have access to all your tickets here and will be able to filter and sort the tickets to do internal reconciliations more readily.\r\n\r\nClick “Verify email” to start experiencing the benefits of Dump Truck Dispatcher.\r\n\r\nTenancy name: {TenancyName}\r\nUser name: {UserName}\r\nPassword: {Password}\r\n\r\nPlease click the link below to verify your email address:\r\n{VerifyEmailButton}\r\n\r\nButton not working? click the link below\r\n{VerifyEmailLink}", scopes: SettingScopes.Tenant),

            };
        }

        private IEnumerable<SettingDefinition> GetDashboardSettings()
        {
            return DashboardSettings.All.Select(x => x.SettingDefinition);
        }

        private IEnumerable<SettingDefinition> GetSharedSettings()
        {
            return new[]
            {
                new SettingDefinition(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled,
                    GetFromAppSettings(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled, "false"),
                    scopes: SettingScopes.Application | SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.UserManagement.IsCookieConsentEnabled,
                    GetFromAppSettings(AppSettings.UserManagement.IsCookieConsentEnabled, "false"),
                    scopes: SettingScopes.Application | SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.UserManagement.IsQuickThemeSelectEnabled,
                    GetFromAppSettings(AppSettings.UserManagement.IsQuickThemeSelectEnabled, "false"),
                    scopes: SettingScopes.Application | SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.UserManagement.UseCaptchaOnLogin,
                    GetFromAppSettings(AppSettings.UserManagement.UseCaptchaOnLogin, "false"),
                    scopes: SettingScopes.Application | SettingScopes.Tenant, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
                new SettingDefinition(AppSettings.UserManagement.SessionTimeOut.IsEnabled,
                    GetFromAppSettings(AppSettings.UserManagement.SessionTimeOut.IsEnabled, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.SessionTimeOut.TimeOutSecond,
                    GetFromAppSettings(AppSettings.UserManagement.SessionTimeOut.TimeOutSecond, "30"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond,
                    GetFromAppSettings(AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond, "30"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut,
                    GetFromAppSettings(AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser,
                    GetFromAppSettings(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.AllowUsingGravatarProfilePicture,
                    GetFromAppSettings(AppSettings.UserManagement.AllowUsingGravatarProfilePicture, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.Application | SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.UseGravatarProfilePicture,
                    GetFromAppSettings(AppSettings.UserManagement.UseGravatarProfilePicture, "false"),
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider, scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.DriverApp.LogLevel,
                    GetFromAppSettings(AppSettings.DriverApp.LogLevel, LogLevel.Error.ToIntString()),
                    isVisibleToClients: true,
                    scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.DriverApp.LocationTimeout,
                    GetFromAppSettings(AppSettings.DriverApp.LocationTimeout, "20000"),
                    isVisibleToClients: true,
                    scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.DriverApp.LocationMaxAge,
                    GetFromAppSettings(AppSettings.DriverApp.LocationMaxAge, "10000"),
                    isVisibleToClients: true,
                    scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.DriverApp.EnableLocationHighAccuracy,
                    GetFromAppSettings(AppSettings.DriverApp.EnableLocationHighAccuracy, "false"),
                    isVisibleToClients: true,
                    scopes: SettingScopes.All),
            };
        }

        private static IEnumerable<SettingDefinition> GetListCacheSettings()
        {
            var cacheSides = ListCacheSides.All;
            var cacheNames = ListCacheNames.All;

            return cacheSides.SelectMany(cacheSide =>
                cacheNames.SelectMany(cacheName =>
                    new[]
                    {
                        new
                        {
                            Name = AppSettings.ListCaches.IsEnabled(cacheName, cacheSide),
                            DefaultValue = "true",
                            CacheSide = cacheSide,
                        },
                        new
                        {
                            Name = AppSettings.ListCaches.SlidingExpirationTimeMinutes(cacheName, cacheSide),
                            DefaultValue = "1440",
                            CacheSide = cacheSide,
                        },
                    }.Select(cacheSetting => new SettingDefinition(
                        cacheSetting.Name,
                        cacheSetting.DefaultValue,
                        scopes: SettingScopes.Application,
                        isVisibleToClients: true
                    ))
                )
            ).Union(
                new[]
                {
                    new SettingDefinition(
                        AppSettings.ListCaches.GlobalCacheVersion,
                        "1",
                        scopes: SettingScopes.Application,
                        isVisibleToClients: true
                    ),
                    new SettingDefinition(
                        AppSettings.ListCaches.SyncRequestDebounceDelayMs,
                        "200",
                        scopes: SettingScopes.Application,
                        isVisibleToClients: true
                    ),
                    new SettingDefinition(
                        AppSettings.ListCaches.DateKeyLookup.IsEnabled,
                        "true",
                        scopes: SettingScopes.Application,
                        isVisibleToClients: true
                    ),
                    new SettingDefinition(
                        AppSettings.ListCaches.DateKeyLookup.SlidingExpirationTimeMinutes,
                        "14400",
                        scopes: SettingScopes.Application,
                        isVisibleToClients: true
                    ),
                }
            );
        }

        private string GetFromAppSettings(string name, string defaultValue = null)
        {
            return GetFromSettings("App:" + name, defaultValue);
        }

        private string GetFromSettings(string name, string defaultValue = null)
        {
            return _appConfiguration[name] ?? defaultValue;
        }

        #region ABP Dashboard Settings
        //private IEnumerable<SettingDefinition> GetDashboardSettings()
        //{
        //    var mvcDefaultSettings = GetDefaultMvcDashboardViews();
        //    var mvcDefaultSettingsJson = JsonConvert.SerializeObject(mvcDefaultSettings);

        //    var angularDefaultSettings = GetDefaultAngularDashboardViews();
        //    var angularDefaultSettingsJson = JsonConvert.SerializeObject(angularDefaultSettings);

        //    return new[]
        //    {
        //        new SettingDefinition(
        //            AppSettings.DashboardCustomization.Configuration + "." +
        //            DispatcherWebDashboardCustomizationConsts.Applications.Mvc, mvcDefaultSettingsJson,
        //            scopes: SettingScopes.All, clientVisibilityProvider: _visibleSettingClientVisibilityProvider),
        //        new SettingDefinition(
        //            AppSettings.DashboardCustomization.Configuration + "." +
        //            DispatcherWebDashboardCustomizationConsts.Applications.Angular, angularDefaultSettingsJson,
        //            scopes: SettingScopes.All, clientVisibilityProvider: _visibleSettingClientVisibilityProvider)
        //    };
        //}

        //public List<Dashboard> GetDefaultMvcDashboardViews()
        //{
        //    //It is the default dashboard view which your user will see if they don't do any customization.
        //    return new List<Dashboard>
        //    {
        //        new Dashboard
        //        {
        //            DashboardName = DispatcherWebDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard,
        //            Pages = new List<Page>
        //            {
        //                new Page
        //                {
        //                    Name = DispatcherWebDashboardCustomizationConsts.DefaultPageName,
        //                    Widgets = new List<Widget>
        //                    {
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .GeneralStats, // General Stats
        //                            Height = 9,
        //                            Width = 6,
        //                            PositionX = 0,
        //                            PositionY = 19
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .ProfitShare, // Profit Share
        //                            Height = 13,
        //                            Width = 6,
        //                            PositionX = 0,
        //                            PositionY = 28
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId =
        //                                DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                    .MemberActivity, // Memeber Activity
        //                            Height = 13,
        //                            Width = 6,
        //                            PositionX = 6,
        //                            PositionY = 28
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .RegionalStats, // Regional Stats
        //                            Height = 14,
        //                            Width = 6,
        //                            PositionX = 6,
        //                            PositionY = 5
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .DailySales, // Daily Sales
        //                            Height = 9,
        //                            Width = 6,
        //                            PositionX = 6,
        //                            PositionY = 19
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .TopStats, // Top Stats
        //                            Height = 5,
        //                            Width = 12,
        //                            PositionX = 0,
        //                            PositionY = 0
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .SalesSummary, // Sales Summary
        //                            Height = 14,
        //                            Width = 6,
        //                            PositionX = 0,
        //                            PositionY = 5
        //                        }
        //                    }
        //                }
        //            }
        //        },
        //        new Dashboard
        //        {
        //            DashboardName = DispatcherWebDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard,
        //            Pages = new List<Page>
        //            {
        //                new Page
        //                {
        //                    Name = DispatcherWebDashboardCustomizationConsts.DefaultPageName,
        //                    Widgets = new List<Widget>
        //                    {
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Host
        //                                .TopStats, // Top Stats
        //                            Height = 6,
        //                            Width = 12,
        //                            PositionX = 0,
        //                            PositionY = 0
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Host
        //                                .RecentTenants, // Recent tenants
        //                            Height = 10,
        //                            Width = 5,
        //                            PositionX = 7,
        //                            PositionY = 17
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Host
        //                                .SubscriptionExpiringTenants, // Subscription expiring tenants
        //                            Height = 10,
        //                            Width = 7,
        //                            PositionX = 0,
        //                            PositionY = 17
        //                        },
        //                    }
        //                }
        //            }
        //        }
        //    };
        //}

        //public List<Dashboard> GetDefaultAngularDashboardViews()
        //{
        //    //It is the default dashboard view which your user will see if they don't do any customization.
        //    return new List<Dashboard>
        //    {
        //        new Dashboard
        //        {
        //            DashboardName = DispatcherWebDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard,
        //            Pages = new List<Page>
        //            {
        //                new Page
        //                {
        //                    Name = DispatcherWebDashboardCustomizationConsts.DefaultPageName,
        //                    Widgets = new List<Widget>
        //                    {
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .TopStats, // Top Stats
        //                            Height = 4,
        //                            Width = 12,
        //                            PositionX = 0,
        //                            PositionY = 0
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .SalesSummary, // Sales Summary
        //                            Height = 12,
        //                            Width = 6,
        //                            PositionX = 0,
        //                            PositionY = 4
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .RegionalStats, // Regional Stats
        //                            Height = 12,
        //                            Width = 6,
        //                            PositionX = 6,
        //                            PositionY = 4
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .GeneralStats, // General Stats
        //                            Height = 8,
        //                            Width = 6,
        //                            PositionX = 0,
        //                            PositionY = 16
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .DailySales, // Daily Sales
        //                            Height = 8,
        //                            Width = 6,
        //                            PositionX = 6,
        //                            PositionY = 16
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                .ProfitShare, // Profit Share
        //                            Height = 11,
        //                            Width = 6,
        //                            PositionX = 0,
        //                            PositionY = 24
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId =
        //                                DispatcherWebDashboardCustomizationConsts.Widgets.Tenant
        //                                    .MemberActivity, // Member Activity
        //                            Height = 11,
        //                            Width = 6,
        //                            PositionX = 6,
        //                            PositionY = 24
        //                        }
        //                    }
        //                }
        //            }
        //        },
        //        new Dashboard
        //        {
        //            DashboardName = DispatcherWebDashboardCustomizationConsts.DashboardNames.DefaultHostDashboard,
        //            Pages = new List<Page>
        //            {
        //                new Page
        //                {
        //                    Name = DispatcherWebDashboardCustomizationConsts.DefaultPageName,
        //                    Widgets = new List<Widget>
        //                    {
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Host
        //                                .TopStats, // Top Stats
        //                            Height = 4,
        //                            Width = 12,
        //                            PositionX = 0,
        //                            PositionY = 0
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId =
        //                                DispatcherWebDashboardCustomizationConsts.Widgets.Host
        //                                    .RecentTenants, // Recent tenants
        //                            Height = 9,
        //                            Width = 5,
        //                            PositionX = 7,
        //                            PositionY = 12
        //                        },
        //                        new Widget
        //                        {
        //                            WidgetId = DispatcherWebDashboardCustomizationConsts.Widgets.Host
        //                                .SubscriptionExpiringTenants, // Subscription expiring tenants
        //                            Height = 9,
        //                            Width = 7,
        //                            PositionX = 0,
        //                            PositionY = 12
        //                        },
        //                    }
        //                }
        //            }
        //        }
        //    };
        //}
        #endregion

        private IEnumerable<SettingDefinition> GetExternalLoginProviderSettings()
        {
            return GetFacebookExternalLoginProviderSettings()
                .Union(GetGoogleExternalLoginProviderSettings())
                .Union(GetTwitterExternalLoginProviderSettings())
                .Union(GetMicrosoftExternalLoginProviderSettings())
                .Union(GetOpenIdConnectExternalLoginProviderSettings())
                .Union(GetWsFederationExternalLoginProviderSettings());
        }

        private SettingDefinition[] GetFacebookExternalLoginProviderSettings()
        {
            string appId = GetFromSettings("Authentication:Facebook:AppId");
            string appSecret = GetFromSettings("Authentication:Facebook:AppSecret");

            var facebookExternalLoginProviderInfo = new FacebookExternalLoginProviderSettings
            {
                AppId = appId,
                AppSecret = appSecret,
            };

            return new[]
            {
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Host.Facebook,
                    facebookExternalLoginProviderInfo.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated,
                    "false",
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition( //default is empty for tenants
                    AppSettings.ExternalLoginProvider.Tenant.Facebook,
                    "",
                    isVisibleToClients: false,
                    scopes: SettingScopes.Tenant,
                    isEncrypted: true
                ),
            };
        }

        private SettingDefinition[] GetGoogleExternalLoginProviderSettings()
        {
            string clientId = GetFromSettings("Authentication:Google:ClientId");
            string clientSecret = GetFromSettings("Authentication:Google:ClientSecret");
            string userInfoEndPoint = GetFromSettings("Authentication:Google:UserInfoEndpoint");

            var googleExternalLoginProviderInfo = new GoogleExternalLoginProviderSettings
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                UserInfoEndpoint = userInfoEndPoint,
            };

            return new[]
            {
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Host.Google,
                    googleExternalLoginProviderInfo.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated,
                    "false",
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition( //default is empty for tenants
                    AppSettings.ExternalLoginProvider.Tenant.Google,
                    "",
                    isVisibleToClients: false,
                    scopes: SettingScopes.Tenant,
                    isEncrypted: true
                ),
            };
        }

        private SettingDefinition[] GetTwitterExternalLoginProviderSettings()
        {
            string consumerKey = GetFromSettings("Authentication:Twitter:ConsumerKey");
            string consumerSecret = GetFromSettings("Authentication:Twitter:ConsumerSecret");

            var twitterExternalLoginProviderInfo = new TwitterExternalLoginProviderSettings
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
            };

            return new[]
            {
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Host.Twitter,
                    twitterExternalLoginProviderInfo.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated,
                    "false",
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition( //default is empty for tenants
                    AppSettings.ExternalLoginProvider.Tenant.Twitter,
                    "",
                    isVisibleToClients: false,
                    scopes: SettingScopes.Tenant,
                    isEncrypted: true
                ),
            };
        }

        private SettingDefinition[] GetMicrosoftExternalLoginProviderSettings()
        {
            string consumerKey = GetFromSettings("Authentication:Microsoft:ConsumerKey");
            string consumerSecret = GetFromSettings("Authentication:Microsoft:ConsumerSecret");

            var microsoftExternalLoginProviderInfo = new MicrosoftExternalLoginProviderSettings
            {
                ClientId = consumerKey,
                ClientSecret = consumerSecret,
            };


            return new[]
            {
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Host.Microsoft,
                    microsoftExternalLoginProviderInfo.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated,
                    "false",
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition( //default is empty for tenants
                    AppSettings.ExternalLoginProvider.Tenant.Microsoft,
                    "",
                    isVisibleToClients: false,
                    scopes: SettingScopes.Tenant,
                    isEncrypted: true
                ),
            };
        }

        private SettingDefinition[] GetOpenIdConnectExternalLoginProviderSettings()
        {
            var clientId = GetFromSettings("Authentication:OpenId:ClientId");
            var clientSecret = GetFromSettings("Authentication:OpenId:ClientSecret");
            var authority = GetFromSettings("Authentication:OpenId:Authority");
            var loginUrl = GetFromSettings("Authentication:OpenId:LoginUrl");
            var validateIssuerStr = GetFromSettings("Authentication:OpenId:ValidateIssuer");

            bool.TryParse(validateIssuerStr, out bool validateIssuer);

            var openIdConnectExternalLoginProviderInfo = new OpenIdConnectExternalLoginProviderSettings
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Authority = authority,
                ValidateIssuer = validateIssuer,
            };

            if (!loginUrl.IsNullOrEmpty())
            {
                openIdConnectExternalLoginProviderInfo.LoginUrl = loginUrl;
            }

            var jsonClaimMappings = new List<JsonClaimMapDto>();
            _appConfiguration.GetSection("Authentication:OpenId:ClaimsMapping").Bind(jsonClaimMappings);

            return new[]
            {
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Host.OpenIdConnect,
                    openIdConnectExternalLoginProviderInfo.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated,
                    "false",
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition( //default is empty for tenants
                    AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect,
                    "",
                    isVisibleToClients: false,
                    scopes: SettingScopes.Tenant,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims,
                    jsonClaimMappings.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application | SettingScopes.Tenant
                ),
            };
        }

        private SettingDefinition[] GetWsFederationExternalLoginProviderSettings()
        {
            var clientId = GetFromSettings("Authentication:WsFederation:ClientId");
            var wtrealm = GetFromSettings("Authentication:WsFederation:Wtrealm");
            var authority = GetFromSettings("Authentication:WsFederation:Authority");
            var tenant = GetFromSettings("Authentication:WsFederation:Tenant");
            var metaDataAddress = GetFromSettings("Authentication:WsFederation:MetaDataAddress");

            var wsFederationExternalLoginProviderInfo = new WsFederationExternalLoginProviderSettings
            {
                ClientId = clientId,
                Tenant = tenant,
                Authority = authority,
                Wtrealm = wtrealm,
                MetaDataAddress = metaDataAddress,
            };

            var jsonClaimMappings = new List<JsonClaimMapDto>();
            _appConfiguration.GetSection("Authentication:WsFederation:ClaimsMapping").Bind(jsonClaimMappings);

            return new[]
            {
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Host.WsFederation,
                    wsFederationExternalLoginProviderInfo.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application,
                    isEncrypted: true
                ),
                new SettingDefinition( //default is empty for tenants
                    AppSettings.ExternalLoginProvider.Tenant.WsFederation,
                    "",
                    isVisibleToClients: false,
                    scopes: SettingScopes.Tenant,
                    isEncrypted: true
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated,
                    "false",
                    clientVisibilityProvider: _visibleSettingClientVisibilityProvider,
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettings.ExternalLoginProvider.WsFederationMappedClaims,
                    jsonClaimMappings.ToJsonString(),
                    isVisibleToClients: false,
                    scopes: SettingScopes.Application | SettingScopes.Tenant
                ),
            };
        }
    }
}
