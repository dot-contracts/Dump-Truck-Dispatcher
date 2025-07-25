using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedChargeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargeId",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Charge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ChargeAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    OrderLineId = table.Column<int>(type: "int", nullable: false),
                    IsBilled = table.Column<bool>(type: "bit", nullable: false),
                    ChargeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_Charge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Charge_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charge_Service_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charge_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_ChargeId",
                table: "InvoiceLine",
                column: "ChargeId",
                unique: true,
                filter: "[ChargeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Charge_ItemId",
                table: "Charge",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Charge_OrderLineId",
                table: "Charge",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Charge_UnitOfMeasureId",
                table: "Charge",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Charge_ChargeId",
                table: "InvoiceLine",
                column: "ChargeId",
                principalTable: "Charge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Charge_ChargeId",
                table: "InvoiceLine");

            migrationBuilder.DropTable(
                name: "Charge");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_ChargeId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "ChargeId",
                table: "InvoiceLine");
        }
    }
}
