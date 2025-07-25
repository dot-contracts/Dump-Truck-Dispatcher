using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedHaulZoneAndHaulingServiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine");

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureBaseId",
                table: "UnitOfMeasure",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulZonId",
                table: "Service",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialItemId",
                table: "ReceiptLine",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ReceiptLine
                SET MaterialItemId = ServiceId
            ");

            migrationBuilder.AddColumn<int>(
                name: "MaterialItemId",
                table: "QuoteService",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE QuoteService
                SET MaterialItemId = ServiceId
            ");

            migrationBuilder.AddColumn<int>(
                name: "MaterialItemId",
                table: "OrderLine",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE OrderLine
                SET MaterialItemId = ServiceId
            ");

            migrationBuilder.AddColumn<int>(
                name: "MaterialItemId",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE InvoiceLine
                SET MaterialItemId = ItemId
            ");

            migrationBuilder.CreateTable(
                name: "HaulingService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    TruckCategoryId = table.Column<int>(type: "int", nullable: true),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    MinimumBillableUnits = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
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
                    table.PrimaryKey("PK_HaulingService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaulingService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HaulingService_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HaulingService_VehicleCategory_TruckCategoryId",
                        column: x => x.TruckCategoryId,
                        principalTable: "VehicleCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HaulZone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Minimum = table.Column<float>(type: "real", nullable: false),
                    Maximum = table.Column<float>(type: "real", nullable: false),
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
                    table.PrimaryKey("PK_HaulZone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaulZone_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureBase", x => x.Id);
                });

            var uomsToAdd = new[]
            {
                "Drive Miles",
                "Air Miles",
                "Drive KMs",
                "Air KMs",
            };

            foreach (var uom in uomsToAdd)
            {
                migrationBuilder.Sql(@$"
                    Insert into UnitOfMeasure
                    (Name, CreationTime, IsDeleted, TenantId)
                    Select '{uom}', GETDATE(), 0, Id from AbpTenants
                ");
            }

            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT UnitOfMeasureBase ON;

                INSERT INTO UnitOfMeasureBase (Id, Name) VALUES
                (1, 'Hours'),
                (2, 'Tons'),
                (3, 'Loads'),
                (4, 'Cubic Yards'),
                (5, 'Each'),
                (6, 'Cubic Meters'),
                (7, 'Miles'),
                (8, 'Drive Miles'),
                (9, 'Air Miles'),
                (10, 'Drive KMs'),
                (11, 'Air KMs');

                SET IDENTITY_INSERT UnitOfMeasureBase OFF;
            ");

            migrationBuilder.Sql(@"
                UPDATE u
                SET u.UnitOfMeasureBaseId = b.Id
                FROM UnitOfMeasure u
                INNER JOIN UnitOfMeasureBase b ON u.Name = b.Name;
            ");

            migrationBuilder.CreateTable(
                name: "HaulingServicePrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    HaulingServiceId = table.Column<int>(type: "int", nullable: false),
                    PricingTierId = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
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
                    table.PrimaryKey("PK_HaulingServicePrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaulingServicePrice_HaulingService_HaulingServiceId",
                        column: x => x.HaulingServiceId,
                        principalTable: "HaulingService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HaulingServicePrice_PricingTier_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasure_UnitOfMeasureBaseId",
                table: "UnitOfMeasure",
                column: "UnitOfMeasureBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_HaulZonId",
                table: "Service",
                column: "HaulZonId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_MaterialItemId",
                table: "ReceiptLine",
                column: "MaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_MaterialItemId",
                table: "QuoteService",
                column: "MaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_MaterialItemId",
                table: "OrderLine",
                column: "MaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_MaterialItemId",
                table: "InvoiceLine",
                column: "MaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HaulingService_ServiceId",
                table: "HaulingService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_HaulingService_TruckCategoryId",
                table: "HaulingService",
                column: "TruckCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HaulingService_UnitOfMeasureId",
                table: "HaulingService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_HaulingServicePrice_HaulingServiceId",
                table: "HaulingServicePrice",
                column: "HaulingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_HaulingServicePrice_PricingTierId",
                table: "HaulingServicePrice",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_HaulZone_UnitOfMeasureId",
                table: "HaulZone",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Service_MaterialItemId",
                table: "InvoiceLine",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Service_MaterialItemId",
                table: "OrderLine",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Service_MaterialItemId",
                table: "QuoteService",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Service_MaterialItemId",
                table: "ReceiptLine",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_HaulZone_HaulZonId",
                table: "Service",
                column: "HaulZonId",
                principalTable: "HaulZone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitOfMeasure_UnitOfMeasureBase_UnitOfMeasureBaseId",
                table: "UnitOfMeasure",
                column: "UnitOfMeasureBaseId",
                principalTable: "UnitOfMeasureBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("Update [Service] set [Type] = 2 where [Type] = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Service_MaterialItemId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Service_MaterialItemId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Service_MaterialItemId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Service_MaterialItemId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Service_HaulZone_HaulZonId",
                table: "Service");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitOfMeasure_UnitOfMeasureBase_UnitOfMeasureBaseId",
                table: "UnitOfMeasure");

            migrationBuilder.DropTable(
                name: "HaulingServicePrice");

            migrationBuilder.DropTable(
                name: "HaulZone");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureBase");

            migrationBuilder.DropTable(
                name: "HaulingService");

            migrationBuilder.DropIndex(
                name: "IX_UnitOfMeasure_UnitOfMeasureBaseId",
                table: "UnitOfMeasure");

            migrationBuilder.DropIndex(
                name: "IX_Service_HaulZonId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptLine_MaterialItemId",
                table: "ReceiptLine");

            migrationBuilder.DropIndex(
                name: "IX_QuoteService_MaterialItemId",
                table: "QuoteService");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_MaterialItemId",
                table: "OrderLine");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_MaterialItemId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureBaseId",
                table: "UnitOfMeasure");

            migrationBuilder.DropColumn(
                name: "HaulZonId",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "MaterialItemId",
                table: "ReceiptLine");

            migrationBuilder.DropColumn(
                name: "MaterialItemId",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "MaterialItemId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "MaterialItemId",
                table: "InvoiceLine");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id");
        }
    }
}
