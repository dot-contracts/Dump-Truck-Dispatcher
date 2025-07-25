using System;
using Abp.Extensions;
using DispatcherWeb.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DispatcherWeb.Web.Startup
{
    public static class AuthConfigurer
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            var cookieDomain = configuration.GetCookieDomain();

            var authenticationBuilder = services.AddAuthentication().AddCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Domain = cookieDomain;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".AspNet.SharedCookie";
                options.Cookie.Path = "/";
                options.Cookie.Domain = cookieDomain;
            });

            if (bool.Parse(configuration["Authentication:OpenId:IsEnabled"]))
            {
                authenticationBuilder.AddOpenIdConnect(options =>
                {
                    options.ClientId = configuration["Authentication:OpenId:ClientId"];
                    options.Authority = configuration["Authentication:OpenId:Authority"];
                    options.SignedOutRedirectUri = configuration["App:WebSiteRootAddress"] + "Account/Logout";
                    options.ResponseType = OpenIdConnectResponseType.IdToken;

                    var clientSecret = configuration["Authentication:OpenId:ClientSecret"];
                    if (!clientSecret.IsNullOrEmpty())
                    {
                        options.ClientSecret = clientSecret;
                    }
                });
            }

            if (bool.Parse(configuration["Authentication:Microsoft:IsEnabled"]))
            {
                authenticationBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = configuration["Authentication:Microsoft:ConsumerKey"];
                    options.ClientSecret = configuration["Authentication:Microsoft:ConsumerSecret"];
                });
            }

            if (bool.Parse(configuration["Authentication:Google:IsEnabled"]))
            {
                authenticationBuilder.AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                });
            }

            if (bool.Parse(configuration["Authentication:Twitter:IsEnabled"]))
            {
                authenticationBuilder.AddTwitter(options =>
                {
                    options.ConsumerKey = configuration["Authentication:Twitter:ConsumerKey"];
                    options.ConsumerSecret = configuration["Authentication:Twitter:ConsumerSecret"];
                    options.RetrieveUserDetails = true;
                });
            }

            if (bool.Parse(configuration["Authentication:Facebook:IsEnabled"]))
            {
                authenticationBuilder.AddFacebook(options =>
                {
                    options.AppId = configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = configuration["Authentication:Facebook:AppSecret"];

                    options.Scope.Add("email");
                    options.Scope.Add("public_profile");
                });
            }

            //if (bool.Parse(configuration["Authentication:WsFederation:IsEnabled"]))
            //{
            //    authenticationBuilder.AddWsFederation(options =>
            //    {
            //        options.MetadataAddress = configuration["Authentication:WsFederation:MetaDataAddress"];
            //        options.Wtrealm = configuration["Authentication:WsFederation:Wtrealm"];
            //        options.Events.OnSecurityTokenValidated = context =>
            //        {
            //            var emailClaim = context.Principal.Claims.ToList().FirstOrDefault(c => c.Type == ClaimTypes.Name);

            //            if (emailClaim == null)
            //            {
            //                return Task.FromResult(0);
            //            }

            //            context.Principal.AddIdentity(new ClaimsIdentity(new List<Claim>
            //            {
            //                new Claim(ClaimTypes.Email, emailClaim.Value),
            //            }));

            //            return Task.FromResult(0);
            //        };
            //    });
            //}

            if (bool.Parse(configuration["Authentication:JwtBearer:IsEnabled"]))
            {
                authenticationBuilder.AddJwtBearer(options =>
                {
                    options.UseSecurityTokenValidators = true; //TODO: we need to fix the validation to use the new recommended .NET9 approach and set this line back to its default `false` value
                    options.Authority = configuration["Authentication:JwtBearer:Issuer"];
                    options.Audience = configuration["Authentication:JwtBearer:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,

                        // Validate the JWT Issuer (iss) claim
                        ValidateIssuer = bool.Parse(configuration["Authentication:JwtBearer:ValidateIssuer"] ?? "false"),
                        ValidIssuer = configuration["Authentication:JwtBearer:Issuer"],

                        // Validate the JWT Audience (aud) claim
                        ValidateAudience = true,
                        ValidAudience = configuration["Authentication:JwtBearer:Audience"],

                        // Validate the token expiry
                        ValidateLifetime = true,

                        // If you want to allow a certain amount of clock drift, set that here
                        ClockSkew = TimeSpan.Zero,
                    };
                });
            }
        }

    }
}
