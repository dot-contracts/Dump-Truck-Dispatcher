using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddShiftFieldToDriverAssignmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Shift",
                table: "DriverAssignment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Shift",
                table: "DriverAssignment");
        }
    }
}
