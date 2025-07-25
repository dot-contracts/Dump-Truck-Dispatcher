namespace DispatcherWeb.Authorization.Users.Profile.Dto
{
    public class CurrentUserOptionsEditDto
    {
        public bool DontShowZeroQuantityWarning { get; set; }
        public bool PlaySoundForNotifications { get; set; }
        public HostEmailPreference HostEmailPreference { get; set; }
        public bool AllowCounterSalesForUser { get; set; }
        public bool DefaultDesignationToMaterialOnly { get; set; }
        public int? DefaultLoadAtLocationId { get; set; }
        public string DefaultLoadAtLocationName { get; set; }
        public int? DefaultMaterialItemId { get; set; }
        public string DefaultMaterialItemName { get; set; }
        public int? DefaultMaterialUomId { get; set; }
        public string DefaultMaterialUomName { get; set; }
        public bool DefaultAutoGenerateTicketNumber { get; set; }
        public bool CCMeOnInvoices { get; set; }
        public bool DoNotShowWaitingForTicketDownload { get; set; }
    }
}
