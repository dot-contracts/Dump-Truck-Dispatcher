using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddNoteFieldsToDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryNote",
                table: "Dispatch",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupNote",
                table: "Dispatch",
                maxLength: 400,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryNote",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "PickupNote",
                table: "Dispatch");
        }
    }
}
