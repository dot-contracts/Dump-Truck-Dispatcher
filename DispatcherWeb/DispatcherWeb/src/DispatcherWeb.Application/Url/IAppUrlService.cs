using System;
using System.Threading.Tasks;

namespace DispatcherWeb.Url
{
    public interface IAppUrlService
    {
        Task<string> CreateEmailActivationUrlFormatAsync(int? tenantId);

        Task<string> CreatePasswordResetUrlFormatAsync(int? tenantId);

        string CreateEmailActivationUrlFormat(string tenancyName);

        string CreateLeaseHaulerInvitationUrlFormat(Guid oneTimeLoginId);

        string CreateLinkToSchedule(int? tenantId);
    }
}
