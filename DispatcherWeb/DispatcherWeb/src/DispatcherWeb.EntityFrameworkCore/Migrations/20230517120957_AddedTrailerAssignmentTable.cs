using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedTrailerAssignmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineTruck_Truck_TruckId",
                table: "OrderLineTruck");

            migrationBuilder.AddColumn<int>(
                name: "TrailerId",
                table: "OrderLineTruck",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderLineVehicleCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    OrderLineId = table.Column<int>(type: "int", nullable: false),
                    VehicleCategoryId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_OrderLineVehicleCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineVehicleCategory_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLineVehicleCategory_VehicleCategory_VehicleCategoryId",
                        column: x => x.VehicleCategoryId,
                        principalTable: "VehicleCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrailerAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shift = table.Column<byte>(type: "tinyint", nullable: true),
                    OfficeId = table.Column<int>(type: "int", nullable: true),
                    TractorId = table.Column<int>(type: "int", nullable: false),
                    TrailerId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_TrailerAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrailerAssignment_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrailerAssignment_Truck_TractorId",
                        column: x => x.TractorId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrailerAssignment_Truck_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineTruck_TrailerId",
                table: "OrderLineTruck",
                column: "TrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineVehicleCategory_OrderLineId",
                table: "OrderLineVehicleCategory",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineVehicleCategory_VehicleCategoryId",
                table: "OrderLineVehicleCategory",
                column: "VehicleCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrailerAssignment_OfficeId",
                table: "TrailerAssignment",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrailerAssignment_TractorId",
                table: "TrailerAssignment",
                column: "TractorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrailerAssignment_TrailerId",
                table: "TrailerAssignment",
                column: "TrailerId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineTruck_Truck_TrailerId",
                table: "OrderLineTruck",
                column: "TrailerId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineTruck_Truck_TruckId",
                table: "OrderLineTruck",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineTruck_Truck_TrailerId",
                table: "OrderLineTruck");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineTruck_Truck_TruckId",
                table: "OrderLineTruck");

            migrationBuilder.DropTable(
                name: "OrderLineVehicleCategory");

            migrationBuilder.DropTable(
                name: "TrailerAssignment");

            migrationBuilder.DropIndex(
                name: "IX_OrderLineTruck_TrailerId",
                table: "OrderLineTruck");

            migrationBuilder.DropColumn(
                name: "TrailerId",
                table: "OrderLineTruck");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineTruck_Truck_TruckId",
                table: "OrderLineTruck",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
