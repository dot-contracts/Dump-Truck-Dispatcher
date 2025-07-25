using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Trucks
{
    public class TruckViewListModel
    {
        public string OfficeName { get; set; }
        public GetTrucksInput Filter { get; set; }
        public bool IsGpsIntegrationConfigured { get; set; }
        public bool IsDtdTrackerConfigured { get; set; }
        public bool IsIntelliShiftConfigured { get; set; }
    }
}
