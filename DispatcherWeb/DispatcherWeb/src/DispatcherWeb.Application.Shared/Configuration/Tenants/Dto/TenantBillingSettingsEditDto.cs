using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class TenantBillingSettingsEditDto
    {
        public string LegalName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string RemitToInformation { get; set; }
        public string DefaultMessageOnInvoice { get; set; }
        public string InvoiceEmailSubjectTemplate { get; set; }
        public string InvoiceEmailBodyTemplate { get; set; }
        public string TaxVatNo { get; set; }
        public TaxCalculationType TaxCalculationType { get; set; }
        public decimal? DefaultTaxRate { get; set; }
        public bool AutopopulateDefaultTaxRate { get; set; }
        public string InvoiceTermsAndConditions { get; set; }
        public InvoiceTemplateEnum InvoiceTemplate { get; set; }
        public bool AllowInvoiceApprovalFlow { get; set; }
        public bool HideLoadAtAndDeliverToOnHourlyInvoices { get; set; }
        public bool CalculateMinimumFreightAmount { get; set; }
        public decimal? MinimumFreightAmountForTons { get; set; }
        public decimal? MinimumFreightAmountForHours { get; set; }

        public QuickbooksIntegrationKind? QuickbooksIntegrationKind { get; set; }

        [StringLength(4)]
        public string QuickbooksInvoiceNumberPrefix { get; set; }

        //public bool IsQuickbooksConnected { get; set; }

        //QuickBooksDesktop
        public string QbdTaxAgencyVendorName { get; set; }
        public string QbdDefaultIncomeAccountName { get; set; }
        public string QbdDefaultIncomeAccountType { get; set; }
        public string QbdAccountsReceivableAccountName { get; set; }
        public string QbdTaxAccountName { get; set; }
        public List<SelectListDto> QbdIncomeAccountTypes { get; set; }
    }
}