using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedOfficeToQuotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "Quote",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quote_OfficeId",
                table: "Quote",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quote_Office_OfficeId",
                table: "Quote",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quote_Office_OfficeId",
                table: "Quote");

            migrationBuilder.DropIndex(
                name: "IX_Quote_OfficeId",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Quote");
        }
    }
}
