using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedJobNumberAndPoNumberToInvoices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PoNumber",
                table: "Receipt",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JobNumber",
                table: "Receipt",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "Invoice",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PoNumber",
                table: "Invoice",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "PoNumber",
                table: "Invoice");

            migrationBuilder.AlterColumn<string>(
                name: "PoNumber",
                table: "Receipt",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JobNumber",
                table: "Receipt",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
