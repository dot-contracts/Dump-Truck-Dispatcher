using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedPayStatementDriverDateConflictKindField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConflictKind",
                table: "PayStatementDriverDateConflict",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update PayStatementDriverDateConflict set ConflictKind = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConflictKind",
                table: "PayStatementDriverDateConflict");
        }
    }
}
