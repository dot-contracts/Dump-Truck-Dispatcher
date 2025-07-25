using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketListItemViewModel : OrderTicketEditDto
    {
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public int? LeaseHaulerId { get; set; }
        public bool? TruckCanPullTrailer { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public int? ReceiptLineId { get; set; }
        public bool IsInternal { get; set; }
    }
}
