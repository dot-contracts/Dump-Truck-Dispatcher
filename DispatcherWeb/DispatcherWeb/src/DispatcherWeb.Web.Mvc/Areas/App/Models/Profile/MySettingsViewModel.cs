using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.Extensions;
using DispatcherWeb.Authorization.Users.Profile.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Profile
{
    public class MySettingsViewModel : CurrentUserProfileEditDto
    {
        public List<ComboboxItemDto> TimezoneItems { get; set; }

        public bool SmsVerificationEnabled { get; set; }

        public bool CanChangeUserName => UserName != AbpUserBase.AdminUserName;

        public string Code { get; set; }

        public MySettingsViewModel(CurrentUserProfileEditDto currentUserProfileEditDto)
        {
            this.Name = currentUserProfileEditDto.Name;
            this.Surname = currentUserProfileEditDto.Surname;
            this.UserName = currentUserProfileEditDto.UserName;
            this.EmailAddress = currentUserProfileEditDto.EmailAddress;
            this.PhoneNumber = currentUserProfileEditDto.PhoneNumber;
            this.IsPhoneNumberConfirmed = currentUserProfileEditDto.IsPhoneNumberConfirmed;
            this.Timezone = currentUserProfileEditDto.Timezone;
            this.QrCodeSetupImageUrl = currentUserProfileEditDto.QrCodeSetupImageUrl;
            this.IsGoogleAuthenticatorEnabled = currentUserProfileEditDto.IsGoogleAuthenticatorEnabled;
            this.Options = currentUserProfileEditDto.Options;
        }

        public bool CanVerifyPhoneNumber()
        {
            return SmsVerificationEnabled && !PhoneNumber.IsNullOrEmpty() && !PhoneNumber.Trim().IsNullOrEmpty();
        }
    }
}
