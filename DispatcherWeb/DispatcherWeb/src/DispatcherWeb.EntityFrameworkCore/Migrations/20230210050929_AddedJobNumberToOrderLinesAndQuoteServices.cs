using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedJobNumberToOrderLinesAndQuoteServices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "QuoteService",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "OrderLine",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JobNumber",
                table: "Invoice",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.Sql(@"
                update ol set JobNumber = o.JobNumber
                from OrderLine ol
                inner join [Order] o on o.Id = ol.OrderId
                where o.JobNumber is not null
            ");

            migrationBuilder.Sql(@"
                update ql set JobNumber = q.JobNumber
                from QuoteService ql
                inner join Quote q on q.Id = ql.QuoteId
                where q.JobNumber is not null
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "OrderLine");

            migrationBuilder.AlterColumn<string>(
                name: "JobNumber",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
