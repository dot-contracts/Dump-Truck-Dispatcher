using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTypeValueInInsuranceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE InsuranceType SET Type = CASE WHEN Name IN ('Automotive Liability', 'General Liability', 'Workers Comp') THEN 1 WHEN Name IN ('Motor Carrier Permit', 'Clean Truck Check', 'California Air Resources Board', 'Workers Comp Insurance', 'Alcohol & Drug Testing', 'Other')  THEN 2 ELSE Type  END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
