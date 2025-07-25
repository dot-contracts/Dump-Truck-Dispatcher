using System;
using System.Threading.Tasks;
using DispatcherWeb.Url;

namespace DispatcherWeb.Test.Base.Url
{
    public class FakeAppUrlService : IAppUrlService
    {
        public string CreateEmailActivationUrlFormat(int? tenantId)
        {
            return "http://test.com/";
        }

        public Task<string> CreateEmailActivationUrlFormatAsync(int? tenantId)
        {
            return Task.FromResult("http://test.com/");
        }

        public string CreatePasswordResetUrlFormat(int? tenantId)
        {
            return "http://test.com/";
        }

        public Task<string> CreatePasswordResetUrlFormatAsync(int? tenantId)
        {
            return Task.FromResult("http://test.com/");
        }

        public string CreateEmailActivationUrlFormat(string tenancyName)
        {
            return "http://test.com/";
        }

        public string CreatePasswordResetUrlFormat(string tenancyName)
        {
            return "http://test.com/";
        }

        public string CreateLeaseHaulerInvitationUrlFormat(Guid oneTimeLoginId)
        {
            return "http://test.com/";
        }

        public string CreateLinkToSchedule(int? tenantId)
        {
            return "http://test.com/";
        }
    }
}
