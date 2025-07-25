using DispatcherWeb.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedCustomerPortal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomerContact",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomerPortalAccess",
                table: "CustomerContact",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CustomerContactId",
                table: "AbpUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_CustomerContactId",
                table: "AbpUsers",
                column: "CustomerContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_CustomerContact_CustomerContactId",
                table: "AbpUsers",
                column: "CustomerContactId",
                principalTable: "CustomerContact",
                principalColumn: "Id");

            var sql = this.ReadSql();
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_CustomerContact_CustomerContactId",
                table: "AbpUsers");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_CustomerContactId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "HasCustomerPortalAccess",
                table: "CustomerContact");

            migrationBuilder.DropColumn(
                name: "CustomerContactId",
                table: "AbpUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomerContact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);
        }
    }
}
