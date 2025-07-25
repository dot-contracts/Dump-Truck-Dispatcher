using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddDailyHistoryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantDailyHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    ActiveTrucks = table.Column<int>(nullable: false),
                    ActiveUsers = table.Column<int>(nullable: false),
                    UsersWithActivity = table.Column<int>(nullable: false),
                    OrderLinesScheduled = table.Column<int>(nullable: false),
                    ActiveCustomers = table.Column<int>(nullable: false),
                    InternalTrucksScheduled = table.Column<int>(nullable: false),
                    InternalScheduledDeliveries = table.Column<int>(nullable: false),
                    LeaseHaulerScheduledDeliveries = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDailyHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantDailyHistory_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDailyHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    ServiceName = table.Column<string>(maxLength: 256, nullable: false),
                    MethodName = table.Column<string>(maxLength: 256, nullable: false),
                    NumberOfTransactions = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDailyHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    NumberOfTransactions = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDailyHistory_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantDailyHistory_TenantId",
                table: "TenantDailyHistory",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyHistory_UserId",
                table: "UserDailyHistory",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantDailyHistory");

            migrationBuilder.DropTable(
                name: "TransactionDailyHistory");

            migrationBuilder.DropTable(
                name: "UserDailyHistory");
        }
    }
}
