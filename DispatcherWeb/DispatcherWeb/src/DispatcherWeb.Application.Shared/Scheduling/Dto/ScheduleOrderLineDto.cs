using System;
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleOrderLineDto
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public int OfficeId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public bool CustomerIsCod { get; set; }

        public string JobNumber { get; set; }

        public string Note { get; set; }

        public string Directions { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName { get; set; }

        public double? NumberOfTrucks { get; set; }

        public double? ScheduledTrucks { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? Time { get; set; }

        public bool IsTimeStaggered { get; set; }

        public bool IsTimeEditable { get; set; }

        public StaggeredTimeKind StaggeredTimeKind { get; set; }

        public DateTime? FirstStaggeredTimeOnJob { get; set; }

        public int? StaggeredTimeInterval { get; set; }

        public bool IsClosed { get; set; }

        public bool IsCancelled { get; set; }

        public int? HaulingCompanyOrderLineId { get; set; }

        public int? MaterialCompanyOrderLineId { get; set; }

        public OrderPriority Priority { get; set; }

        public decimal Utilization { get; set; }

        public decimal MaxUtilization => ScheduledTrucks.HasValue ? Convert.ToDecimal(ScheduledTrucks.Value) : 0;

        public List<ScheduleOrderLineTruckDto> Trucks { get; set; }

        public string Item { get; set; }

        public string FreightItem { get; set; }

        public string MaterialItem { get; set; }

        public string MaterialUom { get; set; }

        public string FreightUom { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public DesignationEnum Designation { get; set; }

        public List<int> VehicleCategoryIds { get; set; }

        public List<string> VehicleCategoryNames { get; set; }

        //
        //public decimal? HoursOnDispatches { get; set; }
        //public decimal? HoursOnDispatchesLoaded { get; set; }
        public string CargoCapacityRequiredError { get; set; }

        public int? DeliveredLoadCount { get; set; }

        public int? LoadedLoadCount { get; set; }

        public int? LoadCount { get; set; }

        public int? DispatchCount { get; set; }

        public decimal? AmountOrdered { get; set; }

        public decimal? AmountLoaded { get; set; }

        public decimal? AmountDelivered { get; set; }

        public List<LeaseHaulerRequestDto> LeaseHaulerRequests { get; set; }

        public string QuantityFormatted
        {
            get
            {
                var material = $"{MaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {MaterialUom}";
                var freight = $"{FreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {FreightUom}";

                if (Designation.MaterialOnly())
                {
                    return material;
                }

                if (Designation == DesignationEnum.FreightAndMaterial)
                {
                    if (MaterialUom == FreightUom)
                    {
                        return material;
                    }

                    return material + Environment.NewLine + freight;
                }

                return freight;
            }
        }
    }
}
