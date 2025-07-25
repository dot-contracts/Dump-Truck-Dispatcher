using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Users
{
    public class UserPermissionsEditViewModel : GetUserPermissionsForEditOutput, IPermissionsEditViewModel
    {
        public UserPermissionsEditViewModel(GetUserPermissionsForEditOutput output)
        {
            UserName = output.UserName;
            this.Permissions = output.Permissions;
            this.GrantedPermissionNames = output.GrantedPermissionNames;
        }
    }
}
