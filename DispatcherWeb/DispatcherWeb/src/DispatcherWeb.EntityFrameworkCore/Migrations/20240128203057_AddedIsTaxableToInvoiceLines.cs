using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsTaxableToInvoiceLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreightRateOverridden",
                table: "InvoiceLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxable",
                table: "InvoiceLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("Update InvoiceLine set IsTaxable = 1");

            migrationBuilder.Sql(@"
                Update il set IsTaxable = s.IsTaxable
                from InvoiceLine il
                inner join [Service] s on s.Id = il.ItemId
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreightRateOverridden",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "IsTaxable",
                table: "InvoiceLine");
        }
    }
}
