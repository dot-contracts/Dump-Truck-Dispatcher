using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ReduceTrackableEmailFieldsLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventContent",
                table: "TrackableEmailEvents");

            string sql = "update TrackableEmails set Subject = SUBSTRING(Subject, 0, 200) where len(Subject) > 200";
            migrationBuilder.Sql(sql);
            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "TrackableEmails",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            sql = "update TrackableEmailReceivers set Email = SUBSTRING(Email, 0, 256) where len(Email) > 256";
            migrationBuilder.Sql(sql);
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TrackableEmailReceivers",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            sql = "update TrackableEmailEvents set SendGridEventId = SUBSTRING(SendGridEventId, 0, 50) where len(SendGridEventId) > 50";
            migrationBuilder.Sql(sql);
            migrationBuilder.AlterColumn<string>(
                name: "SendGridEventId",
                table: "TrackableEmailEvents",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            sql = "update TrackableEmailEvents set FailReason = SUBSTRING(FailReason, 0, 1000) where len(FailReason) > 1000";
            migrationBuilder.Sql(sql);
            migrationBuilder.AlterColumn<string>(
                name: "FailReason",
                table: "TrackableEmailEvents",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            sql = "update TrackableEmailEvents set Event = SUBSTRING(Event, 0, 20) where len(Event) > 20";
            migrationBuilder.Sql(sql);
            migrationBuilder.AlterColumn<string>(
                name: "Event",
                table: "TrackableEmailEvents",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            sql = "update TrackableEmailEvents set Email = SUBSTRING(Email, 0, 256) where len(Email) > 256";
            migrationBuilder.Sql(sql);
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TrackableEmailEvents",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "TrackableEmails",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TrackableEmailReceivers",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SendGridEventId",
                table: "TrackableEmailEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailReason",
                table: "TrackableEmailEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Event",
                table: "TrackableEmailEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TrackableEmailEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventContent",
                table: "TrackableEmailEvents",
                nullable: true);
        }
    }
}
