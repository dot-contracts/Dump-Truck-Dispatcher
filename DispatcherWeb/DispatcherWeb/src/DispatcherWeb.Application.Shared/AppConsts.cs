using System;

namespace DispatcherWeb
{
    /// <summary>
    /// Some consts used in the application.
    /// </summary>
    public class AppConsts
    {
        /// <summary>
        /// Default page size for paged requests.
        /// </summary>
        public const int DefaultPageSize = 10;

        /// <summary>
        /// Maximum allowed page size for paged requests.
        /// </summary>
        public const int MaxPageSize = 1000;

        public const int ResizedMaxProfilePictureBytesUserFriendlyValue = 1024;

        public const int MaxProfilePictureBytesUserFriendlyValue = 5;
        public const int MaxSignaturePictureBytesUserFriendlyValue = 1;

        public const long MaxMoneySize = 922337203685477; //922,337,203,685,477.5807
        public const long MaxHeartlandPaymentSize = 9999999999;
        public const decimal MaxDecimalDatabaseLength = 999999999999999m; //decimal(19, 4)

        public const int MaxSmsLength = 1600;

        public const double TicketPhotoWidthCm = 13;
        public const double TicketPhotoFullPageWidthCm = 18;

        public const string TokenValidityKey = "token_validity_key";
        public const string RefreshTokenValidityKey = "refresh_token_validity_key";
        public const string SecurityStampKey = "AspNet.Identity.SecurityStamp";

        public const string TokenType = "token_type";

        public static string UserIdentifier = "user_identifier";

        public const string ThemeDefault = "default";
        public const string Theme2 = "theme2";
        public const string Theme8 = "theme8";
        public const string Theme11 = "theme11";

        public static TimeSpan AccessTokenExpiration = TimeSpan.FromDays(1);
        public static TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(365);

        public const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:sszzz";
        public const int TruckFileThumbnailSize = 100;

        public const string PreventiveMaintenanceServiceTypeName = "Preventive Maintenance";

        public const int ImportFilesExpireDayNumber = 30;
        public const int DeleteExpiredImportFilesWorkerPeriod = 1000 * 3600 * 24; // Once a day

        public const string NonBreakableSpace = " ";
        public const string FuelSurchargeCalculationBlankName = NonBreakableSpace;
    }
}
