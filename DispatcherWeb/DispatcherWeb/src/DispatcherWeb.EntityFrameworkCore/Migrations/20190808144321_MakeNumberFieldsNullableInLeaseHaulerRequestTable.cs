using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class MakeNumberFieldsNullableInLeaseHaulerRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Scheduled",
                table: "LeaseHaulerRequest",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Available",
                table: "LeaseHaulerRequest",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Approved",
                table: "LeaseHaulerRequest",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Scheduled",
                table: "LeaseHaulerRequest",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Available",
                table: "LeaseHaulerRequest",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Approved",
                table: "LeaseHaulerRequest",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
