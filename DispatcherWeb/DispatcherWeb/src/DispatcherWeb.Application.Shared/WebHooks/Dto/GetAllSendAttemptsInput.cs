using DispatcherWeb.Dto;

namespace DispatcherWeb.WebHooks.Dto
{
    public class GetAllSendAttemptsInput : PagedInputDto
    {
        public string SubscriptionId { get; set; }
    }
}
