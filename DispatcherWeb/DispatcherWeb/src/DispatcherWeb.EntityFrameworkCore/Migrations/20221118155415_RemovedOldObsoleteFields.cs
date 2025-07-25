using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedOldObsoleteFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update Dispatch set TrackableSmsId = null where TrackableSmsId is not null");
            migrationBuilder.Sql("update DriverMessage set TrackableSmsId = null where TrackableSmsId is not null");
            migrationBuilder.Sql("delete from TrackableSms");
            migrationBuilder.Sql("update DriverAssignment set Note = null where Note is not null");
            migrationBuilder.Sql("update Office set BaseFuelCost = null where BaseFuelCost is not null");
            migrationBuilder.Sql("update [Order] set JobSite = null where JobSite is not null");
            migrationBuilder.Sql("update OrderLine set Route = null where Route is not null");
            migrationBuilder.Sql("update OrderLine set PickupNote = null where PickupNote is not null");
            migrationBuilder.Sql("update OrderLine set DeliveryNote = null where DeliveryNote is not null");
            migrationBuilder.Sql("update Receipt set JobSite = null where JobSite is not null");
            migrationBuilder.Sql("update Project set JobSite = null where JobSite is not null");
            migrationBuilder.Sql("update Project set Latitude = null where Latitude is not null");
            migrationBuilder.Sql("update Project set Longitude = null where Longitude is not null");
            migrationBuilder.Sql("update Quote set JobSite = null where JobSite is not null");
            migrationBuilder.Sql("update Service set PredefinedServiceKind = null where PredefinedServiceKind is not null");

            migrationBuilder.DropForeignKey(
                name: "FK_Dispatch_TrackableSms_TrackableSmsId",
                table: "Dispatch");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverMessage_TrackableSms_TrackableSmsId",
                table: "DriverMessage");

            migrationBuilder.DropTable(
                name: "TrackableSms");

            migrationBuilder.DropIndex(
                name: "IX_DriverMessage_TrackableSmsId",
                table: "DriverMessage");

            migrationBuilder.DropIndex(
                name: "IX_Dispatch_TrackableSmsId",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "PredefinedServiceKind",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "JobSite",
                table: "Receipt");

            migrationBuilder.DropColumn(
                name: "JobSite",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "JobSite",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "DeliveryNote",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "PickupNote",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "JobSite",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "BaseFuelCost",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "TrackableSmsId",
                table: "DriverMessage");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "DriverAssignment");

            migrationBuilder.DropColumn(
                name: "TrackableSmsId",
                table: "Dispatch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PredefinedServiceKind",
                table: "Service",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobSite",
                table: "Receipt",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobSite",
                table: "Quote",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobSite",
                table: "Project",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Project",
                type: "decimal(12,9)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Project",
                type: "decimal(12,9)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNote",
                table: "OrderLine",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupNote",
                table: "OrderLine",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "OrderLine",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobSite",
                table: "Order",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "Order",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseFuelCost",
                table: "Office",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrackableSmsId",
                table: "DriverMessage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "DriverAssignment",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrackableSmsId",
                table: "Dispatch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackableSms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Sid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackableSms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverMessage_TrackableSmsId",
                table: "DriverMessage",
                column: "TrackableSmsId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_TrackableSmsId",
                table: "Dispatch",
                column: "TrackableSmsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatch_TrackableSms_TrackableSmsId",
                table: "Dispatch",
                column: "TrackableSmsId",
                principalTable: "TrackableSms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverMessage_TrackableSms_TrackableSmsId",
                table: "DriverMessage",
                column: "TrackableSmsId",
                principalTable: "TrackableSms",
                principalColumn: "Id");
        }
    }
}
