using System.Collections.Generic;
using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift
{
    public class TruckUnitsPage
    {
        [JsonProperty("collection")]
        public List<TruckUnitDto> TruckUnitsCollection { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty("recordCount")]
        public int RecordCount { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("links")]
        public List<TruckUnitLink> TruckUnitLinks { get; set; }

        public bool HasMorePages => PageNumber < TotalPages;
    }
}
