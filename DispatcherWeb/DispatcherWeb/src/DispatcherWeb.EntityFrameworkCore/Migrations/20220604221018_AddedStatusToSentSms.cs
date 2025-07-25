using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedStatusToSentSms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SentSms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //we will run this manually
            //migrationBuilder.Sql(@"
            //    update s set Status = t.Status
            //    from SentSms s
            //    inner join TrackableSms t on t.Sid = s.Sid
            //    where s.Status != t.Status
            //");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SentSms");
        }
    }
}
