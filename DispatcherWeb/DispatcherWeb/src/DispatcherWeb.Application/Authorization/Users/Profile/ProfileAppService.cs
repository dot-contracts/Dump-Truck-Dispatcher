using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Auditing;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Localization;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authentication.TwoFactor.Google;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Authorization.Users.Profile.Cache;
using DispatcherWeb.Authorization.Users.Profile.Dto;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.Friendships;
using DispatcherWeb.Gdpr;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Locations;
using DispatcherWeb.Security;
using DispatcherWeb.Storage;
using DispatcherWeb.Timing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users.Profile
{
    [AbpAuthorize]
    public class ProfileAppService : DispatcherWebAppServiceBase, IProfileAppService
    {
        private const int MaxProfilPictureBytes = 1048576; //1MB
        private readonly IAppFolders _appFolders;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IFriendshipManager _friendshipManager;
        private readonly GoogleTwoFactorAuthenticateService _googleTwoFactorAuthenticateService;
        private readonly ISmsSender _smsSender;
        private readonly ICacheManager _cacheManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IPasswordComplexitySettingStore _passwordComplexitySettingStore;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ProfileImageServiceFactory _profileImageServiceFactory;
        private readonly ISettingDefinitionManager _settingDefinitionManager;
        private readonly IRepository<Location> _locationRepository;
        private readonly ListCacheCollection _listCaches;

        public ProfileAppService(
            IAppFolders appFolders,
            IBinaryObjectManager binaryObjectManager,
            ITimeZoneService timezoneService,
            IFriendshipManager friendshipManager,
            GoogleTwoFactorAuthenticateService googleTwoFactorAuthenticateService,
            ISmsSender smsSender,
            ICacheManager cacheManager,
            ITempFileCacheManager tempFileCacheManager,
            IPasswordComplexitySettingStore passwordComplexitySettingStore,
            IBackgroundJobManager backgroundJobManager,
            ProfileImageServiceFactory profileImageServiceFactory,
            ISettingDefinitionManager settingDefinitionManager,
            IRepository<Location> locationRepository,
            ListCacheCollection listCaches)
        {
            _appFolders = appFolders;
            _binaryObjectManager = binaryObjectManager;
            _timeZoneService = timezoneService;
            _friendshipManager = friendshipManager;
            _googleTwoFactorAuthenticateService = googleTwoFactorAuthenticateService;
            _smsSender = smsSender;
            _cacheManager = cacheManager;
            _tempFileCacheManager = tempFileCacheManager;
            _passwordComplexitySettingStore = passwordComplexitySettingStore;
            _backgroundJobManager = backgroundJobManager;
            _profileImageServiceFactory = profileImageServiceFactory;
            _settingDefinitionManager = settingDefinitionManager;
            _locationRepository = locationRepository;
            _listCaches = listCaches;
        }

        [DisableAuditing]
        public async Task<CurrentUserProfileEditDto> GetCurrentUserProfileForEdit()
        {
            var user = await (await GetCurrentUserQueryAsync())
                .Select(x => new CurrentUserProfileEditDto
                {
                    Name = x.Name,
                    Surname = x.Surname,
                    UserName = x.UserName,
                    EmailAddress = x.EmailAddress,
                    PhoneNumber = x.PhoneNumber,
                    IsPhoneNumberConfirmed = x.IsPhoneNumberConfirmed,
                    GoogleAuthenticatorKey = x.GoogleAuthenticatorKey,
                }).FirstOrDefaultAsync();

            user.QrCodeSetupImageUrl = user.GoogleAuthenticatorKey != null
                ? _googleTwoFactorAuthenticateService.GenerateSetupCode("DispatcherWeb", user.EmailAddress, user.GoogleAuthenticatorKey, 300, 300).QrCodeSetupImageUrl
                : "";

            user.Options = new CurrentUserOptionsEditDto
            {
                DontShowZeroQuantityWarning = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserOptions.DontShowZeroQuantityWarning),
                PlaySoundForNotifications = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserOptions.PlaySoundForNotifications),
                HostEmailPreference = (HostEmailPreference)await SettingManager.GetSettingValueAsync<int>(AppSettings.UserOptions.HostEmailPreference),
                AllowCounterSalesForUser = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForUser),
                DefaultDesignationToMaterialOnly = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly),
                DefaultAutoGenerateTicketNumber = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber),
                CCMeOnInvoices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.CCMeOnInvoices),
                DoNotShowWaitingForTicketDownload = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserOptions.DoNotShowWaitingForTicketDownload),
            };

            if (Clock.SupportsMultipleTimezone)
            {
                user.Timezone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);

                var defaultTimeZoneId = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.User, await AbpSession.GetTenantIdOrNullAsync());
                if (user.Timezone == defaultTimeZoneId)
                {
                    user.Timezone = string.Empty;
                }
            }

            var loadAtId = await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId);
            if (loadAtId > 0)
            {
                var loadAt = await (await _locationRepository.GetQueryAsync())
                    .Where(x => x.Id == loadAtId)
                    .Select(x => new
                    {
                        x.Id,
                        x.DisplayName,
                    }).FirstOrDefaultAsync();
                user.Options.DefaultLoadAtLocationId = loadAt?.Id;
                user.Options.DefaultLoadAtLocationName = loadAt?.DisplayName;
            }

            user.Options.DefaultMaterialItemId = await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DefaultMaterialItemId);
            if (user.Options.DefaultMaterialItemId > 0)
            {
                var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                var item = items.Find(user.Options.DefaultMaterialItemId);
                if (item == null)
                {
                    user.Options.DefaultMaterialItemId = 0;
                }
                else
                {
                    user.Options.DefaultMaterialItemName = item.Name;
                }
            }

            user.Options.DefaultMaterialUomId = await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DefaultMaterialUomId);
            if (user.Options.DefaultMaterialUomId > 0)
            {
                var uoms = await _listCaches.UnitOfMeasure.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                var uom = uoms.Find(user.Options.DefaultMaterialUomId);
                if (uom == null)
                {
                    user.Options.DefaultMaterialUomId = 0;
                }
                else
                {
                    user.Options.DefaultMaterialUomName = uom.Name;
                }
            }

            return user;
        }

        public async Task DisableGoogleAuthenticator()
        {
            var user = await GetCurrentUserAsync();
            user.GoogleAuthenticatorKey = null;
        }

        public async Task<UpdateGoogleAuthenticatorKeyOutput> UpdateGoogleAuthenticatorKey()
        {
            var user = await GetCurrentUserAsync();
            user.GoogleAuthenticatorKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            CheckErrors(await UserManager.UpdateAsync(user));

            return new UpdateGoogleAuthenticatorKeyOutput
            {
                QrCodeSetupImageUrl = _googleTwoFactorAuthenticateService.GenerateSetupCode(
                    "DispatcherWeb",
                    user.EmailAddress, user.GoogleAuthenticatorKey, 300, 300).QrCodeSetupImageUrl,
            };
        }

        public async Task SendVerificationSms(SendVerificationSmsInputDto input)
        {
            var code = RandomHelper.GetRandom(100000, 999999).ToString();
            var cacheKey = (await AbpSession.ToUserIdentifierAsync()).ToString();
            var cacheItem = new SmsVerificationCodeCacheItem { Code = code };

            await _cacheManager.GetSmsVerificationCodeCache().SetAsync(
                cacheKey,
                cacheItem
            );

            await _smsSender.SendAsync(new SmsSendInput
            {
                ToPhoneNumber = input.PhoneNumber,
                Body = L("SmsVerificationMessage", code),
            });
        }

        public async Task VerifySmsCode(VerifySmsCodeInputDto input)
        {
            var cacheKey = (await AbpSession.ToUserIdentifierAsync()).ToString();
            var cash = await _cacheManager.GetSmsVerificationCodeCache().GetOrDefaultAsync(cacheKey);

            if (cash == null)
            {
                throw new Exception("Phone number confirmation code is not found in cache !");
            }

            if (input.Code != cash.Code)
            {
                throw new UserFriendlyException(L("WrongSmsVerificationCode"));
            }

            var user = await UserManager.GetUserAsync(await AbpSession.ToUserIdentifierAsync());
            user.IsPhoneNumberConfirmed = true;
            user.PhoneNumber = input.PhoneNumber;
            await UserManager.UpdateAsync(user);
        }

        public async Task PrepareCollectedData()
        {
            await _backgroundJobManager.EnqueueAsync<UserCollectedDataPrepareJob, UserIdentifier>(
                await AbpSession.ToUserIdentifierAsync()
            );
        }

        public async Task UpdateCurrentUserProfile(CurrentUserProfileEditDto input)
        {
            var user = await GetCurrentUserAsync();

            if (user.PhoneNumber != input.PhoneNumber)
            {
                input.IsPhoneNumberConfirmed = false;
            }
            else if (user.IsPhoneNumberConfirmed)
            {
                input.IsPhoneNumberConfirmed = true;
            }

            user.Name = input.Name;
            user.Surname = input.Surname;
            user.UserName = input.UserName;
            user.EmailAddress = input.EmailAddress;
            user.PhoneNumber = input.PhoneNumber;
            user.IsPhoneNumberConfirmed = input.IsPhoneNumberConfirmed;

            CheckErrors(await UserManager.UpdateAsync(user));

            var userIdentifier = await AbpSession.ToUserIdentifierAsync();

            var settingValues = new List<SettingInfo>();

            if (Clock.SupportsMultipleTimezone)
            {
                if (input.Timezone.IsNullOrEmpty())
                {
                    var defaultValue = await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.User, userIdentifier.TenantId);
                    settingValues.Add(new SettingInfo(TimingSettingNames.TimeZone, defaultValue));
                }
                else
                {
                    settingValues.Add(new SettingInfo(TimingSettingNames.TimeZone, input.Timezone));
                }
            }

            if (input.Options != null)
            {
                settingValues.Add(new SettingInfo(AppSettings.UserOptions.DontShowZeroQuantityWarning, input.Options.DontShowZeroQuantityWarning.ToLowerCaseString()));
                settingValues.Add(new SettingInfo(AppSettings.UserOptions.PlaySoundForNotifications, input.Options.PlaySoundForNotifications.ToLowerCaseString()));
                settingValues.Add(new SettingInfo(AppSettings.UserOptions.HostEmailPreference, input.Options.HostEmailPreference.ToIntString()));
                settingValues.Add(new SettingInfo(AppSettings.UserOptions.DoNotShowWaitingForTicketDownload, input.Options.DoNotShowWaitingForTicketDownload.ToLowerCaseString()));

                input.Options.CCMeOnInvoices &= await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Invoices);
                settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.CCMeOnInvoices, input.Options.CCMeOnInvoices.ToLowerCaseString()));

                if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant))
                {
                    input.Options.AllowCounterSalesForUser &= await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_CounterSales);
                    settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.AllowCounterSalesForUser, input.Options.AllowCounterSalesForUser.ToLowerCaseString()));
                    if (input.Options.AllowCounterSalesForUser)
                    {
                        settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly, input.Options.DefaultDesignationToMaterialOnly.ToLowerCaseString()));
                        settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId, (input.Options.DefaultLoadAtLocationId ?? 0).ToString()));
                        settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.DefaultMaterialItemId, (input.Options.DefaultMaterialItemId ?? 0).ToString()));
                        settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.DefaultMaterialUomId, (input.Options.DefaultMaterialUomId ?? 0).ToString()));
                        settingValues.Add(new SettingInfo(AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber, input.Options.DefaultAutoGenerateTicketNumber.ToLowerCaseString()));
                    }
                    else
                    {
                        var userSettingsToReset = new[]
                        {
                            AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly,
                            AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId,
                            AppSettings.DispatchingAndMessaging.DefaultMaterialItemId,
                            AppSettings.DispatchingAndMessaging.DefaultMaterialUomId,
                            AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber,
                        };
                        foreach (var settingToReset in userSettingsToReset)
                        {
                            var defaultValue = _settingDefinitionManager.GetSettingDefinition(settingToReset).DefaultValue;
                            settingValues.Add(new SettingInfo(settingToReset, defaultValue));
                        }
                    }
                }
            }

            if (settingValues.Count > 0)
            {
                await SettingManager.ChangeSettingsForUserAsync(userIdentifier, settingValues);
            }
        }

        public async Task SetDoNotShowWaitingForTicketDownload(bool value)
        {
            var settingValues = new List<SettingInfo>();
            settingValues.Add(new SettingInfo(AppSettings.UserOptions.DoNotShowWaitingForTicketDownload, value.ToLowerCaseString()));
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            await SettingManager.ChangeSettingsForUserAsync(userIdentifier, settingValues);
        }

        public async Task ChangePassword(ChangePasswordInput input)
        {
            await UserManager.InitializeOptionsAsync(await AbpSession.GetTenantIdOrNullAsync());

            var user = await GetCurrentUserAsync();
            if (await UserManager.CheckPasswordAsync(user, input.CurrentPassword))
            {
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.NewPassword));
            }
            else
            {
                CheckErrors(IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password.",
                }));
            }
        }

        public async Task UpdateProfilePicture(UpdateProfilePictureInput input)
        {
            var allowToUseGravatar = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.AllowUsingGravatarProfilePicture);
            if (!allowToUseGravatar)
            {
                input.UseGravatarProfilePicture = false;
            }

            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            await SettingManager.ChangeSettingForUserAsync(
                userIdentifier,
                AppSettings.UserManagement.UseGravatarProfilePicture,
                input.UseGravatarProfilePicture.ToString().ToLowerInvariant()
            );

            if (input.UseGravatarProfilePicture)
            {
                return;
            }

            byte[] byteArray;

            var imageBytes = await _tempFileCacheManager.GetFileAsync(input.FileToken);

            if (imageBytes == null)
            {
                throw new UserFriendlyException("There is no such image file with the token: " + input.FileToken);
            }
            using (var bmpImage = new Bitmap(new MemoryStream(imageBytes)))
            {
                var width = (input.Width == 0 || input.Width > bmpImage.Width) ? bmpImage.Width : input.Width;
                var height = (input.Height == 0 || input.Height > bmpImage.Height) ? bmpImage.Height : input.Height;
                var bmCrop = bmpImage.Clone(new Rectangle(input.X, input.Y, width, height), bmpImage.PixelFormat);

                using (var stream = new MemoryStream())
                {
                    bmCrop.Save(stream, bmpImage.RawFormat);
                    byteArray = stream.ToArray();
                }
            }


            if (byteArray.Length > MaxProfilPictureBytes)
            {
                throw new UserFriendlyException(L("ResizedProfilePicture_Warn_SizeLimit",
                    AppConsts.ResizedMaxProfilePictureBytesUserFriendlyValue));
            }

            if (input.UserId != Session.UserId)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Administration_Users_Edit);
            }

            var user = await UserManager.GetUserByIdAsync(input.UserId);

            if (user.ProfilePictureId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(user.ProfilePictureId.Value);
            }

            var storedFile = new BinaryObject(userIdentifier.TenantId, byteArray, $"Profile picture of user {AbpSession.UserId}. {DateTime.UtcNow}");
            await _binaryObjectManager.SaveAsync(storedFile);

            user.ProfilePictureId = storedFile.Id;
            await UserManager.UpdateAsync(user);
        }

        public async Task UpdateSignaturePicture(UpdateSignaturePictureInput input)
        {
            var imageBytes = await _tempFileCacheManager.GetFileAsync(input.FileToken);

            if (imageBytes == null)
            {
                throw new UserFriendlyException("There is no such signature image file with the token: " + input.FileToken);
            }

            if (imageBytes.LongLength == 0)
            {
                throw new UserFriendlyException("Resized image is empty. Please try again.");
            }

            //1 MB
            if (imageBytes.LongLength > 1048576)
            {
                throw new UserFriendlyException("Resized image size must be less than 1MB. Please resize down and try again.");
            }

            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            var user = await UserManager.GetUserByIdAsync(await AbpSession.GetUserIdAsync());

            if (user.SignaturePictureId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(user.SignaturePictureId.Value);
            }

            var storedFile = new BinaryObject(userIdentifier.TenantId, imageBytes);
            await _binaryObjectManager.SaveAsync(storedFile);

            user.SignaturePictureId = storedFile.Id;
            await UserManager.UpdateAsync(user);
        }


        [AbpAllowAnonymous]
        public async Task<GetPasswordComplexitySettingOutput> GetPasswordComplexitySetting()
        {
            return new GetPasswordComplexitySettingOutput
            {
                Setting = await _passwordComplexitySettingStore.GetSettingsAsync(),
            };
        }

        [DisableAuditing]
        public async Task<GetProfilePictureOutput> GetProfilePicture()
        {
            var userIdentifier = await AbpSession.ToUserIdentifierAsync();
            using (var profileImageService = await _profileImageServiceFactory.Get(userIdentifier))
            {
                var profilePictureContent = await profileImageService.Object.GetProfilePictureContentForUser(
                    userIdentifier
                );

                return new GetProfilePictureOutput(profilePictureContent);
            }
        }

        public async Task<GetProfilePictureOutput> GetProfilePictureByUserName(string username)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
            {
                return new GetProfilePictureOutput(string.Empty);
            }

            var userIdentifier = new UserIdentifier(await AbpSession.GetTenantIdOrNullAsync(), user.Id);
            using (var profileImageService = await _profileImageServiceFactory.Get(userIdentifier))
            {
                var profileImage = await profileImageService.Object.GetProfilePictureContentForUser(userIdentifier);
                return new GetProfilePictureOutput(profileImage);
            }
        }

        public async Task<GetProfilePictureOutput> GetFriendProfilePicture(GetFriendProfilePictureInput input)
        {
            var friendUserIdentifier = input.ToUserIdentifier();
            var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(
                await AbpSession.ToUserIdentifierAsync(),
                friendUserIdentifier
            );

            if (friendShip == null)
            {
                return new GetProfilePictureOutput(string.Empty);
            }

            using (var profileImageService = await _profileImageServiceFactory.Get(friendUserIdentifier))
            {
                var image = await profileImageService.Object.GetProfilePictureContentForUser(friendUserIdentifier);
                return new GetProfilePictureOutput(image);
            }
        }

        public async Task<GetProfilePictureOutput> GetProfilePictureByUser(long userId)
        {
            var userIdentifier = new UserIdentifier(await AbpSession.GetTenantIdOrNullAsync(), userId);
            using (var profileImageService = await _profileImageServiceFactory.Get(userIdentifier))
            {
                var profileImage = await profileImageService.Object.GetProfilePictureContentForUser(userIdentifier);
                return new GetProfilePictureOutput(profileImage);
            }
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                await AbpSession.ToUserIdentifierAsync(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        private async Task<byte[]> GetProfilePictureByIdOrNull(Guid profilePictureId)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(profilePictureId);
            if (file == null)
            {
                return null;
            }

            return file.Bytes;
        }

        private async Task<GetProfilePictureOutput> GetProfilePictureByIdInternal(Guid profilePictureId)
        {
            var bytes = await GetProfilePictureByIdOrNull(profilePictureId);
            if (bytes == null)
            {
                return new GetProfilePictureOutput(string.Empty);
            }

            return new GetProfilePictureOutput(Convert.ToBase64String(bytes));
        }
    }
}
