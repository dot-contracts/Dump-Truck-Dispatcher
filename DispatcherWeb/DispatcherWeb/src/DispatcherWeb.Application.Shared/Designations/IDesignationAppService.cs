using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Designations
{
    public interface IDesignationAppService
    {
        Task<List<SelectListDto>> GetDesignationSelectListItemsAsync(DesignationEnum? selectedDesignation = null);
    }
}
