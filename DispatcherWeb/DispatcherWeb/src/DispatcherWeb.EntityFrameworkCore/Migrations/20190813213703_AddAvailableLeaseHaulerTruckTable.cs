using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddAvailableLeaseHaulerTruckTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailableLeaseHaulerTruck",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Shift = table.Column<byte>(nullable: true),
                    OfficeId = table.Column<int>(nullable: false),
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    TruckId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableLeaseHaulerTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableLeaseHaulerTruck_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableLeaseHaulerTruck_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableLeaseHaulerTruck_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableLeaseHaulerTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableLeaseHaulerTruck_DriverId",
                table: "AvailableLeaseHaulerTruck",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableLeaseHaulerTruck_LeaseHaulerId",
                table: "AvailableLeaseHaulerTruck",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableLeaseHaulerTruck_OfficeId",
                table: "AvailableLeaseHaulerTruck",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableLeaseHaulerTruck_TruckId",
                table: "AvailableLeaseHaulerTruck",
                column: "TruckId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailableLeaseHaulerTruck");
        }
    }
}
