using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.PayStatements.Dto
{
    public class GetPayStatementsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StatementDateBegin { get; set; }
        public DateTime? StatementDateEnd { get; set; }
        public int? OfficeId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "StatementDate desc";
            }
        }
    }
}
