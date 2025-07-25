using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedDismissedCustomerNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DismissedCustomerNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerNotificationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissedCustomerNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DismissedCustomerNotification_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DismissedCustomerNotification_CustomerNotification_CustomerNotificationId",
                        column: x => x.CustomerNotificationId,
                        principalTable: "CustomerNotification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DismissedCustomerNotification_CustomerNotificationId",
                table: "DismissedCustomerNotification",
                column: "CustomerNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissedCustomerNotification_UserId",
                table: "DismissedCustomerNotification",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DismissedCustomerNotification");
        }
    }
}
