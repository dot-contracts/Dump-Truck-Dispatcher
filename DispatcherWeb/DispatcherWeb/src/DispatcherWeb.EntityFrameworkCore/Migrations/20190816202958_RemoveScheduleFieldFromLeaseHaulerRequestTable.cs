using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemoveScheduleFieldFromLeaseHaulerRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update [LeaseHaulerRequest] set [Scheduled] = null");
            migrationBuilder.DropColumn(
                name: "Scheduled",
                table: "LeaseHaulerRequest");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Scheduled",
                table: "LeaseHaulerRequest",
                nullable: true);
        }
    }
}
