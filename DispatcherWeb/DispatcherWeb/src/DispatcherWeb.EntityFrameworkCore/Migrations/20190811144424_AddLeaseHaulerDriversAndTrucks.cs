using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddLeaseHaulerDriversAndTrucks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "Truck",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "OfficeId",
                table: "DriverAssignment",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "Driver",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LeaseHaulerDriver",
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
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerDriver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerDriver_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerDriver_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHaulerTruck",
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
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    TruckId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerTruck_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerDriver_DriverId",
                table: "LeaseHaulerDriver",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerDriver_LeaseHaulerId",
                table: "LeaseHaulerDriver",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerTruck_LeaseHaulerId",
                table: "LeaseHaulerTruck",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerTruck_TruckId",
                table: "LeaseHaulerTruck",
                column: "TruckId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaseHaulerDriver");

            migrationBuilder.DropTable(
                name: "LeaseHaulerTruck");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "Driver");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "Truck",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfficeId",
                table: "DriverAssignment",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
