using DispatcherWeb.Dto;

namespace DispatcherWeb.Locations.Dto
{
    public class GetLocationsSelectListInput : GetSelectListInput
    {
        public bool IncludeInactive { get; set; }

        public int? LoadAtQuoteId { get; set; }

        public int? DeliverToQuoteId { get; set; }
    }
}
