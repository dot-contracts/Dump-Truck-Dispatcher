using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedMoreDtdFieldsToTrucks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DtdTrackerDeviceTypeId",
                table: "Truck",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DtdTrackerDeviceTypeName",
                table: "Truck",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DtdTrackerPassword",
                table: "Truck",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DtdTrackerServerAddress",
                table: "Truck",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WialonDeviceType",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    DeviceCategory = table.Column<string>(maxLength: 50, nullable: true),
                    TcpPort = table.Column<int>(nullable: true),
                    UdpPort = table.Column<int>(nullable: true),
                    ServerAddress = table.Column<string>(maxLength: 259, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WialonDeviceType", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WialonDeviceType");

            migrationBuilder.DropColumn(
                name: "DtdTrackerDeviceTypeId",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "DtdTrackerDeviceTypeName",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "DtdTrackerPassword",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "DtdTrackerServerAddress",
                table: "Truck");
        }
    }
}
