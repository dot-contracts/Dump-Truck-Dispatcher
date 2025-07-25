using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedLeaseHaulerRequestIdToAvailableLhTrucks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeaseHaulerRequestId",
                table: "AvailableLeaseHaulerTruck",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailableLeaseHaulerTruck_LeaseHaulerRequestId",
                table: "AvailableLeaseHaulerTruck",
                column: "LeaseHaulerRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_AvailableLeaseHaulerTruck_LeaseHaulerRequest_LeaseHaulerRequestId",
                table: "AvailableLeaseHaulerTruck",
                column: "LeaseHaulerRequestId",
                principalTable: "LeaseHaulerRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailableLeaseHaulerTruck_LeaseHaulerRequest_LeaseHaulerRequestId",
                table: "AvailableLeaseHaulerTruck");

            migrationBuilder.DropIndex(
                name: "IX_AvailableLeaseHaulerTruck_LeaseHaulerRequestId",
                table: "AvailableLeaseHaulerTruck");

            migrationBuilder.DropColumn(
                name: "LeaseHaulerRequestId",
                table: "AvailableLeaseHaulerTruck");
        }
    }
}
