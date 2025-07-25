namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteSelectListInfoDto
    {
        public QuoteStatus Status { get; set; }
        public string ChargeTo { get; set; }
        public string PONumber { get; set; }
        public string SpectrumNumber { get; set; }
        public int? ContactId { get; set; }
        public string Directions { get; set; }
        public int? CustomerId { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public int? FuelSurchargeCalculationId { get; set; }
        public string FuelSurchargeCalculationName { get; set; }
        public decimal? BaseFuelCost { get; set; }
        public bool? CanChangeBaseFuelCost { get; set; }
        public bool CustomerIsTaxExempt { get; set; }
        public bool QuoteIsTaxExempt { get; set; }
    }
}
