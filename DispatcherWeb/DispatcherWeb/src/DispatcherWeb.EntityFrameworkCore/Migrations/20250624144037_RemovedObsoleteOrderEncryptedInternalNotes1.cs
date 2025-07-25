using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemovedObsoleteOrderEncryptedInternalNotes1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear data:
            migrationBuilder.Sql("UPDATE [Order] SET EncryptedInternalNotes = NULL, HasInternalNotes = 0 WHERE EncryptedInternalNotes IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "EncryptedInternalNotes",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "HasInternalNotes",
                table: "Order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedInternalNotes",
                table: "Order",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasInternalNotes",
                table: "Order",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
