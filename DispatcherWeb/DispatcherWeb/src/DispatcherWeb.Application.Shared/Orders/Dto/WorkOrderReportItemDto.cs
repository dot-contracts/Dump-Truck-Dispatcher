using System;
using System.Collections.Generic;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportItemDto : IOrderLineTaxDetails
    {
        public int? OrderLineId { get; set; }
        public int LineNumber { get; set; }
        public string FreightItemName { get; set; }

        public string MaterialItemName { get; set; }

        public bool? IsTaxable { get; set; }

        public bool? IsMaterialTaxable { get; set; }

        public bool? IsFreightTaxable { get; set; }
        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? ActualFreightQuantity { get; set; }
        public decimal? ActualMaterialQuantity { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public decimal FreightPrice { get; set; }
        public decimal MaterialPrice { get; set; }
        public bool IsMaterialTotalOverridden { get; set; }
        public bool IsFreightTotalOverridden { get; set; }
        public decimal? Rate => FreightPricePerUnit.HasValue || MaterialPricePerUnit.HasValue ? (FreightPricePerUnit ?? 0) + (MaterialPricePerUnit ?? 0) : (decimal?)null;
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public double NumberOfTrucks { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public bool IsTimeStaggered { get; set; }
        public List<string> OrderLineVehicleCategories { get; set; }

        public string GetQuantityFormatted(decimal? materialQuantityToUse, decimal? freightQuantityToUse)
        {
            var material = $"{materialQuantityToUse?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {MaterialUomName}";
            var freight = $"{freightQuantityToUse?.ToString(Utilities.NumberFormatWithoutRounding) ?? "-"} {FreightUomName}";

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

        public string GetItemFormatted(bool separateItems)
        {
            if (!separateItems)
            {
                return FreightItemName;
            }

            if (Designation == DesignationEnum.MaterialOnly)
            {
                return MaterialItemName;
            }

            if (Designation == DesignationEnum.FreightOnly
                || Designation == DesignationEnum.FreightAndMaterial && FreightUomName == MaterialUomName)
            {
                var result = FreightItemName;
                if (!string.IsNullOrWhiteSpace(MaterialItemName))
                {
                    result += $" of {MaterialItemName}";
                }

                return result;
            }

            return $"{FreightItemName} / {MaterialItemName}";
        }
    }
}
