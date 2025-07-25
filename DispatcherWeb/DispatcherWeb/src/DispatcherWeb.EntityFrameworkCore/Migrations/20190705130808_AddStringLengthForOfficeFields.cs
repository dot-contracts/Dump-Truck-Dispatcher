using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddStringLengthForOfficeFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //do not truncate the field before reducing the length,
            //we want it to fail and increase the field length if some key would be longer than the new limit
            migrationBuilder.AlterColumn<string>(
                name: "HeartlandSecretKey",
                table: "Office",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HeartlandPublicKey",
                table: "Office",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HeartlandSecretKey",
                table: "Office",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HeartlandPublicKey",
                table: "Office",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
