using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class EnablePWAFeatureForAllTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                delete from AbpFeatures where Name = 'DriverApp.WebBasedDriverApp';
                insert into AbpFeatures
                (Name, Value, CreationTime, EditionId, TenantId, Discriminator)
                select
                'DriverApp.WebBasedDriverApp', 'true', getdate(), null, Id, 'TenantFeatureSetting'
                from AbpTenants
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
