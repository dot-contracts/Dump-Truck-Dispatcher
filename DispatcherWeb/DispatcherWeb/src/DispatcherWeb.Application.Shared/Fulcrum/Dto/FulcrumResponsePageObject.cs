namespace DispatcherWeb.Fulcrum.Dto
{
    public class FulcrumResponsePageObject
    {
        public int Skip { get; set; }
        public int Top { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
        public string NextLink { get; set; }
    }
}
