using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class GetWorkOrderLinesInput : SortedInputDto, IShouldNormalize
    {
        public int Id { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Id";
            }
        }

    }
}
