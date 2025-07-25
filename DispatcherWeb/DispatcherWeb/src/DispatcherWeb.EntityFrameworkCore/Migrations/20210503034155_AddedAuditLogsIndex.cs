using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedAuditLogsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogs_ImpersonatorTenantId_TenantId_UserId_ExecutionTime",
                table: "AbpAuditLogs",
                columns: new[] { "ImpersonatorTenantId", "TenantId", "UserId", "ExecutionTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpAuditLogs_ImpersonatorTenantId_TenantId_UserId_ExecutionTime",
                table: "AbpAuditLogs");
        }
    }
}
