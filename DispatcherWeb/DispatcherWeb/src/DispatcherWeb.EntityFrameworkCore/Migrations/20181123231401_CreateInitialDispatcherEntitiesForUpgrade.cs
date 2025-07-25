using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class CreateInitialDispatcherEntitiesForUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignaturePictureId",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AbpUsers",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BackgroundJobHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Job = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: true),
                    Completed = table.Column<bool>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJobHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(maxLength: 30, nullable: true),
                    Address1 = table.Column<string>(maxLength: 128, nullable: true),
                    Address2 = table.Column<string>(maxLength: 128, nullable: true),
                    City = table.Column<string>(maxLength: 100, nullable: true),
                    State = table.Column<string>(maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(maxLength: 15, nullable: true),
                    CreditCardToken = table.Column<string>(nullable: true),
                    CreditCardFirstName = table.Column<string>(nullable: true),
                    CreditCardLastName = table.Column<string>(nullable: true),
                    CreditCardStreetAddress = table.Column<string>(nullable: true),
                    CreditCardZipCode = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHauler",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    StreetAddress1 = table.Column<string>(maxLength: 200, nullable: true),
                    StreetAddress2 = table.Column<string>(maxLength: 200, nullable: true),
                    City = table.Column<string>(maxLength: 100, nullable: true),
                    State = table.Column<string>(maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(maxLength: 12, nullable: true),
                    AccountNumber = table.Column<string>(maxLength: 30, nullable: true),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHauler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Office",
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
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    TruckColor = table.Column<string>(maxLength: 7, nullable: false),
                    BaseFuelCost = table.Column<decimal>(type: "money", nullable: true),
                    CopyDeliverToLoadAtChargeTo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Office", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledReport",
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
                    ReportType = table.Column<int>(nullable: false),
                    SendTo = table.Column<string>(maxLength: 2000, nullable: true),
                    ReportFormat = table.Column<int>(nullable: false),
                    ScheduleTime = table.Column<TimeSpan>(nullable: false),
                    SendOnDaysOfWeek = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Service",
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
                    UnitOfMeasureId = table.Column<int>(nullable: true),
                    Service = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 150, nullable: true),
                    BillPerTon = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    PredefinedServiceKind = table.Column<int>(nullable: true),
                    IsService = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierCategory",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    PredefinedSupplierCategoryKind = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackableEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    CalculatedDeliveryStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackableEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackableEmails_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasure",
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
                    Name = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleService",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    RecommendedTimeInterval = table.Column<int>(nullable: false),
                    RecommendedMileageInterval = table.Column<int>(nullable: false),
                    WarningDays = table.Column<int>(nullable: false),
                    WarningMiles = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleService", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleServiceType",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleServiceType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerContact",
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
                    CustomerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(maxLength: 15, nullable: true),
                    Fax = table.Column<string>(maxLength: 15, nullable: true),
                    Email = table.Column<string>(maxLength: 200, nullable: true),
                    Title = table.Column<string>(maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContact_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHaulerContact",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Title = table.Column<string>(maxLength: 40, nullable: true),
                    Phone = table.Column<string>(maxLength: 20, nullable: true),
                    Email = table.Column<string>(maxLength: 120, nullable: true),
                    LeaseHaulerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerContact_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CannedText",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    OfficeId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannedText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CannedText_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Driver",
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
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    OfficeId = table.Column<int>(nullable: true),
                    IsInactive = table.Column<bool>(nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 256, nullable: true),
                    CellPhoneNumber = table.Column<string>(maxLength: 15, nullable: true),
                    OrderNotifyPreferredFormat = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Driver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Driver_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(nullable: true),
                    StreetAddress = table.Column<string>(maxLength: 200, nullable: true),
                    City = table.Column<string>(maxLength: 100, nullable: true),
                    State = table.Column<string>(maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(maxLength: 12, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10, 6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10, 6)", nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    PredefinedSupplierKind = table.Column<int>(nullable: true),
                    Abbreviation = table.Column<string>(maxLength: 10, nullable: true),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supplier_SupplierCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SupplierCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackableEmailReceivers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TrackableEmailId = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    IsSender = table.Column<bool>(nullable: false),
                    ReceiverKind = table.Column<int>(nullable: false),
                    DeliveryStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackableEmailReceivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackableEmailReceivers_TrackableEmails_TrackableEmailId",
                        column: x => x.TrackableEmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfficeServicePrice",
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
                    ServiceId = table.Column<int>(nullable: false),
                    OfficeId = table.Column<int>(nullable: false),
                    UnitOfMeasureId = table.Column<int>(nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    FreightRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    Designation = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OfficeServicePrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeServicePrice_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeServicePrice_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeServicePrice_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleServiceDocument",
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
                    VehicleServiceId = table.Column<int>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleServiceDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleServiceDocument_VehicleService_VehicleServiceId",
                        column: x => x.VehicleServiceId,
                        principalTable: "VehicleService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Truck",
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
                    TruckCode = table.Column<string>(maxLength: 50, nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    Category = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsOutOfService = table.Column<bool>(nullable: false),
                    DefaultDriverId = table.Column<int>(nullable: true),
                    CurrentMileage = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: true),
                    Make = table.Column<string>(maxLength: 50, nullable: true),
                    Model = table.Column<string>(maxLength: 50, nullable: true),
                    Vin = table.Column<string>(maxLength: 50, nullable: true),
                    Plate = table.Column<string>(maxLength: 20, nullable: true),
                    PlateExpiration = table.Column<DateTime>(nullable: true),
                    CargoCapacity = table.Column<int>(nullable: true),
                    FuelType = table.Column<int>(nullable: true),
                    FuelCapacity = table.Column<int>(nullable: true),
                    SteerTires = table.Column<string>(maxLength: 50, nullable: true),
                    DriveAxleTires = table.Column<string>(maxLength: 50, nullable: true),
                    DropAxleTires = table.Column<string>(maxLength: 50, nullable: true),
                    TrailerTires = table.Column<string>(maxLength: 50, nullable: true),
                    Transmission = table.Column<string>(maxLength: 50, nullable: true),
                    Engine = table.Column<string>(maxLength: 50, nullable: true),
                    RearEnd = table.Column<string>(maxLength: 50, nullable: true),
                    InsurancePolicyNumber = table.Column<string>(maxLength: 50, nullable: true),
                    InsuranceValidUntil = table.Column<DateTime>(nullable: true),
                    PurchaseDate = table.Column<DateTime>(nullable: true),
                    PurchasePrice = table.Column<decimal>(nullable: true),
                    InServiceDate = table.Column<DateTime>(nullable: true),
                    SoldDate = table.Column<DateTime>(nullable: true),
                    SoldPrice = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Truck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Truck_Driver_DefaultDriverId",
                        column: x => x.DefaultDriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Truck_Office_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project",
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
                    CustomerId = table.Column<int>(nullable: true),
                    ContactId = table.Column<int>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    PONumber = table.Column<string>(maxLength: 20, nullable: true),
                    DeliverTo = table.Column<string>(maxLength: 500, nullable: true),
                    LoadAt = table.Column<string>(maxLength: 500, nullable: true),
                    Location = table.Column<string>(maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10, 6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10, 6)", nullable: true),
                    ChargeTo = table.Column<string>(maxLength: 500, nullable: true),
                    Directions = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_CustomerContact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CustomerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Project_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Project_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContact",
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
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Phone = table.Column<string>(maxLength: 20, nullable: true),
                    Email = table.Column<string>(maxLength: 120, nullable: true),
                    Title = table.Column<string>(maxLength: 40, nullable: true),
                    SupplierId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SupplierContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContact_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackableEmailEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    EventContent = table.Column<string>(nullable: true),
                    Event = table.Column<string>(nullable: true),
                    SendGridEventTimestamp = table.Column<long>(nullable: true),
                    SendGridEventId = table.Column<string>(nullable: true),
                    FailReason = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    EmailDeliveryStatus = table.Column<int>(nullable: true),
                    TrackableEmailId = table.Column<Guid>(nullable: true),
                    TrackableEmailReceiverId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackableEmailEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackableEmailEvents_TrackableEmails_TrackableEmailId",
                        column: x => x.TrackableEmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackableEmailEvents_TrackableEmailReceivers_TrackableEmailReceiverId",
                        column: x => x.TrackableEmailReceiverId,
                        principalTable: "TrackableEmailReceivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DriverAssignment",
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
                    Date = table.Column<DateTime>(nullable: false),
                    OfficeId = table.Column<int>(nullable: false),
                    TruckId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverAssignment_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverAssignment_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverAssignment_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutOfServiceHistory",
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
                    TruckId = table.Column<int>(nullable: false),
                    OutOfServiceDate = table.Column<DateTime>(nullable: false),
                    InServiceDate = table.Column<DateTime>(nullable: true),
                    Reason = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutOfServiceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutOfServiceHistory_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveMaintenance",
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
                    TruckId = table.Column<int>(nullable: false),
                    VehicleServiceId = table.Column<int>(nullable: false),
                    LastDate = table.Column<DateTime>(nullable: false),
                    LastMileage = table.Column<int>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: true),
                    DueMileage = table.Column<int>(nullable: true),
                    WarningDate = table.Column<DateTime>(nullable: true),
                    WarningMileage = table.Column<int>(nullable: true),
                    CompletedDate = table.Column<DateTime>(nullable: true),
                    CompletedMileage = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveMaintenance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenance_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenance_VehicleService_VehicleServiceId",
                        column: x => x.VehicleServiceId,
                        principalTable: "VehicleService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedTruck",
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
                    TruckId = table.Column<int>(nullable: false),
                    OfficeId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedTruck_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TruckFile",
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
                    TruckId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    FileId = table.Column<Guid>(nullable: false),
                    ThumbnailId = table.Column<Guid>(nullable: true),
                    FileName = table.Column<string>(maxLength: 500, nullable: true),
                    FileType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckFile_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrder",
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
                    VehicleServiceTypeId = table.Column<int>(nullable: true),
                    IssueDate = table.Column<DateTime>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: true),
                    CompletionDate = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    TruckId = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 500, nullable: true),
                    Odometer = table.Column<int>(nullable: false),
                    AssignedToId = table.Column<long>(nullable: true),
                    TotalLaborCost = table.Column<decimal>(type: "decimal(12, 2)", nullable: false),
                    TotalPartsCost = table.Column<decimal>(type: "decimal(12, 2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(4, 2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(5, 2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(12, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrder_AbpUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrder_VehicleServiceType_VehicleServiceTypeId",
                        column: x => x.VehicleServiceTypeId,
                        principalTable: "VehicleServiceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHaulerAgreement",
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
                    ProjectId = table.Column<int>(nullable: false),
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    LeaseHaulerContactId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 150, nullable: false)
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
                name: "ProjectHistory",
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
                    ProjectId = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<long>(nullable: true),
                    OfficeId = table.Column<int>(nullable: true),
                    Action = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectHistory", x => x.Id);
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
                    table.ForeignKey(
                        name: "FK_ProjectHistory_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectService",
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
                    ServiceId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false),
                    UnitOfMeasureId = table.Column<int>(nullable: false),
                    Designation = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    FreightRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    Quantity = table.Column<decimal>(nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectService", x => x.Id);
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
                        name: "FK_ProjectService_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectService_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quote",
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
                    ProjectId = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    ContactId = table.Column<int>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    LoadAt = table.Column<string>(maxLength: 200, nullable: true),
                    Name = table.Column<string>(maxLength: 155, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    ProposalDate = table.Column<DateTime>(nullable: true),
                    ProposalExpiryDate = table.Column<DateTime>(nullable: true),
                    InactivationDate = table.Column<DateTime>(nullable: true),
                    PONumber = table.Column<string>(maxLength: 20, nullable: true),
                    SpectrumNumber = table.Column<string>(maxLength: 20, nullable: true),
                    BaseFuelCost = table.Column<decimal>(type: "money", nullable: true),
                    DeliverTo = table.Column<string>(maxLength: 500, nullable: true),
                    ChargeTo = table.Column<string>(maxLength: 500, nullable: true),
                    Directions = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    CaptureHistory = table.Column<bool>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    LastQuoteEmailId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quote_CustomerContact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CustomerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quote_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quote_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quote_TrackableEmails_LastQuoteEmailId",
                        column: x => x.LastQuoteEmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quote_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quote_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderLine",
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
                    WorkOrderId = table.Column<int>(nullable: false),
                    VehicleServiceId = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    LaborTime = table.Column<decimal>(type: "decimal(8, 2)", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(8, 2)", nullable: true),
                    LaborRate = table.Column<decimal>(type: "decimal(6, 2)", nullable: true),
                    PartsCost = table.Column<decimal>(type: "decimal(10, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderLine_VehicleService_VehicleServiceId",
                        column: x => x.VehicleServiceId,
                        principalTable: "VehicleService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderLine_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPicture",
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
                    WorkOrderId = table.Column<int>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderPicture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderPicture_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHaulerAgreementService",
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
                    ServiceId = table.Column<int>(nullable: false),
                    LeaseHaulerAgreementId = table.Column<int>(nullable: false),
                    UnitOfMeasureId = table.Column<int>(nullable: false),
                    Designation = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: true),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerAgreementService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_LeaseHaulerAgreement_LeaseHaulerAgreementId",
                        column: x => x.LeaseHaulerAgreementId,
                        principalTable: "LeaseHaulerAgreement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Order",
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
                    DateTime = table.Column<DateTime>(nullable: true),
                    IsPending = table.Column<bool>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: true),
                    Time = table.Column<DateTime>(nullable: true),
                    SharedDateTime = table.Column<DateTime>(nullable: true),
                    CustomerId = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: true),
                    PONumber = table.Column<string>(maxLength: 20, nullable: true),
                    SpectrumNumber = table.Column<string>(maxLength: 20, nullable: true),
                    JobNumber = table.Column<string>(maxLength: 20, nullable: true),
                    ContactId = table.Column<int>(nullable: true),
                    SalesTaxRate = table.Column<decimal>(type: "money", nullable: false),
                    SalesTax = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    CODTotal = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    DeliverTo = table.Column<string>(maxLength: 500, nullable: true),
                    LoadAt = table.Column<string>(maxLength: 500, nullable: true),
                    ChargeTo = table.Column<string>(maxLength: 500, nullable: true),
                    FreightTotal = table.Column<decimal>(nullable: false),
                    MaterialTotal = table.Column<decimal>(nullable: false),
                    IsFreightTotalOverridden = table.Column<bool>(nullable: false),
                    IsMaterialTotalOverridden = table.Column<bool>(nullable: false),
                    FuelSurchargeRate = table.Column<decimal>(nullable: false),
                    FuelSurcharge = table.Column<decimal>(nullable: false),
                    Directions = table.Column<string>(nullable: true),
                    CreditCardInfo = table.Column<string>(nullable: true),
                    EncryptedInternalNotes = table.Column<string>(nullable: true),
                    HasInternalNotes = table.Column<bool>(nullable: false),
                    NumberOfTrucks = table.Column<double>(nullable: true),
                    CreditCardToken = table.Column<string>(nullable: true),
                    CreditCardFirstName = table.Column<string>(nullable: true),
                    CreditCardLastName = table.Column<string>(nullable: true),
                    CreditCardStreetAddress = table.Column<string>(nullable: true),
                    CreditCardZipCode = table.Column<string>(nullable: true),
                    AuthorizationDateTime = table.Column<DateTime>(nullable: true),
                    AuthorizationAmount = table.Column<decimal>(nullable: true),
                    AuthorizedAmount = table.Column<decimal>(nullable: true),
                    AuthorizationTransactionId = table.Column<long>(nullable: true),
                    AuthorizationCaptureDateTime = table.Column<DateTime>(nullable: true),
                    AuthorizationCaptureAmount = table.Column<decimal>(nullable: true),
                    AuthorizationCaptureSettlementAmount = table.Column<decimal>(nullable: true),
                    AuthorizationCaptureTransactionId = table.Column<long>(nullable: true),
                    AuthorizationCaptureResponse = table.Column<string>(nullable: true),
                    LocationId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: true),
                    QuoteId = table.Column<int>(nullable: true),
                    IsClosed = table.Column<bool>(nullable: false),
                    LastQuoteEmailId = table.Column<Guid>(nullable: true),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_CustomerContact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CustomerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_TrackableEmails_LastQuoteEmailId",
                        column: x => x.LastQuoteEmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Office_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Quote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteEmails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuoteId = table.Column<int>(nullable: false),
                    EmailId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteEmails_TrackableEmails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteEmails_Quote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteHistory",
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
                    QuoteId = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    ChangeType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteHistory_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteHistory_Quote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteService",
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
                    ServiceId = table.Column<int>(nullable: false),
                    QuoteId = table.Column<int>(nullable: false),
                    UnitOfMeasureId = table.Column<int>(nullable: false),
                    Designation = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    FreightRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: true),
                    Quantity = table.Column<decimal>(nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.QuoteService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteService_Quote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteService_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteService_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilledOrder",
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
                    OrderId = table.Column<int>(nullable: false),
                    OfficeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BilledOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilledOrder_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilledOrder_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderEmails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(nullable: false),
                    EmailId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderEmails_TrackableEmails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderEmails_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderLeaseHauler",
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
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    UnitOfMeasureId = table.Column<int>(nullable: false),
                    LeaseHaulerRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    Note = table.Column<string>(nullable: true),
                    IsReconciled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLeaseHauler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLeaseHauler_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderLine",
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
                    LineNumber = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    MaterialPricePerUnit = table.Column<decimal>(nullable: true),
                    FreightPricePerUnit = table.Column<decimal>(nullable: true),
                    IsMaterialPricePerUnitOverridden = table.Column<bool>(nullable: false),
                    IsFreightPricePerUnitOverridden = table.Column<bool>(nullable: false),
                    ServiceId = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: true),
                    UnitOfMeasureId = table.Column<int>(nullable: false),
                    Designation = table.Column<int>(nullable: false),
                    MaterialPrice = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    MaterialActualPrice = table.Column<decimal>(type: "money", nullable: false),
                    FreightPrice = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    Note = table.Column<string>(nullable: true),
                    Loads = table.Column<int>(nullable: true),
                    EstimatedAmount = table.Column<decimal>(nullable: true),
                    TimeOnJob = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLine_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLine_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLine_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLine_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderTruck",
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
                    TruckId = table.Column<int>(nullable: false),
                    Utilization = table.Column<decimal>(nullable: false),
                    ParentOrderTruckId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTruck_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTruck_OrderTruck_ParentOrderTruckId",
                        column: x => x.ParentOrderTruckId,
                        principalTable: "OrderTruck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedOrder",
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
                    OfficeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedOrder_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedOrder_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteFieldDiff",
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
                    QuoteHistoryRecordId = table.Column<int>(nullable: false),
                    Field = table.Column<int>(nullable: false),
                    OldId = table.Column<int>(nullable: true),
                    NewId = table.Column<int>(nullable: true),
                    OldDisplayValue = table.Column<string>(nullable: true),
                    NewDisplayValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteFieldDiff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteFieldDiff_QuoteHistory_QuoteHistoryRecordId",
                        column: x => x.QuoteHistoryRecordId,
                        principalTable: "QuoteHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderLineOfficeAmount",
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
                    OrderLineId = table.Column<int>(nullable: false),
                    OfficeId = table.Column<int>(nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLineOfficeAmount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineOfficeAmount_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLineOfficeAmount_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderLineTruck",
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
                    OrderLineId = table.Column<int>(nullable: false),
                    TruckId = table.Column<int>(nullable: false),
                    ParentOrderLineTruckId = table.Column<int>(nullable: true),
                    Utilization = table.Column<decimal>(nullable: false),
                    Sequence = table.Column<int>(nullable: false),
                    TimeOnJob = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLineTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineTruck_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderLineTruck_OrderLineTruck_ParentOrderLineTruckId",
                        column: x => x.ParentOrderLineTruckId,
                        principalTable: "OrderLineTruck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLineTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
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
                    OrderLineId = table.Column<int>(nullable: true),
                    TicketNumber = table.Column<string>(maxLength: 30, nullable: true),
                    Quantity = table.Column<decimal>(nullable: false),
                    TruckId = table.Column<int>(nullable: true),
                    TruckCode = table.Column<string>(maxLength: 50, nullable: true),
                    CarrierId = table.Column<int>(nullable: true),
                    CustomerId = table.Column<int>(nullable: true),
                    ServiceId = table.Column<int>(nullable: true),
                    UnitOfMeasureId = table.Column<int>(nullable: true),
                    TicketDate = table.Column<DateTime>(nullable: true),
                    IsBilled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticket_Customer_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_UnitOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_OfficeId",
                table: "AbpUsers",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_BilledOrder_OfficeId",
                table: "BilledOrder",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_BilledOrder_OrderId",
                table: "BilledOrder",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedText_OfficeId",
                table: "CannedText",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContact_CustomerId",
                table: "CustomerContact",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_OfficeId",
                table: "Driver",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAssignment_DriverId",
                table: "DriverAssignment",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAssignment_OfficeId",
                table: "DriverAssignment",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAssignment_TruckId",
                table: "DriverAssignment",
                column: "TruckId");

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
                name: "IX_LeaseHaulerAgreementService_LeaseHaulerAgreementId",
                table: "LeaseHaulerAgreementService",
                column: "LeaseHaulerAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_ServiceId",
                table: "LeaseHaulerAgreementService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_SupplierId",
                table: "LeaseHaulerAgreementService",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_UnitOfMeasureId",
                table: "LeaseHaulerAgreementService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerContact_LeaseHaulerId",
                table: "LeaseHaulerContact",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeServicePrice_OfficeId",
                table: "OfficeServicePrice",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeServicePrice_ServiceId",
                table: "OfficeServicePrice",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeServicePrice_UnitOfMeasureId",
                table: "OfficeServicePrice",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ContactId",
                table: "Order",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CreatorUserId",
                table: "Order",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                table: "Order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_LastQuoteEmailId",
                table: "Order",
                column: "LastQuoteEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_LocationId",
                table: "Order",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ProjectId",
                table: "Order",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_QuoteId",
                table: "Order",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SupplierId",
                table: "Order",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEmails_EmailId",
                table: "OrderEmails",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEmails_OrderId",
                table: "OrderEmails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_LeaseHaulerId",
                table: "OrderLeaseHauler",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_OrderId",
                table: "OrderLeaseHauler",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_UnitOfMeasureId",
                table: "OrderLeaseHauler",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_OrderId",
                table: "OrderLine",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_ServiceId",
                table: "OrderLine",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_SupplierId",
                table: "OrderLine",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_UnitOfMeasureId",
                table: "OrderLine",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineOfficeAmount_OfficeId",
                table: "OrderLineOfficeAmount",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineOfficeAmount_OrderLineId",
                table: "OrderLineOfficeAmount",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineTruck_OrderLineId",
                table: "OrderLineTruck",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineTruck_ParentOrderLineTruckId",
                table: "OrderLineTruck",
                column: "ParentOrderLineTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineTruck_TruckId",
                table: "OrderLineTruck",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTruck_OrderId",
                table: "OrderTruck",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTruck_ParentOrderTruckId",
                table: "OrderTruck",
                column: "ParentOrderTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTruck_TruckId",
                table: "OrderTruck",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_OutOfServiceHistory_TruckId",
                table: "OutOfServiceHistory",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenance_TruckId",
                table: "PreventiveMaintenance",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenance_VehicleServiceId",
                table: "PreventiveMaintenance",
                column: "VehicleServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ContactId",
                table: "Project",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_CustomerId",
                table: "Project",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_SupplierId",
                table: "Project",
                column: "SupplierId");

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
                name: "IX_ProjectService_ProjectId",
                table: "ProjectService",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_ServiceId",
                table: "ProjectService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_SupplierId",
                table: "ProjectService",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_UnitOfMeasureId",
                table: "ProjectService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_ContactId",
                table: "Quote",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_CreatorUserId",
                table: "Quote",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_CustomerId",
                table: "Quote",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_LastQuoteEmailId",
                table: "Quote",
                column: "LastQuoteEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_ProjectId",
                table: "Quote",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_SupplierId",
                table: "Quote",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteEmails_EmailId",
                table: "QuoteEmails",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteEmails_QuoteId",
                table: "QuoteEmails",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteFieldDiff_QuoteHistoryRecordId",
                table: "QuoteFieldDiff",
                column: "QuoteHistoryRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteHistory_CreatorUserId",
                table: "QuoteHistory",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteHistory_QuoteId",
                table: "QuoteHistory",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_QuoteId",
                table: "QuoteService",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_ServiceId",
                table: "QuoteService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_SupplierId",
                table: "QuoteService",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_UnitOfMeasureId",
                table: "QuoteService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedOrder_OfficeId",
                table: "SharedOrder",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedOrder_OrderId",
                table: "SharedOrder",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedTruck_OfficeId",
                table: "SharedTruck",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedTruck_TruckId",
                table: "SharedTruck",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_CategoryId",
                table: "Supplier",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContact_SupplierId",
                table: "SupplierContact",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CarrierId",
                table: "Ticket",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CustomerId",
                table: "Ticket",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_OrderLineId",
                table: "Ticket",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ServiceId",
                table: "Ticket",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TruckId",
                table: "Ticket",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_UnitOfMeasureId",
                table: "Ticket",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackableEmailEvents_TrackableEmailId",
                table: "TrackableEmailEvents",
                column: "TrackableEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackableEmailEvents_TrackableEmailReceiverId",
                table: "TrackableEmailEvents",
                column: "TrackableEmailReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackableEmailReceivers_TrackableEmailId",
                table: "TrackableEmailReceivers",
                column: "TrackableEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackableEmails_CreatorUserId",
                table: "TrackableEmails",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Truck_DefaultDriverId",
                table: "Truck",
                column: "DefaultDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Truck_LocationId",
                table: "Truck",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckFile_TruckId",
                table: "TruckFile",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleServiceDocument_VehicleServiceId",
                table: "VehicleServiceDocument",
                column: "VehicleServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "VehicleServiceType",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_AssignedToId",
                table: "WorkOrder",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_TruckId",
                table: "WorkOrder",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_VehicleServiceTypeId",
                table: "WorkOrder",
                column: "VehicleServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLine_VehicleServiceId",
                table: "WorkOrderLine",
                column: "VehicleServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLine_WorkOrderId",
                table: "WorkOrderLine",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPicture_WorkOrderId",
                table: "WorkOrderPicture",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_Office_OfficeId",
                table: "AbpUsers",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_Office_OfficeId",
                table: "AbpUsers");

            migrationBuilder.DropTable(
                name: "BackgroundJobHistory");

            migrationBuilder.DropTable(
                name: "BilledOrder");

            migrationBuilder.DropTable(
                name: "CannedText");

            migrationBuilder.DropTable(
                name: "DriverAssignment");

            migrationBuilder.DropTable(
                name: "LeaseHaulerAgreementService");

            migrationBuilder.DropTable(
                name: "OfficeServicePrice");

            migrationBuilder.DropTable(
                name: "OrderEmails");

            migrationBuilder.DropTable(
                name: "OrderLeaseHauler");

            migrationBuilder.DropTable(
                name: "OrderLineOfficeAmount");

            migrationBuilder.DropTable(
                name: "OrderLineTruck");

            migrationBuilder.DropTable(
                name: "OrderTruck");

            migrationBuilder.DropTable(
                name: "OutOfServiceHistory");

            migrationBuilder.DropTable(
                name: "PreventiveMaintenance");

            migrationBuilder.DropTable(
                name: "ProjectHistory");

            migrationBuilder.DropTable(
                name: "ProjectService");

            migrationBuilder.DropTable(
                name: "QuoteEmails");

            migrationBuilder.DropTable(
                name: "QuoteFieldDiff");

            migrationBuilder.DropTable(
                name: "QuoteService");

            migrationBuilder.DropTable(
                name: "ScheduledReport");

            migrationBuilder.DropTable(
                name: "SharedOrder");

            migrationBuilder.DropTable(
                name: "SharedTruck");

            migrationBuilder.DropTable(
                name: "SupplierContact");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "TrackableEmailEvents");

            migrationBuilder.DropTable(
                name: "TruckFile");

            migrationBuilder.DropTable(
                name: "VehicleServiceDocument");

            migrationBuilder.DropTable(
                name: "WorkOrderLine");

            migrationBuilder.DropTable(
                name: "WorkOrderPicture");

            migrationBuilder.DropTable(
                name: "LeaseHaulerAgreement");

            migrationBuilder.DropTable(
                name: "QuoteHistory");

            migrationBuilder.DropTable(
                name: "OrderLine");

            migrationBuilder.DropTable(
                name: "TrackableEmailReceivers");

            migrationBuilder.DropTable(
                name: "VehicleService");

            migrationBuilder.DropTable(
                name: "WorkOrder");

            migrationBuilder.DropTable(
                name: "LeaseHaulerContact");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "UnitOfMeasure");

            migrationBuilder.DropTable(
                name: "Truck");

            migrationBuilder.DropTable(
                name: "VehicleServiceType");

            migrationBuilder.DropTable(
                name: "LeaseHauler");

            migrationBuilder.DropTable(
                name: "Quote");

            migrationBuilder.DropTable(
                name: "Driver");

            migrationBuilder.DropTable(
                name: "TrackableEmails");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Office");

            migrationBuilder.DropTable(
                name: "CustomerContact");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "SupplierCategory");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_OfficeId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "SignaturePictureId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AbpUsers");
        }
    }
}
