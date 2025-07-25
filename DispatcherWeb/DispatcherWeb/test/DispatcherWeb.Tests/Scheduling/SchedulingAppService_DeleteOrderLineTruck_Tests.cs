using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Scheduling.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_DeleteOrderLineTruck_Tests : SchedulingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_throw_ApplicationException_there_is_acknowledge_Dispatch()
        {
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLineId, DispatchStatus.Acknowledged);

            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineTruckId = orderLineTruck.Id,
            })
                .ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_DeleteOrderLineTruck_should_throw_ApplicationException_there_is_loaded_Dispatch()
        {
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLineId, DispatchStatus.Loaded);

            await _schedulingAppService.DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineTruckId = orderLineTruck.Id,
            })
                .ShouldThrowAsync(typeof(UserFriendlyException));
        }
    }
}
