using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedSentSmsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderLinesCreated",
                table: "TenantDailyHistory",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SmsSent",
                table: "TenantDailyHistory",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SentSms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    Sid = table.Column<string>(maxLength: 100, nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    FromSmsNumber = table.Column<string>(maxLength: 15, nullable: true),
                    ToSmsNumber = table.Column<string>(maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentSms", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentSms");

            migrationBuilder.DropColumn(
                name: "OrderLinesCreated",
                table: "TenantDailyHistory");

            migrationBuilder.DropColumn(
                name: "SmsSent",
                table: "TenantDailyHistory");
        }
    }
}
