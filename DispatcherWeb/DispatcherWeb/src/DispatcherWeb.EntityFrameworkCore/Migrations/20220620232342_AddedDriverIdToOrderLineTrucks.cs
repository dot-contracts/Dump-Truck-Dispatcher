using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedDriverIdToOrderLineTrucks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "OrderLineTruck",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineTruck_DriverId",
                table: "OrderLineTruck",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineTruck_Driver_DriverId",
                table: "OrderLineTruck",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
                update olt set DriverId = da.DriverId
                from OrderLineTruck olt
                inner join OrderLine ol on olt.OrderLineId = ol.Id
                inner join [Order] o on o.Id = ol.OrderId
                inner join DriverAssignment da on da.Shift = o.Shift and da.Date = o.DateTime and da.TruckId = olt.TruckId
                where olt.IsDeleted = 0 and ol.IsDeleted = 0 and o.IsDeleted = 0 and da.IsDeleted = 0
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineTruck_Driver_DriverId",
                table: "OrderLineTruck");

            migrationBuilder.DropIndex(
                name: "IX_OrderLineTruck_DriverId",
                table: "OrderLineTruck");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "OrderLineTruck");
        }
    }
}
