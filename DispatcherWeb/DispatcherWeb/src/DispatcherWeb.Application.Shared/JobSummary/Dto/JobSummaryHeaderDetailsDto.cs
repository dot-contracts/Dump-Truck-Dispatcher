using System;
using System.Collections.Generic;

namespace DispatcherWeb.JobSummary.Dto
{
    public class JobSummaryHeaderDetailsDto
    {
        public JobStatus JobStatus { get; set; }

        public int OrderLineId { get; set; }

        public string Customer { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string LoadAt { get; set; }

        public string Item { get; set; }

        public string JobNumber { get; set; }

        public double? NumberOfTrucks { get; set; }

        public string DeliverTo { get; set; }

        public decimal? QuantityOrdered => Designation.HasMaterial()
            ? MaterialQuantity
            : FreightQuantity;

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public string MaterialUomName { get; set; }

        public string FreightUomName { get; set; }

        public string UomName => Designation.HasMaterial()
            ? MaterialUomName
            : FreightUomName;

        public DesignationEnum Designation { get; set; }

        public bool IsComplete { get; set; }

        public List<JobSummaryDispatchDto> Dispatches { get; set; }
    }
}


