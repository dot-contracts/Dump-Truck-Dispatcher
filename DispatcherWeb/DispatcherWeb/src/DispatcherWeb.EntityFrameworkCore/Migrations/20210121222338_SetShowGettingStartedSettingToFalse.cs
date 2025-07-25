using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class SetShowGettingStartedSettingToFalse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
insert into AbpSettings (TenantId, Name, [Value], CreationTime)
select Id, 'App.GettingStarted.ShowGettingStarted', 'false', getDate()
from AbpTenants
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
delete from AbpSettings where Name = 'App.GettingStarted.ShowGettingStarted'
");
        }
    }
}
