using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedFirstStaggeredTimeOnJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstStaggeredTimeOnJob",
                table: "OrderLine",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstStaggeredTimeOnJob",
                table: "OrderLine");
        }
    }
}
