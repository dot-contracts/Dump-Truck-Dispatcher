using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class SplitUomsIntoMaterialAndFreight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "QuoteService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "QuoteService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "QuoteService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "ProjectService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "ProjectService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "ProjectService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "OrderLeaseHauler",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "OrderLeaseHauler",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "OrderLeaseHauler",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "OfficeServicePrice",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "OfficeServicePrice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "OfficeServicePrice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "LeaseHaulerAgreementService",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "LeaseHaulerAgreementService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "LeaseHaulerAgreementService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"update QuoteService set FreightUomId = UnitOfMeasureId, MaterialUomId = UnitOfMeasureId");
            migrationBuilder.Sql(@"update ProjectService set FreightUomId = UnitOfMeasureId, MaterialUomId = UnitOfMeasureId");
            migrationBuilder.Sql(@"update OrderLeaseHauler set FreightUomId = UnitOfMeasureId, MaterialUomId = UnitOfMeasureId");
            migrationBuilder.Sql(@"update OfficeServicePrice set FreightUomId = UnitOfMeasureId, MaterialUomId = UnitOfMeasureId");
            migrationBuilder.Sql(@"update LeaseHaulerAgreementService set FreightUomId = UnitOfMeasureId, MaterialUomId = UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_FreightUomId",
                table: "QuoteService",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_MaterialUomId",
                table: "QuoteService",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_FreightUomId",
                table: "ProjectService",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_MaterialUomId",
                table: "ProjectService",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_FreightUomId",
                table: "OrderLeaseHauler",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_MaterialUomId",
                table: "OrderLeaseHauler",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeServicePrice_FreightUomId",
                table: "OfficeServicePrice",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeServicePrice_MaterialUomId",
                table: "OfficeServicePrice",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_FreightUomId",
                table: "LeaseHaulerAgreementService",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_MaterialUomId",
                table: "LeaseHaulerAgreementService",
                column: "MaterialUomId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_FreightUomId",
                table: "LeaseHaulerAgreementService",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_MaterialUomId",
                table: "LeaseHaulerAgreementService",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_FreightUomId",
                table: "OfficeServicePrice",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_MaterialUomId",
                table: "OfficeServicePrice",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLeaseHauler_UnitOfMeasure_FreightUomId",
                table: "OrderLeaseHauler",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLeaseHauler_UnitOfMeasure_MaterialUomId",
                table: "OrderLeaseHauler",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_UnitOfMeasure_FreightUomId",
                table: "ProjectService",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_UnitOfMeasure_MaterialUomId",
                table: "ProjectService",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_FreightUomId",
                table: "QuoteService",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_MaterialUomId",
                table: "QuoteService",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_FreightUomId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_MaterialUomId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_FreightUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_MaterialUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLeaseHauler_UnitOfMeasure_FreightUomId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLeaseHauler_UnitOfMeasure_MaterialUomId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_UnitOfMeasure_FreightUomId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_UnitOfMeasure_MaterialUomId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_FreightUomId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_MaterialUomId",
                table: "QuoteService");

            migrationBuilder.DropIndex(
                name: "IX_QuoteService_FreightUomId",
                table: "QuoteService");

            migrationBuilder.DropIndex(
                name: "IX_QuoteService_MaterialUomId",
                table: "QuoteService");

            migrationBuilder.DropIndex(
                name: "IX_ProjectService_FreightUomId",
                table: "ProjectService");

            migrationBuilder.DropIndex(
                name: "IX_ProjectService_MaterialUomId",
                table: "ProjectService");

            migrationBuilder.DropIndex(
                name: "IX_OrderLeaseHauler_FreightUomId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropIndex(
                name: "IX_OrderLeaseHauler_MaterialUomId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropIndex(
                name: "IX_OfficeServicePrice_FreightUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropIndex(
                name: "IX_OfficeServicePrice_MaterialUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerAgreementService_FreightUomId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerAgreementService_MaterialUomId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "QuoteService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "ProjectService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "OrderLeaseHauler",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "OfficeServicePrice",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "LeaseHaulerAgreementService",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
