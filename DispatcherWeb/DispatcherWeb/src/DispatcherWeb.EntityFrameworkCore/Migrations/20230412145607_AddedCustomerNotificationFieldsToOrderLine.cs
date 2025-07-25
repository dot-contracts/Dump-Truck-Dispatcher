using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedCustomerNotificationFieldsToOrderLine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerNotificationContactName",
                table: "OrderLine",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNotificationPhoneNumber",
                table: "OrderLine",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCustomerNotification",
                table: "OrderLine",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerNotificationContactName",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "CustomerNotificationPhoneNumber",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "RequiresCustomerNotification",
                table: "OrderLine");
        }
    }
}
