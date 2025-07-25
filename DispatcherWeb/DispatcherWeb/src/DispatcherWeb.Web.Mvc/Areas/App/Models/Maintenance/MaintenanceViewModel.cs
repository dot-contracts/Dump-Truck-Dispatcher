using System.Collections.Generic;
using DispatcherWeb.Caching.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Maintenance
{
    public class MaintenanceViewModel
    {
        public IReadOnlyList<CacheDto> Caches { get; set; }
    }
}