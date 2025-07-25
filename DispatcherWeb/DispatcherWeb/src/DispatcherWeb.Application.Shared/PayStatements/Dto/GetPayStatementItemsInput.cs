using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.PayStatements.Dto
{
    public class GetPayStatementItemsInput : SortedInputDto, IShouldNormalize
    {
        public int PayStatementId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Id";
            }
        }
    }
}
