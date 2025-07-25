using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class UpdatedLuckStoneColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "CustomerTotal",
                table: "LuckStoneEarnings");

            migrationBuilder.RenameColumn(
                name: "StoneRate",
                table: "LuckStoneEarnings",
                newName: "HaulPaymentRate");

            migrationBuilder.RenameColumn(
                name: "StoneAmount",
                table: "LuckStoneEarnings",
                newName: "HaulPayment");

            migrationBuilder.RenameColumn(
                name: "SalesTaxAmount",
                table: "LuckStoneEarnings",
                newName: "FscAmount");

            migrationBuilder.RenameColumn(
                name: "CustomerPONumber",
                table: "LuckStoneEarnings",
                newName: "HaulerRef");

            migrationBuilder.AddColumn<string>(
                name: "HaulPaymentRateUom",
                table: "LuckStoneEarnings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HaulPaymentRateUom",
                table: "LuckStoneEarnings");

            migrationBuilder.RenameColumn(
                name: "HaulerRef",
                table: "LuckStoneEarnings",
                newName: "CustomerPONumber");

            migrationBuilder.RenameColumn(
                name: "HaulPaymentRate",
                table: "LuckStoneEarnings",
                newName: "StoneRate");

            migrationBuilder.RenameColumn(
                name: "HaulPayment",
                table: "LuckStoneEarnings",
                newName: "StoneAmount");

            migrationBuilder.RenameColumn(
                name: "FscAmount",
                table: "LuckStoneEarnings",
                newName: "SalesTaxAmount");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "LuckStoneEarnings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomerTotal",
                table: "LuckStoneEarnings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
