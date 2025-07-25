using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class GetItemsInput : PagedAndSortedInputDto, IShouldNormalize, IGetItemListFilter
    {
        public string Name { get; set; }

        public FilterActiveStatus Status { get; set; }

        public ItemType? Type { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
