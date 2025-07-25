using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRateTableAndRelatedFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesTaxEntityId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesTaxEntityId",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaxRate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_SalesTaxEntityId",
                table: "Order",
                column: "SalesTaxEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_SalesTaxEntityId",
                table: "Invoice",
                column: "SalesTaxEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_TaxRate_SalesTaxEntityId",
                table: "Invoice",
                column: "SalesTaxEntityId",
                principalTable: "TaxRate",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_TaxRate_SalesTaxEntityId",
                table: "Order",
                column: "SalesTaxEntityId",
                principalTable: "TaxRate",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_TaxRate_SalesTaxEntityId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_TaxRate_SalesTaxEntityId",
                table: "Order");

            migrationBuilder.DropTable(
                name: "TaxRate");

            migrationBuilder.DropIndex(
                name: "IX_Order_SalesTaxEntityId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_SalesTaxEntityId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "SalesTaxEntityId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "SalesTaxEntityId",
                table: "Invoice");
        }
    }
}
