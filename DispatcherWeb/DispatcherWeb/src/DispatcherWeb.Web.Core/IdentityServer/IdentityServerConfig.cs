using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.IdentityServer
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<ApiResource> GetApiResources(IConfigurationRoot configuration)
        {
            return new List<ApiResource>
            {
                new ApiResource("default-api", "Default (all) API")
                {
                    Description = "AllFunctionalityYouHaveInTheApplication",
                    ApiSecrets = {new Secret(configuration["IdentityServer:ApiSecret"]) },
                    Scopes = new List<string>
                    {
                        "default-api",
                    },
                },
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope(name: "default-api", displayName: "Default (all) API"),
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
            };
        }

        public static IEnumerable<Client> GetClients(IConfigurationRoot configuration)
        {
            var clients = new List<Client>();
            var corsOrigins = configuration["App:CorsOrigins"]?.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToHashSet() ?? new HashSet<string>();

            foreach (var child in configuration.GetSection("IdentityServer:Clients").GetChildren())
            {
                clients.Add(new Client
                {
                    ClientId = child["ClientId"],
                    ClientName = child["ClientName"],
                    AllowedGrantTypes = child.GetSection("AllowedGrantTypes").GetChildren().Select(c => c.Value).ToArray(),
                    RequireConsent = bool.Parse(child["RequireConsent"] ?? "false"),
                    RequirePkce = bool.Parse(child["RequirePkce"] ?? "false"),
                    AllowOfflineAccess = bool.Parse(child["AllowOfflineAccess"] ?? "false"),
                    ClientSecrets = child.GetSection("ClientSecrets").GetChildren().Select(secret => new Secret(secret["Value"].Sha256())).ToArray(),
                    RequireClientSecret = bool.Parse(child["RequireClientSecret"] ?? "true"),
                    AllowedScopes = child.GetSection("AllowedScopes").GetChildren().Select(c => c.Value).ToArray(),
                    RedirectUris = child.GetSection("RedirectUris").GetChildren().Select(c => c.Value).ToArray(),
                    PostLogoutRedirectUris = child.GetSection("PostLogoutRedirectUris").GetChildren().Select(c => c.Value).ToArray(),
                    AllowAccessTokensViaBrowser = bool.Parse(child["AllowOfflineAccess"] ?? "false"),
                    AccessTokenLifetime = int.Parse(child["AccessTokenLifetime"] ?? "3600"), //3600 seconds = 1 hour
                    //RefreshTokenExpiration = //default: absolute
                    AbsoluteRefreshTokenLifetime = int.Parse(child["AbsoluteRefreshTokenLifetime"] ?? "2592000"), //2592000 seconds = 30 days
                    AllowedCorsOrigins = corsOrigins,

                });
            }

            return clients;
        }
    }
}
