using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

namespace DispatcherWeb.Web.Authentication.JwtBearer
{
    public static class JwtTokenMiddleware
    {
        public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder app, string schema = JwtBearerDefaults.AuthenticationScheme)
        {
            return app.Use(async (ctx, next) =>
            {
                try
                {
                    // Defensive check for context
                    if (ctx == null)
                    {
                        await next();
                        return;
                    }

                    // Defensive check for User and Identity
                    if (ctx.User?.Identity?.IsAuthenticated != true)
                    {
                        try
                        {
                            var result = await ctx.AuthenticateAsync(schema);
                            if (result?.Succeeded == true && result.Principal != null)
                            {
                                ctx.User = result.Principal;
                            }
                        }
                        catch (Exception)
                        {
                            // If authentication fails, continue without setting the user
                            // This prevents the application from crashing
                        }
                    }

                    await next();
                }
                catch (Exception)
                {
                    // If any exception occurs, continue to the next middleware
                    // This prevents the application from crashing due to JWT middleware issues
                    await next();
                }
            });
        }
    }
}
