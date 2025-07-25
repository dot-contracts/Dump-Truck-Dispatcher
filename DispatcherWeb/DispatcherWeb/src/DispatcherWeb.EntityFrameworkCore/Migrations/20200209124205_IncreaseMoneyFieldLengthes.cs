using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class IncreaseMoneyFieldLengthes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PartsCost",
                table: "WorkOrderLine",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LaborRate",
                table: "WorkOrderLine",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LaborCost",
                table: "WorkOrderLine",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SoldPrice",
                table: "Truck",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PurchasePrice",
                table: "Truck",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialRate",
                table: "ReceiptLine",
                type: "decimal(19, 4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightRate",
                table: "ReceiptLine",
                type: "decimal(19, 4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AuthorizationCaptureAmount",
                table: "Payment",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AuthorizationAmount",
                table: "Payment",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialPricePerUnit",
                table: "OrderLine",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightPricePerUnit",
                table: "OrderLine",
                type: "decimal(19, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 4)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PartsCost",
                table: "WorkOrderLine",
                type: "decimal(10, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LaborRate",
                table: "WorkOrderLine",
                type: "decimal(6, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LaborCost",
                table: "WorkOrderLine",
                type: "decimal(8, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SoldPrice",
                table: "Truck",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PurchasePrice",
                table: "Truck",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialRate",
                table: "ReceiptLine",
                type: "decimal(18, 4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightRate",
                table: "ReceiptLine",
                type: "decimal(18, 4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AuthorizationCaptureAmount",
                table: "Payment",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AuthorizationAmount",
                table: "Payment",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialPricePerUnit",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightPricePerUnit",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)",
                oldNullable: true);
        }
    }
}
