using System.Linq;
using Abp.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Users
{
    public class CreateOrEditUserModalViewModel : GetUserForEditOutput, IOrganizationUnitsEditViewModel
    {
        public bool CanChangeUserName
        {
            get { return User.UserName != AbpUserBase.AdminUserName; }
        }

        public int AssignedRoleCount
        {
            get { return Roles.Count(r => r.IsAssigned); }
        }

        public bool IsEditMode
        {
            get { return User.Id.HasValue; }
        }

        public CreateOrEditUserModalViewModel(GetUserForEditOutput output)
        {
            this.User = output.User;
            this.Roles = output.Roles;
            this.AllOrganizationUnits = output.AllOrganizationUnits;
            this.MemberedOrganizationUnits = output.MemberedOrganizationUnits;
        }
    }
}
