using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddOfficeHeartlandKeysFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeartlandPublicKey",
                table: "Office",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeartlandSecretKey",
                table: "Office",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeartlandPublicKey",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "HeartlandSecretKey",
                table: "Office");
        }
    }
}
