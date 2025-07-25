namespace DispatcherWeb.HaulZones.Dto
{
    public class HaulZoneEditDto
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int UnitOfMeasureId { get; set; }

        public string UnitOfMeasureName { get; set; }

        public float Quantity { get; set; }

        public decimal? BillRatePerTon { get; set; }

        public decimal? MinPerLoad { get; set; }

        public decimal? PayRatePerTon { get; set; }

        public bool IsActive { get; set; }
    }
}
