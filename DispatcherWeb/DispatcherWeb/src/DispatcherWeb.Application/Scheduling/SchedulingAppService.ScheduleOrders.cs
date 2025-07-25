using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Sessions;
using DispatcherWeb.Tickets;
using DispatcherWeb.Trucks.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Scheduling
{
    [AbpAuthorize]
    public partial class SchedulingAppService
    {


        private async Task<IQueryable<OrderLine>> GetScheduleQueryAsync(GetScheduleOrdersInput input)
        {
            var query = (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Order.DeliveryDate == input.Date
                    && !ol.Order.IsPending
                    && (ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                )
                .WhereIf(await FeatureChecker.AllowMultiOfficeFeature() && input.OfficeId.HasValue, ol => ol.Order.OfficeId == input.OfficeId)
                .WhereIf(await SettingManager.UseShifts(), ol => ol.Order.Shift == input.Shift)
                .WhereIf(input.TruckCategoryId.HasValue, ol => ol.OrderLineVehicleCategories.Any(olvc => olvc.VehicleCategory.Id == input.TruckCategoryId))
                .WhereIf(input.HideCompletedOrders, ol => !ol.IsComplete);
            return query;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<PagedResultDto<ScheduleOrderLineDto>> GetScheduleOrders(GetScheduleOrdersInput input)
        {
            var permissions = new
            {
                ViewSchedule = await IsGrantedAsync(AppPermissions.Pages_Schedule),
                ViewLeaseHaulerSchedule = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Schedule),
            };

            var items = await GetScheduleOrdersFromCache(input);

            if (permissions.ViewSchedule)
            {
                // do nothing
            }
            else if (permissions.ViewLeaseHaulerSchedule)
            {
                var leaseHaulerIdFilter = Session.GetLeaseHaulerIdOrThrow(this);
                items = items
                    .Where(i => i.Trucks.Any(q => q.LeaseHaulerId == leaseHaulerIdFilter)
                                || i.LeaseHaulerRequests.Any(q => q.LeaseHaulerId == leaseHaulerIdFilter))
                    .ToList();

                foreach (var orderLine in items)
                {
                    orderLine.Trucks.RemoveAll(x => x.LeaseHaulerId != leaseHaulerIdFilter);
                    orderLine.LeaseHaulerRequests.RemoveAll(x => x.LeaseHaulerId != leaseHaulerIdFilter);
                }
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            await ConvertScheduleOrderTimesFromUtcAsync(items);

            var today = await GetToday();
            if (!input.HideProgressBar && input.Date == today)
            {
                await CalculateOrderLineProgressFromCache(items, input);
            }

            return new PagedResultDto<ScheduleOrderLineDto>(
                items.Count,
                items);
        }

        private async Task<List<ScheduleOrderLineDto>> GetScheduleOrdersFromCache(GetScheduleOrdersInput input)
        {
            var cachesToUse = new
            {
                _listCaches.Order,
                _listCaches.OrderLine,
                _listCaches.OrderLineTruck,
                _listCaches.Customer,
                _listCaches.Location,
                _listCaches.Item,
                _listCaches.UnitOfMeasure,
                _listCaches.Truck,
                _listCaches.VehicleCategory,
                _listCaches.LeaseHaulerTruck,
                _listCaches.OrderLineVehicleCategory,
                _listCaches.Dispatch,
                _listCaches.LeaseHaulerRequest,
                _listCaches.RequestedLeaseHaulerTruck,
                _listCaches.LeaseHauler,
            };

            var cachesToCheck = new IListCache[]
            {
                _listCaches.Order,
                _listCaches.OrderLine,
                _listCaches.OrderLineTruck,
                _listCaches.Customer,
                _listCaches.Location,
                _listCaches.Item,
                _listCaches.UnitOfMeasure,
                _listCaches.Truck,
                _listCaches.VehicleCategory,
                _listCaches.LeaseHaulerTruck,
                _listCaches.OrderLineVehicleCategory,
                _listCaches.Dispatch,
                _listCaches.LeaseHaulerRequest,
                _listCaches.RequestedLeaseHaulerTruck,
                _listCaches.LeaseHauler,
            };

            if (await cachesToCheck.AnyAsync(async c => !await c.IsEnabled()))
            {
                var query = await GetScheduleQueryAsync(input);
                var items = await GetScheduleOrders(query)
                    .OrderBy(input.Sorting)
                    .ThenBy(x => x.Id)
                    .ToListAsync();
                return items;
            }

            var shift = await SettingManager.UseShifts() ? input.Shift : null;
            var dateKey = new ListCacheDateKey(await Session.GetTenantIdAsync(), input.Date, shift);
            var tenantKey = new ListCacheTenantKey(await Session.GetTenantIdAsync());
            var cache = new
            {
                Order = await cachesToUse.Order.GetListOrThrow(dateKey),
                OrderLine = await cachesToUse.OrderLine.GetListOrThrow(dateKey),
                OrderLineTruck = await cachesToUse.OrderLineTruck.GetListOrThrow(dateKey),
                Customer = await cachesToUse.Customer.GetListOrThrow(tenantKey),
                Location = await cachesToUse.Location.GetListOrThrow(tenantKey),
                Item = await cachesToUse.Item.GetListOrThrow(tenantKey),
                UnitOfMeasure = await cachesToUse.UnitOfMeasure.GetListOrThrow(tenantKey),
                Truck = await cachesToUse.Truck.GetListOrThrow(tenantKey),
                VehicleCategory = await cachesToUse.VehicleCategory.GetListOrThrow(ListCacheEmptyKey.Instance),
                LeaseHaulerTruck = await cachesToUse.LeaseHaulerTruck.GetListOrThrow(tenantKey),
                OrderLineVehicleCategory = await cachesToUse.OrderLineVehicleCategory.GetListOrThrow(dateKey),
                Dispatch = await cachesToUse.Dispatch.GetListOrThrow(dateKey),
                LeaseHaulerRequest = await cachesToUse.LeaseHaulerRequest.GetListOrThrow(dateKey),
                RequestedLeaseHaulerTruck = await cachesToUse.RequestedLeaseHaulerTruck.GetListOrThrow(dateKey),
                LeaseHauler = await cachesToUse.LeaseHauler.GetListOrThrow(tenantKey),
            };

            var allowMultiOffices = await FeatureChecker.AllowMultiOfficeFeature();

            var orderLines = cache.OrderLine.Items
                .Select(ol =>
                {
                    var order = cache.Order.Items.FirstOrDefault(o => o.Id == ol.OrderId);
                    if (order == null
                        || order.IsPending
                        || !(ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                        || input.OfficeId.HasValue && allowMultiOffices && order.OfficeId != input.OfficeId
                        || input.HideCompletedOrders && ol.IsComplete)
                    {
                        return null;
                    }

                    var orderLineVehicleCategories = cache.OrderLineVehicleCategory.Items.Where(x => x.OrderLineId == ol.Id).ToList();
                    if (input.TruckCategoryId.HasValue && !orderLineVehicleCategories.Any(x => x.VehicleCategoryId == input.TruckCategoryId))
                    {
                        return null;
                    }

                    var customer = cache.Customer.Items.FirstOrDefault(c => c.Id == order.CustomerId);
                    var loadAt = ol.LoadAtId == null ? null : cache.Location.Items.FirstOrDefault(l => l.Id == ol.LoadAtId);
                    var deliverTo = ol.DeliverToId == null ? null : cache.Location.Items.FirstOrDefault(l => l.Id == ol.DeliverToId);
                    var freightItem = ol.FreightItemId == null ? null : cache.Item.Items.FirstOrDefault(i => i.Id == ol.FreightItemId);
                    var materialItem = ol.MaterialItemId == null ? null : cache.Item.Items.FirstOrDefault(i => i.Id == ol.MaterialItemId);
                    var freightUom = ol.FreightUomId == null ? null : cache.UnitOfMeasure.Items.FirstOrDefault(u => u.Id == ol.FreightUomId);
                    var materialUom = ol.MaterialUomId == null ? null : cache.UnitOfMeasure.Items.FirstOrDefault(u => u.Id == ol.MaterialUomId);

                    var orderLineTrucks = cache.OrderLineTruck.Items
                        .Where(olt => olt.OrderLineId == ol.Id)
                        .Select(olt =>
                        {
                            var truck = cache.Truck.Items.FirstOrDefault(t => t.Id == olt.TruckId);
                            if (truck == null)
                            {
                                return null;
                            }
                            var trailer = olt.TrailerId.HasValue ? cache.Truck.Items.FirstOrDefault(t => t.Id == olt.TrailerId.Value) : null;

                            return new
                            {
                                OrderLineTruck = olt,
                                Truck = truck,
                                Trailer = trailer == null ? null : new
                                {
                                    Trailer = trailer,
                                    VehicleCategory = cache.VehicleCategory.Items.FirstOrDefault(vc => vc.Id == trailer.VehicleCategoryId),
                                },
                                VehicleCategory = cache.VehicleCategory.Items.FirstOrDefault(vc => vc.Id == truck.VehicleCategoryId),
                                LeaseHaulerTruck = cache.LeaseHaulerTruck.Items.FirstOrDefault(lht => lht.TruckId == truck.Id),
                                Dispatches = cache.Dispatch.Items.Where(d => d.OrderLineTruckId == olt.Id).ToList(),
                            };
                        })
                        .Where(x => x != null)
                        .ToList();

                    var leaseHaulerRequests = cache.LeaseHaulerRequest.Items
                        .Where(lhr => lhr.OrderLineId == ol.Id)
                        .Select(lhr => new
                        {
                            LeaseHaulerRequest = lhr,
                            LeaseHauler = cache.LeaseHauler.Items.FirstOrDefault(lh => lh.Id == lhr.LeaseHaulerId),
                            RequestedLeaseHaulerTrucks = cache.RequestedLeaseHaulerTruck.Items
                                .Where(rlt => rlt.LeaseHaulerRequestId == lhr.Id)
                                .Select(rlt =>
                                {
                                    var truck = cache.Truck.Items.FirstOrDefault(t => t.Id == rlt.TruckId);
                                    var trailer = truck?.CurrentTrailerId == null ? null : cache.Truck.Items.FirstOrDefault(t => t.Id == truck.CurrentTrailerId.Value);
                                    return new
                                    {
                                        RequestedTruck = rlt,
                                        Truck = truck == null ? null : new
                                        {
                                            Truck = truck,
                                            VehicleCategory = cache.VehicleCategory.Items.FirstOrDefault(vc => vc.Id == truck.VehicleCategoryId),
                                            Trailer = trailer == null ? null : new
                                            {
                                                Trailer = trailer,
                                                VehicleCategory = cache.VehicleCategory.Items.FirstOrDefault(vc => vc.Id == trailer.VehicleCategoryId),
                                            },
                                            LeaseHaulerTruck = cache.LeaseHaulerTruck.Items.FirstOrDefault(lht => lht.TruckId == truck.Id),
                                        },
                                    };
                                })
                                .ToList(),
                        })
                        .ToList();

                    return new ScheduleOrderLineDto
                    {
                        Id = ol.Id,
                        Date = order.DeliveryDate,
                        Shift = order.Shift,
                        OrderId = ol.OrderId,
                        Priority = order.Priority,
                        OfficeId = order.OfficeId,
                        CustomerIsCod = customer?.IsCod == true,
                        CustomerId = order.CustomerId,
                        CustomerName = customer?.Name,
                        IsTimeStaggered = ol.StaggeredTimeKind != StaggeredTimeKind.None,
                        IsTimeEditable = ol.StaggeredTimeKind == StaggeredTimeKind.None,
                        Time = ol.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? ol.FirstStaggeredTimeOnJob : ol.TimeOnJob,
                        StaggeredTimeKind = ol.StaggeredTimeKind,
                        FirstStaggeredTimeOnJob = ol.FirstStaggeredTimeOnJob,
                        StaggeredTimeInterval = ol.StaggeredTimeInterval,
                        LoadAtId = ol.LoadAtId,
                        LoadAtName = loadAt?.DisplayName,
                        DeliverToId = ol.DeliverToId,
                        DeliverToName = deliverTo?.DisplayName,
                        JobNumber = ol.JobNumber,
                        Note = ol.Note,
                        Directions = order.Directions,
                        Item = freightItem?.Name,
                        MaterialItem = materialItem?.Name,
                        FreightItem = freightItem?.Name,
                        MaterialUom = materialUom?.Name,
                        FreightUom = freightUom?.Name,
                        MaterialQuantity = ol.MaterialQuantity,
                        FreightQuantity = ol.FreightQuantity,
                        IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                        IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden,
                        Designation = ol.Designation,
                        NumberOfTrucks = ol.NumberOfTrucks,
                        ScheduledTrucks = ol.ScheduledTrucks,
                        IsClosed = ol.IsComplete,
                        IsCancelled = ol.IsCancelled,
                        HaulingCompanyOrderLineId = ol.HaulingCompanyOrderLineId,
                        MaterialCompanyOrderLineId = ol.MaterialCompanyOrderLineId,
                        VehicleCategoryIds = orderLineVehicleCategories.Select(x => x.VehicleCategoryId).ToList(),
                        VehicleCategoryNames = orderLineVehicleCategories
                            .Select(x => cache.VehicleCategory.Items.First(v => v.Id == x.VehicleCategoryId).Name)
                            .ToList(),
                        Utilization = orderLineTrucks.Where(t => t.VehicleCategory.IsPowered).Select(t => t.OrderLineTruck.Utilization).Sum(),
                        Trucks = orderLineTrucks.Select(olt => new ScheduleOrderLineTruckDto
                        {
                            Id = olt.OrderLineTruck.Id,
                            ParentId = olt.OrderLineTruck.ParentOrderLineTruckId,
                            TruckId = olt.OrderLineTruck.TruckId,
                            TruckCode = olt.Truck.TruckCode,
                            Trailer = olt.Trailer == null ? null : new ScheduleTruckTrailerDto(olt.Trailer.Trailer, olt.Trailer.VehicleCategory),
                            DriverId = olt.OrderLineTruck.DriverId,
                            OrderId = ol.OrderId,
                            OrderLineId = ol.Id,
                            IsExternal = olt.Truck.OfficeId == null,
                            OfficeId = olt.Truck.OfficeId ?? order?.OfficeId /* Available Lease Hauler Truck */,
                            Utilization = olt.OrderLineTruck.Utilization,
                            VehicleCategory = new VehicleCategoryDto(olt.VehicleCategory),
                            AlwaysShowOnSchedule = olt.LeaseHaulerTruck?.AlwaysShowOnSchedule == true,
                            CanPullTrailer = olt.Truck.CanPullTrailer,
                            IsDone = olt.OrderLineTruck.IsDone,
                            TimeOnJob = olt.OrderLineTruck.TimeOnJob ?? ol.TimeOnJob,
                            LeaseHaulerId = olt.LeaseHaulerTruck?.LeaseHaulerId,
                            Dispatches = olt.Dispatches.Select(x => new ScheduleOrderLineTruckDispatchDto
                            {
                                Id = x.Id,
                                Status = x.Status,
                                IsMultipleLoads = x.IsMultipleLoads,
                            }).ToList(),
                        }).ToList(),
                        LeaseHaulerRequests = leaseHaulerRequests.Select(lhr => new LeaseHaulerRequestDto
                        {
                            Id = lhr.LeaseHaulerRequest.Id,
                            LeaseHaulerId = lhr.LeaseHaulerRequest.LeaseHaulerId,
                            LeaseHaulerName = lhr.LeaseHauler.Name,
                            NumberTrucksRequested = lhr.LeaseHaulerRequest.NumberTrucksRequested,
                            Status = lhr.LeaseHaulerRequest.Status,
                            RequestedTrucks = lhr.RequestedLeaseHaulerTrucks.Select(r => new ScheduleRequestedLeaseHaulerTruckDto
                            {
                                Id = r.RequestedTruck.Id,
                                OrderId = ol.OrderId,
                                OrderLineId = ol.Id,
                                TruckId = r.RequestedTruck.TruckId,
                                TruckCode = r.Truck.Truck.TruckCode,
                                Trailer = r.Truck.Trailer == null ? null : new ScheduleTruckTrailerDto(r.Truck.Trailer.Trailer, r.Truck.Trailer.VehicleCategory),
                                DriverId = r.RequestedTruck.DriverId,
                                IsExternal = r.Truck.Truck.OfficeId == null,
                                OfficeId = r.Truck.Truck.OfficeId ?? order?.OfficeId /* Available Lease Hauler Truck */,
                                VehicleCategory = new VehicleCategoryDto(r.Truck.VehicleCategory),
                                AlwaysShowOnSchedule = r.Truck.LeaseHaulerTruck?.AlwaysShowOnSchedule == true,
                                CanPullTrailer = r.Truck.Truck.CanPullTrailer,
                                Status = lhr.LeaseHaulerRequest.Status ?? 0,
                            }).ToList(),
                        })
                        .Where(r => r.NumberTrucksRequested > 0)
                        .ToList(),
                    };
                })
                .Where(x => x != null)
                .AsQueryable()
                .OrderBy(input.Sorting)
                .ThenBy(x => x.Id)
                .ToList();

            return orderLines;
        }

        public static IQueryable<ScheduleOrderLineDto> GetScheduleOrders(IQueryable<OrderLine> query)
        {
            return query
                .Select(ol => new ScheduleOrderLineDto
                {
                    Id = ol.Id,
                    Date = ol.Order.DeliveryDate,
                    Shift = ol.Order.Shift,
                    OrderId = ol.OrderId,
                    Priority = ol.Order.Priority,
                    OfficeId = ol.Order.OfficeId,
                    CustomerIsCod = ol.Order.Customer.IsCod,
                    CustomerId = ol.Order.CustomerId,
                    CustomerName = ol.Order.Customer.Name,
                    IsTimeStaggered = ol.StaggeredTimeKind != StaggeredTimeKind.None,
                    IsTimeEditable = ol.StaggeredTimeKind == StaggeredTimeKind.None,
                    Time = ol.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? ol.FirstStaggeredTimeOnJob : ol.TimeOnJob,
                    StaggeredTimeKind = ol.StaggeredTimeKind,
                    FirstStaggeredTimeOnJob = ol.FirstStaggeredTimeOnJob,
                    StaggeredTimeInterval = ol.StaggeredTimeInterval,
                    LoadAtId = ol.LoadAtId,
                    LoadAtName = ol.LoadAt.DisplayName,
                    DeliverToId = ol.DeliverToId,
                    DeliverToName = ol.DeliverTo.DisplayName,
                    JobNumber = ol.JobNumber,
                    Note = ol.Note,
                    Directions = ol.Order.Directions,
                    Item = ol.FreightItem.Name,
                    MaterialItem = ol.MaterialItem.Name,
                    FreightItem = ol.FreightItem.Name,
                    MaterialUom = ol.MaterialUom.Name,
                    FreightUom = ol.FreightUom.Name,
                    MaterialQuantity = ol.MaterialQuantity,
                    FreightQuantity = ol.FreightQuantity,
                    IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden,
                    Designation = ol.Designation,
                    NumberOfTrucks = ol.NumberOfTrucks,
                    ScheduledTrucks = ol.ScheduledTrucks,
                    IsClosed = ol.IsComplete,
                    IsCancelled = ol.IsCancelled,
                    HaulingCompanyOrderLineId = ol.HaulingCompanyOrderLineId,
                    MaterialCompanyOrderLineId = ol.MaterialCompanyOrderLineId,
                    VehicleCategoryIds = ol.OrderLineVehicleCategories.Select(x => x.VehicleCategoryId).ToList(),
                    VehicleCategoryNames = ol.OrderLineVehicleCategories.Select(x => x.VehicleCategory.Name).ToList(),
                    Utilization = ol.OrderLineTrucks.Where(t => t.Truck.VehicleCategory.IsPowered).Select(t => t.Utilization).Sum(),
                    Trucks = ol.OrderLineTrucks.Select(olt => new ScheduleOrderLineTruckDto
                    {
                        Id = olt.Id,
                        ParentId = olt.ParentOrderLineTruckId,
                        TruckId = olt.TruckId,
                        TruckCode = olt.Truck.TruckCode,
                        Trailer = olt.Trailer == null ? null : new ScheduleTruckTrailerDto
                        {
                            Id = olt.Trailer.Id,
                            TruckCode = olt.Trailer.TruckCode,
                            VehicleCategory = new VehicleCategoryDto
                            {
                                Id = olt.Trailer.VehicleCategory.Id,
                                Name = olt.Trailer.VehicleCategory.Name,
                                AssetType = olt.Trailer.VehicleCategory.AssetType,
                                IsPowered = olt.Trailer.VehicleCategory.IsPowered,
                                SortOrder = olt.Trailer.VehicleCategory.SortOrder,
                            },
                            Make = olt.Trailer.Make,
                            Model = olt.Trailer.Model,
                            BedConstruction = olt.Trailer.BedConstruction,
                        },
                        DriverId = olt.DriverId,
                        OrderId = ol.OrderId,
                        OrderLineId = ol.Id,
                        IsExternal = olt.Truck.OfficeId == null,
                        OfficeId = olt.Truck.OfficeId ?? ol.Order.OfficeId /* Available Lease Hauler Truck */,
                        Utilization = olt.Utilization,
                        VehicleCategory = new VehicleCategoryDto
                        {
                            Id = olt.Truck.VehicleCategory.Id,
                            Name = olt.Truck.VehicleCategory.Name,
                            AssetType = olt.Truck.VehicleCategory.AssetType,
                            IsPowered = olt.Truck.VehicleCategory.IsPowered,
                            SortOrder = olt.Truck.VehicleCategory.SortOrder,
                        },
                        AlwaysShowOnSchedule = olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule == true,
                        CanPullTrailer = olt.Truck.CanPullTrailer,
                        IsDone = olt.IsDone,
                        TimeOnJob = olt.TimeOnJob ?? ol.TimeOnJob,
                        LeaseHaulerId = olt.Truck.LeaseHaulerTruck.LeaseHaulerId,
                        Dispatches = olt.Dispatches.Select(x => new ScheduleOrderLineTruckDispatchDto
                        {
                            Id = x.Id,
                            Status = x.Status,
                            IsMultipleLoads = x.IsMultipleLoads,
                        }).ToList(),
                    }).ToList(),
                    LeaseHaulerRequests = ol.LeaseHaulerRequests.Select(lhr => new LeaseHaulerRequestDto
                    {
                        Id = lhr.Id,
                        LeaseHaulerId = lhr.LeaseHaulerId,
                        LeaseHaulerName = lhr.LeaseHauler.Name,
                        NumberTrucksRequested = lhr.NumberTrucksRequested,
                        Status = lhr.Status,
                        RequestedTrucks = lhr.RequestedLeaseHaulerTrucks.Select(requestedTruck => new ScheduleRequestedLeaseHaulerTruckDto
                        {
                            Id = requestedTruck.Id,
                            OrderId = ol.OrderId,
                            OrderLineId = ol.Id,
                            TruckId = requestedTruck.Truck.Id,
                            TruckCode = requestedTruck.Truck.TruckCode,
                            Trailer = requestedTruck.Truck.CurrentTrailer == null ? null : new ScheduleTruckTrailerDto
                            {
                                Id = requestedTruck.Truck.CurrentTrailer.Id,
                                TruckCode = requestedTruck.Truck.CurrentTrailer.TruckCode,
                                VehicleCategory = new VehicleCategoryDto
                                {
                                    Id = requestedTruck.Truck.CurrentTrailer.VehicleCategory.Id,
                                    Name = requestedTruck.Truck.CurrentTrailer.VehicleCategory.Name,
                                    AssetType = requestedTruck.Truck.CurrentTrailer.VehicleCategory.AssetType,
                                    IsPowered = requestedTruck.Truck.CurrentTrailer.VehicleCategory.IsPowered,
                                    SortOrder = requestedTruck.Truck.CurrentTrailer.VehicleCategory.SortOrder,
                                },
                                Make = requestedTruck.Truck.CurrentTrailer.Make,
                                Model = requestedTruck.Truck.CurrentTrailer.Model,
                                BedConstruction = requestedTruck.Truck.CurrentTrailer.BedConstruction,
                            },
                            DriverId = requestedTruck.DriverId,
                            IsExternal = requestedTruck.Truck.OfficeId == null,
                            OfficeId = requestedTruck.Truck.OfficeId ?? ol.Order.OfficeId /* Available Lease Hauler Truck */,
                            VehicleCategory = new VehicleCategoryDto
                            {
                                Id = requestedTruck.Truck.VehicleCategory.Id,
                                Name = requestedTruck.Truck.VehicleCategory.Name,
                                AssetType = requestedTruck.Truck.VehicleCategory.AssetType,
                                IsPowered = requestedTruck.Truck.VehicleCategory.IsPowered,
                                SortOrder = requestedTruck.Truck.VehicleCategory.SortOrder,
                            },
                            AlwaysShowOnSchedule = requestedTruck.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule == true,
                            CanPullTrailer = requestedTruck.Truck.CanPullTrailer,
                            Status = lhr.Status.Value,
                        }).ToList(),
                    }).Where(r =>
                        r.NumberTrucksRequested.HasValue
                        && r.NumberTrucksRequested.Value > 0
                    ).ToList(),
                });
        }

        private async Task ConvertScheduleOrderTimesFromUtcAsync(List<ScheduleOrderLineDto> orderLines)
        {
            var timezone = await GetTimezone();
            orderLines.ForEach(x => ConvertScheduleOrderTimesFromUtc(x, timezone));
        }

        private void ConvertScheduleOrderTimesFromUtc(ScheduleOrderLineDto orderLine, string timezone)
        {
            orderLine.Time = orderLine.Time?.ConvertTimeZoneTo(timezone);
            orderLine.FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone);
        }

        private async Task CalculateOrderLineProgressFromCache(List<ScheduleOrderLineDto> items, GetScheduleOrdersInput input)
        {
            if (!await SettingManager.DispatchViaDriverApplication())
            {
                return;
            }

            var cachesToUse = new
            {
                _listCaches.OrderLine,
                _listCaches.Dispatch,
                _listCaches.Load,
                _listCaches.Ticket,
                _listCaches.Truck,
            };

            var cachesToCheck = new IListCache[]
            {
                _listCaches.OrderLine,
                _listCaches.Dispatch,
                _listCaches.Load,
                _listCaches.Ticket,
                _listCaches.Truck,
            };

            if (await cachesToCheck.AnyAsync(async c => !await c.IsEnabled()))
            {
                await CalculateOrderLineProgress(items);
                return;
            }

            var shift = await SettingManager.UseShifts() ? input.Shift : null;
            var dateKey = new ListCacheDateKey(await Session.GetTenantIdAsync(), input.Date, shift);
            var tenantKey = new ListCacheTenantKey(await Session.GetTenantIdAsync());
            var cache = new
            {
                OrderLine = await cachesToUse.OrderLine.GetListOrThrow(dateKey),
                Dispatch = await cachesToUse.Dispatch.GetListOrThrow(dateKey),
                Load = await cachesToUse.Load.GetListOrThrow(dateKey),
                Ticket = await cachesToUse.Ticket.GetListOrThrow(dateKey),
                Truck = await cachesToUse.Truck.GetListOrThrow(tenantKey),
            };

            var progressData = cache.OrderLine.Items
                .Select(ol =>
                {
                    var dispatches = cache.Dispatch.Items
                        .Where(d => d.OrderLineId == ol.Id)
                        .Select(d => new
                        {
                            Dispatch = d,
                            Loads = cache.Load.Items
                                .Where(l => l.DispatchId == d.Id)
                                .Select(l => new
                                {
                                    Load = l,
                                    Dispatch = d,
                                    Truck = cache.Truck.Items.FirstOrDefault(t => t.Id == d.TruckId),
                                    Tickets = cache.Ticket.Items
                                        .Where(t => t.LoadId == l.Id)
                                        .ToList(),
                                })
                                .ToList(),
                        })
                        .ToList();

                    var tickets = cache.Ticket.Items
                        .Where(t => t.OrderLineId == ol.Id)
                        .ToList();

                    return new OrderLineProgressDto
                    {
                        Id = ol.Id,
                        DispatchCount = dispatches.Count(d => Dispatch.OpenStatuses.Contains(d.Dispatch.Status) || d.Dispatch.Status == DispatchStatus.Completed),
                        Tickets = tickets.Select(t => t.ToTicketQuantityDto(ol)).ToList(),
                        Loads = dispatches.SelectMany(t => t.Loads).Select(l => new OrderLineProgressDto.LoadDto
                        {
                            DestinationDateTime = l.Load.DestinationDateTime,
                            SourceDateTime = l.Load.SourceDateTime,
                            Dispatch = new OrderLineProgressDto.DispatchDto
                            {
                                Id = l.Dispatch.Id,
                                Acknowledged = l.Dispatch.Acknowledged,
                                Truck = new OrderLineProgressDto.TruckDto
                                {
                                    Id = l.Dispatch.TruckId,
                                    TruckCode = l.Truck.TruckCode,
                                    CargoCapacityTons = l.Truck.CargoCapacity,
                                    CargoCapacityCyds = l.Truck.CargoCapacityCyds,
                                },
                            },
                            Tickets = l.Tickets.Select(t => t.ToTicketQuantityDto(ol)).ToList(),
                        }).ToList(),
                        //DeliveredLoads_FromOrder = ol.Dispatches.Sum(d => d.Loads.Count(l => l.DestinationDateTime.HasValue)),
                    };
                }).ToList();

            CalculateOrderLineProgress(items, progressData);
        }

        private async Task CalculateOrderLineProgress(List<ScheduleOrderLineDto> items)
        {
            if (!await SettingManager.DispatchViaDriverApplication())
            {
                return;
            }

            var orderLineIds = items.Select(x => x.Id).ToList();

            var progressData = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => orderLineIds.Contains(x.Id))
                .Select(ol => new OrderLineProgressDto
                {
                    Id = ol.Id,
                    DispatchCount = ol.Dispatches.Count(d => Dispatch.OpenStatuses.Contains(d.Status) || d.Status == DispatchStatus.Completed),
                    Tickets = ol.Tickets.Select(t => new TicketQuantityDto
                    {
                        TicketId = t.Id,
                        Designation = t.OrderLine.Designation,
                        OrderLineFreightUomId = t.OrderLine.FreightUomId,
                        OrderLineMaterialUomId = t.OrderLine.MaterialUomId,
                        FuelSurcharge = t.FuelSurcharge,
                        FreightQuantity = t.FreightQuantity,
                        MaterialQuantity = t.MaterialQuantity,
                        TicketUomId = t.FreightUomId,
                    }).ToList(),
                    Loads = ol.Dispatches.SelectMany(t => t.Loads).Select(l => new OrderLineProgressDto.LoadDto
                    {
                        DestinationDateTime = l.DestinationDateTime,
                        SourceDateTime = l.SourceDateTime,
                        Dispatch = new OrderLineProgressDto.DispatchDto
                        {
                            Id = l.DispatchId,
                            Acknowledged = l.Dispatch.Acknowledged,
                            Truck = new OrderLineProgressDto.TruckDto
                            {
                                Id = l.Dispatch.TruckId,
                                TruckCode = l.Dispatch.Truck.TruckCode,
                                CargoCapacityTons = l.Dispatch.Truck.CargoCapacity,
                                CargoCapacityCyds = l.Dispatch.Truck.CargoCapacityCyds,
                            },
                        },
                        Tickets = l.Tickets.Select(t => new TicketQuantityDto
                        {
                            TicketId = t.Id,
                            Designation = t.OrderLine.Designation,
                            OrderLineFreightUomId = t.OrderLine.FreightUomId,
                            OrderLineMaterialUomId = t.OrderLine.MaterialUomId,
                            FuelSurcharge = t.FuelSurcharge,
                            FreightQuantity = t.FreightQuantity,
                            MaterialQuantity = t.MaterialQuantity,
                            TicketUomId = t.FreightUomId,
                        }).ToList(),
                    }).ToList(),
                }).ToListAsync();

            CalculateOrderLineProgress(items, progressData);
        }

        private void CalculateOrderLineProgress(List<ScheduleOrderLineDto> items, List<OrderLineProgressDto> progressData)
        {
            foreach (var orderLine in items)
            {
                orderLine.AmountOrdered = orderLine.MaterialQuantity;

                var processedTicketIds = new List<int>();

                var orderLineProgress = progressData.FirstOrDefault(x => x.Id == orderLine.Id);

                if (orderLineProgress != null)
                {
                    orderLine.DispatchCount = orderLineProgress.DispatchCount;
                }

                if (orderLineProgress?.Loads.Any() == true)
                {
                    var deliveredLoads = orderLineProgress.Loads.Where(l => l.DestinationDateTime.HasValue).ToList();
                    var loadedLoads = orderLineProgress.Loads.Where(l => l.SourceDateTime.HasValue).ToList();
                    orderLine.DeliveredLoadCount = deliveredLoads.Count;
                    orderLine.LoadedLoadCount = loadedLoads.Count;
                    orderLine.LoadCount = orderLineProgress.Loads.Count;
                    orderLine.AmountLoaded = 0;
                    orderLine.AmountDelivered = 0;

                    //orderLine.HoursOnDispatches = 0;
                    //foreach (var loads in deliveredLoads
                    //    .Where(x => x.Acknowledged.HasValue)
                    //    .GroupBy(x => x.DispatchId))
                    //{
                    //    var load = loads.OrderByDescending(x => x.DestinationDateTime).FirstOrDefault();
                    //    if (load != null)
                    //    {
                    //        orderLine.HoursOnDispatches += (decimal)(load.DestinationDateTime.Value - load.Acknowledged.Value).TotalHours;
                    //    }
                    //}
                    //
                    //orderLine.HoursOnDispatchesLoaded = 0;
                    //foreach (var loads in loadedLoads
                    //    .Where(x => x.Acknowledged.HasValue)
                    //    .GroupBy(x => x.DispatchId))
                    //{
                    //    var load = loads
                    //        .OrderByDescending(x => x.DestinationDateTime != null)
                    //        .ThenByDescending(x => x.DestinationDateTime)
                    //        .ThenByDescending(x => x.SourceDateTime)
                    //        .FirstOrDefault();
                    //    if (load != null)
                    //    {
                    //        orderLine.HoursOnDispatchesLoaded += load.DestinationDateTime.HasValue
                    //            ? (decimal)(load.DestinationDateTime.Value - load.Acknowledged.Value).TotalHours
                    //            : (decimal)(load.SourceDateTime.Value - load.Acknowledged.Value).TotalHours;
                    //    }
                    //}

                    var validateCargoCapacityTons = false;
                    var validateCargoCapacityCyds = false;

                    foreach (var load in orderLineProgress.Loads)
                    {
                        var ticket = load.Tickets.FirstOrDefault();

                        decimal amountToAdd = 0M;

                        if (ticket != null)
                        {
                            amountToAdd = ticket.GetMaterialQuantity() ?? 0;
                            processedTicketIds.Add(ticket.TicketId);
                        }

                        if (amountToAdd == 0)
                        {
                            switch (orderLine.MaterialUom?.ToLower())
                            {
                                case "ton":
                                case "tons":
                                    validateCargoCapacityTons = true;
                                    amountToAdd = load.Dispatch.Truck.CargoCapacityTons ?? 0;
                                    break;
                                case "tonne":
                                case "tonnes":
                                    //% complete should be calculated by multiplying the number of loads by the CargoCapacity
                                    //and any other conversion factor needed to get to the appropriate weight UOM and dividing by the ordered freight quantity
                                    validateCargoCapacityTons = true;
                                    amountToAdd = (load.Dispatch.Truck.CargoCapacityTons ?? 0) * 2000 / 2204.6M;
                                    break;
                                case "cubic yard":
                                case "cubic yards":
                                    //% complete should be calculated by multiplying the number of loads by the CargoCapacityCyds
                                    //and dividing by the ordered freight quantity.
                                    validateCargoCapacityCyds = true;
                                    amountToAdd = load.Dispatch.Truck.CargoCapacityCyds ?? 0;
                                    break;
                                case "cubic meter":
                                case "cubic meters":
                                    validateCargoCapacityCyds = true;
                                    amountToAdd = (load.Dispatch.Truck.CargoCapacityCyds ?? 0) / 1.30795M;
                                    break;
                            }
                        }

                        if (load.SourceDateTime.HasValue)
                        {
                            orderLine.AmountLoaded += amountToAdd;
                        }
                        if (load.DestinationDateTime.HasValue)
                        {
                            orderLine.AmountDelivered += amountToAdd;
                        }
                    }

                    if (validateCargoCapacityTons)
                    {
                        orderLine.CargoCapacityRequiredError = ValidateCargoCapacityTons();
                    }

                    if (validateCargoCapacityCyds)
                    {
                        orderLine.CargoCapacityRequiredError = ValidateCargoCapacityCyds();
                    }
                }

                if (orderLineProgress?.Tickets.Any() == true)
                {
                    var tickets = orderLineProgress.Tickets.ToList();

                    foreach (var ticket in tickets)
                    {
                        if (!processedTicketIds.Contains(ticket.TicketId))
                        {
                            var quantityToAdd = ticket.GetMaterialQuantity() ?? 0;
                            orderLine.AmountLoaded += quantityToAdd;
                            orderLine.AmountDelivered += quantityToAdd;
                        }
                    }
                }

                string ValidateCargoCapacityTons()
                {
                    var trucksWithNoCargoCapacity = orderLineProgress.Loads
                        .Where(x => x.DestinationDateTime.HasValue || x.SourceDateTime.HasValue)
                        .Select(x => x.Dispatch.Truck)
                        .Where(x => !(x.CargoCapacityTons > 0))
                        .GroupBy(x => x.Id)
                        .Select(x => x.FirstOrDefault()?.TruckCode)
                        .ToList();

                    return FormatCargoCapacityError(trucksWithNoCargoCapacity, "Ave Load(Tons)");
                }

                string ValidateCargoCapacityCyds()
                {
                    var trucksWithNoCargoCapacity = orderLineProgress.Loads
                        .Where(x => x.DestinationDateTime.HasValue || x.SourceDateTime.HasValue)
                        .Select(x => x.Dispatch.Truck)
                        .Where(x => !(x.CargoCapacityCyds > 0))
                        .GroupBy(x => x.Id)
                        .Select(x => x.FirstOrDefault()?.TruckCode)
                        .ToList();

                    return FormatCargoCapacityError(trucksWithNoCargoCapacity, "Ave Load(cyds)");
                }

                string FormatCargoCapacityError(List<string> truckNumbers, string fieldDisplayName)
                {
                    if (!truckNumbers.Any())
                    {
                        return null;
                    }

                    var s = truckNumbers.Count > 1 ? "s" : "";
                    return $"Can't calculate the estimated percentage because the value '{fieldDisplayName}' is not entered for truck{s} {string.Join(", ", truckNumbers)}";
                }
            }
        }
    }
}
