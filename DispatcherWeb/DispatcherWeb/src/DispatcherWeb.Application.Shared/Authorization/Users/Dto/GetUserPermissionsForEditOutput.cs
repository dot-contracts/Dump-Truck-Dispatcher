using System.Collections.Generic;
using DispatcherWeb.Authorization.Permissions.Dto;

namespace DispatcherWeb.Authorization.Users.Dto
{
    public class GetUserPermissionsForEditOutput
    {
        public List<FlatPermissionDto> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }

        public string UserName { get; set; }
    }
}
