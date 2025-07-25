using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedSupplierContactsToLocationContactsStep1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierContact_Location_LocationId",
                table: "SupplierContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.SupplierContact", //"PK_SupplierContact",
                table: "SupplierContact");

            migrationBuilder.RenameTable(
                name: "SupplierContact",
                newName: "LocationContact");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierContact_LocationId",
                table: "LocationContact",
                newName: "IX_LocationContact_LocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationContact",
                table: "LocationContact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationContact_Location_LocationId",
                table: "LocationContact",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationContact_Location_LocationId",
                table: "LocationContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationContact",
                table: "LocationContact");

            migrationBuilder.RenameTable(
                name: "LocationContact",
                newName: "SupplierContact");

            migrationBuilder.RenameIndex(
                name: "IX_LocationContact_LocationId",
                table: "SupplierContact",
                newName: "IX_SupplierContact_LocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupplierContact",
                table: "SupplierContact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierContact_Location_LocationId",
                table: "SupplierContact",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
