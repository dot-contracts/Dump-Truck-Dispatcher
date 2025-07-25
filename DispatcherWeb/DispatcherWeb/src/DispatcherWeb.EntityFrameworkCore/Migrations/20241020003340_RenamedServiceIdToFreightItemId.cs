using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedServiceIdToFreightItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Service_ServiceId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Service_ServiceId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Service_ServiceId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Service_ServiceId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Ticket",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_ServiceId",
                table: "Ticket",
                newName: "IX_Ticket_ItemId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "ReceiptLine",
                newName: "FreightItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptLine_ServiceId",
                table: "ReceiptLine",
                newName: "IX_ReceiptLine_FreightItemId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "QuoteService",
                newName: "FreightItemId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_ServiceId",
                table: "QuoteService",
                newName: "IX_QuoteService_FreightItemId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "OrderLine",
                newName: "FreightItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLine_ServiceId",
                table: "OrderLine",
                newName: "IX_OrderLine_FreightItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Service_FreightItemId",
                table: "OrderLine",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Service_FreightItemId",
                table: "QuoteService",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Service_FreightItemId",
                table: "ReceiptLine",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Service_ItemId",
                table: "Ticket",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Service_FreightItemId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Service_FreightItemId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Service_FreightItemId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Service_ItemId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Ticket",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_ItemId",
                table: "Ticket",
                newName: "IX_Ticket_ServiceId");

            migrationBuilder.RenameColumn(
                name: "FreightItemId",
                table: "ReceiptLine",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptLine_FreightItemId",
                table: "ReceiptLine",
                newName: "IX_ReceiptLine_ServiceId");

            migrationBuilder.RenameColumn(
                name: "FreightItemId",
                table: "QuoteService",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_FreightItemId",
                table: "QuoteService",
                newName: "IX_QuoteService_ServiceId");

            migrationBuilder.RenameColumn(
                name: "FreightItemId",
                table: "OrderLine",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLine_FreightItemId",
                table: "OrderLine",
                newName: "IX_OrderLine_ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Service_ServiceId",
                table: "OrderLine",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Service_ServiceId",
                table: "QuoteService",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Service_ServiceId",
                table: "ReceiptLine",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Service_ServiceId",
                table: "Ticket",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id");
        }
    }
}
