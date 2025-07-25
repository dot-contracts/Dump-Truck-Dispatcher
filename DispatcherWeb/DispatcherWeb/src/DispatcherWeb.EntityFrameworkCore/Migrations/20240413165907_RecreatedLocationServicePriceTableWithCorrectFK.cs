using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RecreatedLocationServicePriceTableWithCorrectFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationServicePrice_LocationService_LocationServiceId1",
                table: "LocationServicePrice");

            migrationBuilder.DropIndex(
                name: "IX_LocationServicePrice_LocationServiceId1",
                table: "LocationServicePrice");

            migrationBuilder.DropColumn(
                name: "LocationServiceId1",
                table: "LocationServicePrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationServiceId1",
                table: "LocationServicePrice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationServicePrice_LocationServiceId1",
                table: "LocationServicePrice",
                column: "LocationServiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationServicePrice_LocationService_LocationServiceId1",
                table: "LocationServicePrice",
                column: "LocationServiceId1",
                principalTable: "LocationService",
                principalColumn: "Id");
        }
    }
}
