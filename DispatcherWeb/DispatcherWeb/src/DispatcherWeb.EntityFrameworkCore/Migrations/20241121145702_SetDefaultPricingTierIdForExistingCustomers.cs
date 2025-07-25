using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultPricingTierIdForExistingCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE c
                SET PricingTierId = pt.Id
                FROM Customer c
                INNER JOIN PricingTier pt ON c.TenantId = pt.TenantId
                WHERE pt.IsDefault = 1 AND c.PricingTierId IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
