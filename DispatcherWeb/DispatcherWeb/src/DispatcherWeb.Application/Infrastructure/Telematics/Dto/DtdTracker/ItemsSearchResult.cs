using System.Collections.Generic;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class ItemsSearchResult<TItem> : WialonResult
    {
        public List<TItem> Items { get; set; }
    }
}
