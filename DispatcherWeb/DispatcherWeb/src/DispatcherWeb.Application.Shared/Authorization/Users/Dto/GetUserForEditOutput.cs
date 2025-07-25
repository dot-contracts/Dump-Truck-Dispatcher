using System.Collections.Generic;
using DispatcherWeb.Organizations.Dto;

namespace DispatcherWeb.Authorization.Users.Dto
{
    public class GetUserForEditOutput
    {
        public UserEditDto User { get; set; }

        public UserRoleDto[] Roles { get; set; }

        public List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

        public List<long> MemberedOrganizationUnits { get; set; }
    }
}
