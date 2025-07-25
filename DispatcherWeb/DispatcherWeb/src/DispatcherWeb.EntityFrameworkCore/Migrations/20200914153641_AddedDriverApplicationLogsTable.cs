using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedDriverApplicationLogsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverApplicationLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BatchOrder = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverApplicationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverApplicationLog_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverApplicationLog_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplicationLog_DriverId",
                table: "DriverApplicationLog",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplicationLog_UserId",
                table: "DriverApplicationLog",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverApplicationLog");
        }
    }
}
