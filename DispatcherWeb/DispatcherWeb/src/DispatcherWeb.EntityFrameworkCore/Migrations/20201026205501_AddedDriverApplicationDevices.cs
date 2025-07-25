using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedDriverApplicationDevices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "DriverPushSubscription",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "DriverApplicationLog",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverApplicationDevice",
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
                    Useragent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverApplicationDevice", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPushSubscription_DeviceId",
                table: "DriverPushSubscription",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplicationLog_DeviceId",
                table: "DriverApplicationLog",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverApplicationLog_DriverApplicationDevice_DeviceId",
                table: "DriverApplicationLog",
                column: "DeviceId",
                principalTable: "DriverApplicationDevice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverPushSubscription_DriverApplicationDevice_DeviceId",
                table: "DriverPushSubscription",
                column: "DeviceId",
                principalTable: "DriverApplicationDevice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverApplicationLog_DriverApplicationDevice_DeviceId",
                table: "DriverApplicationLog");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverPushSubscription_DriverApplicationDevice_DeviceId",
                table: "DriverPushSubscription");

            migrationBuilder.DropTable(
                name: "DriverApplicationDevice");

            migrationBuilder.DropIndex(
                name: "IX_DriverPushSubscription_DeviceId",
                table: "DriverPushSubscription");

            migrationBuilder.DropIndex(
                name: "IX_DriverApplicationLog_DeviceId",
                table: "DriverApplicationLog");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "DriverPushSubscription");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "DriverApplicationLog");
        }
    }
}
