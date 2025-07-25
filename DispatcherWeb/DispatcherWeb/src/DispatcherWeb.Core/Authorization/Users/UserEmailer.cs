using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Extensions;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using Abp.UI;
using DispatcherWeb.Chat;
using DispatcherWeb.Configuration;
using DispatcherWeb.Editions;
using DispatcherWeb.Emailing;
using DispatcherWeb.Localization;
using DispatcherWeb.Url;
using DispatcherWeb.Utils;

namespace DispatcherWeb.Authorization.Users
{
    /// <summary>
    /// Used to send email to users.
    /// </summary>
    public class UserEmailer : DispatcherWebServiceBase, IUserEmailer, ITransientDependency
    {
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly IEmailSender _emailSender;
        private readonly ITenantCache _tenantCache;
        private readonly ICurrentUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ISettingManager _settingManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly IEncryptionService _encryptionService;
        private readonly IWebUrlService _webUrlService;

        // used for styling action links on email messages.
        private const string EmailButtonStyle = "padding-left: 48px; padding-right: 48px; padding-top: 12px; padding-bottom: 12px; color: #ffffff; background-color: #67A2D7; font-size: 11pt; text-decoration: none; display: inline-block;";
        private const string EmailButtonColor = "#67A2D7";

        public UserEmailer(
            IEmailTemplateProvider emailTemplateProvider,
            IEmailSender emailSender,
            ITenantCache tenantCache,
            ICurrentUnitOfWorkProvider unitOfWorkProvider,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            EditionManager editionManager,
            UserManager userManager,
            IEncryptionService encryptionService,
            IWebUrlService webUrlService)
        {
            _emailTemplateProvider = emailTemplateProvider;
            _emailSender = emailSender;
            _tenantCache = tenantCache;
            _unitOfWorkProvider = unitOfWorkProvider;
            _unitOfWorkManager = unitOfWorkManager;
            _settingManager = settingManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _encryptionService = encryptionService;
            _webUrlService = webUrlService;
        }

        /// <summary>
        /// Send email activation link to user's email address.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="link">Email activation link</param>
        /// <param name="plainPassword">
        /// Can be set to user's plain password to include it in the email.
        /// </param>
        public virtual async Task SendEmailActivationLinkAsync(User user, string link, string plainPassword = null, string subjectTemplate = null, string bodyTemplate = null)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await CheckMailSettingsEmptyOrNull();

                var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

                if (user.EmailConfirmationCode.IsNullOrEmpty())
                {
                    throw new Exception("EmailConfirmationCode should be set in order to send email activation link.");
                }

                link = link.Replace("{userId}", user.Id.ToString());
                link = link.Replace("{confirmationCode}", Uri.EscapeDataString(user.EmailConfirmationCode));

                if (user.TenantId.HasValue)
                {
                    link = link.Replace("{tenantId}", user.TenantId.ToString());
                }

                link = EncryptQueryParameters(link);

                var tenancyName = await GetTenancyNameOrNull(user.TenantId);

                if (subjectTemplate.IsNullOrEmpty())
                {
                    subjectTemplate = L("EmailActivation_Subject");
                }

