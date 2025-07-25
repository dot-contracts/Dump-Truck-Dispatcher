using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedServiceToItemStep2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items' WHERE Name = 'Pages.Services';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.HaulZones' WHERE Name = 'Pages.Services.HaulZones';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.Merge' WHERE Name = 'Pages.Services.Merge';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.PricingTiers' WHERE Name = 'Pages.Services.PricingTiers';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.PricingTiers.EditPricingTier' WHERE Name = 'Pages.Services.PricingTiers.EditPricingTier';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.TaxEntities' WHERE Name = 'Pages.Services.TaxEntities';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.TaxRates' WHERE Name = 'Pages.Services.TaxRates';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Items.TaxRates.Edit' WHERE Name = 'Pages.Services.TaxRates.Edit';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Imports.Items' WHERE Name = 'Pages.Imports.Services';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Misc.ReadItemPricing' WHERE Name = 'Pages.Misc.ReadServicePricing';");
            migrationBuilder.Sql(@"UPDATE AbpPermissions SET Name = 'Pages.Misc.SelectLists.Items' WHERE Name = 'Pages.Misc.SelectLists.Services';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
