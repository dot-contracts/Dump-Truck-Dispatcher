using System;
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderTruckQueryResultDto
    {
        public string TruckCode { get; set; }
        public string UnitOfMeasure { get; set; }
        public int LoadsCount { get; set; }
        public decimal Quantity { get; set; }
        public IList<OrderTruckLoadQueryResultDto> Loads { get; set; }
        public int TruckId { get; set; }

        public override string ToString()
        {
            return $"TruckId : {TruckId} TruckCode : {TruckCode} UnitOfMeasure : {UnitOfMeasure} LoadsCount : {LoadsCount} Qty : {Quantity} Loads : {Loads.Count}";
        }

        public class OrderTruckLoadQueryResultDto
        {
            public double?[] SourceCoordinates { get; set; }
            public double?[] DestinationCoordinates { get; set; }
            public int LoadId { get; set; }
            public int? DriverId { get; set; }
            public DateTime? SourceDateTime { get; set; }
            public DateTime? DestinationDateTime { get; set; }

            public override string ToString()
            {
                var sourceCoords = SourceCoordinates.Length > 1 ? $"{SourceCoordinates[0]}, {SourceCoordinates[1]}" : string.Empty;
                var destCoords = DestinationCoordinates.Length > 1 ? $"{DestinationCoordinates[0]}, {DestinationCoordinates[1]}" : string.Empty;
                return $"LoadId : {LoadId} DriverId : {DriverId} SourceDateTime : {SourceDateTime} DestDateTime : {DestinationDateTime} Source : [{sourceCoords}] Dest : [{destCoords}]";
            }
        }
    }
}


