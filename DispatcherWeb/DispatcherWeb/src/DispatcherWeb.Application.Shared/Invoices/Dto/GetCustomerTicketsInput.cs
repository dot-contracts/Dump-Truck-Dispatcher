using System.Collections.Generic;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetCustomerTicketsInput : SortedInputDto, IShouldNormalize
    {
        public int? CustomerId { get; set; }
        public int? OfficeId { get; set; }
        public bool? IsBilled { get; set; }
        public bool? IsVerified { get; set; }
        public bool? HasRevenue { get; set; }
        public bool? HasInvoiceLineId { get; set; }
        public List<int> ExcludeTicketIds { get; set; }
        public List<int> TicketIds { get; set; }
        public List<string> JobNumbers { get; set; }
        public List<int?> OrderLineIds { get; set; }
        public List<decimal?> SalesTaxRates { get; set; }
        public List<int?> SalesTaxEntityIds { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TicketDateTime";
            }
        }
    }
}
