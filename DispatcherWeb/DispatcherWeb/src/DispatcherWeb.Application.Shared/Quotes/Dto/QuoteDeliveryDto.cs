using System;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteDeliveryDto
    {
        public DateTime? Date { get; set; }
        public decimal ActualFreightQuantity { get; set; }
        public decimal ActualMaterialQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();
    }
}
