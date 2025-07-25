using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketEditDto : ITicketEditQuantity
    {
        public TicketControlVisibilityDto VisibleTicketControls { get; set; }
        public int? OrderLineId { get; set; }
        public DesignationEnum? OrderLineDesignation { get; set; }
        public DateTime? OrderDate { get; set; }
        public int Id { get; set; }
        [StringLength(20)]
        public string TicketNumber { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
        public int? FreightItemId { get; set; }
        public string FreightItemName { get; set; }
        public int? MaterialItemId { get; set; }
        public string MaterialItemName { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public Shift? Shift { get; set; }
        public int? FreightUomId { get; set; }
        public string FreightUomName { get; set; }
        public int? MaterialUomId { get; set; }
        public string MaterialUomName { get; set; }
        public bool NonbillableFreight { get; set; }
        public bool NonbillableMaterial { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBilled { get; set; }
        public int? ReceiptLineId { get; set; }
        public int? LoadAtId { get; set; }
        public string LoadAtName { get; set; }
        public int? DeliverToId { get; set; }
        public string DeliverToName { get; set; }
        public bool? ReadOnly { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public bool? OrderLineIsProductionPay { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? TrailerId { get; set; }
        public string TrailerTruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public string CannotEditReason { get; set; }
        public bool IsReadOnly { get; set; }
        public bool HasPayStatements { get; set; }
        public bool HasLeaseHaulerStatements { get; set; }
        public bool IsInternal { get; set; }
        public int? LoadCount { get; set; }
    }
}
