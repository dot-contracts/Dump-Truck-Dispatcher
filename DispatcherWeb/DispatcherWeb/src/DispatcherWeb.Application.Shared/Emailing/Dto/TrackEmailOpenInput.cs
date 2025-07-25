using System;
using System.Web;
using Abp.Application.Services.Dto;
using Abp.Encryption;

namespace DispatcherWeb.Emailing.Dto
{
    public class TrackEmailOpenInput : NullableIdDto<Guid>
    {
        public TrackEmailOpenInput()
        {
        }

        public TrackEmailOpenInput(Guid? id, string email)
            : base(id)
        {
            Email = email;
        }

        public string Email { get; set; }

        /// <summary>
        /// Encrypted values for {Email}
        /// </summary>
        public string C { get; set; }

        public virtual void ResolveParameters(IEncryptionService encryptionService)
        {
            if (!string.IsNullOrEmpty(C))
            {
                var parameters = encryptionService.DecryptIfNotEmpty(C);
                var query = HttpUtility.ParseQueryString(parameters);

                if (query["email"] != null)
                {
                    Email = query["Email"];
                }
            }
        }
    }
}
