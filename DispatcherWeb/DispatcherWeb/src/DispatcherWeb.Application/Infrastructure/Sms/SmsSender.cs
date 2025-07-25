using System;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Localization;
using DispatcherWeb.Sms;
using DispatcherWeb.Url;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace DispatcherWeb.Infrastructure.Sms
{
    public class SmsSender : ISmsSender, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IWebUrlService _webUrlService;
        private readonly IRepository<SentSms> _sentSmsRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly LocalizationHelper _localizationHelper;
        private string _accountSid;
        private string _authToken;
        private string _fromPhoneNumber;

        public ILogger Logger { get; set; }
        public IAbpSession AbpSession { get; set; }

        public SmsSender(
            ISettingManager settingManager,
            IWebUrlService webUrlService,
            IRepository<SentSms> sentSmsRepository,
            IUnitOfWorkManager unitOfWorkManager,
            LocalizationHelper localizationHelper
        )
        {
            _settingManager = settingManager;
            _webUrlService = webUrlService;
            _sentSmsRepository = sentSmsRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _localizationHelper = localizationHelper;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        private async Task ReadSettingsAsync(SmsSendInput input)
        {
            _accountSid = await _settingManager.GetSettingValueAsync(AppSettings.Sms.AccountSid);
            _authToken = await _settingManager.GetSettingValueAsync(AppSettings.Sms.AuthToken);
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            _fromPhoneNumber = tenantId.HasValue
                ? await _settingManager.GetSettingValueForTenantAsync(AppSettings.DispatchingAndMessaging.SmsPhoneNumber, tenantId.Value)
                : null;
            if (_fromPhoneNumber.IsNullOrEmpty() && !input.DisallowFallbackToHostFromPhoneNumber)
            {
                _fromPhoneNumber = await _settingManager.GetSettingValueAsync(AppSettings.Sms.PhoneNumber);
            }
            if (_accountSid.IsNullOrEmpty()
                || _authToken.IsNullOrEmpty()
                || _fromPhoneNumber.IsNullOrEmpty()
            )
            {
                throw new UserFriendlyException("There are no SMS settings. Please contact your administrator.");
            }
        }

        public async Task<SmsSendResult> SendAsync(SmsSendInput input)
        {
            if (input.Body?.Length > AppConsts.MaxSmsLength)
            {
                throw new ApiException(_localizationHelper.L("SmsLengthLimitOf{0}Exceeded", AppConsts.MaxSmsLength));
            }

            await ReadSettingsAsync(input);

            Logger.Debug($"Sending a message with body: '{input.Body}' to number: '{input.ToPhoneNumber}' from number: '{_fromPhoneNumber}'");
            TwilioClient.Init(_accountSid, _authToken);

            Uri callbackUri = null;
            if (input.TrackStatus)
            {
                string siteUrl = _webUrlService.GetSiteRootAddress();
                callbackUri = siteUrl.Contains("://localhost")
                    ? new Uri("https://postb.in/hB8zxLkm") // Localhost. Use https://postb.in/b/hB8zxLkm to check
                    : new Uri($"{siteUrl}app/SmsCallback");
            }

            MessageResource message;
            SentSms sentSms;
            bool sentSmsEntityIsInserted = false;
            try
            {
                message = await MessageResource.CreateAsync(
                    body: input.Body,
                    from: new PhoneNumber(_fromPhoneNumber),
                    to: new PhoneNumber(input.ToPhoneNumber),
                    statusCallback: callbackUri
                );
#if DEBUG
                var smsLogger = Logger.CreateChildLogger("SmsLogger");
                smsLogger.Info($"SMS message:\n Body: {input.Body}\n From: {_fromPhoneNumber}\n To: {input.ContactName}({input.ToPhoneNumber})\n");
#endif
                sentSms = new SentSms
                {
                    FromSmsNumber = _fromPhoneNumber,
                    ToSmsNumber = input.ToPhoneNumber,
                    TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                    Sid = message.Sid,
                    Status = message.Status.ToSmsStatus(),
                };

                if (input.TrackStatus || input.InsertEntity)
                {
                    await _unitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
                    {
                        sentSmsEntityIsInserted = true;
                        await _sentSmsRepository.InsertAndGetIdAsync(sentSms);
                    });
                }
            }
            catch (ApiException e)
            {
                Logger.Error($"Exception when sending the sms: {e.Message}");
                throw;
            }

            if (message.ErrorCode.HasValue)
            {
                sentSms.Status = SmsStatus.Failed;
                Logger.Error($"There was an error: '{message.ErrorMessage}' while sending the sms to {input.ContactName} ({input.ToPhoneNumber}) with the text: '{input.Body}'");
            }
            else
            {
                Logger.Debug($"The sms to the number: '{input.ToPhoneNumber}' with the text: '{input.Body}' was sent");
            }

            return new SmsSendResult
            {
                Sid = message.Sid,
                Status = message.Status.ToSmsStatus(),
                ErrorCode = message.ErrorCode,
                ErrorMessage = message.ErrorMessage,
                SentSmsEntity = sentSms,
                SentSmsEntityIsInserted = sentSmsEntityIsInserted,
            };
        }
    }
}
