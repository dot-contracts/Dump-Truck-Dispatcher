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
    public class ImportVehicleUsageService_Tests : AppTestBase, IAsyncLifetime
    {
        private IImportVehicleUsageAppService _importVehicleUsageAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _importVehicleUsageAppService = Resolve<IImportVehicleUsageAppService>(new
            {
                officeResolver = Resolve<OfficeByNameResolver>(),
            });
            SubstituteServiceDependencies(_importVehicleUsageAppService);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_VehicleUsage_entity_with_ReadingType_miles()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, 1234.56, " + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].ReadingType.ShouldBe(ReadingType.Miles);
            fuelPurchases[0].Reading.ShouldBe(1234.56m);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_VehicleUsage_entity_with_DateTime_in_users_TimeZone()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, 1234.56, " + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);
            SubstituteSettingForUser(TimingSettingNames.TimeZone, "Israel Standard Time", new UserIdentifier(1, 1));

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 16, 38, 12));
            fuelPurchases[0].ReadingType.ShouldBe(ReadingType.Miles);
            fuelPurchases[0].Reading.ShouldBe(1234.56m);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_VehicleUsage_entity_with_ReadingType_miles_when_EngineHours_value_is_bad()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, 1234.56, bad" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].ReadingType.ShouldBe(ReadingType.Miles);
            fuelPurchases[0].Reading.ShouldBe(1234.56m);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_2_VehicleUsage_entity_with_both_ReadingTypes()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, 1234.56, 5678" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            var vehicleUsages = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            vehicleUsages.Count.ShouldBe(2);

            var vehicleUsageMiles = vehicleUsages.First(fp => fp.ReadingType == ReadingType.Miles);
            vehicleUsageMiles.ShouldNotBeNull();
            vehicleUsageMiles.TruckId.ShouldBe(truck.Id);
            vehicleUsageMiles.ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            vehicleUsageMiles.ReadingType.ShouldBe(ReadingType.Miles);
            vehicleUsageMiles.Reading.ShouldBe(1234.56m);

            var vehicleUsageHours = vehicleUsages.First(fp => fp.ReadingType == ReadingType.Hours);
            vehicleUsageHours.ShouldNotBeNull();
            vehicleUsageHours.TruckId.ShouldBe(truck.Id);
            vehicleUsageHours.ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            vehicleUsageHours.ReadingType.ShouldBe(ReadingType.Hours);
            vehicleUsageHours.Reading.ShouldBe(5678m);
        }

        [Fact]
        public async Task Test_Import_should_create_VehicleUsage_entity_with_ReadingType_miles_when_Office_is_empty()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @",101,05/28/2019 7:38:12 PM, 1234.56, " + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].ReadingType.ShouldBe(ReadingType.Miles);
            fuelPurchases[0].Reading.ShouldBe(1234.56m);
        }

        [Fact]
        public async Task Test_Import_should_create_VehicleUsage_entity_with_ReadingType_miles_when_Office_is_empty_and_ignore_LeaseHaulerTruck()
        {
            // Arrange
            const string truckCode = "101";
            var leaseHauler = await CreateLeaseHauler();
            var leaseHaulerTruck = await CreateLeaseHaulerTruck(leaseHauler.Id, null, truckCode);
            var truck = await CreateTruck(truckCode);
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @",101,05/28/2019 7:38:12 PM, 1234.56, " + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            result.HasErrors.ShouldBeFalse();
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].ReadingType.ShouldBe(ReadingType.Miles);
            fuelPurchases[0].Reading.ShouldBe(1234.56m);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_VehicleUsage_entity_with_ReadingType_hours()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, , 1234.56" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].ReadingDateTime.ShouldBe(new DateTime(2019, 5, 28, 19, 38, 12));
            fuelPurchases[0].ReadingType.ShouldBe(ReadingType.Hours);
            fuelPurchases[0].Reading.ShouldBe(1234.56m);
        }

        [Fact]
        public async Task Test_Import_should_not_create_VehicleUsage_entity_when_both_OdometerReading_and_EngineHours_are_empty()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, , " + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_Import_should_not_create_VehicleUsage_entity_when_both_OdometerReading_and_EngineHours_are_bad()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"Office,TruckNumber,ReadingDateTime,OdometerReading,EngineHours" + Environment.NewLine +
                    @"Office1,101,05/28/2019 7:38:12 PM, bad, bad" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importVehicleUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.VehicleUsage });

            // Assert
            result.ImportedNumber.ShouldBe(0);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.VehicleUsages.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }



        public Task DisposeAsync() => Task.CompletedTask;
    }
}
