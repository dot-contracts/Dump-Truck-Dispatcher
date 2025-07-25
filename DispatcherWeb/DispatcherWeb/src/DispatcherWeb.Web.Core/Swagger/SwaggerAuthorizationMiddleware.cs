using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.Swagger
{
    public class SwaggerAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfigurationRoot _appConfiguration;

        public SwaggerAuthorizationMiddleware(RequestDelegate next, IAppConfigurationAccessor configurationAccessor)
        {
            _next = next;
            _appConfiguration = configurationAccessor.Configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_appConfiguration["App:SwaggerEndPoint"]))
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                if (context.User.Identity.IsAuthenticated)
                {
                    long userId;
                    long.TryParse(context.User.FindFirst("sub")?.Value, out userId);

                    using (var _permissionChecker = IocManager.Instance.ResolveAsDisposable<PermissionChecker>())
                    {
                        if (!await _permissionChecker.Object.IsGrantedAsync(userId, AppPermissions.Pages_SwaggerAccess))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Unauthorized");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
