using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedLeaseHaulerRelatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeptOfTransportationNumber",
                table: "LeaseHauler",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EinOrTin",
                table: "LeaseHauler",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "LeaseHauler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingAddress1",
                table: "LeaseHauler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingAddress2",
                table: "LeaseHauler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingCity",
                table: "LeaseHauler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingCountryCode",
                table: "LeaseHauler",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingState",
                table: "LeaseHauler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingZipCode",
                table: "LeaseHauler",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotorCarrierNumber",
                table: "LeaseHauler",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminationDate",
                table: "LeaseHauler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InsuranceType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceType", x => x.Id);
                });

            migrationBuilder.Sql($"insert into InsuranceType (Name) values ('Automotive Liability'), ('General Liability'), ('Workers Comp')");

            migrationBuilder.CreateTable(
                name: "Insurance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LeaseHaulerId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InsuranceTypeId = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedBy = table.Column<string>(type: "nvarchar(63)", maxLength: 63, nullable: true),
                    IssuerPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    BrokerName = table.Column<string>(type: "nvarchar(63)", maxLength: 63, nullable: true),
                    BrokerPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CoverageLimit = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(511)", maxLength: 511, nullable: true),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Insurance_InsuranceType_InsuranceTypeId",
                        column: x => x.InsuranceTypeId,
                        principalTable: "InsuranceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Insurance_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Insurance_InsuranceTypeId",
                table: "Insurance",
                column: "InsuranceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Insurance_LeaseHaulerId",
                table: "Insurance",
                column: "LeaseHaulerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Insurance");

            migrationBuilder.DropTable(
                name: "InsuranceType");

            migrationBuilder.DropColumn(
                name: "DeptOfTransportationNumber",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "EinOrTin",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MailingAddress1",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MailingAddress2",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MailingCity",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MailingCountryCode",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MailingState",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MailingZipCode",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "MotorCarrierNumber",
                table: "LeaseHauler");

            migrationBuilder.DropColumn(
                name: "TerminationDate",
                table: "LeaseHauler");
        }
    }
}
