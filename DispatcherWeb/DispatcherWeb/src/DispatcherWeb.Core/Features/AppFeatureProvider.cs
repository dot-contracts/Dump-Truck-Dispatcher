using Abp.Application.Features;
using Abp.Localization;
using Abp.Runtime.Validation;
using Abp.UI.Inputs;

namespace DispatcherWeb.Features
{
    public class AppFeatureProvider : FeatureProvider
    {
        public override void SetFeatures(IFeatureDefinitionContext context)
        {
            context.Create(
                AppFeatures.MaxUserCount,
                defaultValue: "0", //0 = unlimited
                displayName: L("MaximumUserCount"),
                description: L("MaximumUserCount_Description"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, int.MaxValue))
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                ValueTextNormalizer = value => value == "0" ? L("Unlimited") : new FixedLocalizableString(value),
                IsVisibleOnPricingTable = true,
            };

            #region ######## Example Features - You can delete them #########

            //context.Create("TestTenantScopeFeature", "false", L("TestTenantScopeFeature"), scope: FeatureScopes.Tenant);
            //context.Create("TestEditionScopeFeature", "false", L("TestEditionScopeFeature"), scope: FeatureScopes.Edition);

            //context.Create(
            //    AppFeatures.TestCheckFeature,
            //    defaultValue: "false",
            //    displayName: L("TestCheckFeature"),
            //    inputType: new CheckboxInputType()
            //)[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            //{
            //    IsVisibleOnPricingTable = true,
            //    TextHtmlColor = value => value == "true" ? "#5cb85c" : "#d9534f"
            //};

            //context.Create(
            //    AppFeatures.TestCheckFeature2,
            //    defaultValue: "true",
            //    displayName: L("TestCheckFeature2"),
            //    inputType: new CheckboxInputType()
            //)[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            //{
            //    IsVisibleOnPricingTable = true,
            //    TextHtmlColor = value => value == "true" ? "#5cb85c" : "#d9534f"
            //};

            #endregion

            var chatFeature = context.Create(
                AppFeatures.ChatFeature,
                defaultValue: "false",
                displayName: L("ChatFeature"),
                inputType: new CheckboxInputType()
            );

            chatFeature.CreateChildFeature(
                AppFeatures.TenantToTenantChatFeature,
                defaultValue: "false",
                displayName: L("TenantToTenantChatFeature"),
                inputType: new CheckboxInputType()
            );

