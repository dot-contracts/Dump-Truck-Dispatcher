using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class MergedPickupAndDeliveryNotesToNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Dispatch",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.Sql("update OrderLine set Note = null where Note = ''");
            migrationBuilder.Sql("update OrderLine set Note = Left(Coalesce(Note + CHAR(13)+CHAR(10), '') + Coalesce(PickupNote + CHAR(13)+CHAR(10), '') + Coalesce(DeliveryNote, ''), 1000)");
            migrationBuilder.Sql("update Dispatch set Note = Left(Coalesce(PickupNote + CHAR(13)+CHAR(10), '') + Coalesce(DeliveryNote, ''), 1000)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Dispatch");
        }
    }
}
