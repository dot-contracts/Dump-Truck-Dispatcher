using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOrderTruckTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM OrderTruck");

            migrationBuilder.DropTable(
                name: "OrderTruck");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderTruck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ParentOrderTruckId = table.Column<int>(type: "int", nullable: true),
                    TruckId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Utilization = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTruck_OrderTruck_ParentOrderTruckId",
                        column: x => x.ParentOrderTruckId,
                        principalTable: "OrderTruck",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderTruck_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderTruck_OrderId",
                table: "OrderTruck",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTruck_ParentOrderTruckId",
                table: "OrderTruck",
                column: "ParentOrderTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTruck_TruckId",
                table: "OrderTruck",
                column: "TruckId");
        }
    }
}
