using Abp.Application.Services.Dto;

namespace DispatcherWeb.Dto
{
    public class SortedInputDto : ISortedResultRequest
    {
        public string Sorting { get; set; }
    }
}
