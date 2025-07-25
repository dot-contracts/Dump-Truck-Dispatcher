using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddHoursFieldsToVehicleServiceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WarningMiles",
                table: "VehicleService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "WarningDays",
                table: "VehicleService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "RecommendedTimeInterval",
                table: "VehicleService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "RecommendedMileageInterval",
                table: "VehicleService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<decimal>(
                name: "RecommendedHourInterval",
                table: "VehicleService",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WarningHours",
                table: "VehicleService",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecommendedHourInterval",
                table: "VehicleService");

            migrationBuilder.DropColumn(
                name: "WarningHours",
                table: "VehicleService");

            migrationBuilder.AlterColumn<int>(
                name: "WarningMiles",
                table: "VehicleService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WarningDays",
                table: "VehicleService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RecommendedTimeInterval",
                table: "VehicleService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RecommendedMileageInterval",
                table: "VehicleService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
