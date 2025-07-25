namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public static class WialonCommands
    {
        public const string TokenLogin = "token/login";
        public const string Logout = "core/logout";
        public const string SearchItems = "core/search_items";
        public const string SearchItem = "core/search_item";
        public const string GetBillingPlans = "account/get_billing_plans";
        public const string GetDeviceTypes = "core/get_hw_types";
        public const string CreateUnit = "core/create_unit";
        public const string UpdateDeviceType = "unit/update_device_type";
        public const string UpdateUnitPassword = "unit/update_access_password";
        public const string UpdateProfileField = "item/update_profile_field";
        public const string DeleteItem = "item/delete_item";
        public const string UpdateUnitDriverRankSettings = "unit/update_drive_rank_settings";
        public const string ImportMessages = "exchange/import_messages";
        public static string[] AnonymousCommands = new[] { TokenLogin };
    }
}
