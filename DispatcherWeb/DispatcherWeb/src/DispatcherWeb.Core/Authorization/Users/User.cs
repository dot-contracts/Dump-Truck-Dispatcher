using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Timing;
using DispatcherWeb.Caching;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Authorization.Users
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User : AbpUser<User>, IAuditableCacheItem<long>
    {
        public const int MinPlainPasswordLength = 6;

        //public const int MaxPhoneNumberLength = 20;

        public const int MaxSignaturePictureWidth = 1000;

        public const int MaxSignaturePictureHeight = 300;

        public virtual Guid? ProfilePictureId { get; set; }

        public virtual Guid? SignaturePictureId { get; set; }

        public virtual bool ShouldChangePasswordOnNextLogin { get; set; }

        public DateTime? SignInTokenExpireTimeUtc { get; set; }

        [StringLength(EntityStringFieldLengths.User.SignInToken)]
        public string SignInToken { get; set; }

        [StringLength(EntityStringFieldLengths.User.GoogleAuthenticatorKey)]
        public string GoogleAuthenticatorKey { get; set; }

        public List<UserOrganizationUnit> OrganizationUnits { get; set; }

        public DateTime? LastLoginTime { get; set; }

        //Can add application specific user properties here

        public virtual Office Office { get; set; }
        public int? OfficeId { get; set; }


        [StringLength(200)]
        public string Title { get; set; }

        public virtual ICollection<Driver> Drivers { get; set; }

        public int? CustomerContactId { get; set; }
        public virtual CustomerContact CustomerContact { get; set; }

        public virtual LeaseHaulerUser LeaseHaulerUser { get; set; }

        public User()
        {
            IsLockoutEnabled = true;
            IsTwoFactorEnabled = true;
        }

        /// <summary>
        /// Creates admin <see cref="User"/> for a tenant.
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="emailAddress">Email address</param>
        /// <returns>Created <see cref="User"/> object</returns>
        public static User CreateTenantAdminUser(int tenantId, string firstName, string lastName, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = firstName,
                Surname = lastName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>(),
                OrganizationUnits = new List<UserOrganizationUnit>(),
            };

            user.SetNormalizedNames();

            return user;
        }

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public override void SetNewPasswordResetCode()
        {
            /* This reset code is intentionally kept short.
             * It should be short and easy to enter in a mobile application, where user can not click a link.
             */
            PasswordResetCode = Guid.NewGuid().ToString("N").Truncate(10).ToUpperInvariant();
        }

        public void Unlock()
        {
            AccessFailedCount = 0;
            LockoutEndDateUtc = null;
        }

        public void SetSignInToken()
        {
            SignInToken = Guid.NewGuid().ToString();
            SignInTokenExpireTimeUtc = Clock.Now.AddMinutes(1).ToUniversalTime();
        }

        public User Clone()
        {
            return new User
            {
                CustomerContactId = CustomerContactId,
                GoogleAuthenticatorKey = GoogleAuthenticatorKey,
                LastLoginTime = LastLoginTime,
                OfficeId = OfficeId,
                ProfilePictureId = ProfilePictureId,
                ShouldChangePasswordOnNextLogin = ShouldChangePasswordOnNextLogin,
                SignInToken = SignInToken,
                SignInTokenExpireTimeUtc = SignInTokenExpireTimeUtc,
                SignaturePictureId = SignaturePictureId,
                Title = Title,
                AccessFailedCount = AccessFailedCount,
                AuthenticationSource = AuthenticationSource,
                ConcurrencyStamp = ConcurrencyStamp,
                CreationTime = CreationTime,
                CreatorUserId = CreatorUserId,
                DeleterUserId = DeleterUserId,
                DeletionTime = DeletionTime,
                EmailAddress = EmailAddress,
                EmailConfirmationCode = EmailConfirmationCode,
                Id = Id,
                IsActive = IsActive,
                IsDeleted = IsDeleted,
                IsEmailConfirmed = IsEmailConfirmed,
                IsLockoutEnabled = IsLockoutEnabled,
                IsPhoneNumberConfirmed = IsPhoneNumberConfirmed,
                IsTwoFactorEnabled = IsTwoFactorEnabled,
                LastModificationTime = LastModificationTime,
                TenantId = TenantId,
                Name = Name,
                LastModifierUserId = LastModifierUserId,
                LockoutEndDateUtc = LockoutEndDateUtc,
                NormalizedEmailAddress = NormalizedEmailAddress,
                NormalizedUserName = NormalizedUserName,
                Password = Password,
                PasswordResetCode = PasswordResetCode,
                PhoneNumber = PhoneNumber,
                SecurityStamp = SecurityStamp,
                Surname = Surname,
                UserName = UserName,
                // Not collections or navigation properties:
                //CustomerContact = CustomerContact,
                //LeaseHaulerUser = LeaseHaulerUser,
                //Office = Office,
                //Drivers = Drivers,
                //OrganizationUnits = OrganizationUnits,
                //LastModifierUser = LastModifierUser,
                //Roles = Roles,
                //CreatorUser = CreatorUser,
                //Claims = Claims,
                //DeleterUser = DeleterUser,
                //Logins = Logins,
                //Permissions = Permissions,
                //Settings = Settings,
                //Tokens = Tokens,
            };
        }
    }
}
