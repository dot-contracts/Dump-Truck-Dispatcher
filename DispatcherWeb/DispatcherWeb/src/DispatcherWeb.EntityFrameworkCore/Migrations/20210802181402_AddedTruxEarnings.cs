using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedTruxEarnings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TruxTruckId",
                table: "Truck",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "Ticket",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "Order",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "EmployeeTime",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TruxEarningsBatch",
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
                    FilePath = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruxEarningsBatch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TruxEarnings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    JobId = table.Column<int>(nullable: false),
                    JobName = table.Column<string>(maxLength: 200, nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    TruckType = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: true),
                    TruxTruckId = table.Column<string>(maxLength: 20, nullable: true),
                    DriverName = table.Column<string>(maxLength: 200, nullable: true),
                    HaulerName = table.Column<string>(maxLength: 200, nullable: true),
                    PunchInDatetime = table.Column<DateTime>(nullable: false),
                    PunchOutDatetime = table.Column<DateTime>(nullable: false),
                    Hours = table.Column<decimal>(nullable: false),
                    Tons = table.Column<decimal>(nullable: false),
                    Loads = table.Column<int>(nullable: false),
                    Unit = table.Column<string>(maxLength: 50, nullable: true),
                    Rate = table.Column<decimal>(nullable: false),
                    Total = table.Column<decimal>(nullable: false),
                    BatchId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruxEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruxEarnings_TruxEarningsBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "TruxEarningsBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TruxEarnings_BatchId",
                table: "TruxEarnings",
                column: "BatchId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TruxEarnings");

            migrationBuilder.DropTable(
                name: "TruxEarningsBatch");

            migrationBuilder.DropColumn(
                name: "TruxTruckId",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "EmployeeTime");
        }
    }
}
