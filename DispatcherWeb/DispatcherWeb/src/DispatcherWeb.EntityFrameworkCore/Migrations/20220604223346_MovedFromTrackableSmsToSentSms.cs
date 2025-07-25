using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class MovedFromTrackableSmsToSentSms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SentSmsId",
                table: "DriverMessage",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverMessage_SentSmsId",
                table: "DriverMessage",
                column: "SentSmsId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverMessage_SentSms_SentSmsId",
                table: "DriverMessage",
                column: "SentSmsId",
                principalTable: "SentSms",
                principalColumn: "Id");

            //we will run this manually
            //migrationBuilder.Sql(@"
            //    update d set SentSmsId = s.Id
            //    from DriverMessage d
            //    inner join TrackableSms t on t.Id = d.TrackableSmsId
            //    inner join SentSms s on s.Sid = t.Sid
            //    where d.SentSmsId is null
            //");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverMessage_SentSms_SentSmsId",
                table: "DriverMessage");

            migrationBuilder.DropIndex(
                name: "IX_DriverMessage_SentSmsId",
                table: "DriverMessage");

            migrationBuilder.DropColumn(
                name: "SentSmsId",
                table: "DriverMessage");
        }
    }
}
