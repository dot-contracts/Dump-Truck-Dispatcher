namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class FuelSettingsEditDto
    {
        public bool ShowFuelSurcharge { get; set; }

        public ShowFuelSurchargeOnInvoiceEnum ShowFuelSurchargeOnInvoice { get; set; }

        public int? ItemIdToUseForFuelSurchargeOnInvoice { get; set; }

        public string ItemNameToUseForFuelSurchargeOnInvoice { get; set; }

        public bool OnlyEvenIncrements { get; set; }
    }
}
