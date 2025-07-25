using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedQuickbooksFieldsToInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "QuickbooksExportDate",
                table: "Invoice",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuickbooksInvoiceId",
                table: "Invoice",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuickbooksExportDate",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "QuickbooksInvoiceId",
                table: "Invoice");
        }
    }
}
