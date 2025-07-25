using System;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public interface INewLeaseHaulerStatementTicketDetailsDto
    {
        DateTime? TicketDateTime { get; set; }
        int TicketId { get; }
        int LeaseHaulerId { get; }
        string LeaseHaulerName { get; }
        int? TruckId { get; }
        string TruckCode { get; }
        decimal Quantity { get; }
        decimal? FreightRate { get; set; }
        decimal? LeaseHaulerRate { get; }
        decimal FuelSurcharge { get; }
        bool IsFreightTotalOverridden { get; set; }
        decimal FreightTotal { get; set; }
    }
}
