using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedPayRateToEmployeeTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_EmployeeTimeClassification_EmployeeTimeClassificationId",
                table: "EmployeeTime");

            migrationBuilder.Sql("update EmployeeTime set EmployeeTimeClassificationId = null");

            migrationBuilder.RenameColumn(
                name: "EmployeeTimeClassificationId",
                table: "EmployeeTime",
                newName: "DriverId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTime_EmployeeTimeClassificationId",
                table: "EmployeeTime",
                newName: "IX_EmployeeTime_DriverId");

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "EmployeeTime",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_Driver_DriverId",
                table: "EmployeeTime",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_Driver_DriverId",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "EmployeeTime");

            migrationBuilder.RenameColumn(
                name: "DriverId",
                table: "EmployeeTime",
                newName: "EmployeeTimeClassificationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTime_DriverId",
                table: "EmployeeTime",
                newName: "IX_EmployeeTime_EmployeeTimeClassificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_EmployeeTimeClassification_EmployeeTimeClassificationId",
                table: "EmployeeTime",
                column: "EmployeeTimeClassificationId",
                principalTable: "EmployeeTimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
