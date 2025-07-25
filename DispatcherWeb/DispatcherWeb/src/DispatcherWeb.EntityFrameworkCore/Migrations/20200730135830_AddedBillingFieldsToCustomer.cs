using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedBillingFieldsToCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingAddress1",
                table: "Customer",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddress2",
                table: "Customer",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                table: "Customer",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountryCode",
                table: "Customer",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingState",
                table: "Customer",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingZipCode",
                table: "Customer",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceEmail",
                table: "Customer",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredDeliveryMethod",
                table: "Customer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Terms",
                table: "Customer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingAddress1",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "BillingAddress2",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "BillingCountryCode",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "BillingState",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "BillingZipCode",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "InvoiceEmail",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PreferredDeliveryMethod",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Terms",
                table: "Customer");
        }
    }
}
