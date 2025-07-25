using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedMergedToIdColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MergedToId",
                table: "SupplierContact",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MergedToId",
                table: "Service",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MergedToId",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MergedToId",
                table: "Customer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MergedToId",
                table: "SupplierContact");

            migrationBuilder.DropColumn(
                name: "MergedToId",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "MergedToId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "MergedToId",
                table: "Customer");
        }
    }
}
