using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class GetLeaseHaulerStatementsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StatementDateBegin { get; set; }
        public DateTime? StatementDateEnd { get; set; }
        public int? StatementId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "StatementDate desc";
            }
        }
    }
}
