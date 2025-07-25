using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationServicePriceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PricingTierId",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
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
                    table.PrimaryKey("PK_LocationService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationService_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationService_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PricingTier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PricingTier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationServicePrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LocationServiceId = table.Column<int>(type: "int", nullable: false),
                    PricingTierId = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    LocationServiceId1 = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_LocationServicePrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationServicePrice_LocationService_LocationServiceId",
                        column: x => x.LocationServiceId,
                        principalTable: "LocationService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationServicePrice_LocationService_LocationServiceId1",
                        column: x => x.LocationServiceId1,
                        principalTable: "LocationService",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationServicePrice_PricingTier_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PricingTierId",
                table: "Customer",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationService_LocationId",
                table: "LocationService",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationService_ServiceId",
                table: "LocationService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationService_UnitOfMeasureId",
                table: "LocationService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationServicePrice_LocationServiceId",
                table: "LocationServicePrice",
                column: "LocationServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationServicePrice_LocationServiceId1",
                table: "LocationServicePrice",
                column: "LocationServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_LocationServicePrice_PricingTierId",
                table: "LocationServicePrice",
                column: "PricingTierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_PricingTier_PricingTierId",
                table: "Customer",
                column: "PricingTierId",
                principalTable: "PricingTier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_PricingTier_PricingTierId",
                table: "Customer");

            migrationBuilder.DropTable(
                name: "LocationServicePrice");

            migrationBuilder.DropTable(
                name: "LocationService");

            migrationBuilder.DropTable(
                name: "PricingTier");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PricingTierId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PricingTierId",
                table: "Customer");
        }
    }
}
