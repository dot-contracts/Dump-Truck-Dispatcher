using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddNewFieldsToLeaseHaulerContactTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CellPhoneNumber",
                table: "LeaseHaulerContact",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDispatcher",
                table: "LeaseHaulerContact",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NotifyPreferredFormat",
                table: "LeaseHaulerContact",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CellPhoneNumber",
                table: "LeaseHaulerContact");

            migrationBuilder.DropColumn(
                name: "IsDispatcher",
                table: "LeaseHaulerContact");

            migrationBuilder.DropColumn(
                name: "NotifyPreferredFormat",
                table: "LeaseHaulerContact");
        }
    }
}
