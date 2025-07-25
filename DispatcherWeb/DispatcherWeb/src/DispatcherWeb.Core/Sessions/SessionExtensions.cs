using Abp.UI;
using DispatcherWeb.Localization;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.Sessions
{
    public static class SessionExtensions
    {
        public static int GetOfficeIdOrThrow(this IExtendedAbpSession session)
        {
            if (session.OfficeId.HasValue)
            {
                return session.OfficeId.Value;
            }
            throw new UserFriendlyException("You must have an assigned Office in User Details to use that function");
        }

        public static int GetCustomerIdOrThrow(this IExtendedAbpSession session, ILocalizationHelperProvider localizationHelperProvider)
        {
            if (session.CustomerId.HasValue)
            {
                return session.CustomerId.Value;
            }

            var localizationHelper = localizationHelperProvider.LocalizationHelper;
            throw new UserFriendlyException(localizationHelper.L("CustomerPortalUserWithCustomerIdMissing"));
        }

        public static int GetLeaseHaulerIdOrThrow(this IExtendedAbpSession session, ILocalizationHelperProvider localizationHelperProvider)
        {
            if (session.LeaseHaulerId.HasValue)
            {
                return session.LeaseHaulerId.Value;
            }

            var localizationHelper = localizationHelperProvider.LocalizationHelper;
            throw new UserFriendlyException(localizationHelper.L("LeaseHaulerPortalUserWithLeaseHaulerIdMissing"));
        }
    }
}
