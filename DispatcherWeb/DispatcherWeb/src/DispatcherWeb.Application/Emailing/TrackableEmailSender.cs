using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Encryption;
using Abp.Extensions;
using Abp.Net.Mail.Smtp;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace DispatcherWeb.Emailing
{
    public class TrackableEmailSender : DispatcherWebEmailSender, ITrackableEmailSender, ITransientDependency
    {
        private readonly IEmailAppService _emailAppService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWebUrlService _webUrlService;
        private readonly IExtendedAbpSession _session;
        private readonly bool _trackEmailOpen;

        public TrackableEmailSender(
            IAppConfigurationAccessor configurationAccessor,
            ISmtpEmailSenderConfiguration configuration,
            IWebHostEnvironment env,
            IEmailAppService emailAppService,
            IEncryptionService encryptionService,
            IWebUrlService webUrlService,
            IExtendedAbpSession session
            )
            : base(configuration, env)
        {
            _emailAppService = emailAppService;
            _encryptionService = encryptionService;
            _webUrlService = webUrlService;
            _session = session;
            _trackEmailOpen = configurationAccessor.Configuration["App:TrackEmailOpen"] == "true";
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
            await AppendSendGridApiHeaderAsync(mail, trackableEmailId, _encryptionService, _webUrlService, _session, _trackEmailOpen);
        }

        public static async Task AppendSendGridApiHeaderAsync(MailMessage mail, Guid trackableEmailId, IEncryptionService encryptionService, IWebUrlService webUrlService, IExtendedAbpSession session, bool trackEmailOpen)
        {
            var receivers = mail.GetTrackableEmailReceivers(trackableEmailId);
            var receiverEmails = receivers.Select(x => x.Email).ToList();

            var callbackUrl = webUrlService.GetSiteRootAddress().EnsureEndsWith('/')
                              + "app/Emails/TrackEvents";

            var apiMessage = new
            {
                to = receiverEmails,
                sub = new
                {
                    __trackOpenQuery__ = receiverEmails
                        .Select(r => $"?email={Uri.EscapeDataString(r)}")
                        .Select(q => UserEmailer.EncryptQueryParameters(encryptionService, q))
                        .ToList(),
                },
                unique_args = new
                {
                    trackableEmailId = trackableEmailId,
                    trackableEmailCallbackUrl = callbackUrl,
                    trackableEmailTenantId = await session.GetTenantIdOrNullAsync(),
                },
            };

            mail.Headers.Add("X-SMTPAPI", JsonConvert.SerializeObject(apiMessage));

            if (!mail.IsBodyHtml)
            {
                mail.Body = WebUtility.HtmlDecode(mail.Body);
                mail.Body = mail.Body
                    .Replace("\n", "\n<br>");
                mail.IsBodyHtml = true;
            }

            if (trackEmailOpen)
            {
                var trackOpenUrl = webUrlService.GetSiteRootAddress().EnsureEndsWith('/')
                                   + $"app/Emails/TrackEmailOpen/{trackableEmailId}__trackOpenQuery__";
                mail.Body += "<img src=\"" + trackOpenUrl + "\" height=\"1\" width=\"1\" />";
            }
        }
    }
}
