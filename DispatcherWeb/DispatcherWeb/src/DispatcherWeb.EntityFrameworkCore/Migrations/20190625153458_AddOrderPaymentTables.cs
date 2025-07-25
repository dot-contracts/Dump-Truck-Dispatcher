using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddOrderPaymentTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentHeartlandKey",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    PublicKey = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHeartlandKey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    PaymentHeartlandKeyId = table.Column<int>(nullable: true),
                    CreditCardToken = table.Column<string>(maxLength: 50, nullable: true),
                    CreditCardStreetAddress = table.Column<string>(maxLength: 500, nullable: true),
                    CreditCardZipCode = table.Column<string>(maxLength: 12, nullable: true),
                    AuthorizationUserId = table.Column<long>(nullable: true),
                    AuthorizationDateTime = table.Column<DateTime>(nullable: true),
                    AuthorizationAmount = table.Column<decimal>(nullable: true),
                    AuthorizationTransactionId = table.Column<string>(maxLength: 50, nullable: true),
                    AuthorizationCaptureUserId = table.Column<long>(nullable: true),
                    AuthorizationCaptureDateTime = table.Column<DateTime>(nullable: true),
                    AuthorizationCaptureAmount = table.Column<decimal>(nullable: true),
                    AuthorizationCaptureTransactionId = table.Column<string>(maxLength: 50, nullable: true),
                    AuthorizationCaptureResponse = table.Column<string>(maxLength: 5000, nullable: true),
                    PaymentDescription = table.Column<string>(maxLength: 1000, nullable: true),
                    IsCancelledOrRefunded = table.Column<bool>(nullable: false),
                    CancelOrRefundUserId = table.Column<long>(nullable: true),
                    CardType = table.Column<string>(maxLength: 15, nullable: true),
                    CardLast4 = table.Column<string>(maxLength: 4, nullable: true),
                    TransactionType = table.Column<string>(nullable: true),
                    BatchSummaryId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_AbpUsers_AuthorizationCaptureUserId",
                        column: x => x.AuthorizationCaptureUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_AbpUsers_AuthorizationUserId",
                        column: x => x.AuthorizationUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_AbpUsers_CancelOrRefundUserId",
                        column: x => x.CancelOrRefundUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_PaymentHeartlandKey_PaymentHeartlandKeyId",
                        column: x => x.PaymentHeartlandKeyId,
                        principalTable: "PaymentHeartlandKey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderPayment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    OrderId = table.Column<int>(nullable: false),
                    PaymentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPayment_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderPayment_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_OrderId",
                table: "OrderPayment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_PaymentId",
                table: "OrderPayment",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_AuthorizationCaptureUserId",
                table: "Payment",
                column: "AuthorizationCaptureUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_AuthorizationUserId",
                table: "Payment",
                column: "AuthorizationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_CancelOrRefundUserId",
                table: "Payment",
                column: "CancelOrRefundUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentHeartlandKeyId",
                table: "Payment",
                column: "PaymentHeartlandKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHeartlandKey_PublicKey",
                table: "PaymentHeartlandKey",
                column: "PublicKey",
                unique: true);


            migrationBuilder.Sql(@"ALTER TABLE [dbo].[Payment] ADD [OrderId] int NULL");

            migrationBuilder.Sql(@"insert into Payment
                                    (     [CreationTime]
                                          ,[IsDeleted]
                                          ,[TenantId]
                                          ,[CreditCardToken]
                                          ,[CreditCardStreetAddress]
                                          ,[CreditCardZipCode]
                                          ,[AuthorizationDateTime]
                                          ,[AuthorizationAmount]
                                          ,[AuthorizationTransactionId]
                                          ,[AuthorizationCaptureDateTime]
                                          ,[AuthorizationCaptureAmount]
                                          ,[AuthorizationCaptureTransactionId]
                                          ,[AuthorizationCaptureResponse]
                                          ,[PaymentDescription]
                                          ,[IsCancelledOrRefunded]
                                          ,[OrderId])
                                    
                                    SELECT getdate() as creationtime
                                    	  ,0 as IsDeleted
                                          ,o.[TenantId]
                                          ,o.[CreditCardToken]
                                          ,o.[CreditCardStreetAddress]
                                          ,o.[CreditCardZipCode]
                                          ,o.[AuthorizationDateTime]
                                          ,o.[AuthorizationAmount]
                                          ,o.[AuthorizationTransactionId]
                                          ,o.[AuthorizationCaptureDateTime]
                                          ,coalesce(o.[AuthorizationCaptureSettlementAmount], o.[AuthorizationCaptureAmount])
                                          ,o.[AuthorizationCaptureTransactionId]
                                          ,o.[AuthorizationCaptureResponse]
                                    	  ,Concat('OrderId: ', o.Id, ' for ', l.Name, ', Customer: ', c.Name) as PaymentDescription
                                    	  ,0 as IsCancelled
                                    	  ,o.[Id] as OrderId
                                      FROM [Order] o
                                      left join Customer c on c.Id = o.CustomerId 
                                      left join Office l on l.Id = o.LocationId
                                      where o.AuthorizationTransactionId is not null or o.AuthorizationCaptureTransactionId is not null
                                    ");

            migrationBuilder.Sql(@"insert into OrderPayment
                                    (     [CreationTime]
                                          ,[IsDeleted]
                                          ,[TenantId]
                                          ,[PaymentId]
                                          ,[OrderId])
                                    
                                    SELECT getdate() as creationtime
                                    	  ,0 as IsDeleted
                                          ,p.[TenantId]
                                          ,p.[Id]
                                          ,p.[OrderId]
                                      FROM [Payment] p where p.OrderId is not null");

            migrationBuilder.Sql(@"ALTER TABLE [dbo].[Payment] DROP COLUMN [OrderId]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderPayment");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PaymentHeartlandKey");
        }
    }
}
