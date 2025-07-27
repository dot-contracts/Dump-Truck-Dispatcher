using System.Collections.Generic;
using DispatcherWeb.Authorization.Accounts;
using DispatcherWeb.Authorization.Permissions;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Delegation;
using DispatcherWeb.Authorization.Users.Profile;
using DispatcherWeb.Chat;
using DispatcherWeb.Common;
using DispatcherWeb.Configuration;
using DispatcherWeb.Configuration.Host;
using DispatcherWeb.Configuration.Tenants;
using DispatcherWeb.CspReports;
using DispatcherWeb.CustomerNotifications;
using DispatcherWeb.DashboardCustomization;
using DispatcherWeb.Designations;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Settings;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.Emailing;
using DispatcherWeb.Friendships;
using DispatcherWeb.Fulcrum;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Locations;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.Accounting;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Sessions;
using DispatcherWeb.Sms;
using DispatcherWeb.TaxRates;
using DispatcherWeb.Tests.Security.AnonymousAccess.Dto;
using DispatcherWeb.Timing;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using DispatcherWeb.Web.Areas.app.Controllers;
using DispatcherWeb.Web.Areas.App.Controllers;
using DispatcherWeb.Web.Controllers;
using AssemblyNames = DispatcherWeb.DispatcherWebConsts.AssemblyNames;
using HomeController = DispatcherWeb.Web.Controllers.HomeController;

