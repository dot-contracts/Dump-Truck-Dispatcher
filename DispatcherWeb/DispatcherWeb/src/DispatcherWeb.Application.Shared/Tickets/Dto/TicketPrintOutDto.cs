using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketPrintOutDto : ITicketQuantity
    {
        public bool SeparateItems { get; set; }
        public byte[] LogoBytes { get; set; }
        public string LegalName { get; set; }
        public string LegalAddress { get; set; }
        public string BillingPhoneNumber { get; set; }


        public string TicketNumber { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public string CustomerName { get; set; }
        public int? OfficeId { get; set; }
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public string Note { get; set; }
        public bool DebugLayout { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? TicketUomId { get; set; }
    }
}
