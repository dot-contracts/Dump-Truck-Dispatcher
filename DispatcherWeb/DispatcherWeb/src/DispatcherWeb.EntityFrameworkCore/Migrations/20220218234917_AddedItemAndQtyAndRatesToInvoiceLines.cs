using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedItemAndQtyAndRatesToInvoiceLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreightRate",
                table: "InvoiceLine",
                type: "decimal(19, 4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "InvoiceLine",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialRate",
                table: "InvoiceLine",
                type: "decimal(19, 4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "InvoiceLine",
                type: "decimal(19, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_ItemId",
                table: "InvoiceLine",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_ItemId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "FreightRate",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "MaterialRate",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InvoiceLine");
        }
    }
}