namespace DispatcherWeb.Tests.Security.AnonymousAccess
{
    public static class AnonymousAccessExceptions
    {
        public static readonly AnonymousAccessExceptionDto ExplicitAnonymousAccess = new AnonymousAccessExceptionDto()
            .AddExceptions(AssemblyNames.ApplicationApi, new List<(string className, string methodName, string justification)>
            {
                (nameof(EmailAppService), nameof(EmailAppService.TrackEmailOpen), "Might get called from email clients"),
                (nameof(EmailAppService), nameof(EmailAppService.TrackEvents), "Needs to be accessible by SendGrid"),
                (nameof(SmsAppService), nameof(SmsAppService.SetSmsStatus), "Needs to be accessible by Twilio"),
                (nameof(DispatchingAppService), nameof(DispatchingAppService.UploadAnonymousLogs), "Used to upload anonymous logs"),
                (nameof(FulcrumAppService), nameof(FulcrumAppService.CompleteDtdTicket), "Needs to be accessible by Fulcrum"),

                //These methods might be used during login process
                (nameof(ProfileAppService), nameof(ProfileAppService.GetPasswordComplexitySetting), "This needs to be accessible on Register views"),
                (nameof(SettingsAppServiceBase), nameof(SettingsAppServiceBase.GetEnabledSocialLoginSettings), "Per name, this might be used for login. Doesn't look like this is in use at the moment though"),
                (nameof(HostSettingsAppService), nameof(HostSettingsAppService.GetEnabledSocialLoginSettings), "Per name, this might be used for login. Doesn't look like this is in use at the moment though"),
                (nameof(TenantSettingsAppService), nameof(TenantSettingsAppService.GetEnabledSocialLoginSettings), "Per name, this might be used for login. Doesn't look like this is in use at the moment though"),
                (nameof(CspReportAppService), nameof(CspReportAppService.PostReport), "Needs to be available on all views, including login/register"),
                (nameof(SessionAppService), nameof(SessionAppService.GetCurrentLoginInformations), "Needs to be available on all views, including login"),
                (nameof(AccountAppService), nameof(AccountAppService.IsTenantAvailable), "Used on Login view"),
                (nameof(AccountAppService), nameof(AccountAppService.Register), "Used on Login view"),
                (nameof(AccountAppService), nameof(AccountAppService.SendPasswordResetCode), "Used on Login view"),
                (nameof(AccountAppService), nameof(AccountAppService.ResetPassword), "Used on Login view"),
                (nameof(AccountAppService), nameof(AccountAppService.SendEmailActivationLink), "Used on Login view"),
                (nameof(AccountAppService), nameof(AccountAppService.ActivateEmail), "Used on Login view"),

                //These methods are used by anonymous LH users
                (nameof(LeaseHaulerAppService), nameof(LeaseHaulerAppService.GetLeaseHaulerDriversSelectList), "Anonymous LH users use this method"),
                (nameof(LeaseHaulerAppService), nameof(LeaseHaulerAppService.GetLeaseHaulerTrucksSelectList), "Anonymous LH users use this method"),
                (nameof(LeaseHaulerRequestEditAppService), nameof(LeaseHaulerRequestEditAppService.GetAvailableTrucksEditDto), "Anonymous LH users use this method. This is also a bit more protected because it requires to pass a guid"),
                (nameof(LeaseHaulerRequestEditAppService), nameof(LeaseHaulerRequestEditAppService.EditAvailableTrucks), "Anonymous LH users use this method. This is also a bit more protected because it requires to pass a guid"),

                //These methods call custom auth methods internally
                (nameof(DispatchingAppService), nameof(DispatchingAppService.GetDriverDispatchesForCurrentUser), "Calls AuthDriverByDriverGuidIfNeeded internally"),
                (nameof(DispatchingAppService), nameof(DispatchingAppService.GetOrderLineTrucksForCurrentUser), "Calls AuthDriverByDriverGuidIfNeeded internally"),
                (nameof(DispatchingAppService), nameof(DispatchingAppService.ExecuteDriverApplicationAction), "Calls AuthDriverByDriverGuid internally"),
                (nameof(DriverApplicationAppService), nameof(DriverApplicationAppService.GetScheduledStartTimeInfo), "Calls GetDriverIdFromSessionOrGuid internally"),
                (nameof(DriverApplicationAppService), nameof(DriverApplicationAppService.GetEmployeeTimesForCurrentUser), "Calls AuthDriverByDriverGuidIfNeeded internally"),

                //Tenant registration related methods need to be accessible by anonymous users
                (nameof(TenantRegistrationAppService), nameof(TenantRegistrationAppService.RegisterTenant), "See the top comment for this tenant registration related block"),
                (nameof(TenantRegistrationAppService), nameof(TenantRegistrationAppService.GetEditionsForSelect), "See the top comment for this tenant registration related block"),
                (nameof(TenantRegistrationAppService), nameof(TenantRegistrationAppService.GetEdition), "See the top comment for this tenant registration related block"),

                //Payment related methods were already accessible by anonymous users, we're adding them to exceptions for now since it looks like new users might need to use them
                //These weren't looked into in detail, so they might need to be removed from here and protected later
                //As of now, none of these are supposed to be in use
                (nameof(PaymentAppService), nameof(PaymentAppService.CreatePayment), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.CancelPayment), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.GetPaymentHistory), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.GetActiveGateways), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.GetPaymentAsync), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.GetLastCompletedPayment), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.BuyNowSucceed), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.NewRegistrationSucceed), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.UpgradeSucceed), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.ExtendSucceed), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.PaymentFailed), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.SwitchBetweenFreeEditions), "See the top comment for this payment related block"),
                (nameof(PaymentAppService), nameof(PaymentAppService.UpgradeSubscriptionCostsLessThenMinAmount), "See the top comment for this payment related block"),
                (nameof(PayPalPaymentAppService), nameof(PayPalPaymentAppService.ConfirmPayment), "See the top comment for this payment related block"),
                (nameof(PayPalPaymentAppService), nameof(PayPalPaymentAppService.GetConfiguration), "See the top comment for this payment related block"),
                (nameof(StripePaymentAppService), nameof(StripePaymentAppService.GetConfiguration), "See the top comment for this payment related block"),
                (nameof(StripePaymentAppService), nameof(StripePaymentAppService.GetPaymentAsync), "See the top comment for this payment related block"),
                (nameof(StripePaymentAppService), nameof(StripePaymentAppService.CreatePaymentSession), "See the top comment for this payment related block"),
                (nameof(StripePaymentAppService), nameof(StripePaymentAppService.GetPaymentResult), "See the top comment for this payment related block"),
                (nameof(InvoiceAppService), nameof(InvoiceAppService.GetInvoiceInfo), "See the top comment for this payment related block"),
                (nameof(InvoiceAppService), nameof(InvoiceAppService.CreateInvoice), "See the top comment for this payment related block"),
            })
            .AddExceptions(AssemblyNames.DriverAppApi, new List<(string className, string methodName, string justification)>
            {
                (nameof(LogAppService), nameof(LogAppService.Post), "Deprecated, empty body kept for backwards compatibility"),
                (nameof(LogAppService), nameof(LogAppService.GetMinLevelToUpload), "Deprecated, kept for backwards compatibility, static body poses no security risk"),
                (nameof(DriverApp.Account.AccountAppService), nameof(DriverApp.Account.AccountAppService.SendPasswordResetCode), "Used to reset password from RN app"),
                (nameof(DriverApp.Account.AccountAppService), nameof(DriverApp.Account.AccountAppService.ResetPassword), "Used to reset password from RN app"),
                (nameof(DriverApp.Account.AccountAppService), nameof(DriverApp.Account.AccountAppService.ValidateTenant), "Used to validate the tenant name before logging in"),
                (nameof(DriverApp.Account.AccountAppService), nameof(DriverApp.Account.AccountAppService.TestUserFriendlyException), "This always returns an exception and is needed for testing purposes"),
                (nameof(DriverApp.Account.AccountAppService), nameof(DriverApp.Account.AccountAppService.TestInternalError), "This always returns an exception and is needed for testing purposes"),
            })
            .AddExceptions(AssemblyNames.WebMvc, new List<(string className, string methodName, string justification)>
            {
                //Most of these were previously allowed and should probably continue to be allowed to all anonymous users
                (nameof(EmailsController), nameof(EmailsController.TrackEmailOpen), "Might get called from email clients"),
                (nameof(EmailsController), nameof(EmailsController.TrackEvents), "Needs to be accessible by SendGrid"),
                (nameof(SmsCallbackController), nameof(SmsCallbackController.Index), "Needs to be accessible by Twilio"),
                (nameof(LeaseHaulerRequestsController), nameof(LeaseHaulerRequestsController.AvailableTrucks), "Anonymous LH users use this method. This is also a bit more protected because it requires to pass a guid"),
                (nameof(AccountController), "*", "Most of the methods in this controller are used during the login process"),
                (nameof(HomeController), nameof(HomeController.Index), "See the block comment above"),
                (nameof(HomeController), nameof(HomeController.Error), "See the block comment above"),
                (nameof(PaymentController), "*", "See the block comment above"),
                (nameof(TenantRegistrationController), nameof(TenantRegistrationController.SelectEdition), "See the block comment above"),
                (nameof(TenantRegistrationController), nameof(TenantRegistrationController.Register), "See the block comment above"),
                (nameof(CspReportsController), nameof(CspReportsController.Post), "CSP needs to be available on all views, including login/register"),
                (nameof(InvoiceController), nameof(InvoiceController.Index), "See the block comment above"),
            });


        public static readonly AnonymousAccessExceptionDto ImplicitAnonymousAccess = new AnonymousAccessExceptionDto();
        //none should be accepted, we should always add an explicit AbpAnonymousAccess attribute


        public static readonly AnonymousAccessExceptionDto NoPermissionRequired = new AnonymousAccessExceptionDto()
            .AddExceptions(AssemblyNames.ApplicationApi, new List<(string className, string methodName, string justification)>
            {
                (nameof(ProfileAppService), nameof(ProfileAppService.GetProfilePictureByUser), "Users can see profile photos of other users without any additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.GetProfilePictureByUserName), "Users can see profile photos of other users"),
                (typeof(ReportAppServiceBase<>).Name, nameof(ReportAppServiceBase<object>.CreateCsv), "This is a method of abstract class"),
                (typeof(ReportAppServiceBase<>).Name, nameof(ReportAppServiceBase<object>.CreatePdf), "This is a method of abstract class"),
                (nameof(UnitOfMeasureAppService), nameof(UnitOfMeasureAppService.GetUnitsOfMeasureSelectList), "UOMs are not secret"),
                (nameof(TruckAppService), nameof(TruckAppService.GetVehicleCategories), "Vehicle categories are not secret and the method doesn't have any special filtering that should be Tenant/Portal/LH specific"),
                (nameof(TaxRateAppService), nameof(TaxRateAppService.GetTaxRatesSelectList), "Tax rates (list only) is probably not secret"),
                (nameof(DesignationAppService), nameof(DesignationAppService.GetDesignationSelectListItemsAsync), "Designations are enum and not secret"),
                (nameof(LocationAppService), nameof(LocationAppService.GetLocationCategorySelectList), "Location Categories are predefined"),
                (nameof(OfficeAppService), nameof(OfficeAppService.GetOfficesSelectList), "Office list is not secret, especially since GetAllOffices will be available anyway"),
                (nameof(OfficeAppService), nameof(OfficeAppService.GetAllOffices), "Required for global CSS styles"),
                (nameof(SessionAppService), nameof(SessionAppService.UpdateUserSignInToken), "This is used in the login process and doesn't require additional permissions"),
                (nameof(TimingAppService), nameof(TimingAppService.GetTimezones), "Timezones are not secret, but we still need them to be authenticated to get the correct setting. No additional permissions are required."),
                (nameof(TimingAppService), nameof(TimingAppService.GetTimezoneComboboxItems), "Timezones are not secret, but we still need them to be authenticated to get the correct setting. No additional permissions are required."),
                (nameof(PermissionAppService), nameof(PermissionAppService.GetGrantedPermissions), "They don't need permissions to read their permissions"),
                (nameof(PermissionAppService), nameof(PermissionAppService.GetAllPermissionsAsync), "The list of all permissions is not secret, they can already access it using other ways"),
                (nameof(DriverAssignmentAppService), nameof(DriverAssignmentAppService.ThereAreOpenDispatchesForDriverOnDate), "This method does not disclose any private information except for telling the user whether there are open dispatches for a given driver id"),
                (nameof(DriverAssignmentAppService), nameof(DriverAssignmentAppService.ThereAreOpenDispatchesForTruckOnDate), "This method does not disclose any private information except for telling the user whether there are open dispatches for a given truck id"),
                (nameof(AccountAppService), nameof(AccountAppService.DelegatedImpersonate), "This method compares TargetUserId with Session.UserId and does not require a permission check"),
                (nameof(AccountAppService), nameof(AccountAppService.BackToImpersonator), "This method checks AbpSession.ImpersonatorUserId value"),
                (nameof(AccountAppService), nameof(AccountAppService.SwitchToLinkedAccount), "This method runs _userLinkManager.AreUsersLinked check"),
                (nameof(CommonLookupAppService), nameof(CommonLookupAppService.GetEditionsForCombobox), "Might be needed for tenant self registration"),
                (nameof(CommonLookupAppService), nameof(CommonLookupAppService.GetDefaultEditionName), "Returns a constant"),
                (nameof(UserLoginAppService), nameof(UserLoginAppService.GetUserLoginAttempts), "Returns records belonging to the the logged in user"),

                //All users can read and modify their own notifications without having to have a special permission assigned. Unless something changes in the future.
                (nameof(NotificationAppService), nameof(NotificationAppService.GetUserNotifications), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.GetUnreadPriorityNotifications), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.SetAllNotificationsAsRead), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.SetNotificationAsRead), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.GetNotificationSettings), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.UpdateNotificationSettings), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.DeleteNotification), "All users can read their own notifications"),
                (nameof(NotificationAppService), nameof(NotificationAppService.DeleteAllUserNotifications), "All users can read their own notifications"),

                //All profile app service methods look like they should be accessible by the user without any additional permissions
                (nameof(ProfileAppService), nameof(ProfileAppService.GetCurrentUserProfileForEdit), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.DisableGoogleAuthenticator), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.UpdateGoogleAuthenticatorKey), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.SendVerificationSms), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.VerifySmsCode), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.PrepareCollectedData), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.UpdateCurrentUserProfile), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.ChangePassword), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.UpdateProfilePicture), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.UpdateSignaturePicture), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.GetProfilePicture), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.GetFriendProfilePicture), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.ChangeLanguage), "All users can change their own profile settings without additional permissions"),
                (nameof(ProfileAppService), nameof(ProfileAppService.SetDoNotShowWaitingForTicketDownload), "All users can set do not show waiting for ticket download settings without additional permissions"),
                
                //We can allow all users to link other users without requiring additional permissions
                (nameof(UserLinkAppService), nameof(UserLinkAppService.LinkToUser), "All users can link users with no additional permissions"),
                (nameof(UserLinkAppService), nameof(UserLinkAppService.GetLinkedUsers), "All users can link users with no additional permissions"),
                (nameof(UserLinkAppService), nameof(UserLinkAppService.GetRecentlyUsedLinkedUsers), "All users can link users with no additional permissions"),
                (nameof(UserLinkAppService), nameof(UserLinkAppService.UnlinkUser), "All users can link users with no additional permissions"),

                //Doesn't look like this is implemented. Added to allowed exceptions for now so that it doesn't fail the test.
                //I don't see this posing a security risk if allowed to all users with no additional permissions
                (nameof(UserDelegationAppService), nameof(UserDelegationAppService.GetDelegatedUsers), "See the block comment above"),
                (nameof(UserDelegationAppService), nameof(UserDelegationAppService.DelegateNewUser), "See the block comment above"),
                (nameof(UserDelegationAppService), nameof(UserDelegationAppService.RemoveDelegation), "See the block comment above"),
                (nameof(UserDelegationAppService), nameof(UserDelegationAppService.GetActiveUserDelegations), "See the block comment above"),

                //We don't have a chat specific permission and we can keep it this way for now
                (nameof(ChatAppService), nameof(ChatAppService.GetUserChatFriendsWithSettings), "See the block comment above"),
                (nameof(ChatAppService), nameof(ChatAppService.GetUserChatMessages), "See the block comment above"),
                (nameof(ChatAppService), nameof(ChatAppService.MarkAllUnreadMessagesOfUserAsRead), "See the block comment above"),

                //We don't have a specific permission for friendship
                (nameof(FriendshipAppService), nameof(FriendshipAppService.CreateFriendshipRequest), "See the block comment above"),
                (nameof(FriendshipAppService), nameof(FriendshipAppService.CreateFriendshipRequestByUserName), "See the block comment above"),
                (nameof(FriendshipAppService), nameof(FriendshipAppService.BlockUser), "See the block comment above"),
                (nameof(FriendshipAppService), nameof(FriendshipAppService.UnblockUser), "See the block comment above"),
                (nameof(FriendshipAppService), nameof(FriendshipAppService.AcceptFriendshipRequest), "See the block comment above"),

                //Dashboard Customization affects only the user's own dashboard and doesn't require additional permissions
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.GetUserDashboard), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.SavePage), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.RenamePage), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.AddNewPage), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.DeletePage), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.AddWidget), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.GetDashboardDefinition), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.GetAllWidgetDefinitions), "Dashboard customization is only for the user's own dashboard"),
                (nameof(DashboardCustomizationAppService), nameof(DashboardCustomizationAppService.GetSettingName), "Dashboard customization is only for the user's own dashboard"),

                //We don't have a specific permission for customer notifications. Host admin can select specific roles before sending the customer notification to avoid LH Portal users receiving them.
                (nameof(CustomerNotificationAppService), nameof(CustomerNotificationAppService.GetCustomerNotificationsToShow), "See the block comment above"),
                (nameof(CustomerNotificationAppService), nameof(CustomerNotificationAppService.DismissCustomerNotifications), "See the block comment above"),

            })
            .AddExceptions(AssemblyNames.DriverAppApi, new List<(string className, string methodName, string justification)>
            {
                (nameof(SettingsAppService), nameof(SettingsAppService.Get), "We're returning their permissions, features and settings here (read-only), so we do not need to require additional permissions for this method"),
            })
            .AddExceptions(AssemblyNames.WebMvc, new List<(string className, string methodName, string justification)>
            {
                //All of these were previously allowed and should probably continue to be allowed to all users regardless of the users
                (nameof(AccountController), nameof(AccountController.SwitchToLinkedAccount), "See the block comment above"),
                (nameof(AccountController), nameof(AccountController.TestNotification), "See the block comment above"),
                (nameof(DownloadReportFileController), nameof(DownloadReportFileController.Index), "See the block comment above"),
                (nameof(ChatController), nameof(ChatController.GetImage), "See the block comment above"),
                (nameof(ChatController), nameof(ChatController.GetFile), "See the block comment above"),
                (nameof(CommonController), nameof(CommonController.LookupModal), "See the block comment above"),
                (nameof(CommonController), nameof(CommonController.EntityTypeHistoryModal), "See the block comment above"),
                (nameof(CommonController), nameof(CommonController.PermissionTreeModal), "See the block comment above"),
                (nameof(CommonController), nameof(CommonController.InactivityControllerNotifyModal), "See the block comment above"),
                (nameof(NotificationsController), "*", "Profile methods should be available to all users"),
                (nameof(DispatcherWeb.Web.Controllers.ProfileController), "*", "Profile methods should be available to all users"), //profile controller
                (nameof(DispatcherWeb.Web.Areas.App.Controllers.ProfileController), "*", "Profile methods should be available to all users"), //app profile controller
                (nameof(WelcomeController), nameof(WelcomeController.Index), "See the block comment above"),
                (nameof(WelcomeController), nameof(WelcomeController.ChooseRedirectTarget), "See the block comment above"),
            });


        public static readonly AnonymousAccessExceptionDto ImplicitLeaseHaulerPortalAccess = new AnonymousAccessExceptionDto();


        public static readonly AnonymousAccessExceptionDto ImplicitCustomerPortalAccess = new AnonymousAccessExceptionDto();
    }
}
