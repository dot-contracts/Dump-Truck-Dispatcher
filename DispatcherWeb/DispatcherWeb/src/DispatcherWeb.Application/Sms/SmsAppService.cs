using System.Threading;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Twilio.Rest.Api.V2010.Account;

namespace DispatcherWeb.Sms
{
    [AbpAuthorize]
    public class SmsAppService : DispatcherWebAppServiceBase, ISmsAppService
    {
        private readonly IRepository<SentSms> _sentSmsRepository;
        private const int DefaultSmsCallbackMaxThreadCount = 4;
        private static readonly SemaphoreSlim _smsCallbackSemaphore = new SemaphoreSlim(0, 10000);

        public SmsAppService(
            IRepository<SentSms> sentSmsRepository
        )
        {
            _sentSmsRepository = sentSmsRepository;
        }

        public static void ConfigureSmsCallbackSemaphore(IConfigurationRoot appConfiguration)
        {
            var newValue = DefaultSmsCallbackMaxThreadCount;
            var smsCallbackMaxThreadCountString = appConfiguration["App:SmsCallbackMaxThreadCount"];
            if (!string.IsNullOrEmpty(smsCallbackMaxThreadCountString))
            {
                int.TryParse(smsCallbackMaxThreadCountString, out newValue);
                if (newValue > 10000 || newValue < 1)
                {
                    newValue = DefaultSmsCallbackMaxThreadCount;
                }
            }
            if (_smsCallbackSemaphore.CurrentCount == 0)
            {
                _smsCallbackSemaphore.Release(newValue);
            }
        }

        [AbpAllowAnonymous]
        [UnitOfWork(IsDisabled = true)]
        public async Task SetSmsStatus(string sid, string smsStatus)
        {
            await _smsCallbackSemaphore.WaitAsync();
            try
            {
                await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
                {
                    var entity = await _sentSmsRepository.FirstOrDefaultAsync(ts => ts.Sid == sid);
                    if (entity == null)
                    {
                        return;
                    }
                    var messageResource = new MessageResource.StatusEnum();
                    messageResource.FromString(smsStatus.ToLowerInvariant());
                    entity.Status = messageResource.ToSmsStatus();
                });
            }
            finally
            {
                _smsCallbackSemaphore.Release();
            }
        }
    }
}
