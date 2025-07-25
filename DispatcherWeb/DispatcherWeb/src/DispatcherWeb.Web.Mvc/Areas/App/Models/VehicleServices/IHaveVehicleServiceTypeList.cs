using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Models.VehicleServices
{
    public interface IHaveVehicleServiceTypeList
    {
        SelectList VehicleServiceTypeList { get; set; }

    }
}
