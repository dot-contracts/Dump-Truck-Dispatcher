using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemovedProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Project_ProjectId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Quote_Project_ProjectId",
                table: "Quote");

            migrationBuilder.Sql("Truncate table [ProjectHistory]");
            migrationBuilder.DropTable(
                name: "ProjectHistory");

            migrationBuilder.Sql("Truncate table [ProjectService]");
            migrationBuilder.DropTable(
                name: "ProjectService");

            migrationBuilder.Sql("Truncate table [Project]");
            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Quote_ProjectId",
                table: "Quote");

            migrationBuilder.DropIndex(
                name: "IX_Order_ProjectId",
                table: "Order");

            //Doesn't look like this is required
            //migrationBuilder.Sql("Update [Quote] set ProjectId = null where ProjectId is not null");
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Quote");

            //Doesn't look like this is required
            //migrationBuilder.Sql("Update [Order] set ProjectId = null where ProjectId is not null");
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Order");

            migrationBuilder.Sql("Delete from [QuoteFieldDiff] where [Field] = 31"); //31 == Project
            migrationBuilder.Sql("Delete from AbpPermissions where Name = 'Pages.Projects'");
            migrationBuilder.Sql("Delete from AbpPermissions where Name = 'Pages.Misc.SelectLists.Projects'");
            migrationBuilder.Sql("Update AbpSettings set Name = 'App.Quote.DefaultNotes' where Name = 'App.Project.DefaultNotes'");
            migrationBuilder.Sql("Delete from AbpFeatures where Name = 'App.AllowProjects'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Quote",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChargeTo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Directions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectHistory_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHistory_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHistory_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliverToId = table.Column<int>(type: "int", nullable: true),
                    FreightUomId = table.Column<int>(type: "int", nullable: true),
                    LoadAtId = table.Column<int>(type: "int", nullable: true),
                    MaterialUomId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Designation = table.Column<int>(type: "int", nullable: false),
                    FreightQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FreightRate = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    MaterialQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectService_Location_DeliverToId",
                        column: x => x.DeliverToId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectService_Location_LoadAtId",
                        column: x => x.LoadAtId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectService_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectService_UnitOfMeasure_FreightUomId",
                        column: x => x.FreightUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectService_UnitOfMeasure_MaterialUomId",
                        column: x => x.MaterialUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quote_ProjectId",
                table: "Quote",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ProjectId",
                table: "Order",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHistory_OfficeId",
                table: "ProjectHistory",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHistory_ProjectId",
                table: "ProjectHistory",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHistory_UserId",
                table: "ProjectHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_DeliverToId",
                table: "ProjectService",
                column: "DeliverToId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_FreightUomId",
                table: "ProjectService",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_LoadAtId",
                table: "ProjectService",
                column: "LoadAtId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_MaterialUomId",
                table: "ProjectService",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_ProjectId",
                table: "ProjectService",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_ServiceId",
                table: "ProjectService",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Project_ProjectId",
                table: "Order",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quote_Project_ProjectId",
                table: "Quote",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
