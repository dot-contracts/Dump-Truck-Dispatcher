using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Sms;

namespace DispatcherWeb.Infrastructure.Sms
{
    public class NullSmsSender : ISmsSender
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<SentSms> _sentSmsRepository;

        public ILogger Logger { get; set; }
        public IAbpSession AbpSession { get; set; }

        public NullSmsSender(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<SentSms> sentSmsRepository
        )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _sentSmsRepository = sentSmsRepository;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        public async Task<SmsSendResult> SendAsync(SmsSendInput input)
        {
            Logger.Info($"NullSmsSender: Sending a message with text: '{input.Body}' to number: '{input.ToPhoneNumber}'");
#if DEBUG
            var smsLogger = Logger.CreateChildLogger("SmsLogger");
            smsLogger.Info($"SMS message:\n Body: {input.Body}\n To: {input.ToPhoneNumber}\n");
#endif

            var sid = Guid.NewGuid().ToString();
            var sentSms = new SentSms
            {
                FromSmsNumber = "",
                ToSmsNumber = input.ToPhoneNumber,
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                Sid = sid,
                Status = SmsStatus.Unknown,
            };
            bool sentSmsEntityIsInserted = false;
            if (input.TrackStatus || input.InsertEntity)
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
                {
                    sentSmsEntityIsInserted = true;
                    await _sentSmsRepository.InsertAndGetIdAsync(sentSms);
                });
            }

            return new SmsSendResult
            {
                Sid = sid,
                Status = SmsStatus.Unknown,
                ErrorCode = null,
                ErrorMessage = null,
                SentSmsEntity = sentSms,
                SentSmsEntityIsInserted = sentSmsEntityIsInserted,
            };
        }
    }
}
