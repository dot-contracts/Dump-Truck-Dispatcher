using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeparateInvoiceLineQuantityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Item_ItemId",
                table: "InvoiceLine");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "InvoiceLine",
                newName: "FreightItemId");

            migrationBuilder.RenameColumn(
                name: "IsTaxable",
                table: "InvoiceLine",
                newName: "IsFreightTaxable");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceLine_ItemId",
                table: "InvoiceLine",
                newName: "IX_InvoiceLine_FreightItemId");

            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "InvoiceLine",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMaterialTaxable",
                table: "InvoiceLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "InvoiceLine",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Item_FreightItemId",
                table: "InvoiceLine",
                column: "FreightItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Item_FreightItemId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "IsMaterialTaxable",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "InvoiceLine");

            migrationBuilder.RenameColumn(
                name: "IsFreightTaxable",
                table: "InvoiceLine",
                newName: "IsTaxable");

            migrationBuilder.RenameColumn(
                name: "FreightItemId",
                table: "InvoiceLine",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceLine_FreightItemId",
                table: "InvoiceLine",
                newName: "IX_InvoiceLine_ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Item_ItemId",
                table: "InvoiceLine",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
