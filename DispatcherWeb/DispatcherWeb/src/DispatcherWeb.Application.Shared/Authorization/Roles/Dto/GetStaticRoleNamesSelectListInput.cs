using Abp.MultiTenancy;

namespace DispatcherWeb.Authorization.Roles.Dto
{
    public class GetStaticRoleNamesSelectListInput
    {
        public MultiTenancySides? MultiTenancySide { get; set; }
    }
}
