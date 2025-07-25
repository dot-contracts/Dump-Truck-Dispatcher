using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementCreationTicketDto
    {
        public int TicketId { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int DriverId { get; set; }
        public long? UserId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }
        public bool IsFreightTotalOverridden { get; set; }
        public decimal FreightTotal { get; set; }
        public DateTime TicketCreationTime { get; set; }
        public string FreightUomName { get; set; }
        public bool LoadBased { get; set; }
    }
}
