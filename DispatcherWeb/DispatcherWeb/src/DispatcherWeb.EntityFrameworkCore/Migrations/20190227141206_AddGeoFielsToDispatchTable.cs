using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddGeoFielsToDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceLatitude",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceLongitude",
                table: "Dispatch",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "SourceLatitude",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "SourceLongitude",
                table: "Dispatch");
        }
    }
}
