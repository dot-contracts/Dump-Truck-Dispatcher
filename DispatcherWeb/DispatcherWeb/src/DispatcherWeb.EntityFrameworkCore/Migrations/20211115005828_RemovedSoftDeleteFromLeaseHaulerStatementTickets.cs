using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedSoftDeleteFromLeaseHaulerStatementTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update LeaseHaulerStatementTicket set DeleterUserId = null, DeletionTime = null, IsDeleted = 0");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "LeaseHaulerStatementTicket");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "LeaseHaulerStatementTicket");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LeaseHaulerStatementTicket");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "LeaseHaulerStatementTicket",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "LeaseHaulerStatementTicket",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LeaseHaulerStatementTicket",
                nullable: false,
                defaultValue: false);
        }
    }
}
