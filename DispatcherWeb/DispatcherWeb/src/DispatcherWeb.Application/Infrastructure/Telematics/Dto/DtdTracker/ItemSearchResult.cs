namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class ItemSearchResult<TItem> : WialonResult
    {
        public TItem Item { get; set; }
    }
}
