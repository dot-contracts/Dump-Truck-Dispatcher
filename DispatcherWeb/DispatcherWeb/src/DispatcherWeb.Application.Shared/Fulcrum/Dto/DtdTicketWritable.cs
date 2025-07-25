namespace DispatcherWeb.Fulcrum.Dto
{
    public class DtdTicketWritable
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }

        public string DriverId { get; set; }

        public int? TrailerId { get; set; }

        public int DispatchId { get; set; }

        public int MaterialId { get; set; }

        public int? FreightId { get; set; }

        public string MaterialUnitOfMeasure { get; set; }

        public string FreightUnitOfMeasure { get; set; }

        public string JobNumber { get; set; }

        public decimal LeaseHaulerRate { get; set; }

        public decimal MaterialPricePerUnit { get; set; }

        public decimal FreightPricePerUnit { get; set; }

        public bool IsFreightFlatRate { get; set; }

        public bool IsMaterialFlatRate { get; set; }

        public decimal? FlatRate { get; set; }

        public decimal? MaterialPrice { get; set; }

        public string TaxEntity { get; set; }

        public decimal TaxRate { get; set; }
    }
}
