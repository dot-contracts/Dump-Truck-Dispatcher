using System;
using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.MultiTenancy;

namespace DispatcherWeb.MultiTenancy.Dto
{
    public class CreateTenantInput
    {
        [Required]
        [StringLength(TenantConsts.MaxNameLength)]
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

        [StringLength(AbpUserBase.MaxPasswordLength)]
        [DisableAuditing]
        public string AdminPassword { get; set; }

        [MaxLength(AbpTenantBase.MaxConnectionStringLength)]
        [DisableAuditing]
        public string ConnectionString { get; set; }

        public bool ShouldChangePasswordOnNextLogin { get; set; }

        public bool SendActivationEmail { get; set; }

        public int? EditionId { get; set; }

        public bool IsActive { get; set; }

        public DateTime? SubscriptionEndDateUtc { get; set; }

        public bool IsInTrialPeriod { get; set; }
    }
}