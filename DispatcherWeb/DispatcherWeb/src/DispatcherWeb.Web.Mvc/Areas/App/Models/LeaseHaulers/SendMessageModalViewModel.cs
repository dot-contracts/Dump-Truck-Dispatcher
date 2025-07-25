using Abp.Application.Services.Dto;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.LeaseHaulers
{
    public class SendMessageModalViewModel
    {
        public int LeaseHaulerId { get; set; }
        public ListResultDto<LeaseHaulerContactSelectListDto> Contacts { get; set; }
        public LeaseHaulerMessageType MessageType { get; set; }
    }

}
