using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddOneToOneRelationToTruckAndLeaseHaulerTruckTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerTruck_TruckId",
                table: "LeaseHaulerTruck");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerTruck_TruckId",
                table: "LeaseHaulerTruck",
                column: "TruckId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerTruck_TruckId",
                table: "LeaseHaulerTruck");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerTruck_TruckId",
                table: "LeaseHaulerTruck",
                column: "TruckId");
        }
    }
}
