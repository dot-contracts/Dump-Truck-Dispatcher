using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequireTicketValueScript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE q SET q.RequireTicket = CASE WHEN s.Value = 'true' THEN 'true' ELSE 'false' END FROM QuoteLine q JOIN AbpSettings s ON s.TenantId = q.TenantId AND s.Name = 'App.DispatchingAndMessaging.RequireDriversToEnterTickets';");
            migrationBuilder.Sql($"UPDATE o SET o.RequireTicket = CASE WHEN s.Value = 'true' THEN 'true' ELSE 'false' END FROM OrderLine o JOIN AbpSettings s ON s.TenantId = o.TenantId AND s.Name = 'App.DispatchingAndMessaging.RequireDriversToEnterTickets';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
