using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedQuoteServiceVehicleCategoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuoteServiceVehicleCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    QuoteServiceId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_QuoteServiceVehicleCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteServiceId",
                        column: x => x.QuoteServiceId,
                        principalTable: "QuoteService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteServiceVehicleCategory_VehicleCategory_VehicleCategoryId",
                        column: x => x.VehicleCategoryId,
                        principalTable: "VehicleCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteServiceVehicleCategory_QuoteServiceId",
                table: "QuoteServiceVehicleCategory",
                column: "QuoteServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteServiceVehicleCategory_VehicleCategoryId",
                table: "QuoteServiceVehicleCategory",
                column: "VehicleCategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteServiceVehicleCategory");
        }
    }
}
