using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedObsoleteFieldsFromEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "IsEmbedded",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "Receipt");

            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "Load");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Truck",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmbedded",
                table: "Truck",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "Receipt",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "Quote",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "Load",
                type: "int",
                nullable: true);
        }
    }
}
