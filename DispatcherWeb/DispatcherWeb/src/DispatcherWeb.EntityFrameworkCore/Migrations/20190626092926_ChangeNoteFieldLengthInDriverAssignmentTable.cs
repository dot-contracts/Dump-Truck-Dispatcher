using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ChangeNoteFieldLengthInDriverAssignmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string sql = "update DriverAssignment set Note = SUBSTRING(Note, 0, 600) where len(Note) > 600";
            migrationBuilder.Sql(sql);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "DriverAssignment",
                maxLength: 600,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "DriverAssignment",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 600,
                oldNullable: true);
        }
    }
}
