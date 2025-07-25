using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddIndexToAuditLogTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogs_ExecutionTime",
                table: "AbpAuditLogs",
                column: "ExecutionTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpAuditLogs_ExecutionTime",
                table: "AbpAuditLogs");
        }
    }
}
