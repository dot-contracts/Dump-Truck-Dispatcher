using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderLineIdToPayStatementTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderLineId",
                table: "PayStatementTime",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementTime_OrderLineId",
                table: "PayStatementTime",
                column: "OrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayStatementTime_OrderLine_OrderLineId",
                table: "PayStatementTime",
                column: "OrderLineId",
                principalTable: "OrderLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTime_OrderLine_OrderLineId",
                table: "PayStatementTime");

            migrationBuilder.DropIndex(
                name: "IX_PayStatementTime_OrderLineId",
                table: "PayStatementTime");

            migrationBuilder.DropColumn(
                name: "OrderLineId",
                table: "PayStatementTime");
        }
    }
}
