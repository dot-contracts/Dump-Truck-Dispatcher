using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Microsoft.AspNetCore.Http;

namespace DispatcherWeb.Web.Session
{
    public class CookieMigrationMiddleware
    {
        public static string CookieDomain = "";
        public static string OldCookieDomain = "";

        private readonly RequestDelegate _next;

        public CookieMigrationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            const string migratedToWildcardDomainCookieName = "MigratedToWildCardDomain2";

            if (string.IsNullOrEmpty(CookieDomain)
                || string.IsNullOrEmpty(OldCookieDomain)
                || context.Request.Cookies[migratedToWildcardDomainCookieName] == "true"
            )
            {
                await _next(context);
                return;
            }

            ReplaceCookieWithWildcardCookie(context, "Abp.TenantId", new CookieOptions
            {
                Expires = Clock.Now.AddYears(5),
            });

            ReplaceCookieWithWildcardCookie(context, ".AspNet.SharedCookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            context.Response.Cookies.Append(migratedToWildcardDomainCookieName, "true", new CookieOptions
            {
                Domain = CookieDomain,
                Expires = Clock.Now.AddYears(5),
                SameSite = SameSiteMode.None,
                Secure = true,
            });

            await _next(context);
        }

        private static void ReplaceCookieWithWildcardCookie(HttpContext context, string cookieName, CookieOptions cookieOptions)
        {
            if (!context.Request.Cookies.Any(x => x.Key == cookieName))
            {
                return;
            }

            var value = context.Request.Cookies[cookieName];

            cookieOptions.Domain = null;
            context.Response.Cookies.Delete(cookieName, cookieOptions);

            cookieOptions.Domain = OldCookieDomain;
            context.Response.Cookies.Delete(cookieName, cookieOptions);

            cookieOptions.Domain = CookieDomain;
            cookieOptions.SameSite = SameSiteMode.None;
            cookieOptions.Secure = true;
            context.Response.Cookies.Append(cookieName, value ?? "", cookieOptions);
        }
    }
}
