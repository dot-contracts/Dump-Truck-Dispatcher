using System;
using System.Web;
using Abp.Encryption;
using DispatcherWeb.Authorization.Accounts.Dto;

namespace DispatcherWeb.Web.Models.Account
{
    public class EmailConfirmationViewModel : ActivateEmailInput
    {
        public int? TenantId { get; set; }

        /// <summary>
        /// Encrypted values for {TenantId}, {UserId} and {ConfirmationCode}
        /// </summary>
        public string C { get; set; }

        public void ResolveParameters(IEncryptionService encryptionService)
        {
            if (!string.IsNullOrEmpty(C))
            {
                var parameters = encryptionService.DecryptIfNotEmpty(C);
                var query = HttpUtility.ParseQueryString(parameters);

                if (query["userId"] != null)
                {
                    UserId = Convert.ToInt32(query["userId"]);
                }

                if (query["confirmationCode"] != null)
                {
                    ConfirmationCode = query["confirmationCode"];
                }

                if (query["tenantId"] != null)
                {
                    TenantId = Convert.ToInt32(query["tenantId"]);
                }
            }
        }
    }
}
