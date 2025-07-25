using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemoveWhitespaceNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update OrderLine set Note = '' where Note = CHAR(13) + CHAR(10)");
            migrationBuilder.Sql("update Dispatch set Note = '' where Note = CHAR(13) + CHAR(10)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
