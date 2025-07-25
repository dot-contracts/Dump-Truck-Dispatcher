using System.Threading.Tasks;
using Abp.MailKit;
using Abp.Net.Mail.Smtp;
using MailKit.Net.Smtp;

namespace DispatcherWeb.Emailing
{
    public class DispatcherWebMailKitSmtpBuilder : DefaultMailKitSmtpBuilder
    {
        public DispatcherWebMailKitSmtpBuilder(
            ISmtpEmailSenderConfiguration smtpEmailSenderConfiguration,
            IAbpMailKitConfiguration abpMailKitConfiguration) : base(smtpEmailSenderConfiguration, abpMailKitConfiguration)
        {

        }

        protected override async Task ConfigureClientAsync(SmtpClient client)
        {
            client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            await base.ConfigureClientAsync(client);
        }
    }
}
