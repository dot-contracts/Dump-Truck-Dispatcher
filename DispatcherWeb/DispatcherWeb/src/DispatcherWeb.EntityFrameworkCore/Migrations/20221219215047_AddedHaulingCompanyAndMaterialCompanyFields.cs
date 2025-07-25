using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedHaulingCompanyAndMaterialCompanyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTicketId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyTenantId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyTicketId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyOrderLineId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyOrderLineId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyTenantId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyOrderId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyOrderId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyTenantId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "LeaseHauler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialCompanyTenantId",
                table: "Customer",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTicketId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyTenantId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyTicketId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyOrderLineId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyOrderLineId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyTenantId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyOrderId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyOrderId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyTenantId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MaterialCompanyTenantId",
                table: "Customer");
        }
    }
}
