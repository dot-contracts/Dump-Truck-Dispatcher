using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using DispatcherWeb.MultiTenancy.Payments;

namespace DispatcherWeb.MultiTenancy.Dto
{
    public class RegisterTenantInput
    {
        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string AdminFirstName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string AdminLastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string AdminEmailAddress { get; set; }

        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string AdminPassword { get; set; }

        [DisableAuditing]
        public string CaptchaResponse { get; set; }

        public Payments.SubscriptionStartType SubscriptionStartType { get; set; }

        public SubscriptionPaymentGatewayType? Gateway { get; set; }

        public int? EditionId { get; set; }

        public string PaymentId { get; set; }
    }
}
