using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class ActivateClosedTrucksModalViewModel
    {
        public SelectList Trucks { get; set; }
        public int OrderLineId { get; set; }
    }
}
