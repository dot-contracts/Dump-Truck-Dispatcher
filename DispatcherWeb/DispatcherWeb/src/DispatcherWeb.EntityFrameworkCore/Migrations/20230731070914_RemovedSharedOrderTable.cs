using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedSharedOrderTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedOrderLine_Office_OfficeId",
                table: "SharedOrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_SharedOrderLine_OrderLine_OrderLineId",
                table: "SharedOrderLine");

            migrationBuilder.DropTable(
                name: "SharedOrder");

            migrationBuilder.DropColumn(
                name: "SharedDateTime",
                table: "Order");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedOrderLine_Office_OfficeId",
                table: "SharedOrderLine",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SharedOrderLine_OrderLine_OrderLineId",
                table: "SharedOrderLine",
                column: "OrderLineId",
                principalTable: "OrderLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedOrderLine_Office_OfficeId",
                table: "SharedOrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_SharedOrderLine_OrderLine_OrderLineId",
                table: "SharedOrderLine");

            migrationBuilder.AddColumn<DateTime>(
                name: "SharedDateTime",
                table: "Order",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SharedOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedOrder_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedOrder_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedOrder_OfficeId",
                table: "SharedOrder",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedOrder_OrderId",
                table: "SharedOrder",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedOrderLine_Office_OfficeId",
                table: "SharedOrderLine",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SharedOrderLine_OrderLine_OrderLineId",
                table: "SharedOrderLine",
                column: "OrderLineId",
                principalTable: "OrderLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
