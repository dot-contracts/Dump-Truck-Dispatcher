using System.Collections.Generic;
using DispatcherWeb.Authorization.Permissions.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }

        List<string> GrantedPermissionNames { get; set; }
    }
}