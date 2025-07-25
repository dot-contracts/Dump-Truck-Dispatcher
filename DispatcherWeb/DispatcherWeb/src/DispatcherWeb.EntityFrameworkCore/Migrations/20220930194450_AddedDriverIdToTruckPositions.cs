using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedDriverIdToTruckPositions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //driverId is required now
            migrationBuilder.Sql("Delete from TruckPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_TruckPosition_Truck_TruckId",
                table: "TruckPosition");

            migrationBuilder.AlterColumn<int>(
                name: "TruckId",
                table: "TruckPosition",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "TruckPosition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TruckPosition_DriverId",
                table: "TruckPosition",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckPosition_Driver_DriverId",
                table: "TruckPosition",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TruckPosition_Truck_TruckId",
                table: "TruckPosition",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckPosition_Driver_DriverId",
                table: "TruckPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_TruckPosition_Truck_TruckId",
                table: "TruckPosition");

            migrationBuilder.DropIndex(
                name: "IX_TruckPosition_DriverId",
                table: "TruckPosition");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "TruckPosition");

            migrationBuilder.AlterColumn<int>(
                name: "TruckId",
                table: "TruckPosition",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TruckPosition_Truck_TruckId",
                table: "TruckPosition",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
