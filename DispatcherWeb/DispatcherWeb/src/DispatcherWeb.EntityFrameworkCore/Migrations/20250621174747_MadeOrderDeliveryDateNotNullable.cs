using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class MadeOrderDeliveryDateNotNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Order_DateTime_Shift_IsPending] ON [Order]");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [nci_msft_1_Order_3BDBA6F331B223D59C338A932FCB101E] ON [Order]");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [nci_wi_Order_6CC56B906037F3E4BBDD16869ECCAF2A] ON [Order]");

            // Set DeliveryDate to date portion of CreationTime
            migrationBuilder.Sql(
                @"UPDATE [Order] SET DeliveryDate = CAST(CreationTime AS DATE) WHERE DeliveryDate IS NULL");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveryDate",
                table: "Order",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_DateTime_Shift_IsPending",
                table: "Order",
                columns: new[] { "DeliveryDate", "Shift", "IsPending" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveryDate",
                table: "Order",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
