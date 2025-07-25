using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedDriverNoteToOrderLineTrucks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverNote",
                table: "OrderLineTruck",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderLineTruckId",
                table: "Dispatch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_OrderLineTruckId",
                table: "Dispatch",
                column: "OrderLineTruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatch_OrderLineTruck_OrderLineTruckId",
                table: "Dispatch",
                column: "OrderLineTruckId",
                principalTable: "OrderLineTruck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                update d set d.OrderLineTruckId = olt.Id
                from Dispatch d
                inner join OrderLineTruck olt on olt.OrderLineId = d.OrderLineId and olt.DriverId = d.DriverId and olt.TruckId = d.TruckId
                where d.OrderLineTruckId is null
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatch_OrderLineTruck_OrderLineTruckId",
                table: "Dispatch");

            migrationBuilder.DropIndex(
                name: "IX_Dispatch_OrderLineTruckId",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "DriverNote",
                table: "OrderLineTruck");

            migrationBuilder.DropColumn(
                name: "OrderLineTruckId",
                table: "Dispatch");
        }
    }
}
