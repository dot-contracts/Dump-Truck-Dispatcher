using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddNoteFieldsToOrderLineTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryNote",
                table: "OrderLine",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupNote",
                table: "OrderLine",
                maxLength: 400,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryNote",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "PickupNote",
                table: "OrderLine");
        }
    }
}
