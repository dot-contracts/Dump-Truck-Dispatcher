using System;
using System.Collections.Generic;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteReportItemDto
    {
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }
        public string FreightItemDescription { get; set; }
        public string MaterialItemDescription { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? LeaseHaulerRate { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }
        public decimal? PricePerUnit { get; set; }
        public decimal? Rate => FreightRate.HasValue || PricePerUnit.HasValue ? (FreightRate ?? 0) + (PricePerUnit ?? 0) : (decimal?)null;
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public List<string> QuoteLineVehicleCategories { get; set; }

        public string QuantityFormatted
        {
            get
            {
                var material = $"{MaterialQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {MaterialUomName}";
                var freight = $"{FreightQuantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {FreightUomName}";

                if (Designation.MaterialOnly())
                {
                    return material;
                }

                if (Designation == DesignationEnum.FreightAndMaterial)
                {
                    if (MaterialUomName == FreightUomName)
                    {
                        return material;
                    }

                    return material + Environment.NewLine + freight;
                }

                return freight;
            }
        }
    }
}
