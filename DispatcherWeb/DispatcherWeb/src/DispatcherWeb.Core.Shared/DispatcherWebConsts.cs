using System;

namespace DispatcherWeb
{
    public static class DispatcherWebConsts
    {
        public const string LocalizationSourceName = "DispatcherWeb";

        public const string ConnectionStringName = "Default";

        public const bool AllowTenantsToChangeEmailSettings = false;

        public const string Currency = "USD";

        public const string CurrencySign = "$";

        public const string AbpApiClientUserAgent = "AbpApiClient";


        public const int PaymentCacheDurationInMinutes = 30;

        public const string EncryptionKey = "EncryptionKey";

        public const string OfficesOrganizationUnitName = "Offices";

        public const string PasswordHasntBeenChanged = "*•*•*•*•*•*•*•*•*•*";

        /// <summary>
        /// Data type to use for most financial amounts
        /// </summary>
        public const string DbTypeDecimal19_4 = "decimal(19, 4)";

        /// <summary>
        /// Data type to use for quantities
        /// </summary>
        public const string DbTypeDecimal18_4 = "decimal(18, 4)";

        /// <summary>
        /// Data type to use for utilization percentages and readings
        /// </summary>
        public const string DbTypeDecimal18_2 = "decimal(18, 2)";

        public const string DbTypeDecimalLocation = "decimal(12, 9)";

        public static DateTime MinDateTime = new DateTime(2000, 1, 1);

        // Note:
        // Minimum accepted payment amount. If a payment amount is less then that minimum value payment progress will continue without charging payment
        // Even though we can use multiple payment methods, users always can go and use the highest accepted payment amount.
        //For example, you use Stripe and PayPal. Let say that stripe accepts min 5$ and PayPal accepts min 3$. If your payment amount is 4$.
        // User will prefer to use a payment method with the highest accept value which is a Stripe in this case.
        public const decimal MinimumUpgradePaymentAmount = 1M;

        public static class SignalRGroups
        {
            public static class ListCacheSyncRequest
            {
                public const string AllTenants = "ListCacheSyncRequest";
                public static string Tenant(int? tenantId)
                {
                    return $"ListCacheSyncRequest_Tenant{tenantId}";
                }
            }
        }

        public static class Session
        {
            public const string IsSidebarCollapsed = "IsSidebarCollapsed";
            public const string IsHideCompletedOrdersSet = "IsHideCompletedOrdersSet";
        }

        public static class Claims
        {
            public const string UserOfficeId = "Application_UserOfficeId";
            public const string UserOfficeName = "Application_UserOfficeName";
            public const string UserOfficeCopyChargeTo = "Application_UserOfficeCopyDeliverToLoadAtChargeTo";
            public const string UserCustomerId = "Application_UserCustomerId";
            public const string UserCustomerName = "Application_UserCustomerName";
            public const string UserName = "Application_UserName";
            public const string UserEmail = "Application_UserEmail";
            public const string UserLeaseHaulerId = "Application_UserLeaseHaulerId";
        }

        public static class DefaultSettings
        {
            public static class Quote
            {
                public const string DefaultNotes = "";
                public const string GeneralTermsAndConditions = "";
            }
        }

        public static class AssemblyNames
        {
            public const string ApplicationApi = "DispatcherWeb.Application";
            public const string ActiveReportsApi = "DispatcherWeb.Application.ActiveReports";
            public const string DriverAppApi = "DispatcherWeb.Application.DriverApp";
            public const string WebMvc = "DispatcherWeb.Web.Mvc";
        }
    }
}
