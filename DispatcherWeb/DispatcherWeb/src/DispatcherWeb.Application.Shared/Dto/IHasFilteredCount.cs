namespace DispatcherWeb.Dto
{
    public interface IHasFilteredCount
    {
        /// <summary>Total count of Items after applied filters.</summary>
        int FilteredCount { get; set; }
    }
}
