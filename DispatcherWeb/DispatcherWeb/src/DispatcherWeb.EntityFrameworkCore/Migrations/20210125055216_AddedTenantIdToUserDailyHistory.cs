using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedTenantIdToUserDailyHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "UserDailyHistory",
                nullable: true);

            migrationBuilder.Sql(@"
update h set h.TenantId = u.TenantId
from UserDailyHistory h
left join AbpUsers u on u.Id = h.UserId
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UserDailyHistory");
        }
    }
}
