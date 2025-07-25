using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddHoursFieldsToPreventiveMaintenanceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CompletedHour",
                table: "PreventiveMaintenance",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DueHour",
                table: "PreventiveMaintenance",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastHour",
                table: "PreventiveMaintenance",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WarningHour",
                table: "PreventiveMaintenance",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedHour",
                table: "PreventiveMaintenance");

            migrationBuilder.DropColumn(
                name: "DueHour",
                table: "PreventiveMaintenance");

            migrationBuilder.DropColumn(
                name: "LastHour",
                table: "PreventiveMaintenance");

            migrationBuilder.DropColumn(
                name: "WarningHour",
                table: "PreventiveMaintenance");
        }
    }
}
