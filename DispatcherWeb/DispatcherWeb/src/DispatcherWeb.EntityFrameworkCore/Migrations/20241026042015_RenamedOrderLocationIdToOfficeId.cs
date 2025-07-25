using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedOrderLocationIdToOfficeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Office_LocationId",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Order",
                newName: "OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_LocationId",
                table: "Order",
                newName: "IX_Order_OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Office_OfficeId",
                table: "Order",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Office_OfficeId",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "OfficeId",
                table: "Order",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_OfficeId",
                table: "Order",
                newName: "IX_Order_LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Office_LocationId",
                table: "Order",
                column: "LocationId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
