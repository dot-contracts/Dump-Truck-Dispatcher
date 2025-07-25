using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedSalesPersonIdToQuotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SalesPersonId",
                table: "Quote",
                nullable: true);

            migrationBuilder.Sql("update Quote set SalesPersonId = CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_SalesPersonId",
                table: "Quote",
                column: "SalesPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quote_AbpUsers_SalesPersonId",
                table: "Quote",
                column: "SalesPersonId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quote_AbpUsers_SalesPersonId",
                table: "Quote");

            migrationBuilder.DropIndex(
                name: "IX_Quote_SalesPersonId",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "SalesPersonId",
                table: "Quote");
        }
    }
}
