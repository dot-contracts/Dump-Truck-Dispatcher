using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.Runtime.Validation;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching.Dto;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.FuelSurchargeCalculations.Dto;
using DispatcherWeb.Items.Dto;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Offices.Dto;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.TaxRates.Dto;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.UnitOfMeasures.Dto;
using DispatcherWeb.VehicleCategories.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Caching
{
    [AbpAuthorize]
    public partial class CachingAppService
    {
        //TODO update the permission to use a new Pages_ListCaches_Drivers and LeaseHaulerPortal_ListCaches_Drivers permission
        //TODO filter items based on the permission before returning them

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, OrderCacheItem, int>> OrderListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Order.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, OrderLineCacheItem, int>> OrderLineListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.OrderLine.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, OrderLineTruckCacheItem, int>> OrderLineTruckListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.OrderLineTruck.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, DriverAssignmentCacheItem, int>> DriverAssignmentListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.DriverAssignment.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, DriverCacheItem, int>> DriverListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Driver.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, LeaseHaulerDriverCacheItem, int>> LeaseHaulerDriverListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.LeaseHaulerDriver.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, TruckListCacheItem, int>> TruckListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Truck.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, LeaseHaulerTruckCacheItem, int>> LeaseHaulerTruckListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.LeaseHaulerTruck.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, AvailableLeaseHaulerTruckCacheItem, int>> AvailableLeaseHaulerTruckListCache(
            GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.AvailableLeaseHaulerTruck.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, LeaseHaulerCacheItem, int>> LeaseHaulerListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.LeaseHauler.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, InsuranceCacheItem, int>> InsuranceListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Insurance.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, UserCacheItem, long>> UserListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.User.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, LeaseHaulerUserCacheItem, int>> LeaseHaulerUserListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.LeaseHaulerUser.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, CustomerCacheItem, int>> CustomerListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Customer.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, CustomerContactCacheItem, int>> CustomerContactListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.CustomerContact.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, LocationCacheItem, int>> LocationListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Location.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, ItemCacheItem, int>> ItemListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Item.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheEmptyKey, VehicleCategoryCacheItem, int>> VehicleCategoryListCache(GetListCacheListInput<ListCacheEmptyKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.VehicleCategory.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, UnitOfMeasureCacheItem, int>> UnitOfMeasureListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.UnitOfMeasure.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, OrderLineVehicleCategoryCacheItem, int>> OrderLineVehicleCategoryListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.OrderLineVehicleCategory.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, LeaseHaulerRequestCacheItem, int>> LeaseHaulerRequestListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.LeaseHaulerRequest.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, RequestedLeaseHaulerTruckCacheItem, int>> RequestedLeaseHaulerTruckListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.RequestedLeaseHaulerTruck.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, DispatchCacheItem, int>> DispatchListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Dispatch.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, LoadCacheItem, int>> LoadListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Load.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheDateKey, TicketCacheItem, int>> TicketListCache(GetListCacheListInput<ListCacheDateKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Ticket.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, FuelSurchargeCalculationCacheItem, int>> FuelSurchargeCalculationListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.FuelSurchargeCalculation.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, OfficeCacheItem, int>> OfficeListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.Office.GetList(input);

            return items;
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages)]
        public async Task<ListCacheItemDto<ListCacheTenantKey, TaxRateCacheItem, int>> TaxRateListCache(GetListCacheListInput<ListCacheTenantKey> input)
        {
            await ValidateInputAsync(input);

            var items = await _listCaches.TaxRate.GetList(input);

            return items;
        }

        private async Task ValidateInputAsync<TListKey>(GetListCacheListInput<TListKey> input)
            where TListKey : ListCacheKey
        {
            if (typeof(TListKey) == typeof(ListCacheEmptyKey))
            {
                input.Key = ListCacheEmptyKey.Instance as TListKey;
            }

            if (input.Key == null)
            {
                throw new AbpValidationException("Key cannot be null.");
            }

            if (typeof(ListCacheTenantKey).IsAssignableFrom(typeof(TListKey)))
            {
                if (input.Key is ListCacheTenantKey tenantKey)
                {
                    tenantKey.TenantId = await Session.GetTenantIdAsync();
                }
            }
        }
    }
}
