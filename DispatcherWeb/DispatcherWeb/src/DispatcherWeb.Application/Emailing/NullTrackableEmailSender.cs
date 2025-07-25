using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Encryption;
using Abp.Net.Mail;
using Abp.Net.Mail.Smtp;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Url;

namespace DispatcherWeb.Emailing
{
    public class NullTrackableEmailSender : NullEmailSender, ITrackableEmailSender
    {
        private readonly IEmailAppService _emailAppService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWebUrlService _webUrlService;
        private readonly IExtendedAbpSession _session;

        public NullTrackableEmailSender(
            ISmtpEmailSenderConfiguration configuration,
            IEmailAppService emailAppService,
            IEncryptionService encryptionService,
            IWebUrlService webUrlService,
            IExtendedAbpSession session
            )
            : base(configuration)
        {
            _emailAppService = emailAppService;
            _encryptionService = encryptionService;
            _webUrlService = webUrlService;
            _session = session;
        }

        public async Task<Guid> SendTrackableAsync(MailMessage mail, bool normalize = true)
        {
            if (normalize)
            {
                await NormalizeMailAsync(mail);
            }

            var trackableEmailId = await _emailAppService.AddTrackableEmailAsync(mail);
            await AppendSendGridApiHeaderAsync(mail, trackableEmailId);

            await SendEmailAsync(mail);

            return trackableEmailId;
        }

        private async Task AppendSendGridApiHeaderAsync(MailMessage mail, Guid trackableEmailId)
        {
            await TrackableEmailSender.AppendSendGridApiHeaderAsync(mail, trackableEmailId, _encryptionService, _webUrlService, _session, false);
        }
    }
}
