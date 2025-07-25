namespace DispatcherWeb.Caching.Dto
{
    public class CacheDto
    {
        public string Name { get; set; }
        public long? TotalHits { get; set; }
        public long? TotalMisses { get; set; }
        public long? CurrentEntryCount { get; set; }
        public long? CurrentEstimatedSize { get; set; }
    }
}
