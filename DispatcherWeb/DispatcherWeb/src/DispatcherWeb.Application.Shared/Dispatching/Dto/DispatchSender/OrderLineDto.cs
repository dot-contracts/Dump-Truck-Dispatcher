using System;
using System.Collections.Generic;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class OrderLineDto : OrderLineDataForDispatchMessage
    {
        public bool IsFreightPriceOverridden { get; set; }
        public bool IsMaterialPriceOverridden { get; set; }
        public DateTime? TimeOnJobUtc { get; set; }
        public List<OrderLineTruckDto> OrderLineTrucks { get; set; }
        public int OfficeId { get; set; }
        public bool IsComplete { get; set; }
        public int LineNumber { get; set; }
        public UnitOfMeasureBaseEnum? MaterialUomBaseId { get; set; }
        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }

    }
}
