using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispatch",
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
                    TruckId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    OrderLineId = table.Column<int>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    PhoneNumber = table.Column<string>(maxLength: 15, nullable: true),
                    Sent = table.Column<DateTime>(nullable: false),
                    Acknowledged = table.Column<DateTime>(nullable: true),
                    Loaded = table.Column<DateTime>(nullable: true),
                    Complete = table.Column<DateTime>(nullable: true),
                    TrackableSmsId = table.Column<int>(nullable: true),
                    Message = table.Column<string>(maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispatch_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dispatch_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dispatch_TrackableSms_TrackableSmsId",
                        column: x => x.TrackableSmsId,
                        principalTable: "TrackableSms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dispatch_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dispatch_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_DriverId",
                table: "Dispatch",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_OrderLineId",
                table: "Dispatch",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_TrackableSmsId",
                table: "Dispatch",
                column: "TrackableSmsId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_TruckId",
                table: "Dispatch",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_UserId",
                table: "Dispatch",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dispatch");
        }
    }
}
