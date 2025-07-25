using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class GetItemPricesInput : SortedInputDto, IShouldNormalize
    {
        public int ItemId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "MaterialUomName";
            }
        }
    }
}
