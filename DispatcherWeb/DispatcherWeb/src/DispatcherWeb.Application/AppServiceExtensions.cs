using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb
{
    public static class AppServiceExtensions
    {
        public static async Task<PagedResultDto<SelectListDto>> GetSelectListResult(this IQueryable<SelectListDto> query, GetSelectListInput input)
        {
            return await query.GetSelectListResult(input, x => x);
        }

        public static async Task<PagedResultDto<T>> GetExtendedSelectListResult<T>(this IQueryable<T> query, GetSelectListInput input) where T : SelectListDto
        {
            return await query.GetSelectListResult(input, x => x);
        }

        public static async Task<PagedResultDto<TOut>> GetSelectListResult<TOut, TIn>(this IQueryable<TIn> query, GetSelectListInput input, Func<TIn, TOut> resultConverter) where TIn : SelectListDto
        {
            query = query
                .OrderBy(x => x.Name)
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => x.Name.ToLower().Contains(input.Term.ToLower()));

            var startsWithQuery = query
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => x.Name.ToLower().StartsWith(input.Term.ToLower()));
            var containsQuery = query
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => !x.Name.ToLower().StartsWith(input.Term.ToLower()) && x.Name.Contains(input.Term.ToLower()));

            var startsWithCount = await startsWithQuery.CountAsync();
            var containsCount = input.Term.IsNullOrEmpty() ? 0 : await containsQuery.CountAsync();
            var totalCount = containsCount + startsWithCount;

            var items = await startsWithQuery.PageBy(input).ToListAsync();
            if (items.Count < input.MaxResultCount && !input.Term.IsNullOrEmpty())
            {
                input.SkipCount -= (startsWithCount - items.Count);
                input.MaxResultCount -= items.Count;
                items.AddRange(await containsQuery.PageBy(input).ToListAsync());
            }

            return new PagedResultDto<TOut>(
                totalCount,
                items.Select(resultConverter).ToList());
        }
        public static PagedResultDto<SelectListDto> GetSelectListResult(this IReadOnlyCollection<SelectListDto> list, GetSelectListInput input)
        {
            return list.GetSelectListResult(input, x => x);
        }

        public static PagedResultDto<T> GetExtendedSelectListResult<T>(this IReadOnlyCollection<T> list, GetSelectListInput input) where T : SelectListDto
        {
            return list.GetSelectListResult(input, x => x);
        }

        public static PagedResultDto<TOut> GetSelectListResult<TOut, TIn>(this IReadOnlyCollection<TIn> list, GetSelectListInput input, Func<TIn, TOut> resultConverter) where TIn : SelectListDto
        {
            list = list
                .OrderBy(x => x.Name)
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => x.Name.ToLower().Contains(input.Term.ToLower()))
                .ToList();

            var startsWithList = list
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => x.Name.ToLower().StartsWith(input.Term.ToLower()))
                .ToList();
            var containsList = list
                .WhereIf(!input.Term.IsNullOrEmpty(),
                            x => !x.Name.ToLower().StartsWith(input.Term.ToLower()) && x.Name.Contains(input.Term.ToLower()))
                .ToList();

            var startsWithCount = startsWithList.Count;
            var containsCount = input.Term.IsNullOrEmpty() ? 0 : containsList.Count;
            var totalCount = containsCount + startsWithCount;

            var items = startsWithList.Union(containsList).ToList();

            return new PagedResultDto<TOut>(
                totalCount,
                items.Select(resultConverter).ToList());
        }

        public static IQueryable<WorkOrderReportDto> GetWorkOrderReportDtoQuery(this IQueryable<Receipt> query, GetWorkOrderReportInput input, int officeId)
        {
            var newQuery = query
                .Select(r => new
                {
                    Order = r.Order,
                    Receipt = r,
                    Payment = r.OrderPayments
                        .Where(x => x.OfficeId == officeId)
                        .Select(x => x.Payment).FirstOrDefault(x => !x.IsCancelledOrRefunded),
                })
                .Select(o => new WorkOrderReportDto
                {
                    Id = o.Order.Id,
                    ContactEmail = o.Order.CustomerContact.Email,
                    ContactPhoneNumber = o.Order.CustomerContact.PhoneNumber,
                    ContactName = o.Order.CustomerContact.Name,
                    CustomerName = o.Order.Customer.Name,
                    CustomerAccountNumber = o.Order.Customer.AccountNumber,
                    ChargeTo = o.Order.ChargeTo,
                    CodTotal = o.Receipt.Total,
                    OrderDeliveryDate = o.Order.DeliveryDate,
                    OrderShift = o.Order.Shift,
                    OrderIsPending = o.Order.IsPending,
                    OfficeId = o.Order.Office.Id,
                    OfficeName = o.Order.Office.Name,
                    Directions = o.Order.Directions,
                    FreightTotal = o.Receipt.FreightTotal,
                    MaterialTotal = o.Receipt.MaterialTotal,
                    PoNumber = o.Order.PONumber,
                    SpectrumNumber = o.Order.SpectrumNumber,
                    SalesTaxRate = o.Receipt.SalesTaxRate,
                    SalesTax = o.Receipt.SalesTax,
                    AuthorizationDateTime = o.Payment.AuthorizationDateTime,
                    AuthorizationCaptureDateTime = o.Payment.AuthorizationCaptureDateTime,
                    AuthorizationCaptureSettlementAmount = o.Payment.AuthorizationCaptureAmount,
                    AuthorizationCaptureTransactionId = o.Payment.AuthorizationCaptureTransactionId,
                    AllTrucksNonDistinct = o.Order.OrderLines.SelectMany(ol => ol.OrderLineTrucks).Select(olt =>
                        new WorkOrderReportDto.TruckDriverDto
                        {
                            TruckId = olt.TruckId,
                            TruckCode = olt.Truck.TruckCode,
                            DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                            AssetType = olt.Truck.VehicleCategory.AssetType,
                            IsPowered = olt.Truck.VehicleCategory.IsPowered,
                            IsLeased = olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule || olt.Truck.OfficeId == null,
                        }
                    ).ToList(),
                    Items = o.Receipt.ReceiptLines
                        .Select(s => new WorkOrderReportItemDto
                        {
                            OrderLineId = s.OrderLineId,
                            LineNumber = s.LineNumber,
                            MaterialQuantity = s.MaterialQuantity,
                            FreightQuantity = s.FreightQuantity,
                            MaterialUomName = s.MaterialUom.Name,
                            FreightUomName = s.FreightUom.Name,
                            FreightUomBaseId = (UnitOfMeasureBaseEnum?)s.FreightUom.UnitOfMeasureBaseId,
                            FreightPricePerUnit = s.FreightRate,
                            MaterialPricePerUnit = s.MaterialRate,
                            FreightPrice = s.FreightAmount,
                            MaterialPrice = s.MaterialAmount,
                            IsFreightTotalOverridden = s.IsFreightAmountOverridden,
                            IsMaterialTotalOverridden = s.IsMaterialAmountOverridden,
                            Designation = s.Designation,
                            LoadAtName = s.LoadAt.DisplayName,
                            DeliverToName = s.DeliverTo.DisplayName,
                            FreightItemName = s.FreightItem.Name,
                            MaterialItemName = s.MaterialItem.Name,
                            IsTaxable = s.FreightItem.IsTaxable,
                            IsFreightTaxable = s.FreightItem.IsTaxable,
                            IsMaterialTaxable = s.MaterialItem.IsTaxable,
                            JobNumber = s.JobNumber,
                            Note = s.OrderLine.Note,
                            NumberOfTrucks = s.OrderLine.NumberOfTrucks ?? 0,
                            TimeOnJob = s.OrderLine.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? s.OrderLine.FirstStaggeredTimeOnJob : s.OrderLine.TimeOnJob,
                            IsTimeStaggered = s.OrderLine != null && s.OrderLine.StaggeredTimeKind != StaggeredTimeKind.None,
                            OrderLineVehicleCategories = s.OrderLine.OrderLineVehicleCategories
                                .Select(vc => vc.VehicleCategory.Name)
                                .ToList(),
                        }).ToList(),
                    DeliveryInfoItems = o.Order.OrderLines
                        .SelectMany(x => x.Tickets)
                        .Select(x => new WorkOrderReportDeliveryInfoDto
                        {
                            OrderLineId = x.OrderLineId,
                            TicketNumber = x.TicketNumber,
                            TruckNumber = x.TruckCode,
                            DriverName = x.Driver == null ? null : x.Driver.LastName + ", " + x.Driver.FirstName,
                            FreightQuantity = x.FreightQuantity,
                            MaterialQuantity = x.MaterialQuantity,
                            FreightUomName = x.FreightUom.Name,
                            MaterialUomName = x.MaterialUom.Name,
                            TicketPhotoId = x.TicketPhotoId,
                            TicketPhotoFilename = x.TicketPhotoFilename,
                            Load = x.Load == null ? null : new WorkOrderReportLoadDto
                            {
                                LoadTime = x.Load.SourceDateTime,
                                DeliveryTime = x.Load.DestinationDateTime,
                                TravelTime = x.Load.TravelTime,
                                SignatureName = x.Load.SignatureName,
                                SignatureId = x.Load.SignatureId,
                            },
                        }).ToList(),
                });

            return newQuery;
        }

        public static IQueryable<WorkOrderReportDto> GetWorkOrderReportDtoQuery(this IQueryable<Order> query, GetWorkOrderReportInput input, int officeId)
        {
            var newQuery = query
                .Select(o => new
                {
                    Order = o,
                    Payment = o.OrderPayments
                        .Where(x => x.OfficeId == officeId)
                        .Select(x => x.Payment)
                        .FirstOrDefault(x => !x.IsCancelledOrRefunded),
                })
                .Select(o => new WorkOrderReportDto
                {
                    Id = o.Order.Id,
                    ContactEmail = o.Order.CustomerContact.Email,
                    ContactPhoneNumber = o.Order.CustomerContact.PhoneNumber,
                    ContactName = o.Order.CustomerContact.Name,
                    CustomerName = o.Order.Customer.Name,
                    CustomerAccountNumber = o.Order.Customer.AccountNumber,
                    ChargeTo = o.Order.ChargeTo,
                    CodTotal = o.Order.CODTotal,
                    OrderDeliveryDate = o.Order.DeliveryDate,
                    OrderShift = o.Order.Shift,
                    OrderIsPending = o.Order.IsPending,
                    OfficeId = o.Order.Office.Id,
                    OfficeName = o.Order.Office.Name,
                    Directions = o.Order.Directions,
                    FreightTotal = o.Order.FreightTotal,
                    MaterialTotal = o.Order.MaterialTotal,
                    PoNumber = o.Order.PONumber,
                    SpectrumNumber = o.Order.SpectrumNumber,
                    SalesTaxRate = o.Order.SalesTaxRate,
                    SalesTax = o.Order.SalesTax,
                    AuthorizationDateTime = o.Payment.AuthorizationDateTime,
                    AuthorizationCaptureDateTime = o.Payment.AuthorizationCaptureDateTime,
                    AuthorizationCaptureSettlementAmount = o.Payment.AuthorizationCaptureAmount,
                    AuthorizationCaptureTransactionId = o.Payment.AuthorizationCaptureTransactionId,
                    AllTrucksNonDistinct = o.Order.OrderLines.SelectMany(ol => ol.OrderLineTrucks).Select(olt =>
                        new WorkOrderReportDto.TruckDriverDto
                        {
                            TruckId = olt.TruckId,
                            TruckCode = olt.Truck.TruckCode,
                            DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                            AssetType = olt.Truck.VehicleCategory.AssetType,
                            IsPowered = olt.Truck.VehicleCategory.IsPowered,
                            IsLeased = olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule || olt.Truck.OfficeId == null,
                        }
                    ).ToList(),
                    Items = o.Order.OrderLines
                        .Select(s => new WorkOrderReportItemDto
                        {
                            OrderLineId = s.Id,
                            LineNumber = s.LineNumber,
                            MaterialQuantity = s.MaterialQuantity,
                            FreightQuantity = s.FreightQuantity,
                            ActualFreightQuantity = s.Tickets.Where(t => !t.NonbillableFreight).Sum(t => t.FreightQuantity ?? 0),
                            ActualMaterialQuantity = s.Tickets.Where(t => !t.NonbillableMaterial).Sum(t => t.MaterialQuantity ?? 0),
                            MaterialUomName = s.MaterialUom.Name,
                            FreightUomName = s.FreightUom.Name,
                            FreightUomBaseId = (UnitOfMeasureBaseEnum?)s.FreightUom.UnitOfMeasureBaseId,
                            FreightPricePerUnit = s.FreightPricePerUnit,
                            MaterialPricePerUnit = s.MaterialPricePerUnit,
                            FreightPrice = s.FreightPrice,
                            MaterialPrice = s.MaterialPrice,
                            IsFreightTotalOverridden = s.IsFreightPriceOverridden,
                            IsMaterialTotalOverridden = s.IsMaterialPriceOverridden,
                            Designation = s.Designation,
                            LoadAtName = s.LoadAt.DisplayName,
                            DeliverToName = s.DeliverTo.DisplayName,
                            FreightItemName = s.FreightItem.Name,
                            MaterialItemName = s.MaterialItem.Name,
                            IsTaxable = s.FreightItem.IsTaxable,
                            IsFreightTaxable = s.FreightItem.IsTaxable,
                            IsMaterialTaxable = s.MaterialItem.IsTaxable,
                            JobNumber = s.JobNumber,
                            Note = s.Note,
                            NumberOfTrucks = s.NumberOfTrucks ?? 0,
                            TimeOnJob = s.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? s.FirstStaggeredTimeOnJob : s.TimeOnJob,
                            IsTimeStaggered = s.StaggeredTimeKind != StaggeredTimeKind.None,
                            OrderLineVehicleCategories = s.OrderLineVehicleCategories
                                .Select(vc => vc.VehicleCategory.Name)
                                .ToList(),
                        }).ToList(),
                    DeliveryInfoItems = o.Order.OrderLines
                        .SelectMany(x => x.Tickets)
                        .Select(x => new WorkOrderReportDeliveryInfoDto
                        {
                            OrderLineId = x.OrderLineId,
                            TicketNumber = x.TicketNumber,
                            TruckNumber = x.TruckCode,
                            DriverName = x.Driver == null ? null : x.Driver.LastName + ", " + x.Driver.FirstName,
                            FreightQuantity = x.FreightQuantity,
                            MaterialQuantity = x.MaterialQuantity,
                            FreightUomName = x.FreightUom.Name,
                            MaterialUomName = x.MaterialUom.Name,
                            TicketPhotoId = x.TicketPhotoId,
                            TicketPhotoFilename = x.TicketPhotoFilename,
                            Load = x.Load == null ? null : new WorkOrderReportLoadDto
                            {
                                LoadTime = x.Load.SourceDateTime,
                                DeliveryTime = x.Load.DestinationDateTime,
                                TravelTime = x.Load.TravelTime,
                                SignatureName = x.Load.SignatureName,
                                SignatureId = x.Load.SignatureId,
                            },
                        }).ToList(),
                });

            return newQuery;
        }

        public static async Task<List<OrderSummaryReportItemDto>> GetOrderSummaryReportItems(this IQueryable<OrderLine> query, IDictionary<Shift, string> shiftDictionary, OrderTaxCalculator taxCalculator, ISettingManager settingManager, bool separateItems)
        {
            var showTrailersOnSchedule = await settingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule);

            var items = await query.Select(o => new OrderSummaryReportItemDto
            {
                OrderId = o.OrderId,
                OrderLineId = o.Id,
                CustomerName = o.Order.Customer.Name,
                SalesTaxRate = o.Order.SalesTaxRate,
                OrderDeliveryDate = o.Order.DeliveryDate,
                OrderShift = o.Order.Shift,
                TimeOnJob = o.TimeOnJob,
                Trucks = o.OrderLineTrucks.Select(olt => new OrderSummaryReportItemDto.ItemOrderLineTruck
                {
                    TruckCode = olt.Truck.TruckCode,
                    TrailerTruckCode = showTrailersOnSchedule ? olt.Trailer.TruckCode : null,
                    DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                }).ToList(),
                NumberOfTrucks = o.NumberOfTrucks,
                LoadAtName = o.LoadAt.DisplayName,
                DeliverToName = o.DeliverTo.DisplayName,
                Item = new OrderSummaryReportItemDto.ItemOrderLine
                {
                    MaterialQuantity = o.MaterialQuantity,
                    FreightQuantity = o.FreightQuantity,
                    MaterialPrice = o.MaterialPrice,
                    FreightPrice = o.FreightPrice,
                    FreightItemName = o.FreightItem.Name,
                    MaterialItemName = o.MaterialItem.Name,
                    IsTaxable = o.FreightItem.IsTaxable,
                    IsFreightTaxable = o.FreightItem.IsTaxable,
                    IsMaterialTaxable = o.MaterialItem.IsTaxable,
                    NumberOfTrucks = o.NumberOfTrucks ?? 0,
                    Designation = o.Designation,
                    MaterialUomName = o.MaterialUom.Name,
                    FreightUomName = o.FreightUom.Name,
                },
            })
            .OrderBy(item => item.CustomerName).ThenBy(item => item.TimeOnJob)
            .ToListAsync();

            var taxCalculationType = await taxCalculator.GetTaxCalculationTypeAsync();

            items.ForEach(x =>
            {
                //if (x.Items.Count > 1)
                //    // ReSharper disable once CompareOfFloatsByEqualityOperator
                //    x.Items.RemoveAll(s => (s.MaterialQuantity ?? 0) == 0 && (s.FreightQuantity ?? 0) == 0 && s.NumberOfTrucks == 0);
                x.OrderShiftName = x.OrderShift.HasValue && shiftDictionary.TryGetValue(x.OrderShift.Value, out var value) ? value : "";
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, x.Item, x.SalesTaxRate, separateItems);
            });

            return items;
        }

        public static List<ScheduleTruckDto> OrderByTruck(this IEnumerable<ScheduleTruckDto> list)
        {
            return list
                .OrderByDescending(x => !x.IsExternal)
                .ThenByDescending(x => x.VehicleCategory.IsPowered)
                //.ThenBy(x => x.VehicleCategory.SortOrder)
                .ThenBy(x => x.TruckCode)
                .ToList();
        }

        public static bool IsQuantityValid(this OrderLine orderLine)
        {
            return orderLine.MaterialQuantity > 0 || orderLine.FreightQuantity > 0 || !(orderLine.FreightPrice > 0 || orderLine.MaterialPrice > 0);
        }

        public static void RemoveStaggeredTimeIfNeeded(this OrderLine orderLine)
        {
            if (orderLine.Id == 0 || orderLine.StaggeredTimeKind == StaggeredTimeKind.None || orderLine.NumberOfTrucks > 1)
            {
                return;
            }

            orderLine.StaggeredTimeKind = StaggeredTimeKind.None;
            orderLine.StaggeredTimeInterval = null;
            orderLine.FirstStaggeredTimeOnJob = null;
        }

        public static async Task EnsureCanEditTicket(this IRepository<Ticket> ticketRepository, int? ticketId)
        {
            var cannotEditReason = await ticketRepository.GetCannotEditTicketReason(ticketId);
            if (!string.IsNullOrEmpty(cannotEditReason))
            {
                throw new UserFriendlyException(cannotEditReason);
            }
        }

        public static async Task<string> GetCannotEditTicketReason(this IRepository<Ticket> ticketRepository, int? ticketId)
        {
            if (ticketId > 0 && await (await ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.Id == ticketId
                    && x.InvoiceLine != null))
            {
                return "You can't edit already invoiced tickets";
            }

            if (ticketId > 0 && await (await ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.Id == ticketId
                    && x.PayStatementTickets.Any()))
            {
                return "You can't edit tickets that were added to pay statements";
            }

            if (ticketId > 0 && await (await ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.Id == ticketId
                    && x.LeaseHaulerStatementTicket != null))
            {
                return "You can't edit tickets that were added to lease hauler statements";
            }

            if (ticketId > 0 && await (await ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.Id == ticketId
                    && x.ReceiptLineId != null))
            {
                return "You can't edit tickets that were added to receipts";
            }

            return null;
        }

        public static async Task<bool> CanOverrideTotals(this IRepository<OrderLine> orderLineRepository, int orderLineId, int officeId)
        {
            return !await (await orderLineRepository.GetQueryAsync())
                .AnyAsync(x => x.Id == orderLineId
                    && x.Tickets.Any(a => a.OfficeId != officeId));
        }

        public static async Task<bool> IsEntityDeleted<T>(this IRepository<T> repository, EntityDto input, IActiveUnitOfWork uow) where T : Entity<int>, ISoftDelete
        {
            using (uow.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var result = await (await repository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new
                    {
                        x.IsDeleted,
                    })
                    .FirstOrDefaultAsync();

                return result?.IsDeleted == true;
            }
        }

        public static async Task<T> GetMaybeDeletedEntity<T>(this IRepository<T> repository, EntityDto input, IActiveUnitOfWork uow) where T : Entity<int>, ISoftDelete
        {
            T entity;
            using (uow.DisableFilter(AbpDataFilters.SoftDelete))
            {
                entity = await (await repository.GetQueryAsync()).FirstOrDefaultAsync(x => x.Id == input.Id);
            }

            return entity;
        }

        public static async Task<T> GetDeletedEntity<T>(this IRepository<T> repository, EntityDto input, IActiveUnitOfWork uow) where T : Entity<int>, ISoftDelete
        {
            var entity = await repository.GetMaybeDeletedEntity(input, uow);

            if (entity != null && entity.IsDeleted)
            {
                return entity;
            }

            return null;
        }

        public static async Task<CultureInfo> GetCurrencyCultureAsync(this ISettingManager settingManager)
        {
            var currencySymbol = await settingManager.GetSettingValueAsync(AppSettings.General.CurrencySymbol);

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = currencySymbol;
            return culture;
        }
    }
}
