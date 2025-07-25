using DispatcherWeb.MultiTenancy.Dto;

namespace DispatcherWeb.Web.Models.TenantRegistration
{
    public class TenantRegisterResultViewModel : RegisterTenantOutput
    {
        public string TenantLoginAddress { get; set; }
    }
}
