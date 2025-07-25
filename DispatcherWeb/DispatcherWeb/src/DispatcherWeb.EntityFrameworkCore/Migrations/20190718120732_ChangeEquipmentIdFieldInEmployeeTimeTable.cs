using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ChangeEquipmentIdFieldInEmployeeTimeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_Truck_EquipmentId",
                table: "EmployeeTime");

            migrationBuilder.AlterColumn<int>(
                name: "EquipmentId",
                table: "EmployeeTime",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_Truck_EquipmentId",
                table: "EmployeeTime",
                column: "EquipmentId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_Truck_EquipmentId",
                table: "EmployeeTime");

            migrationBuilder.AlterColumn<int>(
                name: "EquipmentId",
                table: "EmployeeTime",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_Truck_EquipmentId",
                table: "EmployeeTime",
                column: "EquipmentId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
