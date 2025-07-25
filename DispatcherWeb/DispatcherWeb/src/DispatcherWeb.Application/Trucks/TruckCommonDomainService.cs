using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    internal class TruckCommonDomainService : DispatcherWebDomainServiceBase, ITruckCommonDomainService
    {
        private readonly IEmailSender _emailSender;
        private readonly IAppNotifier _appNotifier;

        public TenantManager TenantManager { get; }

        public TruckCommonDomainService(
            TenantManager tenantManager,
            IEmailSender emailSender,
            IAppNotifier appNotifier
        )
        {
            TenantManager = tenantManager;
            _emailSender = emailSender;
            _appNotifier = appNotifier;
        }

        public async Task UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput input)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var tenantId = await Session.GetTenantIdAsync();

            var originalMaxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
            await TenantManager.SetFeatureValueAsync(tenantId, AppFeatures.NumberOfTrucksFeature, input.NewValue.ToString());

            var notificationsEmail = await SettingManager.GetSettingValueAsync(AppSettings.HostManagement.NotificationsEmail);
            var tenantName = (await TenantManager.GetByIdAsync(tenantId)).TenancyName;
            var body = $"Tenant {tenantName} increased their trucks to {input.NewValue}. Their old limit was {originalMaxNumberOfTrucks}";
            Logger.Info(body);

            try
            {
                await _emailSender.SendAsync(new MailMessage(await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress), notificationsEmail)
                {
                    Subject = "Tenant increased their trucks number",
                    Body = body,
                    IsBodyHtml = false,
                });
            }
            catch (Exception e)
            {
                Logger.Error("Error during sending a email for UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins", e);
                //don't rethrow, send a notification too
            }

            await _appNotifier.SendNotificationAsync(
                        new SendNotificationInput(
                            AppNotificationNames.SimpleMessage,
                            body,
                            NotificationSeverity.Warn
                        )
                        {
                            IncludeLocalUsers = false,
                            IncludeHostUsers = true,
                            RoleFilter = new[] { StaticRoleNames.Host.Admin },
                        });
        }
    }
}
