using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedPayRateAndOrderLineIdToEmployeeTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderLineId",
                table: "EmployeeTime",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "EmployeeTime",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTime_OrderLineId",
                table: "EmployeeTime",
                column: "OrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_OrderLine_OrderLineId",
                table: "EmployeeTime",
                column: "OrderLineId",
                principalTable: "OrderLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_OrderLine_OrderLineId",
                table: "EmployeeTime");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTime_OrderLineId",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "OrderLineId",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "EmployeeTime");
        }
    }
}
