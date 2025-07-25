using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedDriverIsPaidForLoadBasedOnToPayStatementTicket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverIsPaidForLoadBasedOn",
                table: "PayStatementTicket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update PayStatementTicket set DriverIsPaidForLoadBasedOn = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverIsPaidForLoadBasedOn",
                table: "PayStatementTicket");
        }
    }
}
