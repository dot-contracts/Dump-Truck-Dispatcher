using System;
using System.IO;
using System.Threading.Tasks;
using Abp;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Imports.Services
{
    public class ImportFuelUsage_JacobusEnergy_Tests : AppTestBase, IAsyncLifetime
    {
        private IImportFuelUsageAppService _importFuelUsageAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            await UpdateEntity(office, o => o.FuelIds = "001|011|111");
            _importFuelUsageAppService = Resolve<IImportFuelUsageAppService>(new
            {
                officeResolver = Resolve<OfficeByFuelIdResolver>(),
            });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_FuelPurchase_entity()
        {
            // Arrange
            var truck = await CreateTruck();
            var office2 = await CreateOffice();
            var csvString =
                    @"ShipToID,ShipToName,DeliveryTicketNumber,DeliveredDate,CustomerUnitId,DeliveredQty,PricePerGallon" + Environment.NewLine +
                    @"001,-ignore-,tn001,2019-06-03 21:02:39,101,12.3456,3.99" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsageFromJacobusEnergy });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            result.HasErrors.ShouldBeFalse();
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2019, 6, 3, 21, 2, 39));
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBeNull();
            fuelPurchases[0].TicketNumber.ShouldBe("tn001");
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_create_FuelPurchase_entity_for_Office2()
        {
            // Arrange
            var truck = await CreateTruck();
            var office2 = await CreateOffice();
            await UpdateEntity(office2, o => o.FuelIds = "002");
            var truck2 = await CreateTruck(officeId: office2.Id);
            var csvString =
                    @"ShipToID,ShipToName,DeliveryTicketNumber,DeliveredDate,CustomerUnitId,DeliveredQty,PricePerGallon" + Environment.NewLine +
                    @"002,-ignore-,tn001,2019-06-03 21:02:39,101,12.3456,3.99" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsageFromJacobusEnergy });

            // Assert
            result.ImportedNumber.ShouldBe(1);
            result.HasErrors.ShouldBeFalse();
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].TruckId.ShouldBe(truck2.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(new DateTime(2019, 6, 3, 21, 2, 39));
            fuelPurchases[0].Amount.ShouldBe(12.3456m);
            fuelPurchases[0].Rate.ShouldBe(3.99m);
            fuelPurchases[0].Odometer.ShouldBeNull();
            fuelPurchases[0].TicketNumber.ShouldBe("tn001");
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_Import_should_not_create_FuelPurchase_entity_when_ShipToID_does_not_exist_in_Office_FuelIds()
        {
            // Arrange
            var truck = await CreateTruck();
            var csvString =
                    @"ShipToID,ShipToName,DeliveryTicketNumber,DeliveredDate,CustomerUnitId,DeliveredQty,PricePerGallon" + Environment.NewLine +
                    @"444,-ignore-,tn001,2019-06-03 21:02:39,101,12.3456,3.99" + Environment.NewLine
                ;
            TextReader textReader = new StringReader(csvString);

            // Act
            var result = await _importFuelUsageAppService.Import(textReader, new ImportJobArgs { RequestorUser = new UserIdentifier(1, 1), FieldMap = FieldMapping.FuelUsageFromJacobusEnergy });

            // Assert
            result.ImportedNumber.ShouldBe(0);
            result.HasErrors.ShouldBeTrue();
            result.NotFoundOffices.Count.ShouldBe(1);
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(0);
        }



        public Task DisposeAsync() => Task.CompletedTask;
    }
}
