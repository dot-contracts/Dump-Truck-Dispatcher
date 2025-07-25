using System;

namespace DispatcherWeb.FuelPurchases.Dto
{
    public class FuelPurchaseEditDto
    {
        public int Id { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public DateTime FuelDateTime { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Odometer { get; set; }
        public string TicketNumber { get; set; }
    }
}
