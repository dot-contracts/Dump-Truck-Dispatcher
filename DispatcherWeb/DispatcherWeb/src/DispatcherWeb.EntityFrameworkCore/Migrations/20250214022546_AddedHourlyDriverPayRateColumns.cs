using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedHourlyDriverPayRateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverPayTimeClassificationId",
                table: "QuoteLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyDriverPayRate",
                table: "QuoteLine",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverPayTimeClassificationId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyDriverPayRate",
                table: "OrderLine",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLine_DriverPayTimeClassificationId",
                table: "QuoteLine",
                column: "DriverPayTimeClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_DriverPayTimeClassificationId",
                table: "OrderLine",
                column: "DriverPayTimeClassificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_TimeClassification_DriverPayTimeClassificationId",
                table: "OrderLine",
                column: "DriverPayTimeClassificationId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_TimeClassification_DriverPayTimeClassificationId",
                table: "QuoteLine",
                column: "DriverPayTimeClassificationId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_TimeClassification_DriverPayTimeClassificationId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_TimeClassification_DriverPayTimeClassificationId",
                table: "QuoteLine");

            migrationBuilder.DropIndex(
                name: "IX_QuoteLine_DriverPayTimeClassificationId",
                table: "QuoteLine");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_DriverPayTimeClassificationId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "DriverPayTimeClassificationId",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "HourlyDriverPayRate",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "DriverPayTimeClassificationId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "HourlyDriverPayRate",
                table: "OrderLine");
        }
    }
}
