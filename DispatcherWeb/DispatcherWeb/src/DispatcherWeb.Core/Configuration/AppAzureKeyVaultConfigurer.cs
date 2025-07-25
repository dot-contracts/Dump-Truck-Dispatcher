using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Configuration
{
    public class AppAzureKeyVaultConfigurer
    {
        public void Configure(IConfigurationBuilder builder, IConfigurationRoot config)
        {
            var azureKeyVaultConfiguration = config.GetSection("Configuration:AzureKeyVault").Get<AzureKeyVaultConfiguration>();

            if (azureKeyVaultConfiguration == null || !azureKeyVaultConfiguration.IsEnabled)
            {
                return;
            }

            var azureKeyVaultUrl = $"https://{azureKeyVaultConfiguration.KeyVaultName}.vault.azure.net/";
            TokenCredential credential;

            if (azureKeyVaultConfiguration.UsesCertificate())
            {
                using (var store = new X509Store(StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        azureKeyVaultConfiguration.AzureADCertThumbprint,
                        false
                    );

                    var cert = certs.OfType<X509Certificate2>().Single();
                    credential = new ClientCertificateCredential(
                        tenantId: null, // Will be taken from environment or can be explicitly specified
                        clientId: azureKeyVaultConfiguration.AzureADApplicationId,
                        clientCertificate: cert);

                    builder.AddAzureKeyVault(
                        new Uri(azureKeyVaultUrl),
                        credential);

                    store.Close();
                }
            }
            else if (azureKeyVaultConfiguration.UsesManagedIdentity())
            {
                credential = new ClientSecretCredential(
                    tenantId: null, // Will be taken from environment or can be explicitly specified
                    clientId: azureKeyVaultConfiguration.ClientId,
                    clientSecret: azureKeyVaultConfiguration.ClientSecret);

                builder.AddAzureKeyVault(
                    new Uri(azureKeyVaultUrl),
                    credential);
            }
        }
    }
}
