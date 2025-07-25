using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedMaxLengthToNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update WorkOrderLine set Note = Left(Note, 1000) where Len(Note) > 1000");
            migrationBuilder.Sql("update QuoteService set Note = Left(Note, 1000) where Len(Note) > 1000");
            migrationBuilder.Sql("update ProjectService set Note = Left(Note, 1000) where Len(Note) > 1000");
            migrationBuilder.Sql("update OrderLine set Note = Left(Note, 1000) where Len(Note) > 1000");
            migrationBuilder.Sql("update OrderLeaseHauler set Note = Left(Note, 1000) where Len(Note) > 1000");
            migrationBuilder.Sql("update LeaseHaulerAgreementService set Note = Left(Note, 1000) where Len(Note) > 1000");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "WorkOrderLine",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "QuoteService",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ProjectService",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "OrderLine",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "OrderLeaseHauler",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "LeaseHaulerAgreementService",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "WorkOrderLine",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "QuoteService",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ProjectService",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "OrderLine",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "OrderLeaseHauler",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "LeaseHaulerAgreementService",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