                if (bodyTemplate.IsNullOrEmpty())
                {
                    var emailTemplate = GetTitleAndSubTitle(user.TenantId, "", L("EmailActivation_SubTitle"));
                    var mailMessage = new StringBuilder();

                    var imgPath = _webUrlService.GetSiteRootAddress().EnsureEndsWith('/') + $"Common/Images/envelope.png";
                    mailMessage.AppendLine("<img src=\"" + imgPath + "\" />");

                    mailMessage.AppendLine("<div style=\"background: #F4F5F7; padding: 20px; text-align: left; width: 60%; margin-top: 16px;\">");
                    if (!tenancyName.IsNullOrEmpty())
                    {
                        mailMessage.AppendLine(L("TenancyName") + ": <b>" + tenancyName + "</b><br />");
                    }

                    mailMessage.AppendLine(L("UserName") + ": <b>" + user.UserName + "</b><br />");

                    if (!plainPassword.IsNullOrEmpty())
                    {
                        mailMessage.AppendLine(L("Password") + ": <b>" + plainPassword + "</b><br />");
                    }
                    mailMessage.AppendLine("</div>");

                    mailMessage.AppendLine("<p style=\"font-size: 9pt; margin: 16px 0;\">" + L("EmailActivation_ClickTheLinkBelowToVerifyYourEmail") + "</p>");
                    mailMessage.AppendLine("<a style=\"" + EmailButtonStyle + "\" bg-color=\"" + EmailButtonColor + "\" href=\"" + link + "\">" + L("VerifyEmail") + "</a>");
                    mailMessage.AppendLine("<br />");
                    mailMessage.AppendLine("<br />");
                    mailMessage.AppendLine("<span style=\"font-size: 9pt;\">" + L("EmailMessage_CopyTheLinkBelowToYourBrowser") + "</span><br />");
                    mailMessage.AppendLine("<p style=\"font-size: 8pt; color:#6799B2; margin: 0 20px;\">" + link + "</p>");

                    await ReplaceBodyAndSend(user.EmailAddress, subjectTemplate, emailTemplate, mailMessage);
                }
                else
                {
                    var emailTemplate = GetTitleAndSubTitle(user.TenantId, "", "");
                    var mailMessage = new StringBuilder();

                    bodyTemplate = CoreHtmlHelper.Sanitize(bodyTemplate);

                    bodyTemplate = bodyTemplate.Replace("{CompanyName}", companyName);

                    bodyTemplate = bodyTemplate.Replace("{TenancyName}", tenancyName);

                    bodyTemplate = bodyTemplate.Replace("{UserName}", user.UserName);

                    bodyTemplate = bodyTemplate.Replace("{Password}", plainPassword);

                    var verifyEmailButton = "<a style=\"" + EmailButtonStyle + "\" bg-color=\"" + EmailButtonColor + "\" href=\"" + link + "\">" + L("VerifyEmail") + "</a><br />";
                    bodyTemplate = bodyTemplate.Replace("{VerifyEmailButton}", verifyEmailButton);

                    var verifyEmailLink = "<p style=\"font-size: 8pt; color:#6799B2; margin: 0 20px;\">" + link + "</p>";
                    bodyTemplate = bodyTemplate.Replace("{VerifyEmailLink}", verifyEmailLink);

                    bodyTemplate = bodyTemplate.Replace("\r\n", "<br />");

                    mailMessage.AppendLine(bodyTemplate);

                    await ReplaceBodyAndSend(user.EmailAddress, subjectTemplate, emailTemplate, mailMessage);
                }
            });
        }

        public virtual async Task SendLeaseHaulerInviteEmail(User user, string link, string subjectTemplate = null, string bodyTemplate = null)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await CheckMailSettingsEmptyOrNull();

                var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
                subjectTemplate = string.IsNullOrEmpty(subjectTemplate)
                    ? L("LeaseHaulerInviteEmail_Subject", companyName)
                    : subjectTemplate.Replace("{CompanyName}", companyName);

                if (bodyTemplate.IsNullOrEmpty())
                {
                    var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("LeaseHaulerInviteEmail_Title", companyName), L("LeaseHaulerInviteEmail_SubTitle", companyName));
                    var mailMessage = new StringBuilder();

                    mailMessage.AppendLine("<a style=\"" + EmailButtonStyle + "\" bg-color=\"" + EmailButtonColor + "\" href=\"" + link + "\">" + L("AcceptInvitation") + "</a>");
                    mailMessage.AppendLine("<br />");
                    mailMessage.AppendLine("<br />");
                    mailMessage.AppendLine("<p style=\"font-size: 9pt; margin: 30px 0;\">"
                        + "Use "
                        + "<a style=\"color=\"" + EmailButtonColor + "\" href=\"https://dumptruckdispatcher.com\">dumptruckdispatcher.com</a>"
                        + " to easily manage your dispatching business. Automate your daily processes and make your process more efficient than ever."
                        + "</p>");
                    mailMessage.AppendLine("<br />");
                    mailMessage.AppendLine("<br />");
                    mailMessage.AppendLine("<span style=\"font-size: 9pt;\">" + L("EmailMessage_CopyTheLinkBelowToYourBrowser") + "</span><br />");
                    mailMessage.AppendLine("<p style=\"font-size: 8pt; color:#6799B2; margin: 0 20px;\">" + link + "</p>");

                    await ReplaceBodyAndSend(user.EmailAddress, subjectTemplate, emailTemplate, mailMessage);
                }
                else
                {
                    var emailTemplate = GetTitleAndSubTitle(user.TenantId, "", "");
                    var mailMessage = new StringBuilder();

                    bodyTemplate = CoreHtmlHelper.Sanitize(bodyTemplate);
                    bodyTemplate = bodyTemplate.Replace("{CompanyName}", companyName);
                    bodyTemplate = bodyTemplate.Replace("{AcceptEmailButton}", "<a style=\"" + EmailButtonStyle + "\" bg-color=\"" + EmailButtonColor + "\" href=\"" + link + "\">" + L("AcceptInvitation") + "</a><br />");
                    bodyTemplate = bodyTemplate.Replace("{dumptruckdispatcher.com}", "<a style=\"color:" + EmailButtonColor + "\" href=\"https://dumptruckdispatcher.com\">dumptruckdispatcher.com</a>");
                    bodyTemplate = bodyTemplate.Replace("{AcceptEmailLink}", "<p style=\"font-size: 8pt; color:#6799B2; margin: 0 20px;\">" + link + "</p>");
                    bodyTemplate = bodyTemplate.Replace("\r\n", "<br />");

                    mailMessage.AppendLine(bodyTemplate);

                    await ReplaceBodyAndSend(user.EmailAddress, subjectTemplate, emailTemplate, mailMessage);
                }
            });
        }

        public virtual async Task SendLeaseHaulerJobRequestEmail(User user, int numberOfTrucks, DateTime orderDate, string link, string subjectTemplate, string bodyTemplate)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await CheckMailSettingsEmptyOrNull();

                var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

                subjectTemplate = subjectTemplate.Replace("{CompanyName}", companyName);

                var emailTemplate = GetTitleAndSubTitle(user.TenantId, "", "");
                var mailMessage = new StringBuilder();

                bodyTemplate = CoreHtmlHelper.Sanitize(bodyTemplate);
                bodyTemplate = bodyTemplate.Replace("{CompanyName}", companyName);
                bodyTemplate = bodyTemplate.Replace("{NumberOfTrucks}", numberOfTrucks.ToString());
                bodyTemplate = bodyTemplate.Replace("{TruckPluralization}", numberOfTrucks == 1 ? "truck" : "trucks");
                bodyTemplate = bodyTemplate.Replace("{OrderDate}", orderDate.ToString("d"));
                bodyTemplate = bodyTemplate.Replace("{LinkToSchedulePage}", link);

                mailMessage.AppendLine(bodyTemplate);

                await ReplaceBodyAndSend(user.EmailAddress, subjectTemplate, emailTemplate, mailMessage);
            });
        }

        /// <summary>
        /// Sends a password reset link to user's email.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="link">Reset link</param>
        public async Task SendPasswordResetLinkAsync(User user, string link = null)
        {
            await CheckMailSettingsEmptyOrNull();

            if (user.PasswordResetCode.IsNullOrEmpty())
            {
                throw new Exception("PasswordResetCode should be set in order to send password reset link.");
            }

            var tenancyName = await GetTenancyNameOrNull(user.TenantId);
            var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("PasswordResetEmail_Title"), L("PasswordResetEmail_SubTitle"));
            var mailMessage = new StringBuilder();

            mailMessage.AppendLine("<b>" + L("NameSurname") + "</b>: " + user.Name + " " + user.Surname + "<br />");

            if (!tenancyName.IsNullOrEmpty())
            {
                mailMessage.AppendLine("<b>" + L("TenancyName") + "</b>: " + tenancyName + "<br />");
            }

            mailMessage.AppendLine("<b>" + L("UserName") + "</b>: " + user.UserName + "<br />");
            mailMessage.AppendLine("<b>" + L("ResetCode") + "</b>: " + user.PasswordResetCode + "<br />");

            if (!link.IsNullOrEmpty())
            {
                link = link.Replace("{userId}", user.Id.ToString());
                link = link.Replace("{resetCode}", Uri.EscapeDataString(user.PasswordResetCode));

                if (user.TenantId.HasValue)
                {
                    link = link.Replace("{tenantId}", user.TenantId.ToString());
                }

                link = EncryptQueryParameters(link);

                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine(L("PasswordResetEmail_ClickTheLinkBelowToResetYourPassword") + "<br /><br />");
                mailMessage.AppendLine("<a style=\"" + EmailButtonStyle + "\" bg-color=\"" + EmailButtonColor + "\" href=\"" + link + "\">" + L("Reset") + "</a>");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<br />");
                mailMessage.AppendLine("<span style=\"font-size: 9pt;\">" + L("EmailMessage_CopyTheLinkBelowToYourBrowser") + "</span><br />");
                mailMessage.AppendLine("<span style=\"font-size: 8pt;\">" + link + "</span>");
            }

            await ReplaceBodyAndSend(user.EmailAddress, L("PasswordResetEmail_Subject"), emailTemplate, mailMessage);
        }

        public async Task TryToSendChatMessageMail(User user, string senderUsername, string senderTenancyName, ChatMessage chatMessage)
        {
            try
            {
                await CheckMailSettingsEmptyOrNull();

                var emailTemplate = GetTitleAndSubTitle(user.TenantId, L("NewChatMessageEmail_Title"), L("NewChatMessageEmail_SubTitle"));
                var mailMessage = new StringBuilder();

                mailMessage.AppendLine("<b>" + L("Sender") + "</b>: " + senderTenancyName + "/" + senderUsername + "<br />");
                mailMessage.AppendLine("<b>" + L("Time") + "</b>: " + chatMessage.CreationTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + " UTC<br />");
                mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + chatMessage.Message + "<br />");
                mailMessage.AppendLine("<br />");

                await ReplaceBodyAndSend(user.EmailAddress, L("NewChatMessageEmail_Subject"), emailTemplate, mailMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendSubscriptionExpireEmail(int tenantId, DateTime utcNow)
        {
            try
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        await CheckMailSettingsEmptyOrNull();

                        var tenantAdmin = await _userManager.GetAdminAsync();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var hostAdminLanguage = await _settingManager.GetSettingValueForUserAsync(LocalizationSettingNames.DefaultLanguage, tenantAdmin.TenantId, tenantAdmin.Id);
                        var culture = CultureHelper.GetCultureInfoByChecking(hostAdminLanguage);
                        var emailTemplate = GetTitleAndSubTitle(tenantId, L("SubscriptionExpire_Title"), L("SubscriptionExpire_SubTitle"));
                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + L("SubscriptionExpire_Email_Body", culture, utcNow.ToString("yyyy-MM-dd") + " UTC") + "<br />");
                        mailMessage.AppendLine("<br />");

                        await ReplaceBodyAndSend(tenantAdmin.EmailAddress, L("SubscriptionExpire_Email_Subject"), emailTemplate, mailMessage);
                    }
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendSubscriptionAssignedToAnotherEmail(int tenantId, DateTime utcNow, int expiringEditionId)
        {
            try
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        await CheckMailSettingsEmptyOrNull();

                        var tenantAdmin = await _userManager.GetAdminAsync();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var hostAdminLanguage = await _settingManager.GetSettingValueForUserAsync(LocalizationSettingNames.DefaultLanguage, tenantAdmin.TenantId, tenantAdmin.Id);
                        var culture = CultureHelper.GetCultureInfoByChecking(hostAdminLanguage);
                        var expringEdition = await _editionManager.GetByIdAsync(expiringEditionId);
                        var emailTemplate = GetTitleAndSubTitle(tenantId, L("SubscriptionExpire_Title"), L("SubscriptionExpire_SubTitle"));
                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + L("SubscriptionAssignedToAnother_Email_Body", culture, expringEdition.DisplayName, utcNow.ToString("yyyy-MM-dd") + " UTC") + "<br />");
                        mailMessage.AppendLine("<br />");

                        await ReplaceBodyAndSend(tenantAdmin.EmailAddress, L("SubscriptionExpire_Email_Subject"), emailTemplate, mailMessage);
                    }
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendFailedSubscriptionTerminationsEmail(List<string> failedTenancyNames, DateTime utcNow)
        {
            try
            {
                await CheckMailSettingsEmptyOrNull();

                var hostAdmin = await _userManager.GetAdminAsync();
                if (hostAdmin == null || string.IsNullOrEmpty(hostAdmin.EmailAddress))
                {
                    return;
                }

                var hostAdminLanguage = await _settingManager.GetSettingValueForUserAsync(LocalizationSettingNames.DefaultLanguage, hostAdmin.TenantId, hostAdmin.Id);
                var culture = CultureHelper.GetCultureInfoByChecking(hostAdminLanguage);
                var emailTemplate = GetTitleAndSubTitle(null, L("FailedSubscriptionTerminations_Title"), L("FailedSubscriptionTerminations_SubTitle"));
                var mailMessage = new StringBuilder();

                mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + L("FailedSubscriptionTerminations_Email_Body", culture, string.Join(",", failedTenancyNames), utcNow.ToString("yyyy-MM-dd") + " UTC") + "<br />");
                mailMessage.AppendLine("<br />");

                await ReplaceBodyAndSend(hostAdmin.EmailAddress, L("FailedSubscriptionTerminations_Email_Subject"), emailTemplate, mailMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        public async Task TryToSendSubscriptionExpiringSoonEmail(int tenantId, DateTime dateToCheckRemainingDayCount)
        {
            try
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        await CheckMailSettingsEmptyOrNull();

                        var tenantAdmin = await _userManager.GetAdminAsync();
                        if (tenantAdmin == null || string.IsNullOrEmpty(tenantAdmin.EmailAddress))
                        {
                            return;
                        }

                        var tenantAdminLanguage = await _settingManager.GetSettingValueForUserAsync(LocalizationSettingNames.DefaultLanguage, tenantAdmin.TenantId, tenantAdmin.Id);
                        var culture = CultureHelper.GetCultureInfoByChecking(tenantAdminLanguage);

                        var emailTemplate = GetTitleAndSubTitle(null, L("SubscriptionExpiringSoon_Title"), L("SubscriptionExpiringSoon_SubTitle"));
                        var mailMessage = new StringBuilder();

                        mailMessage.AppendLine("<b>" + L("Message") + "</b>: " + L("SubscriptionExpiringSoon_Email_Body", culture, dateToCheckRemainingDayCount.ToString("yyyy-MM-dd") + " UTC") + "<br />");
                        mailMessage.AppendLine("<br />");

                        await ReplaceBodyAndSend(tenantAdmin.EmailAddress, L("SubscriptionExpiringSoon_Email_Subject"), emailTemplate, mailMessage);
                    }
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
            }
        }

        private async Task<string> GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId == null)
            {
                return null;
            }

            using (_unitOfWorkProvider.Current.SetTenantId(null))
            {
                return (await _tenantCache.GetAsync(tenantId.Value)).TenancyName;
            }
        }

        private StringBuilder GetTitleAndSubTitle(int? tenantId, string title, string subTitle)
        {
            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetDefaultTemplate(tenantId));
            emailTemplate.Replace("{EMAIL_TITLE}", title);
            emailTemplate.Replace("{EMAIL_SUB_TITLE}", subTitle);

            return emailTemplate;
        }

        private async Task ReplaceBodyAndSend(string emailAddress, string subject, StringBuilder emailTemplate, StringBuilder mailMessage)
        {
            emailTemplate.Replace("{EMAIL_BODY}", mailMessage.ToString());
            await _emailSender.SendAsync(new MailMessage
            {
                To = { emailAddress },
                Subject = subject,
                Body = emailTemplate.ToString(),
                IsBodyHtml = true,
            });
        }

        private string EncryptQueryParameters(string link, string encryptedParameterName = "c")
        {
            return EncryptQueryParameters(_encryptionService, link, encryptedParameterName);
        }

        /// <summary>
        /// Returns link with encrypted parameters
        /// </summary>
        /// <param name="link"></param>
        /// <param name="encryptedParameterName"></param>
        /// <returns></returns>
        public static string EncryptQueryParameters(IEncryptionService encryptionService, string link, string encryptedParameterName = "c")
        {
            if (!link.Contains('?'))
            {
                return link;
            }

            var basePath = link[..link.IndexOf('?')];
            var query = link[link.IndexOf('?')..].TrimStart('?');

            return basePath + "?" + encryptedParameterName + "=" + HttpUtility.UrlEncode(encryptionService.EncryptIfNotEmpty(query));
        }

        private async Task CheckMailSettingsEmptyOrNull()
        {
#if DEBUG
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            if (
                (await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress)).IsNullOrEmpty()
                || (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host)).IsNullOrEmpty()
            )
            {
                throw new UserFriendlyException(L("SMTPSettingsNotProvidedWarningText"));
            }
#pragma warning restore CS0162 // Unreachable code detected

            if (await _settingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials))
            {
                return;
            }

            if (
                (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName)).IsNullOrEmpty()
                || (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password)).IsNullOrEmpty()
            )
            {
                throw new UserFriendlyException(L("SMTPSettingsNotProvidedWarningText"));
            }
        }
    }
}
