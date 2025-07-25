using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedObsoleteServiceFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"update [Service] set BillPerTon = null, IsService = 0, UnitOfMeasureId = null");

            migrationBuilder.DropColumn(
                name: "BillPerTon",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "IsService",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "Service");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BillPerTon",
                table: "Service",
                type: "decimal(19, 4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsService",
                table: "Service",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "Service",
                nullable: true);
        }
    }
}
