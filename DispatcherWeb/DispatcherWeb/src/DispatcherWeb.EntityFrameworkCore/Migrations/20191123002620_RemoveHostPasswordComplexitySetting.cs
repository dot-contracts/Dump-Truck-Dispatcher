using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemoveHostPasswordComplexitySetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AbpSettings where Name = 'Abp.Zero.UserManagement.PasswordComplexity.RequiredLength' and TenantId is null and Value = '5'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
