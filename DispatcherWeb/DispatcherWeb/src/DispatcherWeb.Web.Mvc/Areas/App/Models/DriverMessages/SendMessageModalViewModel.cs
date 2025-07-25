using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.DriverMessages
{
    public class SendMessageModalViewModel
    {
        public int? OrderLineId { get; set; }
        public int? SelectedDriverId { get; set; }
        public PagedResultDto<SelectListDto> Drivers { get; set; }
    }
}
