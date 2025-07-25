using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedOrderLeaseHaulers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from OrderLeaseHauler");

            migrationBuilder.DropTable(
                name: "OrderLeaseHauler");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderLeaseHauler",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    FreightUomId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsReconciled = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    MaterialUomId = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 1000, nullable: true),
                    OrderId = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLeaseHauler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_UnitOfMeasure_FreightUomId",
                        column: x => x.FreightUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_UnitOfMeasure_MaterialUomId",
                        column: x => x.MaterialUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_FreightUomId",
                table: "OrderLeaseHauler",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_LeaseHaulerId",
                table: "OrderLeaseHauler",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_MaterialUomId",
                table: "OrderLeaseHauler",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_OrderId",
                table: "OrderLeaseHauler",
                column: "OrderId");
        }
    }
}
