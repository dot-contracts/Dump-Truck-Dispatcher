using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class GetRatesInput : SortedInputDto, IShouldNormalize
    {
        public int ItemId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "UOM";
            }
        }
    }
}
