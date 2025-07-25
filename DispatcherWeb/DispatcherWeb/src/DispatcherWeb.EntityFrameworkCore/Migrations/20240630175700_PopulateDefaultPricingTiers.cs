using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class PopulateDefaultPricingTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO PricingTier (Name, TenantId, IsDeleted, CreationTime, IsDefault) SELECT 'Retail', Id, 0, GETDATE(), 1 FROM AbpTenants WHERE NOT EXISTS(SELECT TenantId FROM PricingTier WHERE TenantId = AbpTenants.Id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
