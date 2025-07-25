using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class ChangedChargeInvoiceLineRelationshipToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_ChargeId",
                table: "InvoiceLine");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_ChargeId",
                table: "InvoiceLine",
                column: "ChargeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_ChargeId",
                table: "InvoiceLine");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_ChargeId",
                table: "InvoiceLine",
                column: "ChargeId",
                unique: true,
                filter: "[ChargeId] IS NOT NULL");
        }
    }
}
