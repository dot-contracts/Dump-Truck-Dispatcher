using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredTicketEntrySettingScript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @CurrentTime DATETIME = GETDATE(); 
                INSERT INTO AbpSettings (TenantId, Name, Value, CreationTime) 
                SELECT t.Id, 'App.DispatchingAndMessaging.RequiredTicketEntry', '1', @CurrentTime 
                FROM AbpTenants t 
                LEFT JOIN AbpSettings s ON s.TenantId = t.Id 
                AND s.Name = 'App.DispatchingAndMessaging.RequireDriversToEnterTickets' 
                WHERE s.Value = 'true' 
                AND NOT EXISTS (
                    SELECT 1 FROM AbpSettings 
                    WHERE TenantId = t.Id 
                    AND Name = 'App.DispatchingAndMessaging.RequiredTicketEntry'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
