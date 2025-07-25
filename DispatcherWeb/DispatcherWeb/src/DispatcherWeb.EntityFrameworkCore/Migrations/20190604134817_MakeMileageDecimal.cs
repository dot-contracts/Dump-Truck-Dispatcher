using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class MakeMileageDecimal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Odometer",
                table: "WorkOrder",
                type: "decimal(19, 1)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<decimal>(
                name: "WarningMileage",
                table: "PreventiveMaintenance",
                type: "decimal(19, 1)",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LastMileage",
                table: "PreventiveMaintenance",
                type: "decimal(19, 1)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<decimal>(
                name: "DueMileage",
                table: "PreventiveMaintenance",
                type: "decimal(19, 1)",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletedMileage",
                table: "PreventiveMaintenance",
                type: "decimal(19, 1)",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Odometer",
                table: "WorkOrder",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)");

            migrationBuilder.AlterColumn<int>(
                name: "WarningMileage",
                table: "PreventiveMaintenance",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LastMileage",
                table: "PreventiveMaintenance",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)");

            migrationBuilder.AlterColumn<int>(
                name: "DueMileage",
                table: "PreventiveMaintenance",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CompletedMileage",
                table: "PreventiveMaintenance",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)",
                oldNullable: true);
        }
    }
}
