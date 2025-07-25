using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Net.Mail.Smtp;
using DispatcherWeb.Debugging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Emailing
{
    public class DispatcherWebEmailSender : SmtpEmailSender
    {
        private readonly IWebHostEnvironment _env;

        public DispatcherWebEmailSender(ISmtpEmailSenderConfiguration configuration, IWebHostEnvironment env)
            : base(configuration)
        {
            _env = env;
        }

        protected override async Task NormalizeMailAsync(MailMessage mail)
        {
            await base.NormalizeMailAsync(mail);

            if (!_env.IsProduction())
            {
                mail.Subject = $"{_env.EnvironmentName}{(DebugHelper.IsDebug ? " Debug" : "")}: {mail.Subject}";
            }
        }
    }
}
