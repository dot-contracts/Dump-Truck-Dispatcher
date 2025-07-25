using System;
using System.Web;
using Abp.Encryption;
using DispatcherWeb.Authorization.Accounts.Dto;

namespace DispatcherWeb.Web.Models.Account
{
    public class ResetPasswordViewModel : ResetPasswordInput
    {
        public int? TenantId { get; set; }

        /// <summary>
        /// Encrypted values for {TenantId}, {UserId} and {ResetCode}
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

                if (query["resetCode"] != null)
                {
                    ResetCode = query["resetCode"];
                }

                if (query["tenantId"] != null)
                {
                    TenantId = Convert.ToInt32(query["tenantId"]);
                }
            }
        }
    }
}
