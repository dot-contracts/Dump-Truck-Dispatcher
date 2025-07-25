using System.Collections.Generic;
using DispatcherWeb.Organizations.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Common
{
    public interface IOrganizationUnitsEditViewModel
    {
        List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

        List<long> MemberedOrganizationUnits { get; set; }
    }
}
