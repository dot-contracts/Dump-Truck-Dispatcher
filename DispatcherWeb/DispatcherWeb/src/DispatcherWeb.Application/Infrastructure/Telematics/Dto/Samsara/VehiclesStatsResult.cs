using System.Collections.Generic;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.Samsara
{
    public class VehiclesStatsResult
    {
        public List<VehicleStats> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
