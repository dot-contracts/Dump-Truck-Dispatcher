using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddLatitudeLongitudeToEmployeeTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "EmployeeTime",
                type: "decimal(10, 6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "EmployeeTime",
                type: "decimal(10, 6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "EmployeeTime");
        }
    }
}
