using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedTruckLocationIdToOfficeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Office_LocationId",
                table: "Truck");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Truck",
                newName: "OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_Truck_LocationId",
                table: "Truck",
                newName: "IX_Truck_OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Office_OfficeId",
                table: "Truck",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Office_OfficeId",
                table: "Truck");

            migrationBuilder.RenameColumn(
                name: "OfficeId",
                table: "Truck",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Truck_OfficeId",
                table: "Truck",
                newName: "IX_Truck_LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Office_LocationId",
                table: "Truck",
                column: "LocationId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
