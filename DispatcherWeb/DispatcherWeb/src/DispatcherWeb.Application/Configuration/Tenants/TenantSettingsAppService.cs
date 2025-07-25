using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Zero.Configuration;
using DispatcherWeb.Authentication;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration.Dto;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.Configuration.Tenants.Dto;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Security;
using DispatcherWeb.Storage;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Timing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Configuration.Tenants
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Tenant_Settings)]
    public class TenantSettingsAppService : SettingsAppServiceBase, ITenantSettingsAppService
    {
        public IExternalLoginOptionsCacheManager ExternalLoginOptionsCacheManager { get; set; }

        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppSettingAvailabilityProvider _settingAvailabilityProvider;
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly ListCacheCollection _listCaches;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDtdTrackerTelematics _dtdTrackerTelematics;

        public TenantSettingsAppService(
            IMultiTenancyConfig multiTenancyConfig,
            ITimeZoneService timeZoneService,
            IEmailSender emailSender,
            IBinaryObjectManager binaryObjectManager,
            IAppConfigurationAccessor configurationAccessor,
            IAppSettingAvailabilityProvider settingAvailabilityProvider,
            ISettingDefinitionManager settingDefinitionManager,
            IRepository<Driver> driverRepository,
            IRepository<Customer> customerRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Office> officeRepository,
            ListCacheCollection listCaches,
            ISyncRequestSender syncRequestSender,
            IDtdTrackerTelematics dtdTrackerTelematics
            ) : base(emailSender, configurationAccessor)
        {
            ExternalLoginOptionsCacheManager = NullExternalLoginOptionsCacheManager.Instance;

            _multiTenancyConfig = multiTenancyConfig;
            _timeZoneService = timeZoneService;
            _binaryObjectManager = binaryObjectManager;
            _settingAvailabilityProvider = settingAvailabilityProvider;
            _settingDefinitionManager = settingDefinitionManager;
            _driverRepository = driverRepository;
            _customerRepository = customerRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _orderLineRepository = orderLineRepository;
            _officeRepository = officeRepository;
            _listCaches = listCaches;
            _syncRequestSender = syncRequestSender;
            _dtdTrackerTelematics = dtdTrackerTelematics;
        }

        #region Get Settings

        public async Task<TenantSettingsEditDto> GetAllSettings()
        {
            var settings = new TenantSettingsEditDto
            {
                General = await GetGeneralSettingsAsync(),
                UserManagement = await GetUserManagementSettingsAsync(),
                Security = await GetSecuritySettingsAsync(),
                Billing = await GetBillingSettingsAsync(),
                OtherSettings = await GetOtherSettingsAsync(),
                Email = await GetEmailSettingsAsync(),
                ExternalLoginProviderSettings = await GetExternalLoginProviderSettings(),
            };

            if (!_multiTenancyConfig.IsEnabled)
            {
                settings.Sms = await SettingManager.GetSmsSettingsAsync();

                settings.General.WebSiteRootAddress = await SettingManager.GetSettingValueAsync(AppSettings.General.WebSiteRootAddress);
            }

            settings.Integration = await GetIntegrationSettingsAsync();

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowPaymentProcessingFeature))
            {
                settings.Payment = await GetPaymentSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings))
            {
                settings.Trux = await GetTruxSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingLuckStoneEarnings))
            {
                settings.LuckStone = await GetLuckStoneSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingIronSheepdogEarnings))
            {
                settings.IronSheepdog = await GetIronSheepdogSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(false, AppFeatures.GpsIntegrationFeature, AppFeatures.SmsIntegrationFeature, AppFeatures.DispatchingFeature))
            {
                settings.DispatchingAndMessaging = await GetDispatchingAndMessagingSettingsAsync();
            }

            settings.Tickets = await GetTicketsSettingsAsync();

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature))
            {
                settings.LeaseHaulers = await GetLeaseHaulerSettingsEditDto();
            }

            settings.TimeAndPay = await GetTimeAndPaySettingsAsync();

            settings.Fuel = await GetFuelSettingsAsync();

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.HaulZone))
            {
                settings.HaulZone = await GetHaulZoneSettingsAsync();
            }

            settings.Quote = await GetQuoteSettingsAsync();

            settings.Job = await GetJobSettingsAsync();

            settings.EmailTemplate = await GetEmailTemplateSettingsAsync();

            return settings;
        }

        private async Task<TenantEmailSettingsEditDto> GetEmailSettingsAsync()
        {
            var tenantId = await AbpSession.GetTenantIdAsync();
            var useHostDefaultEmailSettings = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.Email.UseHostDefaultEmailSettings, tenantId);

            if (useHostDefaultEmailSettings)
            {
                return new TenantEmailSettingsEditDto
                {
                    UseHostDefaultEmailSettings = true,
                };
            }

            return new TenantEmailSettingsEditDto
            {
                UseHostDefaultEmailSettings = false,
                DefaultFromAddress = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.DefaultFromAddress, tenantId),
                DefaultFromDisplayName = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.DefaultFromDisplayName, tenantId),
                SmtpHost = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Host, tenantId),
                SmtpPort = await SettingManager.GetSettingValueForTenantAsync<int>(EmailSettingNames.Smtp.Port, tenantId),
                SmtpUserName = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.UserName, tenantId),
                SmtpPassword = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Password, tenantId),
                SmtpDomain = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Domain, tenantId),
                SmtpEnableSsl = await SettingManager.GetSettingValueForTenantAsync<bool>(EmailSettingNames.Smtp.EnableSsl, tenantId),
                SmtpUseDefaultCredentials = await SettingManager.GetSettingValueForTenantAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials, tenantId),
            };
        }

        private async Task<GeneralSettingsEditDto> GetGeneralSettingsAsync()
        {
            var settings = new GeneralSettingsEditDto();

            settings.OrderEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailSubjectTemplate);
            settings.OrderEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailBodyTemplate);
            settings.ReceiptEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailSubjectTemplate);
            settings.ReceiptEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailBodyTemplate);
            settings.CompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
            settings.DefaultMapLocationAddress = await SettingManager.GetSettingValueAsync(AppSettings.General.DefaultMapLocationAddress);
            settings.DefaultMapLocation = await SettingManager.GetSettingValueAsync(AppSettings.General.DefaultMapLocation);
            settings.CurrencySymbol = await SettingManager.GetSettingValueAsync(AppSettings.General.CurrencySymbol);
            settings.UserDefinedField1 = await SettingManager.GetSettingValueAsync(AppSettings.General.UserDefinedField1);
            settings.DontValidateDriverAndTruckOnTickets = !await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ValidateDriverAndTruckOnTickets);
            settings.ShowDriverNamesOnPrintedOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowDriverNamesOnPrintedOrder);
            settings.ShowLoadAtOnPrintedOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowLoadAtOnPrintedOrder);
            settings.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles);
            settings.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices);
            settings.SplitBillingByOffices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices);
            settings.ShowOfficeOnTicketsByDriver = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowOfficeOnTicketsByDriver);
            settings.ShowAggregateCost = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowAggregateCost);
            settings.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders);

            settings.UseShifts = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts);
            settings.ShiftName1 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName1);
            settings.ShiftName2 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName2);
            settings.ShiftName3 = await SettingManager.GetSettingValueAsync(AppSettings.General.ShiftName3);

            settings.DriverOrderEmailTitle = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailTitle);
            settings.DriverOrderEmailBody = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.EmailBody);
            settings.DriverOrderSms = await SettingManager.GetSettingValueAsync(AppSettings.DriverOrderNotification.Sms);

            if (Clock.SupportsMultipleTimezone)
            {
                var timezone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, await AbpSession.GetTenantIdAsync());

                settings.Timezone = timezone;
                settings.TimezoneForComparison = timezone;
            }

            var defaultTimeZoneId = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Tenant, await AbpSession.GetTenantIdOrNullAsync());

            if (settings.Timezone == defaultTimeZoneId)
            {
                settings.Timezone = string.Empty;
            }

            return settings;
        }

        private async Task<TimeAndPaySettingsEditDto> GetTimeAndPaySettingsAsync()
        {
            var settings = new TimeAndPaySettingsEditDto
            {
                BasePayOnHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.BasePayOnHourlyJobRate),
                UseDriverSpecificHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate),
                AllowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature),
                DefaultToProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay),
                PreventProductionPayOnHourlyJobs = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs),
                AllowDriverPayRateDifferentFromFreightRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate),
                TimeTrackingDefaultTimeClassificationId = await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId),
                DriverIsPaidForLoadBasedOn = (DriverIsPaidForLoadBasedOnEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.DriverIsPaidForLoadBasedOn),
                AllowLoadBasedRates = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowLoadBasedRates),
                PayStatementReportOrientation = (PayStatementReportOrientation)await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.PayStatementReportOrientation),
                ShowFreightRateOnDriverPayStatementReport = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.ShowFreightRateOnDriverPayStatementReport),
                ShowDriverPayRateOnDriverPayStatementReport = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.ShowDriverPayRateOnDriverPayStatementReport),
                ShowQuantityOnDriverPayStatementReport = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.ShowQuantityOnDriverPayStatementReport),
            };

            if (settings.TimeTrackingDefaultTimeClassificationId > 0)
            {
                var timeClassification = await (await _timeClassificationRepository.GetQueryAsync())
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    })
                    .FirstOrDefaultAsync(x => x.Id == settings.TimeTrackingDefaultTimeClassificationId);

                if (timeClassification == null)
                {
                    settings.TimeTrackingDefaultTimeClassificationId = 0;
                }
                else
                {
                    settings.TimeTrackingDefaultTimeClassificationName = timeClassification.Name;
                }
            }

            return settings;
        }

        private async Task<TenantUserManagementSettingsEditDto> GetUserManagementSettingsAsync()
        {
            return new TenantUserManagementSettingsEditDto
            {
                AllowSelfRegistration = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowSelfRegistration),
                IsNewRegisteredUserActiveByDefault = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault),
                IsEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin),
                UseCaptchaOnRegistration = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnRegistration),
                UseCaptchaOnLogin = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.UseCaptchaOnLogin),
                IsCookieConsentEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsCookieConsentEnabled),
                IsQuickThemeSelectEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsQuickThemeSelectEnabled),
                AllowUsingGravatarProfilePicture = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowUsingGravatarProfilePicture),
                SessionTimeOutSettings = new SessionTimeOutSettingsEditDto
                {
                    IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SessionTimeOut.IsEnabled),
                    TimeOutSecond = await SettingManager.GetSettingValueAsync<int>(AppSettings.UserManagement.SessionTimeOut.TimeOutSecond),
                    ShowTimeOutNotificationSecond = await SettingManager.GetSettingValueAsync<int>(AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond),
                    ShowLockScreenWhenTimedOut = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut),
                },
            };
        }

        private async Task<SecuritySettingsEditDto> GetSecuritySettingsAsync()
        {
            var passwordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit),
                RequireLowercase = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase),
                RequireNonAlphanumeric = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric),
                RequireUppercase = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase),
                RequiredLength = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength),
            };

            var defaultPasswordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit),
                RequireLowercase = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase),
                RequireNonAlphanumeric = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric),
                RequireUppercase = await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase),
                RequiredLength = await SettingManager.GetSettingValueForApplicationAsync<int>(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength),
            };

            return new SecuritySettingsEditDto
            {
                UseDefaultPasswordComplexitySettings = passwordComplexitySetting.Equals(defaultPasswordComplexitySetting),
                PasswordComplexity = passwordComplexitySetting,
                DefaultPasswordComplexity = defaultPasswordComplexitySetting,
                UserLockOut = await GetUserLockOutSettingsAsync(),
                TwoFactorLogin = await GetTwoFactorLoginSettingsAsync(),
                AllowOneConcurrentLoginPerUser = await GetOneConcurrentLoginPerUserSetting(),
            };
        }

        private async Task<TenantBillingSettingsEditDto> GetBillingSettingsAsync()
        {
            var autopopulateDefaultTaxRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.AutopopulateDefaultTaxRate);
            return new TenantBillingSettingsEditDto
            {
                LegalName = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingLegalName),
                Address = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingAddress),
                PhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingPhoneNumber),
                RemitToInformation = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.RemitToInformation),
                DefaultMessageOnInvoice = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.DefaultMessageOnInvoice),
                InvoiceEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailSubjectTemplate),
                InvoiceEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailBodyTemplate),
                TaxVatNo = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingTaxVatNo),
                TaxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType),
                AutopopulateDefaultTaxRate = autopopulateDefaultTaxRate,
                InvoiceTermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.TermsAndConditions),
                DefaultTaxRate = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.DefaultTaxRate),
                InvoiceTemplate = (InvoiceTemplateEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.InvoiceTemplate),
                AllowInvoiceApprovalFlow = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.AllowInvoiceApprovalFlow),
                HideLoadAtAndDeliverToOnHourlyInvoices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.HideLoadAtAndDeliverToOnHourlyInvoices),
                CalculateMinimumFreightAmount = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.CalculateMinimumFreightAmount),
                MinimumFreightAmountForTons = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.MinimumFreightAmountForTons),
                MinimumFreightAmountForHours = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.MinimumFreightAmountForHours),
                QuickbooksIntegrationKind = (QuickbooksIntegrationKind)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.Quickbooks.IntegrationKind),
                QuickbooksInvoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix),
                //IsQuickbooksConnected = await SettingManager.IsQuickbooksConnected(),
                QbdTaxAgencyVendorName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName),
                QbdDefaultIncomeAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName),
                QbdDefaultIncomeAccountType = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType),
                QbdAccountsReceivableAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName),
                QbdTaxAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAccountName),
                QbdIncomeAccountTypes = QuickbooksDesktop.Models.AccountTypes.GetIncomeTypesSelectList(),
            };
        }

        public async Task<QuoteSettingsEditDto> GetQuoteSettingsAsync()
        {
            var settings = new QuoteSettingsEditDto();
            settings.PromptForDisplayingQuarryInfoOnQuotes = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Quote.PromptForDisplayingQuarryInfoOnQuotes);
            settings.QuoteDefaultNote = await SettingManager.GetSettingValueAsync(AppSettings.Quote.DefaultNotes);
            settings.QuoteEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate);
            settings.QuoteEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailBodyTemplate);
            settings.QuoteChangedNotificationEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate);
            settings.QuoteChangedNotificationEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.BodyTemplate);
            settings.QuoteGeneralTermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Quote.GeneralTermsAndConditions);
            return settings;
        }

        public async Task<JobSettingsEditDto> GetJobSettingsAsync()
        {
            var settings = new JobSettingsEditDto();
            settings.HideBedConstruction = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Job.HideBedConstruction);
            settings.HideSubContractorRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Job.HideSubContractorRate);
            settings.HideTaxInformation = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Job.HideTaxInformation);
            settings.HideChargeTo = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Job.HideChargeTo);
            return settings;
        }

        private async Task<FuelSettingsEditDto> GetFuelSettingsAsync()
        {
            var settings = new FuelSettingsEditDto();
            settings.ShowFuelSurcharge = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            settings.ShowFuelSurchargeOnInvoice = (ShowFuelSurchargeOnInvoiceEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ShowFuelSurchargeOnInvoice);
            settings.OnlyEvenIncrements = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.OnlyEvenIncrements);

            settings.ItemIdToUseForFuelSurchargeOnInvoice = await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            if (settings.ItemIdToUseForFuelSurchargeOnInvoice > 0)
            {
                var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                var item = items.Find(settings.ItemIdToUseForFuelSurchargeOnInvoice);
                if (item == null)
                {
                    settings.ItemIdToUseForFuelSurchargeOnInvoice = 0;
                }
                else
                {
                    settings.ItemNameToUseForFuelSurchargeOnInvoice = item.Name;
                }
            }

            return settings;
        }

        private async Task<HaulZoneSettingsEditDto> GetHaulZoneSettingsAsync()
        {
            var settings = new HaulZoneSettingsEditDto
            {
                HaulRateCalculationBaseUomIdForCod = await SettingManager.GetSettingValueAsync<int>(AppSettings.HaulZone.HaulRateCalculation.BaseUomIdForCod),
                HaulRateCalculationBaseUomId = await SettingManager.GetSettingValueAsync<int>(AppSettings.HaulZone.HaulRateCalculation.BaseUomId),
            };

            if (settings.HaulRateCalculationBaseUomIdForCod > 0)
            {
                var uoms = await _listCaches.UnitOfMeasure.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                var uom = uoms.Items.FirstOrDefault(x => (int?)x.UomBaseId == settings.HaulRateCalculationBaseUomIdForCod);

                if (uom == null)
                {
                    settings.HaulRateCalculationBaseUomIdForCod = 0;
                }
                else
                {
                    settings.HaulRateCalculationBaseUomNameForCod = uom.Name;
                }
            }

            if (settings.HaulRateCalculationBaseUomId > 0)
            {
                var uoms = await _listCaches.UnitOfMeasure.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                var uom = uoms.Items.FirstOrDefault(x => (int?)x.UomBaseId == settings.HaulRateCalculationBaseUomId);

                if (uom == null)
                {
                    settings.HaulRateCalculationBaseUomId = 0;
                }
                else
                {
                    settings.HaulRateCalculationBaseUomName = uom.Name;
                }
            }

            return settings;
        }

        private async Task<EmailTemplateSettingsEditDto> GetEmailTemplateSettingsAsync()
        {
            var settings = new EmailTemplateSettingsEditDto();
            settings.UserEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.UserEmailSubjectTemplate);
            settings.UserEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.UserEmailBodyTemplate);
            settings.DriverEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.DriverEmailSubjectTemplate);
            settings.DriverEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.DriverEmailBodyTemplate);
            settings.LeaseHaulerInviteEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerInviteEmailSubjectTemplate);
            settings.LeaseHaulerInviteEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerInviteEmailBodyTemplate);
            settings.LeaseHaulerDriverEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerDriverEmailSubjectTemplate);
            settings.LeaseHaulerDriverEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerDriverEmailBodyTemplate);
            settings.LeaseHaulerJobRequestEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailSubjectTemplate);
            settings.LeaseHaulerJobRequestEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailBodyTemplate);
            settings.CustomerPortalEmailSubjectTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.CustomerPortalEmailSubjectTemplate);
            settings.CustomerPortalEmailBodyTemplate = await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.CustomerPortalEmailBodyTemplate);

            return settings;
        }

        private async Task<TenantOtherSettingsEditDto> GetOtherSettingsAsync()
        {
            return new TenantOtherSettingsEditDto
            {
                IsQuickThemeSelectEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.IsQuickThemeSelectEnabled),
            };
        }

        private async Task<UserLockOutSettingsEditDto> GetUserLockOutSettingsAsync()
        {
            return new UserLockOutSettingsEditDto
            {
                IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled),
                MaxFailedAccessAttemptsBeforeLockout = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout),
                DefaultAccountLockoutSeconds = await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds),
            };
        }

        private async Task<GpsIntegrationSettingsEditDto> GetGpsIntegrationSettingsAsync()
        {
            return new GpsIntegrationSettingsEditDto
            {
                Platform = (GpsPlatform)await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.Platform),
                Geotab = await GetGeotabSettingsAsync(),
                DtdTracker = await GetDtdTrackerSettingsAsync(),
                Samsara = await GetSamsaraSettingsAsync(),
                IntelliShift = await GetIntelliShiftSettingsAsync(),
            };
        }

        private async Task<GeotabSettingsEditDto> GetGeotabSettingsAsync()
        {
            return new GeotabSettingsEditDto
            {
                Server = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Server),
                Database = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Database),
                User = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.User),
                Password = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.Password),
                MapBaseUrl = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Geotab.MapBaseUrl),
            };
        }
        private async Task<IntelliShiftSettingsEditDto> GetIntelliShiftSettingsAsync()
        {
            return new IntelliShiftSettingsEditDto
            {
                User = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.User),
                Password = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.IntelliShift.Password),
            };
        }

        private async Task<SamsaraSettingsEditDto> GetSamsaraSettingsAsync()
        {
            return new SamsaraSettingsEditDto
            {
                ApiToken = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Samsara.ApiToken),
                BaseUrl = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.Samsara.BaseUrl),
            };
        }

        private async Task<DtdTrackerSettingsEditDto> GetDtdTrackerSettingsAsync()
        {
            return new DtdTrackerSettingsEditDto
            {
                EnableDriverAppGps = await SettingManager.GetSettingValueAsync<bool>(AppSettings.GpsIntegration.DtdTracker.EnableDriverAppGps),
                AccountName = await SettingManager.GetSettingValueAsync(AppSettings.GpsIntegration.DtdTracker.AccountName),
                AccountId = await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.AccountId),
            };
        }

        private async Task<DispatchingAndMessagingSettingsEditDto> GetDispatchingAndMessagingSettingsAsync()
        {
            var timezone = await GetTimezone();
            var result = new DispatchingAndMessagingSettingsEditDto
            {
                DispatchVia = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia),
                AllowSmsMessages = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSmsMessages),
                SendSmsOnDispatching = (SendSmsOnDispatchingEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.SendSmsOnDispatching),
                SmsPhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.SmsPhoneNumber),
                DriverDispatchSms = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate),
                DriverStartTime = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate),
                HideTicketControlsInDriverApp = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp),
                RequiredTicketEntry = (RequiredTicketEntryEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.RequiredTicketEntry),
                RequireSignature = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireSignature),
                RequireTicketPhoto = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.RequireTicketPhoto),
                TextForSignatureView = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.TextForSignatureView),
                DispatchesLockedToTruck = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck),
                DefaultStartTime = (await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.DispatchingAndMessaging.DefaultStartTime)).ConvertTimeZoneTo(timezone),
                ShowTrailersOnSchedule = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule),
                ShowStaggerTimes = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowStaggerTimes),
                ValidateUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization),
                AllowSchedulingTrucksWithoutDrivers = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers),
                AllowCounterSalesForTenant = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant),
                AutoGenerateTicketNumbers = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AutoGenerateTicketNumbers),
                DisableTicketNumberOnDriverApp = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DisableTicketNumberOnDriverApp),
                AllowLoadCountOnHourlyJobs = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs),
                AllowEditingTimeOnHourlyJobs = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowEditingTimeOnHourlyJobs),
                AllowMultipleDispatchesToBeInProgressAtTheSameTime = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowMultipleDispatchesToBeInProgressAtTheSameTime),
                HideDriverAppTimeScreen = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideDriverAppTimeScreen),
                LoggingLevel = (LogLevel)await SettingManager.GetSettingValueAsync<int>(AppSettings.DriverApp.LoggingLevel),
                SyncDataOnButtonClicks = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DriverApp.SyncDataOnButtonClicks),
            };

            return result;
        }

        private async Task<TicketsSettingsEditDto> GetTicketsSettingsAsync()
        {
            return new TicketsSettingsEditDto
            {
                PrintPdfTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Tickets.PrintPdfTicket),
            };
        }

        private async Task<LeaseHaulerSettingsEditDto> GetLeaseHaulerSettingsEditDto()
        {
            return new LeaseHaulerSettingsEditDto
            {
                ShowLeaseHaulerRateOnQuote = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnQuote),
                ShowLeaseHaulerRateOnOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnOrder),
                AllowSubcontractorsToDriveCompanyOwnedTrucks = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks),
                AllowLeaseHaulerRequestProcess = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowLeaseHaulerRequestProcess),
                BrokerFee = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.LeaseHaulers.BrokerFee),
                ThankYouForTrucksTemplate = await SettingManager.GetSettingValueAsync(AppSettings.LeaseHaulers.ThankYouForTrucksTemplate),
                NotAllowSchedulingLeaseHaulersWithExpiredInsurance = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.NotAllowSchedulingLeaseHaulersWithExpiredInsurance),
            };
        }

        private async Task<PaymentSettingsEditDto> GetPaymentSettingsAsync()
        {
            return new PaymentSettingsEditDto
            {
                PaymentProcessor = (PaymentProcessor)(await SettingManager.GetSettingValueAsync<int>(AppSettings.General.PaymentProcessor)),
                HeartlandPublicKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey),
                HeartlandSecretKey = (await SettingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey)).IsNullOrEmpty()
                    ? string.Empty
                    : DispatcherWebConsts.PasswordHasntBeenChanged,
            };
        }

        private async Task<TruxSettingsEditDto> GetTruxSettingsAsync()
        {
            var truxSettings = new TruxSettingsEditDto
            {
                AllowImportingTruxEarnings = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Trux.AllowImportingTruxEarnings),
                UseForProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Trux.UseForProductionPay),
            };

            var customerId = await SettingManager.GetSettingValueAsync<int>(AppSettings.Trux.TruxCustomerId);
            if (customerId > 0)
            {
                var customer = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Id == customerId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    }).FirstOrDefaultAsync();
                truxSettings.TruxCustomerId = customer?.Id;
                truxSettings.TruxCustomerName = customer?.Name;
            }

            if (truxSettings.TruxCustomerId == null)
            {
                var customers = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Name.Contains("Trux"))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    })
                    .Take(2)
                    .ToListAsync();

                if (customers.Count == 1)
                {
                    truxSettings.TruxCustomerId = customers.First().Id;
                    truxSettings.TruxCustomerName = customers.First().Name;
                }
            }

            return truxSettings;
        }

        private async Task<LuckStoneSettingsEditDto> GetLuckStoneSettingsAsync()
        {
            var luckStoneSettings = new LuckStoneSettingsEditDto
            {
                AllowImportingLuckStoneEarnings = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LuckStone.AllowImportingLuckStoneEarnings),
                HaulerRef = await SettingManager.GetSettingValueAsync(AppSettings.LuckStone.HaulerRef),
                UseForProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LuckStone.UseForProductionPay),
            };

            var customerId = await SettingManager.GetSettingValueAsync<int>(AppSettings.LuckStone.LuckStoneCustomerId);
            if (customerId > 0)
            {
                var customer = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Id == customerId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    }).FirstOrDefaultAsync();
                luckStoneSettings.LuckStoneCustomerId = customer?.Id;
                luckStoneSettings.LuckStoneCustomerName = customer?.Name;
            }

            if (luckStoneSettings.LuckStoneCustomerId == null)
            {
                var customers = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Name.Contains("LuckStone") || x.Name.Contains("Luck Stone"))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    })
                    .Take(2)
                    .ToListAsync();

                if (customers.Count == 1)
                {
                    luckStoneSettings.LuckStoneCustomerId = customers.First().Id;
                    luckStoneSettings.LuckStoneCustomerName = customers.First().Name;
                }
            }

            return luckStoneSettings;
        }

        private async Task<IronSheepdogSettingsEditDto> GetIronSheepdogSettingsAsync()
        {
            var ironSheepdogSettings = new IronSheepdogSettingsEditDto
            {
                AllowImportingIronSheepdogEarnings = await SettingManager.GetSettingValueAsync<bool>(AppSettings.IronSheepdog.AllowImportingIronSheepdogEarnings),
                UseForProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.IronSheepdog.UseForProductionPay),
            };

            var customerId = await SettingManager.GetSettingValueAsync<int>(AppSettings.IronSheepdog.IronSheepdogCustomerId);
            if (customerId > 0)
            {
                var customer = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Id == customerId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    }).FirstOrDefaultAsync();
                ironSheepdogSettings.IronSheepdogCustomerId = customer?.Id;
                ironSheepdogSettings.IronSheepdogCustomerName = customer?.Name;
            }

            if (ironSheepdogSettings.IronSheepdogCustomerId == null)
            {
                var customers = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Name.Contains("Chaney Enterprises / Sustainable Land Use"))
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    })
                    .Take(2)
                    .ToListAsync();

                if (customers.Count == 1)
                {
                    ironSheepdogSettings.IronSheepdogCustomerId = customers.First().Id;
                    ironSheepdogSettings.IronSheepdogCustomerName = customers.First().Name;
                }
            }

            return ironSheepdogSettings;
        }

        private Task<bool> IsTwoFactorLoginEnabledForApplicationAsync()
        {
            return SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled);
        }

        private async Task<TwoFactorLoginSettingsEditDto> GetTwoFactorLoginSettingsAsync()
        {
            var settings = new TwoFactorLoginSettingsEditDto
            {
                IsEnabledForApplication = await IsTwoFactorLoginEnabledForApplicationAsync(),
            };

            if (_multiTenancyConfig.IsEnabled && !settings.IsEnabledForApplication)
            {
                return settings;
            }

            settings.IsEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled);
            settings.IsRememberBrowserEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled);

            if (!_multiTenancyConfig.IsEnabled)
            {
                settings.IsEmailProviderEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEmailProviderEnabled);
                settings.IsSmsProviderEnabled = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsSmsProviderEnabled);
                settings.IsGoogleAuthenticatorEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled);
            }

            return settings;
        }

        private async Task<bool> GetOneConcurrentLoginPerUserSetting()
        {
            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser);
        }

        private async Task<ExternalLoginProviderSettingsEditDto> GetExternalLoginProviderSettings()
        {
            var tenantId = await AbpSession.GetTenantIdAsync();
            var facebookSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Facebook, tenantId);
            var googleSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Google, tenantId);
            var twitterSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Twitter, tenantId);
            var microsoftSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.Microsoft, tenantId);

            var openIdConnectSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect, tenantId);
            var openIdConnectMappedClaims = await SettingManager.GetSettingValueAsync(AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims);

            var wsFederationSettings = await SettingManager.GetSettingValueForTenantAsync(AppSettings.ExternalLoginProvider.Tenant.WsFederation, tenantId);
            var wsFederationMappedClaims = await SettingManager.GetSettingValueAsync(AppSettings.ExternalLoginProvider.WsFederationMappedClaims);

            return new ExternalLoginProviderSettingsEditDto
            {
                Facebook_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated, tenantId),
                Facebook = facebookSettings.IsNullOrWhiteSpace()
                    ? new FacebookExternalLoginProviderSettings()
                    : facebookSettings.FromJsonString<FacebookExternalLoginProviderSettings>(),

                Google_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated, tenantId),
                Google = googleSettings.IsNullOrWhiteSpace()
                    ? new GoogleExternalLoginProviderSettings()
                    : googleSettings.FromJsonString<GoogleExternalLoginProviderSettings>(),

                Twitter_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated, tenantId),
                Twitter = twitterSettings.IsNullOrWhiteSpace()
                    ? new TwitterExternalLoginProviderSettings()
                    : twitterSettings.FromJsonString<TwitterExternalLoginProviderSettings>(),

                Microsoft_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated, tenantId),
                Microsoft = microsoftSettings.IsNullOrWhiteSpace()
                    ? new MicrosoftExternalLoginProviderSettings()
                    : microsoftSettings.FromJsonString<MicrosoftExternalLoginProviderSettings>(),

                OpenIdConnect_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated, tenantId),
                OpenIdConnect = openIdConnectSettings.IsNullOrWhiteSpace()
                    ? new OpenIdConnectExternalLoginProviderSettings()
                    : openIdConnectSettings.FromJsonString<OpenIdConnectExternalLoginProviderSettings>(),
                OpenIdConnectClaimsMapping = openIdConnectMappedClaims.IsNullOrWhiteSpace()
                    ? new List<JsonClaimMapDto>()
                    : openIdConnectMappedClaims.FromJsonString<List<JsonClaimMapDto>>(),

                WsFederation_IsDeactivated = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated, tenantId),
                WsFederation = wsFederationSettings.IsNullOrWhiteSpace()
                    ? new WsFederationExternalLoginProviderSettings()
                    : wsFederationSettings.FromJsonString<WsFederationExternalLoginProviderSettings>(),
                WsFederationClaimsMapping = wsFederationMappedClaims.IsNullOrWhiteSpace()
                    ? new List<JsonClaimMapDto>()
                    : wsFederationMappedClaims.FromJsonString<List<JsonClaimMapDto>>(),
            };
        }

        private async Task<FulcrumIntegrationSettingsEditDto> GetFulcrumIntegrationSettingsAsync()
        {
            return new FulcrumIntegrationSettingsEditDto
            {
                FulcrumIntegrationIsEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.FulcrumIntegration.IsEnabled),
                FulcrumCustomerNumber = await SettingManager.GetSettingValueAsync(AppSettings.FulcrumIntegration.CustomerNumber),
                FulcrumUserName = await SettingManager.GetSettingValueAsync(AppSettings.FulcrumIntegration.UserName),
                FulcrumPassword = await SettingManager.GetSettingValueAsync(AppSettings.FulcrumIntegration.Password),
            };
        }

        private async Task<IntegrationSettingsEditDto> GetIntegrationSettingsAsync()
        {
            var integrationSettings = new IntegrationSettingsEditDto { };

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                integrationSettings.Gps = await GetGpsIntegrationSettingsAsync();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.FulcrumIntegration))
            {
                integrationSettings.Fulcrum = await GetFulcrumIntegrationSettingsAsync();
            }
            return integrationSettings;
        }
        #endregion

        private void AddSettingValue(List<SettingInfo> settingValues, string name, string value)
        {
            settingValues.Add(new SettingInfo(name, value));
        }

        #region Update Settings
        public async Task UpdateAllSettings(TenantSettingsEditDto input)
        {
            var tenantId = await AbpSession.GetTenantIdAsync();

            var settingValues = new List<SettingInfo>();

            UpdateUserManagementSettings(settingValues, input.UserManagement);
            await UpdateSecuritySettingsAsync(settingValues, input.Security);
            UpdateBillingSettings(settingValues, input.Billing);
            await UpdateEmailSettingsAsync(settingValues, input.Email);
            await UpdateExternalLoginSettingsAsync(settingValues, input.ExternalLoginProviderSettings);

            //Time Zone
            if (Clock.SupportsMultipleTimezone)
            {
                var oldTimezone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, tenantId);
                string newTimezone;
                if (input.General.Timezone.IsNullOrEmpty())
                {
                    var defaultValue = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Tenant, tenantId);
                    newTimezone = defaultValue;
                }
                else
                {
                    newTimezone = input.General.Timezone;
                }
                if (oldTimezone != newTimezone)
                {
                    AddSettingValue(settingValues, TimingSettingNames.TimeZone, newTimezone);
                    var offices = await (await _officeRepository.GetQueryAsync()).ToListAsync();
                    foreach (var office in offices)
                    {
                        office.DefaultStartTime = office.DefaultStartTime?.ConvertTimeZoneTo(oldTimezone).ConvertTimeZoneFrom(newTimezone);
                    }
                }
            }

            if (!_multiTenancyConfig.IsEnabled)
            {
                UpdateOtherSettings(settingValues, input.OtherSettings);

                input.ValidateHostSettings();

                SettingManager.UpdateSmsSettings(settingValues, input.Sms);

                await SettingManager.ChangeSettingForApplicationAsync(AppSettings.General.WebSiteRootAddress, input.General.WebSiteRootAddress);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                UpdateGpsIntegrationSettings(settingValues, input.Integration.Gps);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.FulcrumIntegration))
            {
                UpdateFulcrumIntegrationSettings(settingValues, input.Integration.Fulcrum);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowPaymentProcessingFeature))
            {
                UpdatePaymentSettings(settingValues, input.Payment);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings))
            {
                UpdateTruxSettings(settingValues, input.Trux);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingLuckStoneEarnings))
            {
                UpdateLuckStoneSettings(settingValues, input.LuckStone);
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingIronSheepdogEarnings))
            {
                UpdateIronSheepdogSettings(settingValues, input.IronSheepdog);
            }

            UpdateFuelSettings(settingValues, input.Fuel);

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.HaulZone))
            {
                UpdateHaulZoneSettings(settingValues, input.HaulZone);
            }

            if (await FeatureChecker.IsEnabledAsync(false, AppFeatures.GpsIntegrationFeature, AppFeatures.SmsIntegrationFeature, AppFeatures.DispatchingFeature))
            {
                await UpdateDispatchingAndMessagingSettingsAsync(settingValues, input.DispatchingAndMessaging);
            }

            await UpdateTicketsSettingsAsync(settingValues, input.Tickets);

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature))
            {
                await UpdateLeaseHaulerSettingsAsync(settingValues, input.LeaseHaulers);
            }

            AddSettingValue(settingValues, AppSettings.Quote.DefaultNotes, input.Quote.QuoteDefaultNote);
            AddSettingValue(settingValues, AppSettings.Quote.EmailSubjectTemplate, input.Quote.QuoteEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.Quote.EmailBodyTemplate, input.Quote.QuoteEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate, input.Quote.QuoteChangedNotificationEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.Quote.ChangedNotificationEmail.BodyTemplate, input.Quote.QuoteChangedNotificationEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.Quote.PromptForDisplayingQuarryInfoOnQuotes, input.Quote.PromptForDisplayingQuarryInfoOnQuotes.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Quote.GeneralTermsAndConditions, input.Quote.QuoteGeneralTermsAndConditions);
            AddSettingValue(settingValues, AppSettings.Job.HideBedConstruction, input.Job.HideBedConstruction.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Job.HideSubContractorRate, input.Job.HideSubContractorRate.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Job.HideTaxInformation, input.Job.HideTaxInformation.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Job.HideChargeTo, input.Job.HideChargeTo.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Order.EmailSubjectTemplate, input.General.OrderEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.Order.EmailBodyTemplate, input.General.OrderEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.Receipt.EmailSubjectTemplate, input.General.ReceiptEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.Receipt.EmailBodyTemplate, input.General.ReceiptEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.General.CompanyName, input.General.CompanyName);
            AddSettingValue(settingValues, AppSettings.General.DefaultMapLocationAddress, input.General.DefaultMapLocationAddress);
            AddSettingValue(settingValues, AppSettings.General.DefaultMapLocation, input.General.DefaultMapLocation);
            AddSettingValue(settingValues, AppSettings.General.CurrencySymbol, input.General.CurrencySymbol);
            AddSettingValue(settingValues, AppSettings.General.UserDefinedField1, input.General.UserDefinedField1);
            AddSettingValue(settingValues, AppSettings.General.ValidateDriverAndTruckOnTickets, (!input.General.DontValidateDriverAndTruckOnTickets).ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.ShowDriverNamesOnPrintedOrder, input.General.ShowDriverNamesOnPrintedOrder.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.ShowLoadAtOnPrintedOrder, input.General.ShowLoadAtOnPrintedOrder.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles, input.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices, input.General.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.SplitBillingByOffices, input.General.SplitBillingByOffices.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.ShowOfficeOnTicketsByDriver, input.General.ShowOfficeOnTicketsByDriver.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.ShowAggregateCost, input.General.ShowAggregateCost.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders, input.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.DriverOrderNotification.EmailTitle, input.General.DriverOrderEmailTitle);
            AddSettingValue(settingValues, AppSettings.DriverOrderNotification.EmailBody, input.General.DriverOrderEmailBody);
            AddSettingValue(settingValues, AppSettings.DriverOrderNotification.Sms, input.General.DriverOrderSms);

            AddSettingValue(settingValues, AppSettings.General.UseShifts, input.General.UseShifts.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.General.ShiftName1, input.General.ShiftName1);
            AddSettingValue(settingValues, AppSettings.General.ShiftName2, input.General.ShiftName2);
            AddSettingValue(settingValues, AppSettings.General.ShiftName3, input.General.ShiftName3);

            AddSettingValue(settingValues, AppSettings.EmailTemplate.UserEmailSubjectTemplate, input.EmailTemplate.UserEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.UserEmailBodyTemplate, input.EmailTemplate.UserEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.DriverEmailSubjectTemplate, input.EmailTemplate.DriverEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.DriverEmailBodyTemplate, input.EmailTemplate.DriverEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.LeaseHaulerInviteEmailSubjectTemplate, input.EmailTemplate.LeaseHaulerInviteEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.LeaseHaulerInviteEmailBodyTemplate, input.EmailTemplate.LeaseHaulerInviteEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.LeaseHaulerDriverEmailSubjectTemplate, input.EmailTemplate.LeaseHaulerDriverEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.LeaseHaulerDriverEmailBodyTemplate, input.EmailTemplate.LeaseHaulerDriverEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailSubjectTemplate, input.EmailTemplate.LeaseHaulerJobRequestEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailBodyTemplate, input.EmailTemplate.LeaseHaulerJobRequestEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.CustomerPortalEmailSubjectTemplate, input.EmailTemplate.CustomerPortalEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.EmailTemplate.CustomerPortalEmailBodyTemplate, input.EmailTemplate.CustomerPortalEmailBodyTemplate);

            var defaultJob = await (await _timeClassificationRepository.GetQueryAsync())
                    .Select(x => new
                    {
                        x.Id,
                        x.IsProductionBased,
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.TimeAndPay.TimeTrackingDefaultTimeClassificationId);

            if (defaultJob == null)
            {
                throw new UserFriendlyException(L("DefaultJobIsRequired"));
            }
            if (!input.TimeAndPay.AllowProductionPay)
            {
                if (defaultJob.IsProductionBased)
                {
                    throw new UserFriendlyException(L("CannotDisallowProductionPayBecauseOfDefaultJob"));
                }
                //var hasEmployeeTimeClassifications = await (await _employeeTimeClassificationRepository.GetQueryAsync())
                //    .Where(x => x.TimeClassification.IsProductionBased)
                //    .AnyAsync();
                //if (hasEmployeeTimeClassifications)
                //{
                //    throw new UserFriendlyException(L("CannotDisallowProductionPayBecauseOfDrivers"));
                //}
            }
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            if (!allowProductionPay && input.TimeAndPay.AllowProductionPay)
            {
                var driversWithMissingClassifications = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.OfficeId != null && !x.EmployeeTimeClassifications.Any(t => t.TimeClassification.IsProductionBased))
                    .Select(x => x.Id)
                    .ToListAsync();

                var productionPay = await (await _timeClassificationRepository.GetQueryAsync())
                    .Where(x => x.IsProductionBased)
                    .FirstOrDefaultAsync();

                if (productionPay == null)
                {
                    productionPay = new TimeClassification { TenantId = tenantId, Name = "Production Pay", IsProductionBased = true };
                    await _timeClassificationRepository.InsertAndGetIdAsync(productionPay);
                }

                foreach (var driverId in driversWithMissingClassifications)
                {
                    await _employeeTimeClassificationRepository.InsertAsync(new EmployeeTimeClassification
                    {
                        DriverId = driverId,
                        TimeClassificationId = productionPay.Id,
                    });
                }
            }
            else if (allowProductionPay && !input.TimeAndPay.AllowProductionPay)
            {
                var today = await GetToday();
                var orderLines = await (await _orderLineRepository.GetQueryAsync()).Where(x => x.Order.DeliveryDate >= today && x.ProductionPay).ToListAsync();
                orderLines.ForEach(x => x.ProductionPay = false);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            AddSettingValue(settingValues, AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, input.TimeAndPay.TimeTrackingDefaultTimeClassificationId.ToString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.BasePayOnHourlyJobRate, input.TimeAndPay.BasePayOnHourlyJobRate.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate, input.TimeAndPay.UseDriverSpecificHourlyJobRate.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.AllowProductionPay, input.TimeAndPay.AllowProductionPay.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.DefaultToProductionPay, input.TimeAndPay.DefaultToProductionPay.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate, input.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs, input.TimeAndPay.PreventProductionPayOnHourlyJobs.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.DriverIsPaidForLoadBasedOn, input.TimeAndPay.DriverIsPaidForLoadBasedOn.ToIntString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.AllowLoadBasedRates, input.TimeAndPay.AllowLoadBasedRates.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.PayStatementReportOrientation, input.TimeAndPay.PayStatementReportOrientation.ToIntString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.ShowFreightRateOnDriverPayStatementReport, input.TimeAndPay.ShowFreightRateOnDriverPayStatementReport.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.ShowDriverPayRateOnDriverPayStatementReport, input.TimeAndPay.ShowDriverPayRateOnDriverPayStatementReport.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.TimeAndPay.ShowQuantityOnDriverPayStatementReport, input.TimeAndPay.ShowQuantityOnDriverPayStatementReport.ToLowerCaseString());

            await SettingManager.ChangeSettingsForTenantAsync(tenantId, settingValues);

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Settings, new ChangedSettings()));
        }

        private void UpdateOtherSettings(List<SettingInfo> settingValues, TenantOtherSettingsEditDto input)
        {
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.IsQuickThemeSelectEnabled,
                input.IsQuickThemeSelectEnabled.ToString().ToLowerInvariant()
            );
        }

        private void UpdateBillingSettings(List<SettingInfo> settingValues, TenantBillingSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.TenantManagement.BillingLegalName, input.LegalName);
            AddSettingValue(settingValues, AppSettings.TenantManagement.BillingAddress, input.Address);
            AddSettingValue(settingValues, AppSettings.TenantManagement.BillingPhoneNumber, input.PhoneNumber);
            AddSettingValue(settingValues, AppSettings.Invoice.RemitToInformation, input.RemitToInformation);
            AddSettingValue(settingValues, AppSettings.Invoice.DefaultMessageOnInvoice, input.DefaultMessageOnInvoice);
            AddSettingValue(settingValues, AppSettings.Invoice.EmailSubjectTemplate, input.InvoiceEmailSubjectTemplate);
            AddSettingValue(settingValues, AppSettings.Invoice.EmailBodyTemplate, input.InvoiceEmailBodyTemplate);
            AddSettingValue(settingValues, AppSettings.TenantManagement.BillingTaxVatNo, input.TaxVatNo);
            AddSettingValue(settingValues, AppSettings.Invoice.TaxCalculationType, ((int)input.TaxCalculationType).ToString("N0"));
            AddSettingValue(settingValues, AppSettings.Invoice.AutopopulateDefaultTaxRate, (input.TaxCalculationType == TaxCalculationType.NoCalculation ? false : input.AutopopulateDefaultTaxRate).ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Invoice.TermsAndConditions, input.InvoiceTermsAndConditions);
            AddSettingValue(settingValues, AppSettings.Invoice.DefaultTaxRate, (input.DefaultTaxRate ?? 0).ToString());
            AddSettingValue(settingValues, AppSettings.Invoice.InvoiceTemplate, ((int)input.InvoiceTemplate).ToString("N0"));
            AddSettingValue(settingValues, AppSettings.Invoice.AllowInvoiceApprovalFlow, input.AllowInvoiceApprovalFlow.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Invoice.HideLoadAtAndDeliverToOnHourlyInvoices, input.HideLoadAtAndDeliverToOnHourlyInvoices.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Invoice.CalculateMinimumFreightAmount, input.CalculateMinimumFreightAmount.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Invoice.MinimumFreightAmountForTons, (input.MinimumFreightAmountForTons ?? 0).ToString());
            AddSettingValue(settingValues, AppSettings.Invoice.MinimumFreightAmountForHours, (input.MinimumFreightAmountForHours ?? 0).ToString());
            AddSettingValue(settingValues, AppSettings.Invoice.Quickbooks.IntegrationKind, ((int)(input.QuickbooksIntegrationKind ?? 0)).ToString());
            AddSettingValue(settingValues, AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix, input.QuickbooksInvoiceNumberPrefix);
            AddSettingValue(settingValues, AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName, input.QbdTaxAgencyVendorName);
            AddSettingValue(settingValues, AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName, input.QbdDefaultIncomeAccountName);
            AddSettingValue(settingValues, AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType, input.QbdDefaultIncomeAccountType);
            AddSettingValue(settingValues, AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName, input.QbdAccountsReceivableAccountName);
            AddSettingValue(settingValues, AppSettings.Invoice.QuickbooksDesktop.TaxAccountName, input.QbdTaxAccountName);
        }

        private async Task UpdateEmailSettingsAsync(List<SettingInfo> settingValues, TenantEmailSettingsEditDto input)
        {
            if (_multiTenancyConfig.IsEnabled && !DispatcherWebConsts.AllowTenantsToChangeEmailSettings)
            {
                return;
            }
            var useHostDefaultEmailSettings = _multiTenancyConfig.IsEnabled && input.UseHostDefaultEmailSettings;

            if (useHostDefaultEmailSettings)
            {
                input = new TenantEmailSettingsEditDto
                {
                    UseHostDefaultEmailSettings = true,
                    DefaultFromAddress = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.DefaultFromAddress),
                    DefaultFromDisplayName = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.DefaultFromDisplayName),
                    SmtpHost = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.Host),
                    SmtpPort = await SettingManager.GetSettingValueForApplicationAsync<int>(EmailSettingNames.Smtp.Port),
                    SmtpUserName = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.UserName),
                    SmtpPassword = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.Password),
                    SmtpDomain = await SettingManager.GetSettingValueForApplicationAsync(EmailSettingNames.Smtp.Domain),
                    SmtpEnableSsl = await SettingManager.GetSettingValueForApplicationAsync<bool>(EmailSettingNames.Smtp.EnableSsl),
                    SmtpUseDefaultCredentials = await SettingManager.GetSettingValueForApplicationAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials),
                };
            }

            AddSettingValue(settingValues, AppSettings.Email.UseHostDefaultEmailSettings, useHostDefaultEmailSettings.ToString().ToLowerInvariant());
            AddSettingValue(settingValues, EmailSettingNames.DefaultFromAddress, input.DefaultFromAddress);
            AddSettingValue(settingValues, EmailSettingNames.DefaultFromDisplayName, input.DefaultFromDisplayName);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Host, input.SmtpHost);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Port, input.SmtpPort.ToString(CultureInfo.InvariantCulture));
            AddSettingValue(settingValues, EmailSettingNames.Smtp.UserName, input.SmtpUserName);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Password, input.SmtpPassword);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.Domain, input.SmtpDomain);
            AddSettingValue(settingValues, EmailSettingNames.Smtp.EnableSsl, input.SmtpEnableSsl.ToString().ToLowerInvariant());
            AddSettingValue(settingValues, EmailSettingNames.Smtp.UseDefaultCredentials, input.SmtpUseDefaultCredentials.ToString().ToLowerInvariant());
        }

        private void UpdateUserManagementSettings(List<SettingInfo> settingValues, TenantUserManagementSettingsEditDto settings)
        {
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.AllowSelfRegistration,
                settings.AllowSelfRegistration.ToLowerCaseString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault,
                settings.IsNewRegisteredUserActiveByDefault.ToLowerCaseString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin,
                settings.IsEmailConfirmationRequiredForLogin.ToLowerCaseString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.UseCaptchaOnRegistration,
                settings.UseCaptchaOnRegistration.ToLowerCaseString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.UseCaptchaOnLogin,
                settings.UseCaptchaOnLogin.ToString().ToLowerInvariant()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.IsCookieConsentEnabled,
                settings.IsCookieConsentEnabled.ToLowerCaseString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.AllowUsingGravatarProfilePicture,
                settings.AllowUsingGravatarProfilePicture.ToString().ToLowerInvariant()
            );

            UpdateUserManagementSessionTimeOutSettings(settingValues, settings.SessionTimeOutSettings);
        }

        private void UpdateUserManagementSessionTimeOutSettings(List<SettingInfo> settingValues, SessionTimeOutSettingsEditDto settings)
        {
            if (settings == null)
            {
                return;
            }

            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.IsEnabled,
                settings.IsEnabled.ToString().ToLowerInvariant()
            );
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.TimeOutSecond,
                settings.TimeOutSecond.ToString()
            );
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond,
                settings.ShowTimeOutNotificationSecond.ToString()
            );
            AddSettingValue(
                settingValues,
                AppSettings.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut,
                settings.ShowLockScreenWhenTimedOut.ToString()
            );
        }

        private async Task UpdateSecuritySettingsAsync(List<SettingInfo> settingValues, SecuritySettingsEditDto settings)
        {
            if (settings.UseDefaultPasswordComplexitySettings)
            {
                UpdatePasswordComplexitySettings(settingValues, settings.DefaultPasswordComplexity);
            }
            else
            {
                UpdatePasswordComplexitySettings(settingValues, settings.PasswordComplexity);
            }

            UpdateUserLockOutSettings(settingValues, settings.UserLockOut);
            await UpdateTwoFactorLoginSettingsAsync(settingValues, settings.TwoFactorLogin);
            await UpdateOneConcurrentLoginPerUserSetting(settings.AllowOneConcurrentLoginPerUser);
        }

        private void UpdatePasswordComplexitySettings(List<SettingInfo> settingValues, PasswordComplexitySetting settings)
        {
            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireDigit,
                settings.RequireDigit.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireLowercase,
                settings.RequireLowercase.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric,
                settings.RequireNonAlphanumeric.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase,
                settings.RequireUppercase.ToString()
            );

            AddSettingValue(
                settingValues,
                AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength,
                settings.RequiredLength.ToString()
            );
        }

        private void UpdateUserLockOutSettings(List<SettingInfo> settingValues, UserLockOutSettingsEditDto settings)
        {
            AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled, settings.IsEnabled.ToLowerCaseString());
            AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds, settings.DefaultAccountLockoutSeconds.ToString());
            AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout, settings.MaxFailedAccessAttemptsBeforeLockout.ToString());
        }

        private async Task UpdateTwoFactorLoginSettingsAsync(List<SettingInfo> settingValues, TwoFactorLoginSettingsEditDto settings)
        {
            if (_multiTenancyConfig.IsEnabled
                && !await IsTwoFactorLoginEnabledForApplicationAsync()) //Two factor login can not be used by tenants if disabled by the host
            {
                return;
            }

            AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled, settings.IsEnabled.ToLowerCaseString());
            AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled, settings.IsRememberBrowserEnabled.ToLowerCaseString());

            if (!_multiTenancyConfig.IsEnabled)
            {
                //These settings can only be changed by host, in a multitenant application.
                AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEmailProviderEnabled, settings.IsEmailProviderEnabled.ToLowerCaseString());
                AddSettingValue(settingValues, AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsSmsProviderEnabled, settings.IsSmsProviderEnabled.ToLowerCaseString());
                AddSettingValue(settingValues, AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled, settings.IsGoogleAuthenticatorEnabled.ToLowerCaseString());
            }
        }


        private void UpdateGpsIntegrationSettings(List<SettingInfo> settingValues, GpsIntegrationSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.GpsIntegration.Platform, input.Platform.ToIntString());

            //AddSettingValue(settingValues, AppSettings.GpsIntegration.DtdTracker.AccountName, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.Server, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.Database, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.User, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.Password, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.MapBaseUrl, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Samsara.ApiToken, null);
            //AddSettingValue(settingValues, AppSettings.GpsIntegration.Samsara.BaseUrl, null);

            if (input.Platform == GpsPlatform.DtdTracker)
            {
                AddSettingValue(settingValues, AppSettings.GpsIntegration.DtdTracker.EnableDriverAppGps, input.DtdTracker.EnableDriverAppGps.ToLowerCaseString());
                //AddSettingValue(settingValues, AppSettings.GpsIntegration.DtdTracker.AccountName, input.DtdTracker.AccountName);
            }
            else if (input.Platform == GpsPlatform.Geotab)
            {
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.Server, input.Geotab.Server);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.Database, input.Geotab.Database);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.User, input.Geotab.User);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.Password, input.Geotab.Password);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Geotab.MapBaseUrl, input.Geotab.MapBaseUrl);
            }
            else if (input.Platform == GpsPlatform.Samsara)
            {
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Samsara.ApiToken, input.Samsara.ApiToken);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.Samsara.BaseUrl, input.Samsara.BaseUrl);
            }
            else if (input.Platform == GpsPlatform.IntelliShift)
            {
                AddSettingValue(settingValues, AppSettings.GpsIntegration.IntelliShift.User, input.IntelliShift.User);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.IntelliShift.Password, input.IntelliShift.Password);
            }
        }

        private async Task UpdateDispatchingAndMessagingSettingsAsync(List<SettingInfo> settingValues, DispatchingAndMessagingSettingsEditDto input)
        {
            var tenantId = await AbpSession.GetTenantIdAsync();
            var timezone = await GetTimezone();
            var allowCounterSalesForTenant = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant);
            var allowCounterSalesForTenantWasUnchecked = allowCounterSalesForTenant && !input.AllowCounterSalesForTenant
                && await _settingAvailabilityProvider.IsSettingAvailableAsync(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant);

            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.DispatchVia, input.DispatchVia.ToIntString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AllowSmsMessages, input.AllowSmsMessages.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.SendSmsOnDispatching, input.SendSmsOnDispatching.ToIntString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.SmsPhoneNumber, input.SmsPhoneNumber);
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate, input.DriverDispatchSms);
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate, input.DriverStartTime);
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp, input.HideTicketControlsInDriverApp.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.RequiredTicketEntry, input.RequiredTicketEntry.ToIntString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.RequireSignature, input.RequireSignature.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.RequireTicketPhoto, input.RequireTicketPhoto.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.TextForSignatureView, input.TextForSignatureView);
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.DispatchesLockedToTruck, input.DispatchesLockedToTruck.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.DefaultStartTime, input.DefaultStartTime.ConvertTimeZoneFrom(timezone).ToString("s"));
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule, input.ShowTrailersOnSchedule.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.ShowStaggerTimes, input.ShowStaggerTimes.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers, input.AllowSchedulingTrucksWithoutDrivers.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.ValidateUtilization, input.ValidateUtilization.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant, input.AllowCounterSalesForTenant.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AutoGenerateTicketNumbers, input.AutoGenerateTicketNumbers.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.DisableTicketNumberOnDriverApp, input.DisableTicketNumberOnDriverApp.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AllowLoadCountOnHourlyJobs, input.AllowLoadCountOnHourlyJobs.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AllowEditingTimeOnHourlyJobs, input.AllowEditingTimeOnHourlyJobs.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.AllowMultipleDispatchesToBeInProgressAtTheSameTime, input.AllowMultipleDispatchesToBeInProgressAtTheSameTime.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DispatchingAndMessaging.HideDriverAppTimeScreen, input.HideDriverAppTimeScreen.ToLowerCaseString());
            if (await PermissionChecker.IsGrantedAsync(AppPermissions.DebugDriverApp))
            {
                await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DriverApp.LoggingLevel, input.LoggingLevel.ToIntString());
                await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.DriverApp.SyncDataOnButtonClicks, input.SyncDataOnButtonClicks.ToLowerCaseString());
            }

            if (allowCounterSalesForTenantWasUnchecked)
            {
                var userSettingsToReset = new[]
                {
                    AppSettings.DispatchingAndMessaging.AllowCounterSalesForUser,
                    AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly,
                    AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId,
                    AppSettings.DispatchingAndMessaging.DefaultMaterialItemId,
                    AppSettings.DispatchingAndMessaging.DefaultMaterialUomId,
                    AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber,
                };
                var userIds = await (await UserManager.GetQueryAsync()).Select(x => x.Id).ToListAsync();
                var userSettingValues = new List<SettingInfo>();
                foreach (var settingToReset in userSettingsToReset)
                {
                    var defaultValue = _settingDefinitionManager.GetSettingDefinition(settingToReset).DefaultValue;
                    AddSettingValue(userSettingValues, settingToReset, defaultValue);
                }
                foreach (var userId in userIds)
                {
                    var userIdentifier = new UserIdentifier(tenantId, userId);
                    await SettingManager.ChangeSettingsForUserAsync(userIdentifier, userSettingValues);
                }
            }
        }

        private async Task UpdateTicketsSettingsAsync(List<SettingInfo> settingValues, TicketsSettingsEditDto input)
        {
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.Tickets.PrintPdfTicket, input.PrintPdfTickets.ToLowerCaseString());
        }

        private async Task ChangeSettingForTenantIfAvailableAsync(List<SettingInfo> settingValues, string settingName, string newValue)
        {
            if (await _settingAvailabilityProvider.IsSettingAvailableAsync(settingName))
            {
                AddSettingValue(settingValues, settingName, newValue);
            }
        }

        private async Task UpdateLeaseHaulerSettingsAsync(List<SettingInfo> settingValues, LeaseHaulerSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnQuote, input.ShowLeaseHaulerRateOnQuote.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.LeaseHaulers.ShowLeaseHaulerRateOnOrder, input.ShowLeaseHaulerRateOnOrder.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks, input.AllowSubcontractorsToDriveCompanyOwnedTrucks.ToLowerCaseString());
            await ChangeSettingForTenantIfAvailableAsync(settingValues, AppSettings.LeaseHaulers.AllowLeaseHaulerRequestProcess, input.AllowLeaseHaulerRequestProcess.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.LeaseHaulers.BrokerFee, input.BrokerFee.ToString());
            AddSettingValue(settingValues, AppSettings.LeaseHaulers.ThankYouForTrucksTemplate, input.ThankYouForTrucksTemplate);
            AddSettingValue(settingValues, AppSettings.LeaseHaulers.NotAllowSchedulingLeaseHaulersWithExpiredInsurance, input.NotAllowSchedulingLeaseHaulersWithExpiredInsurance.ToLowerCaseString());
        }

        private void UpdatePaymentSettings(List<SettingInfo> settingValues, PaymentSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.General.PaymentProcessor, ((int)input.PaymentProcessor).ToString("N0"));
            AddSettingValue(settingValues, AppSettings.Heartland.PublicKey, input.HeartlandPublicKey);
            if (input.HeartlandSecretKey != DispatcherWebConsts.PasswordHasntBeenChanged)
            {
                AddSettingValue(settingValues, AppSettings.Heartland.SecretKey, input.HeartlandSecretKey);
            }
        }

        private void UpdateTruxSettings(List<SettingInfo> settingValues, TruxSettingsEditDto input)
        {
            if (input.AllowImportingTruxEarnings && !(input.TruxCustomerId > 0))
            {
                throw new UserFriendlyException("Trux Customer is required");
            }
            AddSettingValue(settingValues, AppSettings.Trux.AllowImportingTruxEarnings, input.AllowImportingTruxEarnings.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Trux.UseForProductionPay, input.UseForProductionPay.ToLowerCaseString());
            if (input.AllowImportingTruxEarnings)
            {
                AddSettingValue(settingValues, AppSettings.Trux.TruxCustomerId, input.TruxCustomerId.ToString());
            }
        }

        private void UpdateLuckStoneSettings(List<SettingInfo> settingValues, LuckStoneSettingsEditDto input)
        {
            if (input.AllowImportingLuckStoneEarnings)
            {
                if (!(input.LuckStoneCustomerId > 0))
                {
                    throw new UserFriendlyException("Luck Stone Customer is required");
                }
                if (string.IsNullOrEmpty(input.HaulerRef))
                {
                    throw new UserFriendlyException("HaulerRef is required");
                }
            }
            AddSettingValue(settingValues, AppSettings.LuckStone.AllowImportingLuckStoneEarnings, input.AllowImportingLuckStoneEarnings.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.LuckStone.UseForProductionPay, input.UseForProductionPay.ToLowerCaseString());
            if (input.AllowImportingLuckStoneEarnings)
            {
                AddSettingValue(settingValues, AppSettings.LuckStone.LuckStoneCustomerId, input.LuckStoneCustomerId.ToString());
                AddSettingValue(settingValues, AppSettings.LuckStone.HaulerRef, input.HaulerRef);
            }
        }

        private void UpdateIronSheepdogSettings(List<SettingInfo> settingValues, IronSheepdogSettingsEditDto input)
        {
            if (input.AllowImportingIronSheepdogEarnings)
            {
                if (!(input.IronSheepdogCustomerId > 0))
                {
                    throw new UserFriendlyException("Iron Sheepdog Customer is required");
                }
            }
            AddSettingValue(settingValues, AppSettings.IronSheepdog.AllowImportingIronSheepdogEarnings, input.AllowImportingIronSheepdogEarnings.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.IronSheepdog.UseForProductionPay, input.UseForProductionPay.ToLowerCaseString());
            if (input.AllowImportingIronSheepdogEarnings)
            {
                AddSettingValue(settingValues, AppSettings.IronSheepdog.IronSheepdogCustomerId, input.IronSheepdogCustomerId.ToString());
            }
        }

        private void UpdateFuelSettings(List<SettingInfo> settingValues, FuelSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.Fuel.ShowFuelSurcharge, input.ShowFuelSurcharge.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.Fuel.ShowFuelSurchargeOnInvoice, input.ShowFuelSurchargeOnInvoice.ToIntString());
            AddSettingValue(settingValues, AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice, (input.ItemIdToUseForFuelSurchargeOnInvoice ?? 0).ToString());
            AddSettingValue(settingValues, AppSettings.Fuel.OnlyEvenIncrements, input.OnlyEvenIncrements.ToLowerCaseString());
        }

        private void UpdateHaulZoneSettings(List<SettingInfo> settingValues, HaulZoneSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.HaulZone.HaulRateCalculation.BaseUomIdForCod, (input.HaulRateCalculationBaseUomIdForCod ?? 0).ToString());
            AddSettingValue(settingValues, AppSettings.HaulZone.HaulRateCalculation.BaseUomId, (input.HaulRateCalculationBaseUomId ?? 0).ToString());
        }

        private async Task UpdateOneConcurrentLoginPerUserSetting(bool allowOneConcurrentLoginPerUser)
        {
            if (_multiTenancyConfig.IsEnabled)
            {
                return;
            }
            await SettingManager.ChangeSettingForApplicationAsync(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser, allowOneConcurrentLoginPerUser.ToString());
        }

        private async Task UpdateExternalLoginSettingsAsync(List<SettingInfo> settingValues, ExternalLoginProviderSettingsEditDto input)
        {
            if (input == null)
            {
                return;
            }

            var tenantId = await AbpSession.GetTenantIdAsync();

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Facebook,
                input.Facebook == null || !input.Facebook.IsValid() ? "" : input.Facebook.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated,
                input.Facebook_IsDeactivated.ToString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Google,
                input.Google == null || !input.Google.IsValid() ? "" : input.Google.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated,
                input.Google_IsDeactivated.ToString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Twitter,
                input.Twitter == null || !input.Twitter.IsValid() ? "" : input.Twitter.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated,
                input.Twitter_IsDeactivated.ToString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Microsoft,
                input.Microsoft == null || !input.Microsoft.IsValid() ? "" : input.Microsoft.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated,
                input.Microsoft_IsDeactivated.ToString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect,
                input.OpenIdConnect == null || !input.OpenIdConnect.IsValid() ? "" : input.OpenIdConnect.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated,
                input.OpenIdConnect_IsDeactivated.ToString()
            );

            var openIdConnectMappedClaimsValue = "";
            if (input.OpenIdConnect == null || !input.OpenIdConnect.IsValid() || input.OpenIdConnectClaimsMapping.IsNullOrEmpty())
            {
                openIdConnectMappedClaimsValue = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims);//set default value
            }
            else
            {
                openIdConnectMappedClaimsValue = input.OpenIdConnectClaimsMapping.ToJsonString();
            }

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.OpenIdConnectMappedClaims,
                openIdConnectMappedClaimsValue
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.WsFederation,
                input.WsFederation == null || !input.WsFederation.IsValid() ? "" : input.WsFederation.ToJsonString()
            );

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated,
                input.WsFederation_IsDeactivated.ToString()
            );

            var wsFederationMappedClaimsValue = "";
            if (input.WsFederation == null || !input.WsFederation.IsValid() || input.WsFederationClaimsMapping.IsNullOrEmpty())
            {
                wsFederationMappedClaimsValue = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.ExternalLoginProvider.WsFederationMappedClaims);//set default value
            }
            else
            {
                wsFederationMappedClaimsValue = input.WsFederationClaimsMapping.ToJsonString();
            }

            AddSettingValue(
                settingValues,
                AppSettings.ExternalLoginProvider.WsFederationMappedClaims,
                wsFederationMappedClaimsValue
            );

            ExternalLoginOptionsCacheManager.ClearCache();
        }

        private void UpdateFulcrumIntegrationSettings(List<SettingInfo> settingValues, FulcrumIntegrationSettingsEditDto input)
        {
            AddSettingValue(settingValues, AppSettings.FulcrumIntegration.IsEnabled, input.FulcrumIntegrationIsEnabled.ToLowerCaseString());
            AddSettingValue(settingValues, AppSettings.FulcrumIntegration.CustomerNumber, input.FulcrumIntegrationIsEnabled ? input.FulcrumCustomerNumber : "");
            AddSettingValue(settingValues, AppSettings.FulcrumIntegration.UserName, input.FulcrumIntegrationIsEnabled ? input.FulcrumUserName : "");
            AddSettingValue(settingValues, AppSettings.FulcrumIntegration.Password, input.FulcrumIntegrationIsEnabled ? input.FulcrumPassword : "");
        }
        #endregion

        #region Others

        public async Task ClearLogo()
        {
            var tenant = await GetCurrentTenantAsync();

            if (!tenant.HasLogo())
            {
                return;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.LogoId.Value);
            if (logoObject != null)
            {
                await _binaryObjectManager.DeleteAsync(tenant.LogoId.Value);
            }

            tenant.ClearLogo();
        }

        public async Task ClearReportsLogo()
        {
            var tenant = await GetCurrentTenantAsync();

            if (tenant.ReportsLogoId == null)
            {
                return;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.ReportsLogoId.Value);
            if (logoObject != null)
            {
                await _binaryObjectManager.DeleteAsync(tenant.ReportsLogoId.Value);
            }

            tenant.ReportsLogoId = null;
            tenant.ReportsLogoFileType = null;
        }

        public async Task ClearCustomCss()
        {
            var tenant = await GetCurrentTenantAsync();

            if (!tenant.CustomCssId.HasValue)
            {
                return;
            }

            var cssObject = await _binaryObjectManager.GetOrNullAsync(tenant.CustomCssId.Value);
            if (cssObject != null)
            {
                await _binaryObjectManager.DeleteAsync(tenant.CustomCssId.Value);
            }

            tenant.CustomCssId = null;
        }

        public string GetDefaultDriverDispatchSmsTemplate()
        {
            return _settingDefinitionManager.GetSettingDefinition(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate).DefaultValue;
        }

        #endregion

        public async Task<bool> CanLinkDtdTrackerAccount()
        {
            var platform = await SettingManager.GetGpsPlatformAsync();
            var dtdTrackerAccountId = await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.DtdTracker.AccountId);
            return platform == GpsPlatform.DtdTracker && dtdTrackerAccountId == 0;
        }

        public async Task LinkDtdTrackerAccount(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            var account = await _dtdTrackerTelematics.GetAccountDetailsFromAccessToken(accessToken);
            if (!account.AccountName.IsNullOrEmpty() && account.AccountId > 0)
            {
                var tenantId = await AbpSession.GetTenantIdAsync();
                var settingValues = new List<SettingInfo>();
                AddSettingValue(settingValues, AppSettings.GpsIntegration.DtdTracker.AccountName, account.AccountName);
                AddSettingValue(settingValues, AppSettings.GpsIntegration.DtdTracker.AccountId, account.AccountId.ToString());
                AddSettingValue(settingValues, AppSettings.GpsIntegration.DtdTracker.UserId, account.UserId.ToString());
                await SettingManager.ChangeSettingsForTenantAsync(tenantId, settingValues);
            }
            else
            {
                Logger.Error("Received unexpected DTDTracker account " + Newtonsoft.Json.JsonConvert.SerializeObject(account));
            }
        }
    }
}
