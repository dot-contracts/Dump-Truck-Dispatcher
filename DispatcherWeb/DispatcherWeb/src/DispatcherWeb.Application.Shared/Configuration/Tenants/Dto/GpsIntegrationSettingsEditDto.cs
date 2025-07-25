namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class GpsIntegrationSettingsEditDto
    {
        public GpsIntegrationSettingsEditDto()
        {
            Platform = GpsPlatform.DtdTracker;

        }

        public GpsPlatform Platform { get; set; }
        public DtdTrackerSettingsEditDto DtdTracker { get; set; }
        public GeotabSettingsEditDto Geotab { get; set; }
        public SamsaraSettingsEditDto Samsara { get; set; }
        public IntelliShiftSettingsEditDto IntelliShift { get; set; }
    }
}
