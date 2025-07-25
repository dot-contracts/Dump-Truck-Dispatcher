using System;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.DriverApp.Tickets.Dto
{
    public class TicketDto : ITicketEditQuantity
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public string TicketNumber { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public string TicketPhotoFilename { get; set; }
        public int? LoadId { get; set; }
        public int? DispatchId { get; set; }
        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public int? FreightItemId { get; set; }
        public string FreightItemName { get; set; }
        public int? MaterialItemId { get; set; }
        public string MaterialItemName { get; set; }
        public int? UnitOfMeasureId { get; set; }
        public int? FreightUomId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? LoadCount { get; set; }
        public bool Nonbillable { get; set; }
    }
}