            chatFeature.CreateChildFeature(
                AppFeatures.TenantToHostChatFeature,
                defaultValue: "false",
                displayName: L("TenantToHostChatFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowMultiOfficeFeature,
                defaultValue: "true",
                displayName: L("AllowMultiOfficeFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.NumberOfTrucksFeature,
                defaultValue: "100",
                displayName: L("NumberOfTrucksFeature"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, 1000000))
            );

            context.Create(
                AppFeatures.AllowPaymentProcessingFeature,
                defaultValue: "false",
                displayName: L("AllowPaymentProcessingFeature"),
                inputType: new CheckboxInputType(),
                scope: 0 //display for neither Editions nor individual Tenants
            );

            context.Create(
                AppFeatures.AllowLeaseHaulersFeature,
                defaultValue: "false",
                displayName: L("AllowLeaseHaulersFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowInvoicingFeature,
                defaultValue: "false",
                displayName: L("AllowInvoicingFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowInvoiceApprovalFlow,
                defaultValue: "false",
                displayName: L("AllowInvoiceApprovalFlow"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.DriverProductionPayFeature,
                defaultValue: "false",
                displayName: L("DriverProductionPayFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.GpsIntegrationFeature,
                defaultValue: "false",
                displayName: L("GpsIntegrationFeature"),
                inputType: new CheckboxInputType()
            );
            context.Create(
                AppFeatures.SmsIntegrationFeature,
                defaultValue: "false",
                displayName: L("SmsIntegrationFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.DispatchingFeature,
                defaultValue: "false",
                displayName: L("DispatchingFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.QuickbooksFeature,
                defaultValue: "false",
                displayName: L("QuickbooksFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.QuickbooksImportFeature,
                defaultValue: "false",
                displayName: L("QuickbooksImportFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowImportingTruxEarnings,
                defaultValue: "false",
                displayName: L("AllowImportingTruxEarnings"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowImportingLuckStoneEarnings,
                defaultValue: "false",
                displayName: L("AllowImportingLuckStoneEarnings"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowImportingIronSheepdogEarnings,
                defaultValue: "false",
                displayName: L("AllowImportingIronSheepdogEarnings"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowSendingOrdersToDifferentTenant,
                defaultValue: "false",
                displayName: L("AllowSendingOrdersToDifferentTenant"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.FreeFunctionality,
                defaultValue: "true",
                displayName: L("FreeFunctionality"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.PaidFunctionality,
                defaultValue: "true",
                displayName: L("PaidFunctionality"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.PricingTiers,
                defaultValue: "false",
                displayName: L("PricingTiers"),
                inputType: new CheckboxInputType()
            );

            var tickets = context.Create(
                AppFeatures.TicketsFeature,
                defaultValue: "false",
                displayName: L("Tickets"),
                inputType: new CheckboxInputType()
            );

            tickets.CreateChildFeature(
                AppFeatures.ConvertReceivedPdfTicketImagesToJpgBeforeStoring,
                defaultValue: "false",
                displayName: L("ConvertReceivedPdfTicketImagesToJpgBeforeStoring"),
                inputType: new CheckboxInputType()
            );

            tickets.CreateChildFeature(
                AppFeatures.PrintAlreadyUploadedPdfTicketImages,
                defaultValue: "false",
                displayName: L("PrintAlreadyUploadedPdfTicketImages"),
                inputType: new CheckboxInputType()
            );

            tickets.CreateChildFeature(
                AppFeatures.MaximumNumberOfTicketsPerDownload,
                defaultValue: "500",
                displayName: L("MaximumNumberOfTicketsPerDownload"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, 1000000))
            );

            context.Create(
                AppFeatures.WebBasedDriverApp,
                defaultValue: "false",
                displayName: L("WebBasedDriverApp"),
                inputType: new CheckboxInputType()
            );

            var reactNativeDriverApp = context.Create(
                AppFeatures.ReactNativeDriverApp,
                defaultValue: "false",
                displayName: L("ReactNativeDriverApp"),
                inputType: new CheckboxInputType()
            );

            reactNativeDriverApp.CreateChildFeature(
                AppFeatures.AllowGpsTracking,
                defaultValue: "false",
                displayName: L("AllowGpsTracking"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.SendRnConflictsToUsers,
                defaultValue: "true",
                displayName: L("SendRnConflictsToUsers"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.CustomerPortal,
                defaultValue: "false",
                displayName: L("CustomerPortal"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.JobSummary,
                defaultValue: "false",
                displayName: L("JobSummary"),
                inputType: new CheckboxInputType()
            );

            var leaseHaulerPortal = context.Create(
                AppFeatures.LeaseHaulerPortal,
                defaultValue: "false",
                displayName: L("LeaseHaulerPortal"),
                inputType: new CheckboxInputType()
            );

            leaseHaulerPortal.CreateChildFeature(
                AppFeatures.LeaseHaulerPortalJobBasedLeaseHaulerRequest,
                defaultValue: "false",
                displayName: L("JobBasedLeaseHaulerRequest"),
                inputType: new CheckboxInputType()
            );

            leaseHaulerPortal.CreateChildFeature(
                AppFeatures.LeaseHaulerPortalTruckRequest,
                defaultValue: "false",
                displayName: L("LeaseHaulerTruckRequest"),
                inputType: new CheckboxInputType()
            );

            leaseHaulerPortal.CreateChildFeature(
                AppFeatures.LeaseHaulerPortalContacts,
                defaultValue: "false",
                displayName: L("LeaseHaulerPortalContactsDeprecated"),
                inputType: new CheckboxInputType()
            );

            leaseHaulerPortal.CreateChildFeature(
                AppFeatures.LeaseHaulerPortalTicketsByDriver,
                defaultValue: "false",
                displayName: L("TicketsByDriver"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.PrivateLabel,
                defaultValue: "false",
                displayName: L("PrivateLabel"),
                inputType: new CheckboxInputType()
            );

            var separateMaterialAndFreightItems = context.Create(
                AppFeatures.SeparateMaterialAndFreightItems,
                defaultValue: "false",
                displayName: L("SeparateMaterialAndFreightItems"),
                inputType: new CheckboxInputType()
            );

            separateMaterialAndFreightItems.CreateChildFeature(
                AppFeatures.IncludeTravelTime,
                defaultValue: "false",
                displayName: L("IncludeTravelTime"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.HaulZone,
                defaultValue: "false",
                displayName: L("HaulZones"),
                inputType: new CheckboxInputType()
            );

            var charges = context.Create(
                AppFeatures.Charges,
                defaultValue: "false",
                displayName: L("Charges"),
                inputType: new CheckboxInputType()
            );

            charges.CreateChildFeature(
                AppFeatures.UseMaterialQuantity,
                defaultValue: "false",
                displayName: L("UseMaterialQuantity"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.FulcrumIntegration,
                defaultValue: "false",
                displayName: L("FulcrumIntegration"),
                inputType: new CheckboxInputType()
            );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DispatcherWebConsts.LocalizationSourceName);
        }
    }
}
