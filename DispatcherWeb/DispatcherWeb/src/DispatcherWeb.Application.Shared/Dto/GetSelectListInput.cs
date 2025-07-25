namespace DispatcherWeb.Dto
{
    public class GetSelectListInput : PagedInputDto
    {
        public string Term { get; set; }
        public string IncludeOptionName { get; set; }
    }
}
