using DispatcherWeb.PreventiveMaintenanceSchedule.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.PreventiveMaintenanceSchedule
{
    public class PreventiveMaintenanceListViewModel
    {
        public bool DisableLoadState { get; set; }

        public GetPreventiveMaintenancePagedListInput Filter { get; set; }
    }
}
