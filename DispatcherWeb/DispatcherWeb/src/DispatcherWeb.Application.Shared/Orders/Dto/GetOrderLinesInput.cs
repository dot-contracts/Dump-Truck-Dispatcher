using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderLinesInput : SortedInputDto, IShouldNormalize
    {
        public int? OrderId { get; set; }
        public int? QuoteId { get; set; }

        public int? LoadAtId { get; set; }
        public int? DeliverToId { get; set; }
        public int? ItemId { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public DesignationEnum? Designation { get; set; }
        public bool ForceDuplicateFilters { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = QuoteId.HasValue ? "Id" : "LineNumber";
            }
        }
    }
}
