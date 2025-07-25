using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ICrossTenantOrderSender
    {
        Task<SendOrderLineToHaulingCompanyInput> GetInputForSendOrderLineToHaulingCompany(int orderLineId);
        Task SendInvoiceTicketsToCustomerTenant(EntityDto input);
        Task SendOrderLineToHaulingCompany(SendOrderLineToHaulingCompanyInput input);
        Task SyncLinkedOrderLines(int sourceOrderLineId, int[] alreadySyncedOrderLineIds, List<string> updatedFields, Orders.IOrderLineUpdaterFactory orderLineUpdaterFactory);
        Task SyncMaterialCompanyDriversIfNeeded(int haulingCompanyDriverId);
        Task SyncMaterialCompanyTrucksIfNeeded(int haulingCompanyTruckId);
    }
}
