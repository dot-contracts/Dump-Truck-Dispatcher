using Abp.Extensions;

namespace DispatcherWeb.Authentication
{
    public class FacebookExternalLoginProviderSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }

        public bool IsValid()
        {
            return !AppId.IsNullOrWhiteSpace() && !AppSecret.IsNullOrWhiteSpace();
        }
    }
}