using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddUserIdFieldToDriverTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Driver",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Driver_UserId",
                table: "Driver",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Driver_AbpUsers_UserId",
                table: "Driver",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Driver_AbpUsers_UserId",
                table: "Driver");

            migrationBuilder.DropIndex(
                name: "IX_Driver_UserId",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Driver");
        }
    }
}
