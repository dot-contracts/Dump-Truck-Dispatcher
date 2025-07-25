using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewTypeOptionsInLeaseHaulerInsurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"insert into InsuranceType (Name) values ('Motor Carrier Permit'), ('Clean Truck Check'), ('California Air Resources Board'), ('Workers Comp Insurance'), ('Alcohol & Drug Testing'), ('Other')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
