using System;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class LeaseHaulerStatementTicketReportDto : INewLeaseHaulerStatementTicketDetailsDto
    {
        public DateTime? OrderDate { get; set; }
        public Shift? Shift { get; set; }
        public string ShiftName { get; set; }
        public string CustomerName { get; set; }
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }
        public string TicketNumber { get; set; }
        public string CarrierName { get; set; }
        public string TruckCode { get; set; }
        public string DriverName { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? LeaseHaulerRate { get; set; }
        public decimal BrokerFee { get; set; }
        public decimal FuelSurcharge { get; set; }
        public bool IsFreightTotalOverridden { get; set; }
        public decimal FreightTotal { get; set; }
        public decimal ExtendedAmount { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public int TicketId { get; set; }
        public int LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }
        public int? TruckId { get; set; }
    }
}
