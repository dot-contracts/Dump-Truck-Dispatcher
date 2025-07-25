using System;
using System.Threading.Tasks;

namespace DispatcherWeb.Url
{
    public class NullAppUrlService : IAppUrlService
    {
        public static IAppUrlService Instance { get; } = new NullAppUrlService();

        private NullAppUrlService()
        {

        }

        public string CreateEmailActivationUrlFormat(int? tenantId)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateEmailActivationUrlFormatAsync(int? tenantId)
        {
            throw new NotImplementedException();
        }

        public string CreatePasswordResetUrlFormat(int? tenantId)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreatePasswordResetUrlFormatAsync(int? tenantId)
        {
            throw new NotImplementedException();
        }

        public string CreateEmailActivationUrlFormat(string tenancyName)
        {
            throw new NotImplementedException();
        }

        public string CreatePasswordResetUrlFormat(string tenancyName)
        {
            throw new NotImplementedException();
        }

        public string CreateLeaseHaulerInvitationUrlFormat(Guid oneTimeLoginId)
        {
            throw new NotImplementedException();
        }

        public string CreateLinkToSchedule(int? tenantId)
        {
            throw new NotImplementedException();
        }
    }
}
