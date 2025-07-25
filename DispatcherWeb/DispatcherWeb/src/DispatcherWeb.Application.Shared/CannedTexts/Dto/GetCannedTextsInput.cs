using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.CannedTexts.Dto
{
    public class GetCannedTextsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? OfficeId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
