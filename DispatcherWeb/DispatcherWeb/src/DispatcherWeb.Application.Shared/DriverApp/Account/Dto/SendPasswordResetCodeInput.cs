using System.ComponentModel.DataAnnotations;
using Abp.MultiTenancy;

namespace DispatcherWeb.DriverApp.Account.Dto
{
    public class SendPasswordResetCodeInput : Authorization.Accounts.Dto.SendPasswordResetCodeInput
    {
        [Required]
        [MaxLength(AbpTenantBase.MaxTenancyNameLength)]
        public string TenancyName { get; set; }
    }
}
