using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Items.Dto
{
    public class GetItemsSelectListInput : GetSelectListInput
    {
        public int? QuoteId { get; set; }

        public bool IncludeInactive { get; set; }

        public List<ItemType?> Types { get; set; }
    }
}
