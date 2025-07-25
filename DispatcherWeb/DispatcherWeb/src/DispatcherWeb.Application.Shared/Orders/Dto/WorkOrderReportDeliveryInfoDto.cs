using System;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportDeliveryInfoDto
    {
        public int? OrderLineId { get; set; }
        public string TruckNumber { get; set; }
        public string TicketNumber { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public string TicketPhotoFilename { get; set; }
        public byte[] TicketPhotoBytes { get; set; }
        public WorkOrderReportLoadDto Load { get; set; }
        public string DriverName { get; set; }
    }
}
