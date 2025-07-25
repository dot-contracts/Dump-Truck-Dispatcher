using System.Collections.Generic;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Layout
{
    public class OfficeTruckStylesViewModel
    {
        public IReadOnlyList<OfficeDto> Offices { get; set; }
    }
}
