using System.Collections.Generic;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteLineDto
    {
        public int Id { get; set; }

        public int QuoteId { get; set; }

        public string FreightItemName { get; set; }

        public string MaterialItemName { get; set; }

        public string MaterialUomName { get; set; }

        public string FreightUomName { get; set; }

        public DesignationEnum Designation { get; set; }

        public string DesignationName => Designation.GetDisplayName();

        public string LoadAtName { get; set; }

        public string DeliverToName { get; set; }

        public decimal? PricePerUnit { get; set; }

        public decimal? FreightRate { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public bool ProductionPay { get; set; }

        public bool RequireTicket { get; set; }

        public bool LoadBased { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public string Note { get; set; }

        public decimal? ExtendedMaterialPrice => PricePerUnit * MaterialQuantity;

        public decimal? ExtendedServicePrice => FreightRate * FreightQuantity;

        public decimal? GrandTotal => (ExtendedMaterialPrice ?? 0) + (ExtendedServicePrice ?? 0);

        public List<string> TruckCategory { get; set; }
    }
}
