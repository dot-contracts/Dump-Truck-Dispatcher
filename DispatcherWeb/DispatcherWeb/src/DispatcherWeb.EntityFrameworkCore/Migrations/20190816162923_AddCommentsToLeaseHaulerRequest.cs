using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddCommentsToLeaseHaulerRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerDriver_DriverId",
                table: "LeaseHaulerDriver");

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "LeaseHaulerRequest",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerDriver_DriverId",
                table: "LeaseHaulerDriver",
                column: "DriverId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerDriver_DriverId",
                table: "LeaseHaulerDriver");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "LeaseHaulerRequest");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerDriver_DriverId",
                table: "LeaseHaulerDriver",
                column: "DriverId");
        }
    }
}
