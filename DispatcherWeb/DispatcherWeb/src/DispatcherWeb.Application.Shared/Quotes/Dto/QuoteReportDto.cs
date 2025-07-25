using System;
using System.Collections.Generic;
using System.Globalization;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteReportDto
    {
        public string CompanyName { get; set; }
        public CultureInfo CurrencyCulture { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress1 { get; set; }
        public string CustomerAddress2 { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZipCode { get; set; }
        public string CustomerCountryCode { get; set; }
        public string ContactAttn { get; set; }
        public string ContactPhoneNumber { get; set; }
        public int? OfficeId { get; set; }
        public int QuoteId { get; set; }
        public string QuoteName { get; set; }
        public string QuotePoNumber { get; set; }
        public DateTime? QuoteProposalDate { get; set; }
        public DateTime? QuoteProposalExpiryDate { get; set; }
        public string QuoteNotes { get; set; }
        public decimal? QuoteBaseFuelCost { get; set; }
        public string QuoteNotesFormatted => QuoteNotes
            .Replace("{BaseFuelCost}", QuoteBaseFuelCost.HasValue ? $"{QuoteBaseFuelCost.Value:c2}" : "n/a")
            .Replace("{BaseFuelCost+15}", QuoteBaseFuelCost.HasValue ? $"{(QuoteBaseFuelCost.Value + 0.15M):c2}" : "n/a");
        public string QuoteGeneralTermsAndConditions { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public byte[] LogoBytes { get; set; }
        public byte[] SignatureBytes { get; set; }
        public long? SalesPersonId { get; set; }
        public DateTime Today { get; set; }
        public List<QuoteReportItemDto> Items { get; set; }
        public bool HideLoadAt { get; set; }
        public bool ShowTruckCategories { get; set; }
        public bool SeparateItems { get; set; }
    }
}
