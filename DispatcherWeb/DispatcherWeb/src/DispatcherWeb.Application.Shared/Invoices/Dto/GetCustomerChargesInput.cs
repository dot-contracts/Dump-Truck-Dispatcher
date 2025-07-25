using System;
using System.Collections.Generic;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetCustomerChargesInput : SortedInputDto, IShouldNormalize
    {
        public int? CustomerId { get; set; }
        public List<int?> CustomerIds { get; set; }
        public int? OfficeId { get; set; }
        public bool? IsBilled { get; set; }
        public bool? HasInvoiceLineId { get; set; }
        public List<int> ExcludeChargeIds { get; set; }
        public List<int> ChargeIds { get; set; }
        public List<string> JobNumbers { get; set; }
        public List<int?> OrderLineIds { get; set; }
        public List<decimal?> SalesTaxRates { get; set; }
        public List<int?> SalesTaxEntityIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "ChargeDate";
            }
        }
    }
}
