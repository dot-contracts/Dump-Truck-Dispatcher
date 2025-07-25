using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemoveUnusedOrderCreditCardFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update [Order] set [AuthorizedAmount] = null");
            migrationBuilder.DropColumn(
                name: "AuthorizedAmount",
                table: "Order");

            migrationBuilder.Sql("update [Order] set [CreditCardInfo] = null");
            migrationBuilder.DropColumn(
                name: "CreditCardInfo",
                table: "Order");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AuthorizedAmount",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardInfo",
                table: "Order",
                nullable: true);
        }
    }
}
