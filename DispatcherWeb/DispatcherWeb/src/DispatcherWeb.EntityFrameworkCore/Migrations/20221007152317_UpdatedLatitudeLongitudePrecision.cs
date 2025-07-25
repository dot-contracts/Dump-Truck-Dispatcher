using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class UpdatedLatitudeLongitudePrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "TruckPosition",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "TruckPosition",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Project",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Project",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "LuckStoneLocation",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "LuckStoneLocation",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Location",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Location",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "EmployeeTime",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "EmployeeTime",
                type: "decimal(12,9)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "TruckPosition",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "TruckPosition",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Project",
                type: "decimal(10,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Project",
                type: "decimal(10,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "LuckStoneLocation",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "LuckStoneLocation",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Location",
                type: "decimal(10,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Location",
                type: "decimal(10,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "EmployeeTime",
                type: "decimal(10,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "EmployeeTime",
                type: "decimal(10,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,9)",
                oldNullable: true);
        }
    }
}
