using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteEditDto : IOfficeIdNameDto
    {
        public int? Id { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? OfficeId { get; set; }

        int IOfficeIdNameDto.OfficeId { get => OfficeId ?? 0; set => OfficeId = value; }

        public string OfficeName { get; set; }

        public bool IsSingleOffice { get; set; }

        public int? ContactId { get; set; }

        public string ContactName { get; set; }

        [Required]
        [StringLength(155)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime? ProposalDate { get; set; }

        public DateTime? ProposalExpiryDate { get; set; }

        public DateTime? InactivationDate { get; set; }

        public QuoteStatus Status { get; set; }

        [StringLength(20)]
        public string PONumber { get; set; }

        public bool IsTaxExempt { get; set; }

        [StringLength(20)]
        public string SpectrumNumber { get; set; }

        //[Required(ErrorMessage = "Base Fuel Cost is a required field")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? BaseFuelCost { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public string FuelSurchargeCalculationName { get; set; }

        public bool? CanChangeBaseFuelCost { get; set; }

        [StringLength(EntityStringFieldLengths.Quote.Directions)]
        public string Directions { get; set; }

        [StringLength(EntityStringFieldLengths.Quote.Notes)]
        public string Notes { get; set; }

        public long? SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }

        [StringLength(500)]
        public string ChargeTo { get; set; }

        public bool HasOrders { get; set; }
    }
}
