using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class SplitQuantityIntoMaterialAndFreightQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "QuoteService",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "QuoteService",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "ProjectService",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "ProjectService",
                nullable: true);

            migrationBuilder.Sql(@"update QuoteService set FreightQuantity = Quantity, MaterialQuantity = Quantity");
            migrationBuilder.Sql(@"update ProjectService set FreightQuantity = Quantity, MaterialQuantity = Quantity");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "ProjectService");
        }
    }
}
