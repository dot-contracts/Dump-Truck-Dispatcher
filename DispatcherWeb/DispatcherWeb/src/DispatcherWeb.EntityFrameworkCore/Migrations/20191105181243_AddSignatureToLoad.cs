using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddSignatureToLoad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SignatureId",
                table: "Load",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureName",
                table: "Load",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureId",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "SignatureName",
                table: "Load");
        }
    }
}
