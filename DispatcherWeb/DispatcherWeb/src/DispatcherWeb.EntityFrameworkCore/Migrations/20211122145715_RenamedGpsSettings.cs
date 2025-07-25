using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RenamedGpsSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update AbpSettings set [Name] = 'App.GpsIntegration.GpsPlatform' where [Name] = 'App.GPSIntegration.GPSPlatform'
update AbpSettings set [Name] = 'App.GpsIntegration.DtdTracker.AccountName' where [Name] = 'App.GPSIntegration.DTDTracker.AccountName'
update AbpSettings set [Name] = 'App.GpsIntegration.Geotab.Server' where [Name] = 'App.GPSIntegration.Geotab.Server'
update AbpSettings set [Name] = 'App.GpsIntegration.Geotab.Database' where [Name] = 'App.GPSIntegration.Geotab.Database'
update AbpSettings set [Name] = 'App.GpsIntegration.Geotab.MapBaseUrl' where [Name] = 'App.GPSIntegration.Geotab.MapBaseUrl'
update AbpSettings set [Name] = 'App.GpsIntegration.Geotab.User' where [Name] = 'App.GPSIntegration.Geotab.User'
update AbpSettings set [Name] = 'App.GpsIntegration.Geotab.Password' where [Name] = 'App.GPSIntegration.Geotab.Password'
update AbpSettings set [Name] = 'App.GpsIntegration.Samsara.ApiToken' where [Name] = 'App.GPSIntegration.Samsara.ApiToken'
update AbpSettings set [Name] = 'App.GpsIntegration.Samsara.BaseUrl' where [Name] = 'App.GPSIntegration.Samsara.BaseUrl'
update AbpFeatures set [Name] = 'App.GpsIntegrationFeature' where [Name] = 'App.GPSIntegrationFeature'

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
