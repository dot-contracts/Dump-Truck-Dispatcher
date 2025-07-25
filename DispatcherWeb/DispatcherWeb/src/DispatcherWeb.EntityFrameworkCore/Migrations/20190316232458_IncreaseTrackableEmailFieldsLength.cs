using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class IncreaseTrackableEmailFieldsLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "TrackableEmails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "TrackableEmailReceivers",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SendGridEventId",
                table: "TrackableEmailEvents",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Event",
                table: "TrackableEmailEvents",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TrackableEmails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TrackableEmailReceivers");

            migrationBuilder.AlterColumn<string>(
                name: "SendGridEventId",
                table: "TrackableEmailEvents",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Event",
                table: "TrackableEmailEvents",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 40,
                oldNullable: true);
        }
    }
}
