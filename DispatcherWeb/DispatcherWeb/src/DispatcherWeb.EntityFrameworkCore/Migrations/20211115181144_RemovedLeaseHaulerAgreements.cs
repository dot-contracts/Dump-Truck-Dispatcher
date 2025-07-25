using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedLeaseHaulerAgreements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from LeaseHaulerAgreementService");
            migrationBuilder.Sql("Delete from LeaseHaulerAgreement");

            migrationBuilder.DropTable(
                name: "LeaseHaulerAgreementService");

            migrationBuilder.DropTable(
                name: "LeaseHaulerAgreement");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaseHaulerAgreement",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LeaseHaulerContactId = table.Column<int>(nullable: true),
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    ProjectId = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerAgreement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreement_LeaseHaulerContact_LeaseHaulerContactId",
                        column: x => x.LeaseHaulerContactId,
                        principalTable: "LeaseHaulerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreement_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreement_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHaulerAgreementService",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    Designation = table.Column<int>(nullable: false),
                    FreightUomId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LeaseHaulerAgreementId = table.Column<int>(nullable: false),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    LocationId = table.Column<int>(nullable: true),
                    MaterialUomId = table.Column<int>(nullable: true),
                    Note = table.Column<string>(maxLength: 1000, nullable: true),
                    ServiceId = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerAgreementService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_FreightUomId",
                        column: x => x.FreightUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_LeaseHaulerAgreement_LeaseHaulerAgreementId",
                        column: x => x.LeaseHaulerAgreementId,
                        principalTable: "LeaseHaulerAgreement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_MaterialUomId",
                        column: x => x.MaterialUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreement_LeaseHaulerContactId",
                table: "LeaseHaulerAgreement",
                column: "LeaseHaulerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreement_LeaseHaulerId",
                table: "LeaseHaulerAgreement",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreement_ProjectId",
                table: "LeaseHaulerAgreement",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_FreightUomId",
                table: "LeaseHaulerAgreementService",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_LeaseHaulerAgreementId",
                table: "LeaseHaulerAgreementService",
                column: "LeaseHaulerAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_LocationId",
                table: "LeaseHaulerAgreementService",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_MaterialUomId",
                table: "LeaseHaulerAgreementService",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_ServiceId",
                table: "LeaseHaulerAgreementService",
                column: "ServiceId");
        }
    }
}
