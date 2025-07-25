using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class WorkOrderEditDto
    {
        public int Id { get; set; }
        public int? VehicleServiceTypeId { get; set; }
        public string VehicleServiceTypeName { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public WorkOrderStatus Status { get; set; }
        public string StatusText => Status.GetDisplayName();
        public int TruckId { get; set; }
        public string TruckCode { get; set; }

        [StringLength(EntityStringFieldLengths.WorkOrder.Note)]
        public string Note { get; set; }
        public decimal Odometer { get; set; }
        public decimal Hours { get; set; }
        public long? AssignedToId { get; set; }
        public string AssignedToName { get; set; }
        public decimal TotalLaborCost { get; set; }
        public bool IsTotalLaborCostOverridden { get; set; }
        public decimal TotalPartsCost { get; set; }
        public bool IsTotalPartsCostOverridden { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalCost { get; set; }

        public List<WorkOrderLineEditDto> WorkOrderLines { get; set; }
        public List<WorkOrderPictureEditDto> Pictures { get; set; } = new List<WorkOrderPictureEditDto>();

        /// <summary>
        /// Provided for report purpose unable to parse from list of WorkOrderPictureEditDto
        /// </summary>
        public int PicturesCount =>
            Pictures.Count;
    }
}
