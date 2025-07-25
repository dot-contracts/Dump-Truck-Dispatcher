using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Receipts.Dto
{
    public class ReceiptLineEditDto
    {
        public int? Id { get; set; }

        //public int TenantId { get; set; }

        public int ReceiptId { get; set; }

        public int? OrderLineId { get; set; }

        public int? ChargeId { get; set; }

        public int LineNumber { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName { get; set; }

        public int? FreightItemId { get; set; }

        public string FreightItemName { get; set; }

        public int? MaterialItemId { get; set; }

        public string MaterialItemName { get; set; }


        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public string DesignationName => Designation.GetDisplayName();

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }


        public decimal MaterialRate { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal MaterialAmount { get; set; }


        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        public decimal FreightRate { get; set; }

        public decimal? FreightQuantity { get; set; }

        public bool? UseMaterialQuantity { get; set; }

        public decimal FreightAmount { get; set; }

        public bool IsMaterialAmountOverridden { get; set; }

        public bool IsFreightAmountOverridden { get; set; }


        public bool IsMaterialRateOverridden { get; set; }

        public bool IsFreightRateOverridden { get; set; }

        public bool CanOverrideTotals => true;

        //public bool IsMultipleLoads { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        public List<int> TicketIds { get; set; }
    }
}
