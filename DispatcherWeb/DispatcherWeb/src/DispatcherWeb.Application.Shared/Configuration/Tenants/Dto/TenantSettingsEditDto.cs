using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Extensions;
using Abp.Runtime.Validation;
using Abp.Timing;
using DispatcherWeb.Configuration.Dto;
using DispatcherWeb.Configuration.Host.Dto;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class TenantSettingsEditDto
    {
        public GeneralSettingsEditDto General { get; set; }

        public TimeAndPaySettingsEditDto TimeAndPay { get; set; }

        [Required]
        public TenantUserManagementSettingsEditDto UserManagement { get; set; }

        public TenantEmailSettingsEditDto Email { get; set; }

        [Required]
        public SecuritySettingsEditDto Security { get; set; }

        public SmsSettingsEditDto Sms { get; set; }

        public TenantBillingSettingsEditDto Billing { get; set; }
        public TenantOtherSettingsEditDto OtherSettings { get; set; }

        public ExternalLoginProviderSettingsEditDto ExternalLoginProviderSettings { get; set; }

        public IntegrationSettingsEditDto Integration { get; set; }

        public PaymentSettingsEditDto Payment { get; set; }

        public DispatchingAndMessagingSettingsEditDto DispatchingAndMessaging { get; set; }

        public TicketsSettingsEditDto Tickets { get; set; }

        public LeaseHaulerSettingsEditDto LeaseHaulers { get; set; }

        public TruxSettingsEditDto Trux { get; set; }

        public LuckStoneSettingsEditDto LuckStone { get; set; }

        public IronSheepdogSettingsEditDto IronSheepdog { get; set; }

        public FuelSettingsEditDto Fuel { get; set; }

        public HaulZoneSettingsEditDto HaulZone { get; set; }

        public QuoteSettingsEditDto Quote { get; set; }

        public JobSettingsEditDto Job { get; set; }

        public EmailTemplateSettingsEditDto EmailTemplate { get; set; }

        /// <summary>
        /// This validation is done for single-tenant applications.
        /// Because, these settings can only be set by tenant in a single-tenant application.
        /// </summary>
        public void ValidateHostSettings()
        {
            var validationErrors = new List<ValidationResult>();
            if (Clock.SupportsMultipleTimezone && General == null)
            {
                validationErrors.Add(new ValidationResult("General settings can not be null", new[] { "General" }));
            }
            else
            {
                if (General.WebSiteRootAddress.IsNullOrEmpty())
                {
                    validationErrors.Add(new ValidationResult("General.WebSiteRootAddress can not be null or empty", new[] { "WebSiteRootAddress" }));
                }
            }

            if (Email == null)
            {
                validationErrors.Add(new ValidationResult("Email settings can not be null", new[] { "Email" }));
            }

            if (validationErrors.Count > 0)
            {
                throw new AbpValidationException("Method arguments are not valid! See ValidationErrors for details.", validationErrors);
            }
        }
    }
}
