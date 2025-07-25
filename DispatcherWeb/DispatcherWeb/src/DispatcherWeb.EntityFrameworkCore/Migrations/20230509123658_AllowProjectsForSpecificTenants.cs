using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AllowProjectsForSpecificTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                insert into AbpFeatures (Name, Value, CreationTime, TenantId, Discriminator)
                select 'App.AllowProjects', 'true', getdate(), Id, 'TenantFeatureSetting'
                from AbpTenants 
                where id = 54 and name = 'Action Materials'
                or id = 26 and name = 'JohnstonTrucking'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from AbpFeatures where Name = 'App.AllowProjects'");
        }
    }
}
