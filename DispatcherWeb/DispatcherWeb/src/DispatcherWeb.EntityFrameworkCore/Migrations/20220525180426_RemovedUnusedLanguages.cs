using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedUnusedLanguages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AbpLanguages where name not in ('en', 'es', 'es-MX')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
