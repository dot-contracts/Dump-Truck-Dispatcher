using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedChargeIdToReceiptLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargeId",
                table: "ReceiptLine",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_ChargeId",
                table: "ReceiptLine",
                column: "ChargeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Charge_ChargeId",
                table: "ReceiptLine",
                column: "ChargeId",
                principalTable: "Charge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Charge_ChargeId",
                table: "ReceiptLine");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptLine_ChargeId",
                table: "ReceiptLine");

            migrationBuilder.DropColumn(
                name: "ChargeId",
                table: "ReceiptLine");
        }
    }
}
