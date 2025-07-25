using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Timing;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Imports.Services
{
    public class ImportFuelUsageService_Tests : AppTestBase, IAsyncLifetime
    {
        private IImportFuelUsageAppService _importFuelUsageAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _importFuelUsageAppService = Resolve<IImportFuelUsageAppService>(new
            {
                officeResolver = Resolve<OfficeByNameResolver>(),
            });
            SubstituteServiceDependencies(_importFuelUsageAppService);
        }

        [Theory(Skip = "#14648 Historically failing")]
        [InlineData("Office1")]
        [InlineData("office1")]
        [InlineData("OFFICE1")]
        public async Task Test_Import_should_create_FuelPurchase_entity(string officeName)
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer,TicketNumber" + Environment.NewLine +
                    $"{officeName},101,05/28/2019 7:38:12 PM, 12.3456, 3.99, 1234.5, 12345" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBe(1234.5m);
            fuelPurchases[0].TicketNumber.ShouldBe("12345");
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_FuelPurchase_entity_with_DateTime_in_users_TimeZone()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    "Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer,TicketNumber" + Environment.NewLine +
                    "Office1,101,12/28/2018 7:38:12 PM, 12.3456, 3.99, 1234.5, 12345" + Environment.NewLine +
                    "Office1,101,05/28/2019 7:38:12 PM, 12.3456, 3.99, 1234.5, 12345" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);
            SubstituteSettingForUser(TimingSettingNames.TimeZone, "Israel Standard Time", new UserIdentifier(1, 1));

            // Act
            await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(2);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2018, 12, 28, 17, 38, 12));
            fuelPurchases[1].TruckId.ShouldBe(truck.Id);
            fuelPurchases[1].FuelDateTime.ShouldBe(new DateTime(2019, 5, 28, 16, 38, 12));
        }


        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_update_FuelPurchase_entity_when_it_exists()
        {
            // Arrange
            var date = new DateTime(2019, 5, 28, 19, 38, 12);
            var truck = await CreateTruck();
            var fuelPurchase = await CreateFuelPurchase(truck.Id, date, 0, 0, 0, null);
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer,TicketNumber" + Environment.NewLine +
                    $"Office1,101,05/28/2019 7:38:12 PM, 12.3456, 3.99, 1234.56, 12345" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].Id.ShouldBe(fuelPurchase.Id);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(date);
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBe(1234.6m);
            fuelPurchases[0].TicketNumber.ShouldBe("12345");
        }

        [Fact]
        public async Task Test_Import_should_create_FuelPurchase_entity_when_Office_is_empty()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @",101,05/28/2019 7:38:12 PM, 12.3456, 3.99, 1234.5" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.HasErrors.ShouldBeFalse();
            result.SkippedNumber.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBe(1234.5m);
        }

        [Fact]
        public async Task Test_Import_should_create_FuelPurchase_entity_when_Office_is_empty_and_ignore_LeaseHaulerTruck()
        {
            // Arrange
            const string truckCode = "101";
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerTruck = await CreateLeaseHaulerTruck(leaseHauler.Id, null, truckCode);
            var truck = await CreateTruck(truckCode);
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer,TicketNumber" + Environment.NewLine +
                    ",101,05/28/2019 7:38:12 PM, 12.3456, 3.99, 1234.5, 12345" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.IsImported.ShouldBeTrue();
            result.HasErrors.ShouldBeFalse();
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBe(1234.5m);
            fuelPurchases[0].TicketNumber.ShouldBe("12345");
        }



        [Fact]
        public async Task Test_Import_should_not_create_FuelPurchase_entity_if_Amount_is_empty()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, , 3.99, 1234.56" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.ImportedNumber.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_Import_should_not_create_FuelPurchase_entity_if_Amount_is_bad()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, amount, 3.99, 1234.56" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.ImportedNumber.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_Import_should_not_create_FuelPurchase_entity_when_Office_does_not_exist()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @"NotExistingOffice,101,05/28/2019 7:38:12 PM, 12.3456, 3.99, 1234.56" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.HasErrors.ShouldBeTrue();
            result.NotFoundOffices.Count.ShouldBe(1);
            result.NotFoundOffices[0].ShouldBe("NotExistingOffice");
            result.NotFoundTrucks.Count.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_Import_should_not_create_FuelPurchase_entity_when_FuelDateTime_is_incorrect()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @"Office1,101,2.3, 12.3456, 3.99, 1234.56" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.HasErrors.ShouldBeTrue();
            result.ImportedNumber.ShouldBe(0);
            result.ParseErrors.Count.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_not_create_FuelPurchase_entity_when_Office_is_Empty_and_TruckNumber_exists_in_2_offices()
        {
            // Arrange
            var truck = await CreateTruck();
            var office2 = await CreateOffice();
            var truck2 = await CreateTruck("101", officeId: office2.Id);
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @", 101, 05/28/2019, 12.3456, 3.99, 1234.56" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            result.HasErrors.ShouldBeTrue();
            result.TruckCodeInOffices.Count.ShouldBe(1);
            result.TruckCodeInOffices[0].truckCode.ShouldBe("101");
            result.TruckCodeInOffices[0].offices.Count.ShouldBe(2);
            result.TruckCodeInOffices[0].offices.Count(o => o == "Office1").ShouldBe(1);
            result.TruckCodeInOffices[0].offices.Count(o => o == "Office2").ShouldBe(1);
            result.ImportedNumber.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_FuelPurchase_entity_with_FuelDateTime_without_time()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    @"Office1,101,05/28/2019, 12.3456, 3.99, 1234.5" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2019, 5, 28));
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBe(1234.5m);
        }

        public static object[][] DatesData =
        {
            new object[]  {"05/28/2019", new DateTime(2019, 5, 28) },
            new object[]  {"05/28/2019 10:21", new DateTime(2019, 5, 28, 10, 21, 0) },
            new object[]  {"05/28/2019 10:21:43", new DateTime(2019, 5, 28, 10, 21, 43) },
            new object[]  {"05/28/2019 10:21 PM", new DateTime(2019, 5, 28, 22, 21, 0) },
            new object[]  {"05/28/2019 10:21:43 PM", new DateTime(2019, 5, 28, 22, 21, 43) },
            new object[]  {"2019-05-28", new DateTime(2019, 5, 28) },
            new object[]  {"2019-05-28 19:25:12", new DateTime(2019, 5, 28, 19, 25, 12) },
        };

        [Theory(Skip = "#14648 Historically failing")]
        [MemberData(nameof(DatesData))]
        public async Task Test_Import_should_import_all_supported_date_formats(string date, DateTime expectedDate)
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,FuelDateTime,Amount,FuelRate,Odometer" + Environment.NewLine +
                    $"Office1,101,{date}, 12.3456, 3.99, 1234.5" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsage });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(expectedDate);
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBe(1234.5m);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
