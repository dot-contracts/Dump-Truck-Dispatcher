using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedTruckPositionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from TruckPosition");

            migrationBuilder.DropTable(
                name: "TruckPosition");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TruckPosition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    TruckId = table.Column<int>(type: "int", nullable: true),
                    Accuracy = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActivitiTypeRaw = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActivityConfidence = table.Column<int>(type: "int", nullable: true),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    Altitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BatteryIsCharging = table.Column<bool>(type: "bit", nullable: true),
                    BatteryLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    Event = table.Column<int>(type: "int", nullable: false),
                    EventRaw = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GeofenceAction = table.Column<int>(type: "int", nullable: true),
                    GeofenceActionRaw = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GeofenceIdentifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Heading = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsMoving = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(12,9)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(12,9)", nullable: true),
                    Odometer = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Speed = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckPosition_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TruckPosition_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TruckPosition_DriverId",
                table: "TruckPosition",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckPosition_TruckId",
                table: "TruckPosition",
                column: "TruckId");
        }
    }
}
