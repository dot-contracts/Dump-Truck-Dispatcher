using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedQuoteServicesToQuoteLinesStep1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteServiceId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteServiceId",
                table: "QuoteServiceVehicleCategory");

            migrationBuilder.RenameColumn(
                name: "QuoteServiceId",
                table: "QuoteServiceVehicleCategory",
                newName: "QuoteLineId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteServiceVehicleCategory_QuoteServiceId",
                table: "QuoteServiceVehicleCategory",
                newName: "IX_QuoteServiceVehicleCategory_QuoteLineId");

            migrationBuilder.RenameColumn(
                name: "QuoteServiceId",
                table: "OrderLine",
                newName: "QuoteLineId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLine_QuoteServiceId",
                table: "OrderLine",
                newName: "IX_OrderLine_QuoteLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteLineId",
                table: "OrderLine",
                column: "QuoteLineId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteLineId",
                table: "QuoteServiceVehicleCategory",
                column: "QuoteLineId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteLineId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteLineId",
                table: "QuoteServiceVehicleCategory");

            migrationBuilder.RenameColumn(
                name: "QuoteLineId",
                table: "QuoteServiceVehicleCategory",
                newName: "QuoteServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteServiceVehicleCategory_QuoteLineId",
                table: "QuoteServiceVehicleCategory",
                newName: "IX_QuoteServiceVehicleCategory_QuoteServiceId");

            migrationBuilder.RenameColumn(
                name: "QuoteLineId",
                table: "OrderLine",
                newName: "QuoteServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLine_QuoteLineId",
                table: "OrderLine",
                newName: "IX_OrderLine_QuoteServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteServiceId",
                table: "OrderLine",
                column: "QuoteServiceId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteServiceId",
                table: "QuoteServiceVehicleCategory",
                column: "QuoteServiceId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
