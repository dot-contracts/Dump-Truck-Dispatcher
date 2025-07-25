using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class ReassignTrucksModalViewModel
    {
        public int OrderLineId { get; set; }
        public IEnumerable<SelectListItem> Trucks { get; set; }

    }
}
