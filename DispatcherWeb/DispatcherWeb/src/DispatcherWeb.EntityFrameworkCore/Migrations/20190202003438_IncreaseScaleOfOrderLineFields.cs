using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class IncreaseScaleOfOrderLineFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialPricePerUnit",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightPricePerUnit",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OrderLine",
                type: "decimal(18, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialPricePerUnit",
                table: "OrderLine",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightPricePerUnit",
                table: "OrderLine",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)",
                oldNullable: true);
        }
    }
}
