using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderEditDto : IOfficeIdNameDto
    {
        public OrderEditDto()
        {
            Receipts = new List<ReceiptDto>();
        }

        public bool RemoveNotAvailableTrucks { get; set; }


        public int? Id { get; set; }

        public DateTime? CreationTime { get; set; }

        public string CreatorName { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public string LastModifierName { get; set; }

        public int? MaterialCompanyOrderId { get; set; }

        [Required]
        public DateTime? DeliveryDate { get; set; }

        public Shift? Shift { get; set; }

        public bool IsPending { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAccountNumber { get; set; }

        public bool CustomerIsCod { get; set; }

        [StringLength(EntityStringFieldLengths.Order.PoNumber)]
        public string PONumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.SpectrumNumber)]
        public string SpectrumNumber { get; set; }

        public int? ContactId { get; set; }

        public string ContactName { get; set; }

        public string ContactPhone { get; set; }

        public decimal SalesTaxRate { get; set; }

        public decimal SalesTax { get; set; }

        public int? SalesTaxEntityId { get; set; }

        public string SalesTaxEntityName { get; set; }

        public decimal CODTotal { get; set; }

        [StringLength(EntityStringFieldLengths.Order.ChargeTo)]
        public string ChargeTo { get; set; }

        public decimal FreightTotal { get; set; }

        public decimal MaterialTotal { get; set; }

        public bool IsFreightTotalOverridden { get; set; }

        public bool IsMaterialTotalOverridden { get; set; }

        [StringLength(EntityStringFieldLengths.Order.Directions)]
        public string Directions { get; set; }

        [Required(ErrorMessage = "Office is a required field")]
        public int OfficeId { get; set; }

        public string OfficeName { get; set; }
        public bool IsSingleOffice { get; set; }

        public int? QuoteId { get; set; }

        public string QuoteName { get; set; }

        public DateTime? AuthorizationDateTime { get; set; }

        public DateTime? AuthorizationCaptureDateTime { get; set; }

        public bool IsClosed { get; set; }

        public OrderPriority Priority { get; set; }
        public bool CanEditAnyOrderDirections { get; set; }
        public List<ReceiptDto> Receipts { get; set; }

        public List<OrderLineEditDto> OrderLines { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public string FuelSurchargeCalculationName { get; set; }

        public bool? CanChangeBaseFuelCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? BaseFuelCost { get; set; }

        public string DefaultFuelSurchargeCalculationName { get; set; }

        public decimal? DefaultBaseFuelCost { get; set; }

        public bool? DefaultCanChangeBaseFuelCost { get; set; }

        public int? PricingTierId { get; set; }

        public bool? CustomerIsTaxExempt { get; set; }

        public bool? QuoteIsTaxExempt { get; set; }

        public bool IsTaxExempt { get; set; }
    }
}
