using System.Collections.Generic;
using DispatcherWeb.Dispatching.Dto.DispatchSender;

namespace DispatcherWeb.Dispatching.Dto
{
    public class SendDispatchMessageDto
    {
        public int OrderLineId { get; set; }
        public string Message { get; set; }
        public IList<OrderLineTruckDto> OrderLineTrucks { get; set; }
        public bool IsMultipleLoads { get; set; }
        public string FreightUom { get; set; }
        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }
        public bool UseTruckCapacityForLoadQuantity { get; set; }
        public decimal? LoadMaterialQuantity { get; set; }
        public int? MaterialItemId { get; set; }
    }
}
