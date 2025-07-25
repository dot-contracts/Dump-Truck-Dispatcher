using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedTicketNumberToLuckStoneEarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "LuckStoneEarnings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            //This was generated automatically but doesn't work, throws an exception:
            //'To change the IDENTITY property of a column, the column needs to be dropped and recreated.'
            //migrationBuilder.AlterColumn<int>(
            //    name: "Id",
            //    table: "LuckStoneEarnings",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "NewId",
                table: "LuckStoneEarnings",
                type: "int",
                nullable: false
                //,
                //defaultValue: 0
                )
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.Sql(@"UPDATE LuckStoneEarnings SET TicketNumber = Id");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LuckStoneEarnings",
                table: "LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LuckStoneEarnings");

            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "LuckStoneEarnings",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LuckStoneEarnings",
                table: "LuckStoneEarnings",
                column: "Id");

            //There are already existing records in LuckStoneEarnings table so we can't start ids from "1"
            migrationBuilder.Sql(@"
                DECLARE @maxId INT;
                SELECT @maxId = ISNULL(MAX([Id]), 0) FROM [LuckStoneEarnings];
                DBCC CHECKIDENT ('[LuckStoneEarnings]', RESEED, @maxId);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //doesn't work
            //migrationBuilder.AlterColumn<int>(
            //    name: "Id",
            //    table: "LuckStoneEarnings",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LuckStoneEarnings",
                table: "LuckStoneEarnings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "LuckStoneEarnings",
                newName: "OldId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "LuckStoneEarnings",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [LuckStoneEarnings] ON;
                UPDATE LuckStoneEarnings SET Id = TicketNumber;
                SET IDENTITY_INSERT [LuckStoneEarnings] OFF;
            ");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "LuckStoneEarnings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LuckStoneEarnings",
                table: "LuckStoneEarnings",
                column: "Id");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "LuckStoneEarnings");
        }
    }
}
