using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Net.Mail;
using DispatcherWeb.Configuration.Host;
using Shouldly;

namespace DispatcherWeb.Tests.Configuration.Host
{
    public class HostSettingsAppService_EmailSettings_Test : AppTestBase
    {
        private readonly IHostSettingsAppService _hostSettingsAppService;
        private readonly ISettingManager _settingManager;

        public HostSettingsAppService_EmailSettings_Test()
        {
            _hostSettingsAppService = Resolve<IHostSettingsAppService>();
            _settingManager = Resolve<ISettingManager>();

            LoginAsHostAdmin();
            _ = InitializeTestSettings();
        }

        private async Task InitializeTestSettings()
        {
            await _settingManager.ChangeSettingsForApplicationAsync(new List<SettingInfo>
            {
                new SettingInfo(EmailSettingNames.DefaultFromAddress, "test@mydomain.com"),
                new SettingInfo(EmailSettingNames.DefaultFromDisplayName, ""),
                new SettingInfo(EmailSettingNames.Smtp.Host, "100.101.102.103"),
                new SettingInfo(EmailSettingNames.Smtp.UserName, "myuser"),
                new SettingInfo(EmailSettingNames.Smtp.Password, "123456"),
                new SettingInfo(EmailSettingNames.Smtp.Domain, "mydomain"),
                new SettingInfo(EmailSettingNames.Smtp.EnableSsl, "true"),
                new SettingInfo(EmailSettingNames.Smtp.UseDefaultCredentials, "false"),
            });
        }

        [MultiTenantFact]
        public async Task Should_Change_Email_Settings()
        {
            //Get and check current settings

            //Act
            var settings = await _hostSettingsAppService.GetAllSettings();

            //Assert
            settings.Email.DefaultFromAddress.ShouldBe("test@mydomain.com");
            settings.Email.DefaultFromDisplayName.ShouldBe("");
            settings.Email.SmtpHost.ShouldBe("100.101.102.103");
            settings.Email.SmtpPort.ShouldBe(25); //this is the default value
            settings.Email.SmtpUserName.ShouldBe("myuser");
            settings.Email.SmtpPassword.ShouldBe("123456");
            settings.Email.SmtpDomain.ShouldBe("mydomain");
            settings.Email.SmtpEnableSsl.ShouldBe(true);
            settings.Email.SmtpUseDefaultCredentials.ShouldBe(false);

            //Change and save settings

            //Arrange
            settings.Email.DefaultFromDisplayName = "My daily mailing service";
            settings.Email.SmtpHost = "100.101.102.104";
            settings.Email.SmtpPort = 42;
            settings.Email.SmtpUserName = "changeduser";
            settings.Email.SmtpPassword = "654321";
            settings.Email.SmtpDomain = "changeddomain";
            settings.Email.SmtpEnableSsl = false;

            //Act
            await _hostSettingsAppService.UpdateAllSettings(settings);

            //Assert
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress)).ShouldBe("test@mydomain.com"); //not changed
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromDisplayName)).ShouldBe("My daily mailing service");
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host)).ShouldBe("100.101.102.104");
            (await _settingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port)).ShouldBe(42);
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName)).ShouldBe("changeduser");
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password)).ShouldBe("654321");
            (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Domain)).ShouldBe("changeddomain");
            (await _settingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.EnableSsl)).ShouldBe(false);
            (await _settingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials)).ShouldBe(false); //not changed
        }
    }
}
