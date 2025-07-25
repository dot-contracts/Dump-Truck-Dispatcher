using System;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class NewLeaseHaulerStatementTicketDetailsDto : INewLeaseHaulerStatementTicketDetailsDto
    {
        public DateTime? TicketDateTime { get; set; }
        public int TicketId { get; set; }
        public int LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? LeaseHaulerRate { get; set; }
        public decimal FuelSurcharge { get; set; }
        public bool IsFreightTotalOverridden { get; set; }
        public decimal FreightTotal { get; set; }
    }
}
