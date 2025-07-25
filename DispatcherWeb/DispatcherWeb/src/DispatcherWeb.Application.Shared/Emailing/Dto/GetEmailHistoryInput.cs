using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Emailing.Dto
{
    public class GetEmailHistoryInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? QuoteId { get; set; }

        public string QuoteName { get; set; }

        public int? OrderId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "CreationTime desc";
            }
        }
    }
}
