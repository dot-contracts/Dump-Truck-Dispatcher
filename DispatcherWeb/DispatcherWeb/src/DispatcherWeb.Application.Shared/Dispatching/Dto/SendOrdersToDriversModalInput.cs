using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Dispatching.Dto
{
    public class SendOrdersToDriversModalInput : SendOrdersToDriversInput
    {
        public List<SelectListDto> SelectedOffices { get; set; }
    }
}
