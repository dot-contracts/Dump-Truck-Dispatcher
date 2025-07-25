using System.Collections.Generic;

namespace DispatcherWeb.Web.Common
{
    public static class WebConsts
    {
        public const string SwaggerUiEndPoint = "/swagger";
        public const string HangfireDashboardEndPoint = "/hangfire";

        public static bool HangfireDashboardEnabled = true;

        public static List<string> ReCaptchaIgnoreWhiteList = new List<string>
        {
            DispatcherWebConsts.AbpApiClientUserAgent,
        };

        public static class GraphQL
        {
            public const string PlaygroundEndPoint = "/ui/playground";
            public const string EndPoint = "/graphql";

            public static bool PlaygroundEnabled = false;
            public static bool Enabled = false;
        }

        public static class CacheProfiles
        {
            public const string StaticFiles = "StaticFiles";
        }
    }
}
