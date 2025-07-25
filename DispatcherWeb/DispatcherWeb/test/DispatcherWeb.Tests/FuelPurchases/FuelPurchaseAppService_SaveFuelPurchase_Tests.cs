using System;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.FuelPurchases.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.FuelPurchases
{
    public class FuelPurchaseAppService_SaveFuelPurchase_Tests : FuelPurchaseAppService_Tests_Base
    {
        [Fact]
        public async Task Test_SaveFuelPurchase_should_create_new_FuelPurchaseRecord()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();

            // Act
            var result = await _fuelPurchaseAppService.SaveFuelPurchase(new FuelPurchaseEditDto
            {
                Id = 0,
                TruckId = truck.Id,
                FuelDateTime = date,
                Amount = 10,
                Odometer = 20,
                Rate = 30,
                TicketNumber = "11",
            });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].Id.ShouldBe(result.Id);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(date);
            fuelPurchases[0].Amount.ShouldBe(10);
            fuelPurchases[0].Odometer.ShouldBe(20);
            fuelPurchases[0].Rate.ShouldBe(30);
            fuelPurchases[0].TicketNumber.ShouldBe("11");
        }

        [Fact]
        public async Task Test_SaveFuelPurchase_should_update_existing_FuelPurchaseRecord()
        {
            // Arrange
            var date = DateTime.UtcNow.Date;
            var truck = await CreateTruck();
            var fuelPurchase = await CreateFuelPurchase(truck.Id, date.AddDays(-1), 1, 1, 1, "1");

            // Act
            var result = await _fuelPurchaseAppService.SaveFuelPurchase(new FuelPurchaseEditDto
            {
                Id = fuelPurchase.Id,
                TruckId = truck.Id,
                FuelDateTime = date,
                Amount = 10,
                Odometer = 20,
                Rate = 30,
                TicketNumber = "11",
            });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].Id.ShouldBe(fuelPurchase.Id);
            fuelPurchases[0].TruckId.ShouldBe(truck.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(date);
            fuelPurchases[0].Amount.ShouldBe(10);
            fuelPurchases[0].Odometer.ShouldBe(20);
            fuelPurchases[0].Rate.ShouldBe(30);
            fuelPurchases[0].TicketNumber.ShouldBe("11");
        }

        [Fact]
        public async Task Test_SaveFuelPurchase_should_convert_FuelDateTime_from_IST_TimeZone_to_UTC()
        {
            // Arrange
            var date = new DateTime(2019, 8, 8);
            SubstituteSetting(TimingSettingNames.TimeZone, "India Standard Time");
            var truck = await CreateTruck();

            // Act
            var result = await _fuelPurchaseAppService.SaveFuelPurchase(new FuelPurchaseEditDto
            {
                Id = 0,
                TruckId = truck.Id,
                FuelDateTime = date,
                Amount = 10,
                Odometer = 20,
                Rate = 30,
                TicketNumber = "11",
            });

            // Assert
            var fuelPurchases = await UsingDbContextAsync(async context => await context.FuelPurchases.ToListAsync());
            fuelPurchases.Count.ShouldBe(1);
            fuelPurchases[0].Id.ShouldBe(result.Id);
            fuelPurchases[0].FuelDateTime.ShouldBe(date.AddHours(-5).AddMinutes(-30));
        }


    }
}
