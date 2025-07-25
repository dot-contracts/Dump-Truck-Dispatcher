using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RenamedGPSIntegrationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                update AbpSettings set [Name] = 'App.GPSIntegration.Geotab.Server' where [Name] = 'App.Geotab.Server';
                update AbpSettings set [Name] = 'App.GPSIntegration.Geotab.Database' where [Name] = 'App.Geotab.Database';
                update AbpSettings set [Name] = 'App.GPSIntegration.Geotab.MapBaseUrl' where [Name] = 'App.Geotab.MapBaseUrl';
                update AbpSettings set [Name] = 'App.GPSIntegration.Geotab.User' where [Name] = 'App.Geotab.User';
                update AbpSettings set [Name] = 'App.GPSIntegration.Geotab.Password' where [Name] = 'App.Geotab.Password';
                update AbpSettings set [Name] = 'App.GPSIntegration.Samsara.ApiToken' where [Name] = 'App.Samsara.ApiToken';
                update AbpSettings set [Name] = 'App.GPSIntegration.Samsara.BaseUrl' where [Name] = 'App.Samsara.BaseUrl';
                
                insert into AbpSettings (TenantId, Name, [Value], CreationTime)
                select t.Id, 'App.GPSIntegration.GPSPlatform', '3', getDate()
                from AbpTenants t 
                where exists
                (select * from AbpSettings s
                where s.TenantId = t.Id 
                and s.[Value] != '' and s.[Value] is not null
                and s.[Name] in ('App.GPSIntegration.Samsara.ApiToken', 'App.GPSIntegration.Samsara.BaseUrl')
                )
                and not exists
                (
                select * from AbpSettings s
                where s.TenantId = t.Id
                and s.[Name] = 'App.GPSIntegration.GPSPlatform'
                )
                
                insert into AbpSettings (TenantId, Name, [Value], CreationTime)
                select t.Id, 'App.GPSIntegration.GPSPlatform', '2', getDate()
                from AbpTenants t
                where exists
                (select * from AbpSettings s
                where s.TenantId = t.Id 
                and s.[Value] != '' and s.[Value] is not null
                and s.[Name] in ('App.GPSIntegration.Geotab.Server', 'App.GPSIntegration.Geotab.Database', 'App.GPSIntegration.Geotab.MapBaseUrl', 'App.GPSIntegration.Geotab.User', 'App.GPSIntegration.Geotab.Password')
                )
                and not exists
                (
                select * from AbpSettings s
                where s.TenantId = t.Id
                and s.[Name] = 'App.GPSIntegration.GPSPlatform'
                )
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
