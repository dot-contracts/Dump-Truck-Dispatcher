using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedIncludeProductionPayToPayStatements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeProductionPay",
                table: "PayStatement",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("Update PayStatement set IncludeProductionPay = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeProductionPay",
                table: "PayStatement");
        }
    }
}
