using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.BackgroundJobs.Dto;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.DailyFuelCosts;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.Items;
using DispatcherWeb.Items.Dto;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Net.MimeTypes;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Sessions;
using DispatcherWeb.Storage;
using DispatcherWeb.TaxRates;
using DispatcherWeb.TempFiles;
using DispatcherWeb.TempFiles.Dto;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.Tickets.Exporting;
using DispatcherWeb.Tickets.Reports;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Tickets
{
    [AbpAuthorize]
    public class TicketAppService : DispatcherWebAppServiceBase, ITicketAppService
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IRepository<DailyFuelCost> _dailyFuelCostRepository;
        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly ITicketQuantityHelper _ticketQuantityHelper;
        private readonly ListCacheCollection _listCaches;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITicketListCsvExporter _ticketListCsvExporter;
        private readonly IItemAppService _serviceAppService;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly TicketPrintOutGenerator _ticketPrintOutGenerator;
        private readonly ILogoProvider _logoProvider;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ITempFileAppService _tempFilesService;

        public TicketAppService(
            IRepository<Ticket> ticketRepository,
            IRepository<Truck> truckRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Driver> driverRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IRepository<DailyFuelCost> dailyFuelCostRepository,
            IRepository<TaxRate> taxRateRepository,
            OrderTaxCalculator orderTaxCalculator,
            ITicketQuantityHelper ticketQuantityHelper,
            ListCacheCollection listCaches,
            IBinaryObjectManager binaryObjectManager,
            ITicketListCsvExporter ticketListCsvExporter,
            IItemAppService serviceAppService,
            IRepository<TimeOff> timeOffRepository,
            IRepository<Invoice> invoiceRepository,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            TicketPrintOutGenerator ticketPrintOutGenerator,
            ILogoProvider logoProvider,
            IBackgroundJobManager backgroundJobManager,
            ITempFileAppService tempFilesService
        )
        {
            _ticketRepository = ticketRepository;
            _truckRepository = truckRepository;
            _orderLineRepository = orderLineRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _dispatchRepository = dispatchRepository;
            _driverRepository = driverRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
            _dailyFuelCostRepository = dailyFuelCostRepository;
            _taxRateRepository = taxRateRepository;
            _orderTaxCalculator = orderTaxCalculator;
            _ticketQuantityHelper = ticketQuantityHelper;
            _listCaches = listCaches;
            _binaryObjectManager = binaryObjectManager;
            _ticketListCsvExporter = ticketListCsvExporter;
            _serviceAppService = serviceAppService;
            _timeOffRepository = timeOffRepository;
            _invoiceRepository = invoiceRepository;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _ticketPrintOutGenerator = ticketPrintOutGenerator;
            _logoProvider = logoProvider;
            _backgroundJobManager = backgroundJobManager;
            _tempFilesService = tempFilesService;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<TicketControlVisibilityDto> GetVisibleTicketControls(int orderLineId)
        {
            return await _ticketQuantityHelper.GetVisibleTicketControls(orderLineId);
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<EditOrderTicketOutput> EditOrderTicket(OrderTicketEditDto model)
        {
            await _ticketRepository.EnsureCanEditTicket(model.Id);
            string resultWarningText = null;

            var permissions = new
            {
                EditTickets = await IsGrantedAsync(AppPermissions.Pages_Tickets_Edit),
                EditLeaseHaulerPortalTickets = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Tickets),
            };

            Ticket ticket;
            if (model.Id != 0)
            {
                ticket = await _ticketRepository.GetAsync(model.Id);
                var ticketData = await (await _ticketRepository.GetQueryAsync())
                    .Where(x => x.Id == model.Id)
                    .Select(x => new
                    {
                        LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    }).FirstAsync();
                var oldLeaseHaulerId = ticketData.LeaseHaulerId ?? ticket.CarrierId;
                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_Tickets_Edit,
                    AppPermissions.LeaseHaulerPortal_Tickets,
                    Session.LeaseHaulerId,
                    oldLeaseHaulerId);
            }
            else
            {
                await EnsureCanAddTickets(model.OrderLineId, Session.OfficeId);
                ticket = new Ticket
                {
                    Id = model.Id,
                };
            }

            var originalTruckId = ticket.TruckId;
            var truckWasChanged = model.TruckId != ticket.TruckId;
            var driverWasChanged = ticket.DriverId != model.DriverId;
            var trailerWasChanged = ticket.TrailerId != model.TrailerId;
            var originalDriverId = ticket.DriverId;
            var originalQuantities = new
            {
                ticket.FreightQuantity,
                ticket.MaterialQuantity,
            };

            ticket.OrderLineId = model.OrderLineId;
            ticket.TicketNumber = model.TicketNumber;

            await _ticketQuantityHelper.SetTicketQuantity(ticket, model);

            ticket.TruckId = model.TruckId;
            ticket.TruckCode = model.TruckCode;
            ticket.TrailerId = model.TrailerId;
            ticket.DriverId = model.DriverId;

            var orderLineData = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == model.OrderLineId)
                .Select(ol => new
                {
                    ol.OrderId,
                    ol.Designation,
                    OrderOfficeId = ol.Order.OfficeId,
                    ol.Order.DeliveryDate,
                    ol.Order.Shift,
                    ol.Order.CustomerId,
                    ol.LoadAtId,
                    ol.DeliverToId,
                })
                .FirstAsync();

            ticket.OfficeId = orderLineData.OrderOfficeId;

            var validateDriverAndTruckOnTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ValidateDriverAndTruckOnTickets);
            if (truckWasChanged && ticket.TruckId.HasValue)
            {
                var ticketDriverAndTrailer = await GetDriverAndTrailerForTicketTruck(new GetDriverAndTrailerForTicketTruckInput
                {
                    OrderLineId = model.OrderLineId,
                    TruckId = ticket.TruckId,
                });

                if (validateDriverAndTruckOnTickets)
                {
                    if (ticketDriverAndTrailer.DriverId.HasValue)
                    {
                        ticket.DriverId = ticketDriverAndTrailer.DriverId;
                        model.DriverId = ticketDriverAndTrailer.DriverId;
                        model.DriverName = ticketDriverAndTrailer.DriverName;
                    }
                    if (ticketDriverAndTrailer.TrailerId.HasValue)
                    {
                        ticket.TrailerId = ticketDriverAndTrailer.TrailerId;
                        model.TrailerId = ticketDriverAndTrailer.TrailerId;
                        model.TrailerTruckCode = ticketDriverAndTrailer.TrailerTruckCode;
                    }

                    driverWasChanged = originalDriverId != ticket.DriverId;
                }

                ticket.CarrierId = ticketDriverAndTrailer.CarrierId;
            }
            else if (driverWasChanged && ticket.DriverId.HasValue)
            {
                var ticketTruckAndTrailer = await GetTruckAndTrailerForTicketDriver(new GetTruckAndTrailerForTicketDriverInput
                {
                    OrderLineId = model.OrderLineId,
                    DriverId = ticket.DriverId.Value,
                });

                if (validateDriverAndTruckOnTickets)
                {
                    if (ticketTruckAndTrailer.TruckId.HasValue)
                    {
                        ticket.TruckId = ticketTruckAndTrailer.TruckId;
                        model.TruckId = ticketTruckAndTrailer.TruckId;
                        model.TruckCode = ticketTruckAndTrailer.TruckCode;
                        ticket.CarrierId = ticketTruckAndTrailer.CarrierId;
                    }
                    if (ticketTruckAndTrailer.TrailerId.HasValue)
                    {
                        ticket.TrailerId = ticketTruckAndTrailer.TrailerId;
                        model.TrailerId = ticketTruckAndTrailer.TrailerId;
                        model.TrailerTruckCode = ticketTruckAndTrailer.TrailerTruckCode;
                    }
                }
            }
            else if (trailerWasChanged && ticket.TrailerId.HasValue)
            {
                var ticketTruckAndDriver = await GetTruckAndDriverForTicketTrailer(new GetTruckAndDriverForTicketTrailerInput
                {
                    OrderLineId = model.OrderLineId,
                    TrailerId = ticket.TrailerId.Value,
                });

                if (validateDriverAndTruckOnTickets)
                {
                    if (ticketTruckAndDriver.TruckId.HasValue)
                    {
                        ticket.TruckId = ticketTruckAndDriver.TruckId;
                        model.TruckId = ticketTruckAndDriver.TruckId;
                        model.TruckCode = ticketTruckAndDriver.TruckCode;
                        ticket.CarrierId = ticketTruckAndDriver.CarrierId;
                    }

                    if (ticketTruckAndDriver.DriverId.HasValue)
                    {
                        ticket.DriverId = ticketTruckAndDriver.DriverId;
                        model.DriverId = ticketTruckAndDriver.DriverId;
                        model.DriverName = ticketTruckAndDriver.DriverName;
                    }
                }
            }
            else
            {
                if ((ticket.Id == 0 || ticket.TruckId == null)
                    && !permissions.EditTickets
                    && permissions.EditLeaseHaulerPortalTickets
                    && Session.LeaseHaulerId.HasValue
                    && ticket.CarrierId == null)
                {
                    ticket.CarrierId = Session.LeaseHaulerId;
                }
            }

            var timezone = await GetTimezone();
            if (model.TicketDateTime.HasValue)
            {
                DateTime dateToUse;
                if (ticket.TicketDateTime.HasValue)
                {
                    dateToUse = ticket.TicketDateTime.Value.ConvertTimeZoneTo(timezone).Date;
                }
                else
                {
                    dateToUse = orderLineData.DeliveryDate.Date;
                }
                var newDateWithTime = dateToUse.Add(model.TicketDateTime.Value.TimeOfDay);
                ticket.TicketDateTime = newDateWithTime.ConvertTimeZoneFrom(timezone);
            }
            else
            {
                ticket.TicketDateTime = orderLineData.DeliveryDate.ConvertTimeZoneFrom(timezone);
            }
            ticket.CustomerId = orderLineData.CustomerId;
            ticket.LoadAtId = orderLineData.LoadAtId;
            ticket.DeliverToId = orderLineData.DeliverToId;
            if (ticket.Id == 0)
            {
                ticket.NonbillableFreight = !orderLineData.Designation.HasFreight();
                ticket.NonbillableMaterial = !orderLineData.Designation.HasMaterial();
            }

            if (ticket.TruckId != originalTruckId)
            {
                int? newLeaseHaulerId = null;
                if (ticket.TruckId.HasValue)
                {
                    var truckData = await (await _truckRepository.GetQueryAsync())
                        .Where(x => x.Id == ticket.TruckId)
                        .Select(x => new
                        {
                            LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                        }).FirstAsync();
                    newLeaseHaulerId = truckData.LeaseHaulerId;
                }
                else
                {
                    newLeaseHaulerId = ticket.CarrierId;
                }
                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_Tickets_Edit,
                    AppPermissions.LeaseHaulerPortal_Tickets,
                    Session.LeaseHaulerId,
                    newLeaseHaulerId);
            }

            model.Id = await _ticketRepository.InsertOrUpdateAndGetIdAsync(ticket);

            await CurrentUnitOfWork.SaveChangesAsync();
            var taxDetails = await _orderTaxCalculator.CalculateTotalsAsync(orderLineData.OrderId);
            var quantityWasChanged = ticket.FreightQuantity != originalQuantities.FreightQuantity
                                     || ticket.MaterialQuantity != originalQuantities.MaterialQuantity;
            if (quantityWasChanged)
            {
                await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
            }

            return new EditOrderTicketOutput
            {
                OrderTaxDetails = new OrderTaxDetailsDto(taxDetails),
                Ticket = model,
                WarningText = resultWarningText,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<GetDriverAndTrailerForTicketTruckResult> GetDriverAndTrailerForTicketTruck(GetDriverAndTrailerForTicketTruckInput input)
        {
            if (!input.ValidateInput())
            {
                return new GetDriverAndTrailerForTicketTruckResult();
            }

            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets);

            var order = input.OrderLineId.HasValue ? (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                    x.Order.OfficeId,
                }).FirstOrDefault() : null;

            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .WhereIf(order != null, x =>
                    x.OrderLine.Order.DeliveryDate == order.DeliveryDate
                    && x.OrderLine.Order.Shift == order.Shift
                    && x.OrderLine.Order.OfficeId == order.OfficeId)
                .WhereIf(input.OrderDate.HasValue, x => x.OrderLine.Order.DeliveryDate == input.OrderDate)
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(!string.IsNullOrEmpty(input.TruckCode), x => x.Truck.TruckCode == input.TruckCode)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .Where(x => x.DriverId.HasValue)
                .Select(x => new
                {
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    x.TrailerId,
                    TrailerTruckCode = x.Trailer.TruckCode,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                })
                .ToListAsync();

            var result = new GetDriverAndTrailerForTicketTruckResult();

            if (orderLineTrucks.Select(x => x.TrailerId).Distinct().Count() == 1)
            {
                result.TrailerId = orderLineTrucks[0].TrailerId;
                result.TrailerTruckCode = orderLineTrucks[0].TrailerTruckCode;
            }

            if (orderLineTrucks.Select(x => x.DriverId).Distinct().Count() == 1)
            {
                result.DriverId = orderLineTrucks[0].DriverId;
                result.DriverName = orderLineTrucks[0].DriverName;
                result.CarrierId = orderLineTrucks[0].LeaseHaulerId;
                result.CarrierName = orderLineTrucks[0].LeaseHaulerName;
                result.TruckCodeIsCorrect = true;
            }
            else
            {
                var truckDetails = await (await _truckRepository.GetQueryAsync())
                    .WhereIf(input.TruckId.HasValue, x => x.Id == input.TruckId)
                    .WhereIf(!string.IsNullOrEmpty(input.TruckCode), x => x.TruckCode == input.TruckCode)
                    .Select(x => new
                    {
                        //DefaultDriverId = x.DefaultDriverId,
                        //DefaultDriverName = x.DefaultDriver.FirstName + " " + x.DefaultDriver.LastName,
                        LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                        LeaseHaulerName = x.LeaseHaulerTruck.LeaseHauler.Name,
                    }).FirstOrDefaultAsync();

                result.CarrierId = truckDetails?.LeaseHaulerId;
                result.CarrierName = truckDetails?.LeaseHaulerName;
                result.TruckCodeIsCorrect = truckDetails != null;
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<GetTruckAndTrailerForTicketDriverResult> GetTruckAndTrailerForTicketDriver(GetTruckAndTrailerForTicketDriverInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets);

            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .WhereIf(input.OrderLineId.HasValue, x => x.OrderLineId == input.OrderLineId)
                .WhereIf(input.OrderDate.HasValue, x => x.OrderLine.Order.DeliveryDate == input.OrderDate)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .Where(x => x.DriverId == input.DriverId)
                .Select(x => new
                {
                    x.TruckId,
                    x.Truck.TruckCode,
                    x.TrailerId,
                    TrailerTruckCode = x.Trailer.TruckCode,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                })
                .ToListAsync();

            var result = new GetTruckAndTrailerForTicketDriverResult();

            if (orderLineTrucks.Select(x => x.TruckId).Distinct().Count() == 1)
            {
                result.TruckId = orderLineTrucks[0].TruckId;
                result.TruckCode = orderLineTrucks[0].TruckCode;
                result.CarrierId = orderLineTrucks[0].LeaseHaulerId;
                result.CarrierName = orderLineTrucks[0].LeaseHaulerName;
            }

            if (orderLineTrucks.Select(x => x.TrailerId).Distinct().Count() == 1)
            {
                result.TrailerId = orderLineTrucks[0].TrailerId;
                result.TrailerTruckCode = orderLineTrucks[0].TrailerTruckCode;
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<GetTruckAndDriverForTicketTrailerResult> GetTruckAndDriverForTicketTrailer(GetTruckAndDriverForTicketTrailerInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets);

            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .WhereIf(input.OrderLineId.HasValue, x => x.OrderLineId == input.OrderLineId)
                .WhereIf(input.OrderDate.HasValue, x => x.OrderLine.Order.DeliveryDate == input.OrderDate)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .Where(x => x.TrailerId == input.TrailerId)
                .Select(x => new
                {
                    x.TruckId,
                    x.Truck.TruckCode,
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                })
                .ToListAsync();

            var result = new GetTruckAndDriverForTicketTrailerResult();

            if (orderLineTrucks.Select(x => x.TruckId).Distinct().Count() == 1)
            {
                result.TruckId = orderLineTrucks[0].TruckId;
                result.TruckCode = orderLineTrucks[0].TruckCode;
                result.CarrierId = orderLineTrucks[0].LeaseHaulerId;
                result.CarrierName = orderLineTrucks[0].LeaseHaulerName;
            }

            if (orderLineTrucks.Select(x => x.DriverId).Distinct().Count() == 1)
            {
                result.DriverId = orderLineTrucks[0].DriverId;
                result.DriverName = orderLineTrucks[0].DriverName;
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.CustomerPortal_TicketList)]
        public async Task<TicketEditDto> GetTicketEditDto(NullableIdDto input)
        {
            TicketEditDto ticket;
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            if (input.Id.HasValue)
            {
                ticket = await (await _ticketRepository.GetQueryAsync())
                    .Select(t => new TicketEditDto
                    {
                        Id = t.Id,
                        OrderLineId = t.OrderLineId,
                        OrderLineDesignation = t.OrderLine.Designation,
                        OrderLineIsProductionPay = t.OrderLine.ProductionPay && allowProductionPay,
                        OrderDate = t.OrderLine.Order.DeliveryDate,
                        TicketNumber = t.TicketNumber,
                        TicketDateTime = t.TicketDateTime,
                        Shift = t.Shift ?? t.OrderLine.Order.Shift,
                        CustomerId = t.CustomerId,
                        CustomerName = t.Customer != null ? t.Customer.Name : "",
                        CarrierId = t.CarrierId,
                        CarrierName = t.Carrier != null ? t.Carrier.Name : "",
                        FreightQuantity = t.FreightQuantity,
                        MaterialQuantity = t.MaterialQuantity,
                        FreightItemId = t.FreightItemId,
                        FreightItemName = t.FreightItem.Name,
                        MaterialItemId = t.MaterialItemId,
                        MaterialItemName = t.MaterialItem.Name,
                        TruckCode = t.Truck.TruckCode ?? t.TruckCode,
                        TruckId = t.TruckId,
                        TrailerId = t.TrailerId,
                        TrailerTruckCode = t.Trailer.TruckCode,
                        DriverId = t.DriverId,
                        DriverName = t.Driver.FirstName + " " + t.Driver.LastName,
                        FreightUomId = t.FreightUomId,
                        FreightUomName = t.FreightUom.Name,
                        MaterialUomId = t.MaterialUomId,
                        MaterialUomName = t.MaterialUom.Name,
                        LoadAtId = t.LoadAtId,
                        LoadAtName = t.LoadAt.DisplayName,
                        DeliverToId = t.DeliverToId,
                        DeliverToName = t.DeliverTo.DisplayName,
                        LoadCount = t.LoadCount,
                        NonbillableFreight = t.NonbillableFreight,
                        NonbillableMaterial = t.NonbillableMaterial,
                        IsVerified = t.IsVerified,
                        IsBilled = t.IsBilled,
                        ReceiptLineId = t.ReceiptLineId,
                        TicketPhotoId = t.TicketPhotoId,
                        IsInternal = t.IsInternal,
                        IsReadOnly = t.InvoiceLine != null //already invoiced
                                || t.PayStatementTickets.Any() //already added to pay statements
                                || t.ReceiptLineId != null //already added to receipts
                                || t.LeaseHaulerStatementTicket != null, //added to lease hauler statements
                        HasPayStatements = t.PayStatementTickets.Any(),
                        HasLeaseHaulerStatements = t.LeaseHaulerStatementTicket != null,
                    })
                    .FirstAsync(t => t.Id == input.Id.Value);

                ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(await GetTimezone());
                ticket.CannotEditReason = await _ticketRepository.GetCannotEditTicketReason(input.Id.Value);
            }
            else
            {
                throw new NotImplementedException(
                    "We cannot decide which controls are visible if OrderLineId is unavailable. This modal does not support creating new tickets as of now, only editing existing tickets");
            }

            if (ticket.OrderLineId == null)
            {
                throw new UserFriendlyException("Cannot edit historical tickets not associated with the job");
            }
            ticket.VisibleTicketControls = await _ticketQuantityHelper.GetVisibleTicketControls(ticket.OrderLineId.Value);

            await CheckCustomerSpecificPermissions(AppPermissions.Pages_Tickets_Edit, AppPermissions.CustomerPortal_TicketList, ticket.CustomerId);

            return ticket;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<TicketEditDto> EditTicket(TicketEditDto model)
        {
            if (model.OrderLineId.HasValue)
            {
                await EnsureCanAddTickets(model.OrderLineId.Value, Session.OfficeId);
            }
            await _ticketRepository.EnsureCanEditTicket(model.Id);

            Ticket ticket;
            if (model.Id != 0)
            {
                ticket = await _ticketRepository.GetAsync(model.Id);
            }
            else
            {
                ticket = new Ticket();
            }

            if (model.OrderLineId.HasValue && ticket.Id == 0)
            {
                var orderLineData = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == model.OrderLineId)
                    .Select(x => new
                    {
                        OrderOfficeId = x.Order.OfficeId,
                    }).FirstAsync();

                ticket.OfficeId = orderLineData.OrderOfficeId;
            }

            var oldQuantities = new
            {
                ticket.FreightQuantity,
                ticket.MaterialQuantity,
            };

            ticket.OrderLineId = model.OrderLineId;
            ticket.TicketNumber = model.TicketNumber;
            ticket.TicketDateTime = model.TicketDateTime?.ConvertTimeZoneFrom(await GetTimezone());
            ticket.Shift = model.OrderLineId.HasValue ? null : model.Shift;
            ticket.CustomerId = model.CustomerId;
            ticket.CarrierId = model.CarrierId;
            ticket.TruckCode = model.TruckCode;
            ticket.TruckId = await GetTruckId(model.TruckCode);
            ticket.TrailerId = model.TrailerId;
            ticket.DriverId = model.DriverId;
            ticket.DeliverToId = model.DeliverToId;
            ticket.LoadAtId = model.LoadAtId;
            ticket.LoadCount = model.LoadCount;
            ticket.NonbillableFreight = model.NonbillableFreight;
            ticket.NonbillableMaterial = model.NonbillableMaterial;
            ticket.IsVerified = model.IsVerified;
            ticket.IsBilled = model.IsBilled;

            await _ticketQuantityHelper.SetTicketQuantity(ticket, model);

            if (ticket.TruckId == null && !(model.CarrierId > 0) && !await SettingManager.AllowCounterSalesForTenant())
            {
                throw new UserFriendlyException($"Invalid truck number");
            }
            if (ticket.DriverId == null && !(model.CarrierId > 0) && !await SettingManager.AllowCounterSalesForTenant())
            {
                throw new UserFriendlyException("Driver is required");
            }
            model.Id = await _ticketRepository.InsertOrUpdateAndGetIdAsync(ticket);

            if (model.OrderLineId.HasValue)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsForOrderLineAsync(model.OrderLineId.Value);
                var quantityWasChanged = ticket.FreightQuantity != oldQuantities.FreightQuantity
                                         || ticket.MaterialQuantity != oldQuantities.MaterialQuantity;
                if (quantityWasChanged)
                {
                    await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
                }
            }

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<AddTicketPhotoResult> AddTicketPhoto(AddTicketPhotoInput input)
        {
            //ticket photo can always be added, even if invoiced, on pay statements, LH statements, or receipts
            //await _ticketRepository.EnsureCanEditTicket(input.TicketId);

            var ticket = await _ticketRepository.GetAsync(input.TicketId);
            var ticketData = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == input.TicketId)
                .Select(x => new
                {
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                }).FirstAsync();

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_Tickets_Edit,
                AppPermissions.LeaseHaulerPortal_Tickets,
                Session.LeaseHaulerId,
                ticketData.LeaseHaulerId);

            var bytes = _binaryObjectManager.GetBytesFromUriString(input.TicketPhoto);
            if (!(bytes?.Length > 0))
            {
                throw new UserFriendlyException("Ticket image is required");
            }

            if (input.TicketPhotoFilename?.EndsWith(".pdf") == true
                && await FeatureChecker.IsEnabledAsync(AppFeatures.ConvertReceivedPdfTicketImagesToJpgBeforeStoring))
            {
                input.TicketPhotoFilename = input.TicketPhotoFilename[..^4] + ".jpg";
                bytes = ReportExtensions.ConvertPdfTicketImageToJpg(bytes, AppConsts.TicketPhotoFullPageWidthCm);
                if (!(bytes?.Length > 0))
                {
                    throw new UserFriendlyException("Unable to convert PDF to JPG. Please try to upload a different PDF or an image");
                }
            }

            var dataId = await _binaryObjectManager.UploadByteArrayAsync(bytes, await AbpSession.GetTenantIdAsync());

            if (ticket.TicketPhotoId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(ticket.TicketPhotoId.Value);
            }
            ticket.TicketPhotoId = dataId;
            ticket.TicketPhotoFilename = input.TicketPhotoFilename;

            return new AddTicketPhotoResult
            {
                TicketPhotoId = dataId,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task DeleteTicketPhoto(DeleteTicketPhotoInput input)
        {
            //ticket photo can always be deleted, even if invoiced, on pay statements, LH statements, or receipts
            //await _ticketRepository.EnsureCanEditTicket(input.TicketId);

            var ticket = await _ticketRepository.GetAsync(input.TicketId);
            var ticketData = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == input.TicketId)
                .Select(x => new
                {
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                }).FirstAsync();
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_Tickets_Edit,
                AppPermissions.LeaseHaulerPortal_Tickets,
                Session.LeaseHaulerId,
                ticketData.LeaseHaulerId);
            if (ticket.TicketPhotoId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(ticket.TicketPhotoId.Value);
            }
            ticket.TicketPhotoId = null;
            ticket.TicketPhotoFilename = null;
        }

        private async Task<TicketPhotoDto> GetTicketPhotoDto(int ticketId)
        {
            var ticket = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == ticketId)
                .Select(x => new
                {
                    TicketPhotoData = new TicketPhotoDataDto
                    {
                        TicketId = x.Id,
                        TicketNumber = x.TicketNumber,
                        TicketDateTime = x.TicketDateTime,
                        TicketPhotoFilename = x.TicketPhotoFilename,
                        TicketPhotoId = x.TicketPhotoId,
                        IsInternal = x.IsInternal,
                        LoadAtName = x.LoadAt.Name,
                    },
                    x.CustomerId,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                }).FirstOrDefaultAsync();

            if (ticket?.TicketPhotoData.TicketPhotoId == null)
            {
                return new TicketPhotoDto();
            }

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_Tickets_View,
                AppPermissions.LeaseHaulerPortal_Tickets,
                Session.LeaseHaulerId,
                new[] { ticket.LeaseHaulerId },
                async () =>
                {
                    await CheckCustomerSpecificPermissions(
                        AppPermissions.Pages_Tickets_View,
                        AppPermissions.CustomerPortal_TicketList,
                        ticket?.CustomerId
                    );
                }
            );

            var image = await _binaryObjectManager.GetOrNullAsync(ticket.TicketPhotoData.TicketPhotoId.Value);
            if (image?.Bytes?.Length > 0)
            {
                var imageFilename = GenerateTicketFilename(ticket.TicketPhotoData, await GetTimezone());

                return new TicketPhotoDto
                {
                    FileBytes = image.Bytes,
                    FileName = imageFilename,
                    MimeType = imageFilename?.ToLowerInvariant().EndsWith(".pdf") == true
                        ? MimeTypeNames.ApplicationPdf
                        : MimeTypeNames.ImageJpeg,
                };
            }
            return new TicketPhotoDto();
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<TicketPhotoDto> GetTicketPhoto(int ticketId)
        {
            var ticketPhoto = await GetTicketPhotoDto(ticketId);
            if (ticketPhoto?.FileBytes == null || string.IsNullOrEmpty(ticketPhoto.FileName))
            {
                return null;
            }

            if (ticketPhoto.FileName.ToLowerInvariant().EndsWith(".pdf"))
            {
                return ticketPhoto;
            }

            if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.Tickets.PrintPdfTicket))
            {
                return await GeneratePdfFromImage(ticketPhoto);
            }

            return new TicketPhotoDto
            {
                FileBytes = ticketPhoto.FileBytes,
                FileName = ticketPhoto.FileName,
                MimeType = MimeTypeNames.ImageJpeg,
            };
        }

        private async Task<TicketPhotoDto> GeneratePdfFromImage(TicketPhotoDto ticketPhoto)
        {
            var pdfFileBytes = await Task.Run(() =>
                ReportExtensions.GenerateImagesPdf([ticketPhoto])
            );
            if (pdfFileBytes.Length > 0)
            {
                var pdfFileName = $"{ticketPhoto.FileName.SanitizeFilename()}.pdf";
                return new TicketPhotoDto
                {
                    FileBytes = pdfFileBytes,
                    FileName = pdfFileName,
                    MimeType = MimeTypeNames.ApplicationPdf,
                };
            }
            return new TicketPhotoDto();
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Tickets_Download)]
        public async Task<FileDto> GetTicketsPhotos(GetTicketsPhotosInput input)
        {
            var result = new FileDto();
            var ticketsIds = await (await _ticketRepository.GetQueryAsync())
                .Where(x => input.TicketIds.Contains(x.Id) && (x.TicketPhotoId != null || x.IsInternal))
                .Select(x => x.Id).ToListAsync();

            if (!ticketsIds.Any())
            {
                result.WarningMessage = "Selected tickets do not have images";
                return result;
            }
            int maximumTicketCount;
            int.TryParse(await FeatureChecker.GetValueAsync(AppFeatures.MaximumNumberOfTicketsPerDownload), out maximumTicketCount);
            if (ticketsIds.Count > maximumTicketCount)
            {
                result.WarningMessage = $"There were more than {maximumTicketCount} tickets. Please select fewer tickets before running again.";
                return result;
            }

            await _backgroundJobManager.EnqueueAsync<TicketPhotoDownloadJob, TicketPhotoDownloadJobArgs>(new TicketPhotoDownloadJobArgs
            {
                TicketIds = ticketsIds,
                RequestorUser = await Session.ToUserIdentifierAsync(),
                SuccessMessage = "Your ticket file has been processed.",
                FailedMessage = "Failed to generate PDF for ticket images",
                FileName = "TicketsImages",
            });
            result.SuccessMessage = "This may take a while depending on the number of tickets selected. You may continue working and we’ll send you a notification with a link to the file when it has been processed";
            return result;
        }

        [RemoteService(false)]
        public async Task GenerateTicketImagesPdf(GenerateTicketImagesInput input)
        {
            var ticketImages = new List<FileBytesDto>();
            foreach (var ticket in input.Tickets)
            {
                var ticketImage = await GetTicketImageOrPrintoutOrDefault(ticket);
                if (ticketImage != null)
                {
                    ticketImages.Add(ticketImage);
                }
            }
            var fileBytes = ReportExtensions.GenerateImagesPdf(ticketImages);
            var processTempFileInput = new ProcessTempFileInput
            {
                FileBytes = fileBytes,
                FileName = input.FileName + ".pdf",
                MimeType = MimeTypeNames.ApplicationPdf,
                Message = input.SuccessMessage,
            };
            var tempFile = await _tempFilesService.ProcessTempFile(processTempFileInput);
        }

        [RemoteService(false)]
        public async Task GenerateTicketImagesZip(GenerateTicketImagesInput input)
        {
            var ticketImages = new List<FileBytesDto>();
            foreach (var ticket in input.Tickets)
            {
                var ticketImage = await GetTicketImageOrPrintoutOrDefault(ticket);
                if (ticketImage != null)
                {
                    ticketImages.Add(ticketImage);
                }
            }

            var zipFile = ticketImages.ToZipFile(input.FileName + ".zip");
            var processTempFileInput = new ProcessTempFileInput
            {
                FileBytes = zipFile.FileBytes,
                FileName = zipFile.FileName,
                MimeType = zipFile.MimeType,
                Message = input.SuccessMessage,
            };
            var tempFile = await _tempFilesService.ProcessTempFile(processTempFileInput);
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList)]
        public async Task<bool> InvoiceHasTicketPhotos(int invoiceId)
        {
            return await (await _ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.InvoiceLine.InvoiceId == invoiceId && (x.IsInternal || x.TicketPhotoId != null));
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList)]
        public async Task<FileDto> GetTicketPhotosForInvoice(int invoiceId)
        {
            var result = new FileDto();
            var ticketList = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.InvoiceLine.InvoiceId == invoiceId && (x.IsInternal || x.TicketPhotoId != null))
                .Select(x => new
                {
                    x.Id,
                    x.CustomerId,
                }).ToListAsync();

            if (!ticketList.Any())
            {
                result.WarningMessage = "Selected invoice ticket do not have images";
                return result;
            }

            await CheckCustomerSpecificPermissions(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList,
                ticketList.Select(x => x.CustomerId).Distinct().ToArray());

            int maximumTicketCount;
            int.TryParse(await FeatureChecker.GetValueAsync(AppFeatures.MaximumNumberOfTicketsPerDownload), out maximumTicketCount);
            if (ticketList.Count > maximumTicketCount)
            {
                result.WarningMessage = $"There were more than {maximumTicketCount} tickets. Please select fewer tickets before running again.";
                return result;
            }

            var ticketsIds = ticketList.Select(x => x.Id).ToList();

            var invoiceDetails = await (await _invoiceRepository.GetQueryAsync())
                    .Where(x => x.Id == invoiceId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name,
                    }).FirstOrDefaultAsync();
            var fileName = $"{invoiceDetails?.CustomerName}_{invoiceId}";

            await _backgroundJobManager.EnqueueAsync<TicketPhotoDownloadJob, TicketPhotoDownloadJobArgs>(new TicketPhotoDownloadJobArgs
            {
                TicketIds = ticketsIds,
                RequestorUser = await Session.ToUserIdentifierAsync(),
                SuccessMessage = "Your ticket file has been processed.",
                FailedMessage = "Failed to generate PDF for ticket images",
                FileName = fileName,
            });
            result.SuccessMessage = "This may take a while depending on the number of tickets selected. You may continue working and we’ll send you a notification with a link to the file when it has been processed";
            return result;
        }

        private async Task<FileBytesDto> GetTicketImageOrPrintoutOrDefault(TicketPhotoDataDto ticket)
        {
            var timeZone = await GetTimezone();
            var imageFilename = GenerateTicketFilename(ticket, timeZone);

            if (ticket.TicketPhotoId.HasValue)
            {
                var image = await _binaryObjectManager.GetOrNullAsync(ticket.TicketPhotoId.Value);
                if (image?.Bytes?.Length > 0)
                {
                    return new FileBytesDto
                    {
                        FileBytes = image.Bytes,
                        FileName = imageFilename,
                    };
                }
            }
            else if (ticket.IsInternal)
            {
                var report = await GetTicketPrintOutInternal(new GetTicketPrintOutInput
                {
                    TicketId = ticket.TicketId,
                });

                return new FileBytesDto
                {
                    FileBytes = report.FileBytes,
                    FileName = imageFilename,
                };
            }

            return null;
        }

        private static string GenerateTicketFilename(GenerateTicketFilenameInput ticket, string timeZone)
        {
            var isPdf = ticket.TicketPhotoId == null && ticket.IsInternal
                        || (ticket.TicketPhotoFilename?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false);
            var prefix = string.IsNullOrWhiteSpace(ticket.LoadAtName)
                ? string.Empty
                : $"{ticket.LoadAtName}_";
            var ticketDate = ticket.TicketDateTime?.ConvertTimeZoneTo(timeZone);
            var imageName = $"{prefix}{ticket.TicketNumber}_{ticketDate:yyyyMMdd}".SanitizeFilename();
            return $"{imageName}.{(isPdf ? "pdf" : "jpg")}";
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<string> GenerateTicketNumber(int ticketId)
        {
            await _ticketRepository.EnsureCanEditTicket(ticketId);
            var ticket = await _ticketRepository.GetAsync(ticketId);
            ticket.IsInternal = true;
            ticket.TicketNumber = $"G-{ticket.Id}";
            return ticket.TicketNumber;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task<string> CheckIfTruckIsOutOfServiceOrInactive(TicketEditDto model)
        {
            var result = string.Empty;

            if (!model.IsBilled)
            {
                return result;
            }

            var truck = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.TruckCode == model.TruckCode)
                .Select(x => new
                {
                    x.IsOutOfService,
                    x.IsActive,
                })
                .FirstOrDefaultAsync();

            if (truck == null)
            {
                return result;
            }

            if (truck.IsOutOfService)
            {
                result = "out of service";
            }

            if (!truck.IsActive)
            {
                if (result != "")
                {
                    result += " and ";
                }
                result += "inactive";
            }

            return result;
        }

        private async Task<int?> GetTruckId(string truckCode)
        {
            return (await (await _truckRepository.GetQueryAsync())
                .Where(t => t.TruckCode == truckCode)
                .Select(t => new { t.Id })
                .FirstOrDefaultAsync())?.Id;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<PagedResultDto<TicketListItemViewModel>> LoadTicketsByOrderLineId(int orderLineId)
        {
            var permissions = new
            {
                ViewTickets = await IsGrantedAsync(AppPermissions.Pages_Tickets_View),
                LeaseHaulerTickets = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Tickets),
            };

            var splitBillingByOffices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices);

            List<int?> officeIdFilter = null;
            int? leaseHaulerIdFilter = null;
            if (permissions.ViewTickets)
            {
                if (splitBillingByOffices)
                {
                    officeIdFilter = await GetOfficeIds();
                }
            }
            else if (permissions.LeaseHaulerTickets)
            {
                leaseHaulerIdFilter = Session.GetLeaseHaulerIdOrThrow(this);
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            var query = (await _ticketRepository.GetQueryAsync())
                .Where(t => t.OrderLineId == orderLineId)
                .WhereIf(leaseHaulerIdFilter.HasValue, t => t.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter || t.CarrierId == leaseHaulerIdFilter)
                .WhereIf(officeIdFilter != null, t => officeIdFilter.Contains(t.OfficeId));
            int totalCount = await query.CountAsync();
            var items = await query
                .Select(t => new TicketListItemViewModel
                {
                    Id = t.Id,
                    OrderLineId = t.OrderLineId.Value,
                    TicketNumber = t.TicketNumber,
                    TicketDateTime = t.TicketDateTime,
                    FreightQuantity = t.FreightQuantity,
                    MaterialQuantity = t.MaterialQuantity,
                    FreightUomId = t.FreightUomId,
                    FreightUomName = t.FreightUom.Name,
                    MaterialUomName = t.MaterialUom.Name,
                    FreightItemId = t.FreightItemId,
                    MaterialItemId = t.MaterialItemId,
                    TruckId = t.TruckId,
                    LeaseHaulerId = t.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    TruckCode = t.Truck.TruckCode != null ? t.Truck.TruckCode : t.TruckCode,
                    TruckCanPullTrailer = t.Truck.CanPullTrailer,
                    TrailerId = t.TrailerId,
                    TrailerTruckCode = t.Trailer.TruckCode,
                    DriverId = t.DriverId,
                    DriverName = t.Driver.FirstName + " " + t.Driver.LastName,
                    TicketPhotoId = t.TicketPhotoId,
                    ReceiptLineId = t.ReceiptLineId,
                    IsInternal = t.IsInternal,
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var timeZone = await GetTimezone();
            foreach (var item in items)
            {
                item.TicketDateTime = item.TicketDateTime?.ConvertTimeZoneTo(timeZone);
            }

            return new PagedResultDto<TicketListItemViewModel>(
                totalCount,
                items);
        }

        private async Task<bool> CanDeleteTicket(EntityDto input)
        {
            var ticketDetails = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasInvoiceLines = x.InvoiceLine != null,
                    HasReceiptLines = x.ReceiptLineId != null,
                    HasPayStatements = x.PayStatementTickets.Any(),
                    HasLeaseHaulerStatements = x.LeaseHaulerStatementTicket != null,
                }).FirstOrDefaultAsync();

            if (ticketDetails != null && (ticketDetails.HasInvoiceLines || ticketDetails.HasReceiptLines || ticketDetails.HasPayStatements || ticketDetails.HasLeaseHaulerStatements))
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<DeleteTicketOutput> DeleteTicket(EntityDto input)
        {
            var canDelete = await CanDeleteTicket(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }

            var ticket = await _ticketRepository.FirstOrDefaultAsync(x => x.Id == input.Id);

            if (ticket == null)
            {
                return new DeleteTicketOutput();
            }

            var ticketData = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                }).FirstAsync();

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_Tickets_Edit,
                AppPermissions.LeaseHaulerPortal_Tickets,
                Session.LeaseHaulerId,
                ticketData.LeaseHaulerId ?? ticket.CarrierId);

            await _ticketRepository.DeleteAsync(ticket);

            IOrderTaxDetails orderTaxDetails = null;

            if (ticket.OrderLineId.HasValue)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                orderTaxDetails = await _orderTaxCalculator.CalculateTotalsForOrderLineAsync(ticket.OrderLineId.Value);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            if (ticket.MaterialCompanyTicketId != null
                && ticket.MaterialCompanyTenantId != null)
            {
                using (CurrentUnitOfWork.SetTenantId(ticket.MaterialCompanyTenantId.Value))
                {
                    var destinationTicket = await (await _ticketRepository.GetQueryAsync())
                        .Where(x => x.Id == ticket.MaterialCompanyTicketId.Value)
                        .FirstOrDefaultAsync();
                    if (destinationTicket != null)
                    {
                        destinationTicket.HaulingCompanyTicketId = null;
                        destinationTicket.HaulingCompanyTenantId = null;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
            if (ticket.HaulingCompanyTicketId != null
                && ticket.HaulingCompanyTenantId != null)
            {
                using (CurrentUnitOfWork.SetTenantId(ticket.HaulingCompanyTenantId.Value))
                {
                    var destinationTicket = await (await _ticketRepository.GetQueryAsync())
                        .Where(x => x.Id == ticket.HaulingCompanyTicketId.Value)
                        .FirstOrDefaultAsync();
                    if (destinationTicket != null)
                    {
                        destinationTicket.MaterialCompanyTicketId = null;
                        destinationTicket.MaterialCompanyTenantId = null;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }

            return new DeleteTicketOutput
            {
                OrderTaxDetails = orderTaxDetails != null ? new OrderTaxDetailsDto(orderTaxDetails) : null,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task MarkAsBilledTicket(EntityDto input)
        {
            var ticket = await _ticketRepository.GetAsync(input.Id);
            ticket.IsBilled = true;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_Edit)]
        public async Task EditTicketFromList(EditTicketFromListInput input)
        {
            var ticket = await _ticketRepository.GetAsync(input.Id);
            ticket.IsVerified = input.IsVerified;
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<PagedResultDto<TicketListViewDto>> TicketListView(TicketListInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var query = await GetTicketListQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var shiftDictionary = await SettingManager.GetShiftDictionary();
            items.ForEach(x => x.Shift = x.ShiftRaw.HasValue && shiftDictionary.ContainsKey(x.ShiftRaw.Value) ? shiftDictionary[x.ShiftRaw.Value] : "");

            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            items.ForEach(x =>
            {
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, x, x.SalesTaxRate ?? 0, separateItems);
            });

            return new PagedResultDto<TicketListViewDto>(
                totalCount,
                items);
        }



        [AbpAuthorize(AppPermissions.Pages_Tickets_Export, AppPermissions.CustomerPortal_TicketList_Export, AppPermissions.LeaseHaulerPortal_Tickets)]
        [HttpPost]
        public async Task<FileDto> GetTicketsToCsv(TicketListInput input)
        {
            var timezone = await GetTimezone();
            var query = await GetTicketListQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (items.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            var shiftDictionary = await SettingManager.GetShiftDictionary();
            items.ForEach(x => x.Shift = x.ShiftRaw.HasValue && shiftDictionary.ContainsKey(x.ShiftRaw.Value) ? shiftDictionary[x.ShiftRaw.Value] : "");
            items.ForEach(x => x.Date = x.Date?.ConvertTimeZoneTo(timezone));

            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            items.ForEach(x => x.DriverPay = allowProductionPay ? Math.Round((x.GetFreightQuantity() ?? 0) * (x.FreightRateToPayDrivers ?? 0) * (x.DriverPayPercent ?? 0) / 100, 2) : null);

            var filename = "TicketList.csv";

            if (input.InvoiceId.HasValue)
            {
                var invoiceDetails = await (await _invoiceRepository.GetQueryAsync())
                    .Where(x => x.Id == input.InvoiceId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name,
                    }).FirstOrDefaultAsync();
                filename = $"{invoiceDetails?.CustomerName}InvoiceNumber{input.InvoiceId}.csv";
            }

            var hideColumnsForInvoiceExport = input.InvoiceId.HasValue;

            return await _ticketListCsvExporter.ExportToFileAsync(items, filename, hideColumnsForInvoiceExport);
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList, AppPermissions.LeaseHaulerPortal_Tickets)]
        [HttpPost]
        public async Task<List<int>> GetTicketsIds(TicketListInput input)
        {
            var query = await GetTicketListQueryAsync(input);

            var ticketsIds = await query
                .Select(x => x.Id)
                .ToListAsync();

            return ticketsIds;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        [HttpPost]
        public async Task<bool> AreSomeTicketsUnbillable(int[] ticketIds)
        {
            var query = await GetTicketListQueryAsync(new TicketListInput
            {
                TicketIds = ticketIds,
            });

            var result = await query
                .Where(x => !x.IsBilled && x.InvoiceLineId == null && (x.Revenue <= 0 || !x.IsVerified))
                .AnyAsync();

            return result;
        }

        private async Task<IQueryable<TicketListViewDto>> GetTicketListQueryAsync(TicketListInput input)
        {
            var permissions = new
            {
                ViewAnyTickets = await IsGrantedAsync(AppPermissions.Pages_Tickets_View),
                ViewCustomerTicketsOnly = await IsGrantedAsync(AppPermissions.CustomerPortal_TicketList),
                ViewLeaseHaulerTicketsOnly = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Tickets),
            };

            if (permissions.ViewAnyTickets)
            {
                //do not additionally filter the data
            }
            else if (permissions.ViewCustomerTicketsOnly)
            {
                input.CustomerId = Session.GetCustomerIdOrThrow(this);
            }
            else if (permissions.ViewLeaseHaulerTicketsOnly)
            {
                input.TruckLeaseHaulerId = Session.GetLeaseHaulerIdOrThrow(this);
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            var timezone = await GetTimezone();

            var ticketDateRangeBegin = input.TicketDateRangeBegin?.ConvertTimeZoneFrom(timezone);
            var ticketDateRangeEnd = input.TicketDateRangeEnd?.AddDays(1).ConvertTimeZoneFrom(timezone);
            var orderDateRangeBegin = input.OrderDateRangeBegin;
            var orderDateRangeEnd = input.OrderDateRangeEnd?.AddDays(1);

            var query = await _ticketRepository.GetQueryAsync();

            if (input.TicketStatus == TicketListStatusFilterEnum.PotentialDuplicateTickets)
            {
                query = (from ticket in query
                         join otherTicket in await _ticketRepository.GetQueryAsync()
                         on new { ticket.TicketNumber, ticket.LoadAtId, ticket.DeliverToId }
                         equals new { otherTicket.TicketNumber, otherTicket.LoadAtId, otherTicket.DeliverToId }
                         where otherTicket != null && otherTicket.Id != ticket.Id
                         select ticket)
                        .Distinct();
            }

            return query
                .WhereIf(input.BillingStatus == BillingStatus.Billed, x => x.IsBilled && (!x.NonbillableFreight || !x.NonbillableMaterial))
                .WhereIf(input.BillingStatus == BillingStatus.Unbilled, x => !x.IsBilled && (!x.NonbillableFreight || !x.NonbillableMaterial))
                .WhereIf(input.BillingStatus == BillingStatus.Nonbillable, x => x.NonbillableFreight && x.NonbillableMaterial)
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId.Value)
                .WhereIf(input.InvoiceId.HasValue, x => x.InvoiceLine.InvoiceId == input.InvoiceId.Value)
                .WhereIf(input.CarrierId.HasValue, x => x.CarrierId == input.CarrierId)
                .WhereIf(input.ItemId.HasValue, x => x.FreightItemId == input.ItemId || x.MaterialItemId == input.ItemId)
                .WhereIf(input.DriverId.HasValue, x => x.DriverId == input.DriverId)
                .WhereIf(ticketDateRangeBegin.HasValue, x => x.TicketDateTime >= ticketDateRangeBegin)
                .WhereIf(ticketDateRangeEnd.HasValue, x => x.TicketDateTime < ticketDateRangeEnd)
                .WhereIf(orderDateRangeBegin.HasValue, x => x.OrderLine.Order.DeliveryDate >= orderDateRangeBegin)
                .WhereIf(orderDateRangeEnd.HasValue, x => x.OrderLine.Order.DeliveryDate < orderDateRangeEnd)
                .WhereIf(!string.IsNullOrEmpty(input.TicketNumber), x => x.TicketNumber.Contains(input.TicketNumber))
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(!string.IsNullOrEmpty(input.JobNumber), x => x.OrderLine.JobNumber == input.JobNumber)
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift),
                    t => t.Shift.HasValue && input.Shifts.Contains(t.Shift.Value)
                         || t.OrderLine.Order.Shift.HasValue && input.Shifts.Contains(t.OrderLine.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift),
                    t => !t.Shift.HasValue && !t.OrderLineId.HasValue
                         || input.Shifts.Contains(t.Shift.Value)
                         || t.OrderLineId.HasValue && !t.OrderLine.Order.Shift.HasValue
                         || input.Shifts.Contains(t.OrderLine.Order.Shift.Value))
                .WhereIf(input.IsVerified.HasValue, x => x.IsVerified == input.IsVerified)
                .WhereIf(input.CustomerId.HasValue, x => x.Customer.Id == input.CustomerId)
                .WhereIf(input.TicketStatus == TicketListStatusFilterEnum.MissingTicketsOnly, x => string.IsNullOrEmpty(x.TicketNumber) || x.MaterialQuantity == 0 && x.FreightQuantity == 0)
                .WhereIf(input.TicketStatus == TicketListStatusFilterEnum.EnteredTicketsOnly, x => !(string.IsNullOrEmpty(x.TicketNumber) || x.MaterialQuantity == 0 && x.FreightQuantity == 0))
                .WhereIf(input.TicketIds?.Any() == true, x => input.TicketIds.Contains(x.Id))
                .WhereIf(input.OrderId.HasValue, x => x.OrderLine.OrderId == input.OrderId)
                .WhereIf(input.IsImported.HasValue, x => x.IsImported == input.IsImported)
                .WhereIf(input.LoadAtId.HasValue, x => x.LoadAtId == input.LoadAtId)
                .WhereIf(input.DeliverToId.HasValue, x => x.DeliverToId == input.DeliverToId)
                .WhereIf(input.TruckLeaseHaulerId.HasValue, x => x.Truck.LeaseHaulerTruck.LeaseHaulerId == input.TruckLeaseHaulerId.Value)
                .WhereIf(!string.IsNullOrEmpty(input.PONumber), x => x.OrderLine.Order.PONumber == input.PONumber)
                .WhereIf(input.HasImage == true, x => x.TicketPhotoId != null || x.IsInternal)
                .WhereIf(input.HasImage == false, x => x.TicketPhotoId == null && !x.IsInternal)
                .Select(t => new TicketListViewDto
                {
                    Id = t.Id,
                    IsVerified = t.IsVerified,
                    Date = t.TicketDateTime,
                    OrderDate = t.OrderLine.Order.DeliveryDate,
                    ShiftRaw = t.OrderLineId.HasValue ? t.OrderLine.Order.Shift : t.Shift,
                    Office = t.Office.Name,
                    CustomerName = t.Customer.Name,
                    CustomerNumber = t.Customer.AccountNumber,
                    QuoteName = t.OrderLine.Order.Quote.Name,
                    JobNumber = t.OrderLine.JobNumber,
                    FreightItemName = t.FreightItem.Name,
                    MaterialItemName = t.MaterialItem.Name,
                    TicketNumber = t.TicketNumber,
                    FreightQuantity = t.FreightQuantity,
                    MaterialQuantity = t.MaterialQuantity,
                    FreightUomName = t.FreightUom.Name,
                    MaterialUomName = t.MaterialUom.Name,
                    CarrierId = t.CarrierId,
                    Carrier = t.Carrier.Name,
                    Truck = t.Truck.TruckCode ?? t.TruckCode,
                    TruckOffice = t.Truck.Office.Name,
                    Trailer = t.Trailer.TruckCode,
                    DriverName = t.Driver == null ? null : t.Driver.LastName + ", " + t.Driver.FirstName,
                    DriverOffice = t.Driver.Office.Name,
                    EmployeeId = t.Driver.EmployeeId,
                    IsBilled = t.IsBilled,
                    IsInternal = t.IsInternal,
                    LoadCount = t.LoadCount,
                    TicketPhotoId = t.TicketPhotoId,
                    InvoiceNumber = (int?)t.InvoiceLine.Invoice.Id,
                    InvoiceLineId = (int?)t.InvoiceLine.Id,
                    HasPayStatements = t.PayStatementTickets.Any(),
                    HasLeaseHaulerStatements = t.LeaseHaulerStatementTicket != null,
                    ReceiptId = t.ReceiptLine.Receipt.Id,
                    ReceiptLineId = t.ReceiptLineId,
                    LoadAtName = t.LoadAt.DisplayName,
                    DeliverToName = t.DeliverTo.DisplayName,
                    Designation = t.OrderLine.Designation,
                    OrderLineMaterialUomId = t.OrderLine.MaterialUomId,
                    OrderLineFreightUomId = t.OrderLine.FreightUomId,
                    TicketUomId = t.FreightUomId,
                    MaterialRate = t.OrderLine.MaterialPricePerUnit,
                    MaterialCostRate = t.NonbillableMaterial ? 0 : t.OrderLine.MaterialCostRate,
                    FreightRate = t.OrderLine.FreightPricePerUnit,
                    FreightRateToPayDrivers = t.OrderLine.FreightRateToPayDrivers,
                    DriverPayPercent = t.OrderLine.ProductionPay ? t.Driver.EmployeeTimeClassifications.FirstOrDefault(e => e.TimeClassification.IsProductionBased).PayRate : null,
                    DriverPayTimeClassificationName = t.OrderLine.DriverPayTimeClassification.Name,
                    HourlyDriverPayRate = t.OrderLine.HourlyDriverPayRate,
                    DriverSpecificHourlyRate = t.OrderLine.DriverPayTimeClassification.EmployeeTimeClassifications.FirstOrDefault(e => e.DriverId == t.DriverId).PayRate,
                    FuelSurcharge = t.FuelSurcharge,
                    IsTaxable = t.FreightItem.IsTaxable,
                    IsFreightTaxable = t.FreightItem.IsTaxable,
                    IsMaterialTaxable = t.MaterialItem.IsTaxable,
                    SalesTaxRate = t.OrderLine.Order.SalesTaxRate,
                    Revenue = Math.Round((t.MaterialQuantity ?? 0) * (t.OrderLine.MaterialPricePerUnit ?? 0)
                                        + (t.FreightQuantity ?? 0) * (t.OrderLine.FreightPricePerUnit ?? 0), 2),
                    IsFreightPriceOverridden = t.OrderLine.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = t.OrderLine.IsMaterialPriceOverridden,
                    OrderLineFreightPrice = t.OrderLine.FreightPrice,
                    OrderLineMaterialPrice = t.OrderLine.MaterialPrice,
                    IsImported = t.IsImported,
                    ProductionPay = t.OrderLine.ProductionPay,
                    PayStatementId = t.PayStatementTickets.Select(x => x.PayStatementDetail.PayStatementId).First(),
                    OrderNote = t.OrderLine.Order.Directions,
                    OrderId = t.OrderLine.OrderId,
                    DriverNote = t.OrderLine.OrderLineTrucks.FirstOrDefault(x => x.TruckId == t.TruckId).DriverNote,
                    PONumber = t.OrderLine.Order.PONumber,
                    LeaseHaulerRate = t.OrderLine.LeaseHaulerRate,
                    SalesTaxEntityName = t.OrderLine.Order.SalesTaxEntity.Name,
                });
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver, AppPermissions.LeaseHaulerPortal_TicketsByDriver)]
        public async Task<TicketsByDriverResult> GetTicketsByDriver(GetTicketsByDriverInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_TicketsByDriver, AppPermissions.LeaseHaulerPortal_TicketsByDriver);

            var result = new TicketsByDriverResult
            {
            };

            var orderLineQuery = (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Order.DeliveryDate == input.Date)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.OrderLineTrucks.Any(olt => olt.Driver.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerIdFilter))
                .Select(x => new TicketsByDriverResult.OrderLineDto
                {
                    Id = x.Id,
                    Shift = x.Order.Shift,
                    OrderId = x.OrderId,
                    JobNumber = x.JobNumber,
                    CustomerId = x.Order.CustomerId,
                    QuoteId = x.Order.QuoteId,
                    PoNumber = x.Order.PONumber,
                    SalesTaxEntityId = x.Order.SalesTaxEntityId,
                    SalesTaxEntityName = x.Order.SalesTaxEntity.Name,
                    CustomerName = x.Order.Customer.Name,
                    OfficeId = x.Order.Office.Id,
                    OfficeName = x.Order.Office.Name,
                    OrderDate = x.Order.DeliveryDate,
                    LoadAtId = x.LoadAtId,
                    DeliverToId = x.DeliverToId,
                    LoadAtName = x.LoadAt.DisplayName,
                    DeliverToName = x.DeliverTo.DisplayName,
                    FreightItemId = x.FreightItemId,
                    FreightItemName = x.FreightItem.Name,
                    MaterialItemId = x.MaterialItemId,
                    MaterialItemName = x.MaterialItem.Name,
                    Designation = x.Designation,
                    MaterialUomId = x.MaterialUomId,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomId = x.FreightUomId,
                    FreightUomName = x.FreightUom.Name,
                    FreightRate = x.FreightPricePerUnit,
                    FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                    LeaseHaulerRate = x.LeaseHaulerRate,
                    ProductionPay = x.ProductionPay,
                    MaterialRate = x.MaterialPricePerUnit,
                    FuelSurchargeRate = x.FuelSurchargeRate,
                    MaterialTotal = x.MaterialPrice,
                    FreightTotal = x.FreightPrice,
                    IsMaterialTotalOverridden = x.IsMaterialPriceOverridden,
                    IsFreightTotalOverridden = x.IsFreightPriceOverridden,
                    Note = x.Note,
                    OrderLineTrucks = x.OrderLineTrucks.Select(t => new TicketsByDriverResult.OrderLineTruckDto
                    {
                        Id = t.Id,
                        TruckId = t.TruckId,
                        TrailerId = t.TrailerId,
                        DriverId = t.DriverId,
                        DriverNote = t.DriverNote,
                    }).ToList(),
                    Charges = x.Charges.Select(c => new TicketsByDriverResult.ChargeDto
                    {
                        UseMaterialQuantity = c.UseMaterialQuantity,
                        ChargeAmount = c.ChargeAmount,
                        Rate = c.Rate,
                    }).ToList(),
                    IsComplete = x.IsComplete,
                    IsCancelled = x.IsCancelled,
                    HasMultipleOrderLines = x.Order.OrderLines.Count > 1,
                });

            if (await orderLineQuery.AnyAsync(x => !x.IsComplete))
            {
                var today = await GetToday();
                if (input.Date < today)
                {
                    //for past orders: close open orders if no open dispatches, otherwise show a warning
                    result.HasOpenOrders = await ClosePastOrdersIfNoDispatches(input);
                }
                else
                {
                    //for today's orders: just show a warning
                    result.HasOpenOrders = true;
                }
            }

            result.OrderLines = await orderLineQuery.Where(x => x.IsComplete).ToListAsync();

            result.Tickets = await GetTicketsByDriverResultTickets((await _ticketRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .Where(x => x.OrderLineId.HasValue && x.OrderLine.Order.DeliveryDate == input.Date));

            //we should at least get drivers that are assigned to the above order lines, but if no extra filtering is enforced we can get all drivers.
            var driverIds = result.Tickets.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value)
                .Union(
                    result.OrderLines.SelectMany(o => o.OrderLineTrucks.Select(olt => olt.DriverId)).Where(x => x.HasValue).Select(x => x.Value)
                ).Distinct()
                .ToList();

            result.Drivers = await (await _driverRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerIdFilter || driverIds.Contains(x.Id))
                .Select(x => new TicketsByDriverResult.DriverDto
                {
                    Id = x.Id,
                    Name = x.LastName + ", " + x.FirstName,
                    IsActive = !x.IsInactive,
                    IsExternal = x.IsExternal,
                    LeaseHaulerId = x.LeaseHaulerDriver.LeaseHaulerId,
                })
                //.Where(x => !x.IsExternal || driverIds.Contains(x.Id) || x.LeaseHaulerId != null && leaseHaulerIds.Contains(x.LeaseHaulerId.Value))
                .OrderBy(d => d.Name)
                .ToListAsync();

            result.DriverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(x => x.Date == input.Date)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Driver.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerIdFilter || x.DriverId.HasValue && driverIds.Contains(x.DriverId.Value))
                .Select(x => new TicketsByDriverResult.DriverAssignmentDto
                {
                    Id = x.Id,
                    Shift = x.Shift,
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                }).ToListAsync();

            var truckIds = result.Tickets.Where(x => x.TruckId.HasValue).Select(x => x.TruckId.Value)
                .Union(
                    result.OrderLines.SelectMany(o => o.OrderLineTrucks.Select(olt => olt.TruckId))
                ).Union(
                    result.DriverAssignments.Select(x => x.TruckId)
                ).Distinct()
                .ToList();

            result.Trucks = await (await _truckRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter || truckIds.Contains(x.Id))
                .Select(x => new TicketsByDriverResult.TruckDto
                {
                    Id = x.Id,
                    TruckCode = x.TruckCode,
                    IsActive = x.IsActive,
                    CanPullTrailer = x.CanPullTrailer,
                    VehicleCategory = new TicketsByDriverResult.VehicleCategoryDto
                    {
                        AssetType = x.VehicleCategory.AssetType,
                    },
                    LeaseHaulerId = x.LeaseHaulerTruck.LeaseHaulerId,
                    DefaultDriverId = x.DefaultDriverId,
                    CurrentTrailerId = x.CurrentTrailerId,
                }).ToListAsync();

            result.LeaseHaulers = await (await _leaseHaulerRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Id == leaseHaulerIdFilter)
                .Select(x => new TicketsByDriverResult.LeaseHaulerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .OrderBy(d => d.Name)
                .ToListAsync();

            result.DailyFuelCost = await (await _dailyFuelCostRepository.GetQueryAsync())
                .Where(x => x.Date < input.Date)
                .Select(x => new TicketsByDriverResult.DailyFuelCostDto
                {
                    Date = x.Date,
                    Cost = x.Cost,
                })
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync();

            return result;
        }

        /// <returns>true if there are still open dispatches; false if there are no open dispathes and the orders have been closed</returns>
        private async Task<bool> ClosePastOrdersIfNoDispatches(GetTicketsByDriverInput input)
        {
            var hasDispatches = await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLine.Order.DeliveryDate == input.Date
                    && Dispatch.OpenStatuses.Contains(x.Status));

            if (hasDispatches)
            {
                return true;
            }

            var orderLines = await (await _orderLineRepository.GetQueryAsync())
                .Include(ol => ol.OrderLineTrucks)
                .Where(x => x.Order.DeliveryDate == input.Date && !x.IsComplete)
                .ToListAsync();

            foreach (var orderLine in orderLines)
            {
                orderLine.IsComplete = true;

                foreach (var orderLineTruck in orderLine.OrderLineTrucks)
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return false;
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public async Task<TicketsByDriverResult> EditTicketsByDriver(TicketsByDriverResult model)
        {
            var timezone = await GetTimezone();
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var orderLines = new List<OrderLine>();
            if (model.Tickets?.Any() == true)
            {
                var ticketIds = model.Tickets.Select(x => x.Id).Where(x => x != 0).ToList();
                var tickets = await (await _ticketRepository.GetQueryAsync()).Where(x => ticketIds.Contains(x.Id)).ToListAsync();

                var orderLineIds = model.Tickets.Select(x => x.OrderLineId).Where(x => !orderLines.Any(o => o.Id == x)).Distinct().ToList();
                if (orderLineIds.Any())
                {
                    orderLines.AddRange(await (await _orderLineRepository.GetQueryAsync())
                        .Include(x => x.Order)
                        .Include(x => x.OrderLineTrucks)
                        .Where(x => orderLineIds.Contains(x.Id))
                        .ToListAsync());
                }

                var truckIds = model.Tickets.Select(x => x.TruckId).Where(x => x > 0).Distinct().ToList();
                var trucks = await (await _truckRepository.GetQueryAsync())
                    .Where(x => truckIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.TruckCode,
                        LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                    })
                    .ToListAsync();

                foreach (var ticketModel in model.Tickets)
                {
                    var ticket = ticketModel.Id != 0 ? tickets.FirstOrDefault(x => x.Id == ticketModel.Id) : new Ticket();
                    if (ticket == null)
                    {
                        throw new ApplicationException($"Ticket with id {ticketModel.Id} wasn't found");
                    }

                    if (ticketModel.OrderLineId == 0)
                    {
                        throw new ApplicationException("Ticket must have OrderLineId set");
                    }
                    var orderLine = orderLines.FirstOrDefault(x => x.Id == ticketModel.OrderLineId);
                    if (orderLine == null)
                    {
                        throw new ApplicationException($"OrderLine with id {ticketModel.OrderLineId} wasn't found");
                    }

                    if (ticket.Id == 0)
                    {
                        await EnsureCanAddTickets(ticketModel.OrderLineId.Value, Session.OfficeId);

                        ticket.OrderLineId = orderLine.Id;
                    }
                    if (!await IsGrantedAsync(AppPermissions.Pages_TicketsByDriver_EditTicketsOnInvoicesOrPayStatements))
                    {
                        await _ticketRepository.EnsureCanEditTicket(ticketModel.Id);
                    }

                    var oldTicketQuantities = new
                    {
                        ticket.FreightQuantity,
                        ticket.MaterialQuantity,
                    };

                    //var driverOrTruckHasChanged = false;
                    if (ticket.DriverId != ticketModel.DriverId)
                    {
                        //driverOrTruckHasChanged = true;
                        ticket.DriverId = ticketModel.DriverId;
                        await ThrowIfDriverHasTimeOffRequests(ticket.DriverId, orderLine.Order.DeliveryDate);
                    }
                    ticket.TicketNumber = ticketModel.TicketNumber;
                    ticket.NonbillableFreight = ticketModel.NonbillableFreight;
                    ticket.NonbillableMaterial = ticketModel.NonbillableMaterial;
                    ticket.IsVerified = ticketModel.IsVerified;
                    ticket.LoadCount = ticketModel.LoadCount;
                    ticket.OfficeId = ticketModel.OfficeId;

                    await _ticketQuantityHelper.SetTicketQuantity(ticket, ticketModel);

                    if (ticket.TruckId != ticketModel.TruckId)
                    {
                        //driverOrTruckHasChanged = true;
                        ticket.TruckId = ticketModel.TruckId;
                        if (ticket.TruckId.HasValue)
                        {
                            var truck = trucks.FirstOrDefault(x => x.Id == ticket.TruckId);
                            ticket.TruckCode = truck?.TruckCode;
                            ticketModel.TruckCode = ticket.TruckCode;
                            ticket.CarrierId = truck?.LeaseHaulerId;
                        }
                    }
                    ticket.TrailerId = ticketModel.TrailerId;
                    if (ticketModel.TicketDateTime.HasValue)
                    {
                        ticket.TicketDateTime = ticketModel.TicketDateTime.Value.ConvertTimeZoneFrom(timezone);
                    }
                    else
                    {
                        var newDateWithTime = orderLine.Order.DeliveryDate;
                        ticket.TicketDateTime = newDateWithTime.ConvertTimeZoneFrom(timezone);
                        ticketModel.TicketDateTime = newDateWithTime;
                    }

                    //if (driverOrTruckHasChanged)
                    //{
                    //    if (ticket.TruckId != null)
                    //    {
                    //        if (!orderLine.OrderLineTrucks.Any(olt => olt.TruckId == ticket.TruckId))
                    //        {
                    //            var orderLineTruck = new OrderLineTruck
                    //            {
                    //                OrderLineId = ticket.OrderLineId.Value,
                    //                TruckId = ticket.TruckId.Value,
                    //                DriverId =
                    //                IsDone = true,
                    //                Utilization = 0,
                    //            };
                    //            _orderLineTruckRepository.Insert(orderLineTruck);
                    //            if (!orderLine.OrderLineTrucks.Contains(orderLineTruck))
                    //            {
                    //                orderLine.OrderLineTrucks.Add(orderLineTruck);
                    //            }

                    //            //if (orderLine.Order.DateTime >= await GetToday())
                    //            //{
                    //            //    var existingDriverAssignment = await (await _driverAssignmentRepository.GetQueryAsync())
                    //            //        .Where(da => da.TruckId == ticket.TruckId && da.Date == orderLine.Order.DateTime && da.Shift == orderLine.Order.Shift)
                    //            //        .FirstOrDefaultAsync();
                    //            //    if (existingDriverAssignment == null)
                    //            //    {
                    //            //        var truck = await (await _truckRepository.GetQueryAsync())
                    //            //            .Where(x => x.Id == ticket.TruckId)
                    //            //            .Select(x => new { x.VehicleCategory.IsPowered, x.IsEmbedded })
                    //            //            .FirstOrDefaultAsync();
                    //            //        if (truck != null && truck.IsPowered &&
                    //            //            (!truck.IsEmbedded || await FeatureChecker.AllowLeaseHaulersFeature()))
                    //            //        {
                    //            //            var newDriverAssignment = new DriverAssignment
                    //            //            {
                    //            //                Date = orderLine.Order.DateTime.Value,
                    //            //                Shift = orderLine.Order.Shift,
                    //            //                DriverId = ticket.DriverId,
                    //            //                OfficeId = orderLine.Order.OfficeId,
                    //            //                TruckId = ticket.TruckId.Value,
                    //            //            };
                    //            //            await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                    //            //        }
                    //            //    }
                    //            //}
                    //        }
                    //    }
                    //}

                    if (ticketModel.Id == 0)
                    {
                        ticket.LoadAtId = orderLine.LoadAtId;
                        ticket.DeliverToId = orderLine.DeliverToId;
                        ticket.CustomerId = orderLine.Order.CustomerId;
                        ticket.OfficeId = ticketModel.OfficeId ?? orderLine.Order.OfficeId;
                        if (!separateItems)
                        {
                            ticket.FreightItemId = orderLine.FreightItemId;
                            ticket.FreightUomId = orderLine.Designation.HasMaterial() ? orderLine.MaterialUomId : orderLine.FreightUomId;
                        }

                        ticketModel.Id = await _ticketRepository.InsertAndGetIdAsync(ticket);
                    }

                    var quantityWasChanged = oldTicketQuantities.FreightQuantity != ticket.FreightQuantity
                                             || oldTicketQuantities.MaterialQuantity != ticket.MaterialQuantity;

                    if (quantityWasChanged)
                    {
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }


            var changedTickets = new List<Ticket>();
            if (model.OrderLines?.Any() == true)
            {
                foreach (var orderLineModel in model.OrderLines)
                {
                    var orderLine = await (await _orderLineRepository.GetQueryAsync())
                        .Include(x => x.Tickets)
                        .Include(x => x.Order)
                            .ThenInclude(x => x.OrderLines)
                                .ThenInclude(x => x.Tickets)
                        .FirstOrDefaultAsync(x => x.Id == orderLineModel.Id);

                    if (orderLine == null)
                    {
                        throw new ApplicationException($"OrderLine with id {orderLineModel.Id} wasn't found");
                    }

                    var orderTickets = orderLine.Order.OrderLines.SelectMany(ol => ol.Tickets).ToList();
                    var orderLineTickets = orderLine.Tickets.ToList();
                    var orderHasChanged = false;
                    var rateWasChanged = false;
                    if (orderLine.Order.CustomerId != orderLineModel.CustomerId)
                    {
                        orderLine.Order.CustomerId = orderLineModel.CustomerId;
                        orderLine.Order.ContactId = null;
                        orderLine.Order.QuoteId = null;
                        orderTickets.ForEach(t => t.CustomerId = orderLineModel.CustomerId);
                        orderHasChanged = true;
                    }
                    if (orderLine.LoadAtId != orderLineModel.LoadAtId)
                    {
                        orderLine.LoadAtId = orderLineModel.LoadAtId;
                        orderLineTickets.ForEach(t => t.LoadAtId = orderLineModel.LoadAtId);
                    }
                    if (orderLine.DeliverToId != orderLineModel.DeliverToId)
                    {
                        orderLine.DeliverToId = orderLineModel.DeliverToId;
                        orderLineTickets.ForEach(t => t.DeliverToId = orderLineModel.DeliverToId);
                    }
                    if (orderLine.FreightItemId != orderLineModel.FreightItemId)
                    {
                        orderLine.FreightItemId = orderLineModel.FreightItemId;
                        orderLineTickets.ForEach(t => t.FreightItemId = orderLineModel.FreightItemId);
                    }
                    if (orderLine.MaterialItemId != orderLineModel.MaterialItemId)
                    {
                        orderLine.MaterialItemId = orderLineModel.MaterialItemId;
                        orderLineTickets.ForEach(t => t.MaterialItemId = orderLineModel.MaterialItemId);
                    }
                    if (orderLine.Order.OfficeId != orderLineModel.OfficeId)
                    {
                        orderLine.Order.OfficeId = orderLineModel.OfficeId;
                        orderTickets.ForEach(t => t.OfficeId = orderLineModel.OfficeId);
                        orderHasChanged = true;
                    }
                    if (orderLine.Order.PONumber != orderLineModel.PoNumber)
                    {
                        orderLine.Order.PONumber = orderLineModel.PoNumber;
                        orderHasChanged = true;
                    }
                    if (orderLine.Order.SalesTaxEntityId != orderLineModel.SalesTaxEntityId)
                    {
                        orderLine.Order.SalesTaxEntityId = orderLineModel.SalesTaxEntityId;

                        if (orderLineModel.SalesTaxEntityId == null)
                        {
                            orderLine.Order.SalesTaxRate = 0;
                        }
                        else
                        {
                            var salesTaxEntity = await (await _taxRateRepository.GetQueryAsync())
                                .Where(x => x.Id == orderLineModel.SalesTaxEntityId)
                                .Select(x => new
                                {
                                    x.Rate,
                                })
                                .FirstAsync();
                            orderLine.Order.SalesTaxRate = salesTaxEntity.Rate;
                        }

                        orderHasChanged = true;
                        rateWasChanged = true;
                    }

                    orderLine.JobNumber = orderLineModel.JobNumber;
                    orderLine.LeaseHaulerRate = orderLineModel.LeaseHaulerRate;
                    orderLine.ProductionPay = orderLineModel.ProductionPay;

                    if (orderLine.Designation != orderLineModel.Designation)
                    {
                        var oldDesignation = orderLine.Designation;
                        orderLine.Designation = orderLineModel.Designation;
                        if (orderLine.Designation.MaterialOnly())
                        {
                            orderLine.FreightUomId = null;
                            if (orderLine.FreightPrice != 0 || orderLine.FreightPricePerUnit != 0 || orderLine.FreightQuantity != 0)
                            {
                                rateWasChanged = true;
                            }
                            orderLine.FreightPrice = 0;
                            //orderLine.FreightPricePerUnit = 0; //Rate change will be handled below
                            orderLine.FreightQuantity = 0;
                        }
                        else if (orderLine.Designation.FreightOnly())
                        {
                            if (orderLine.MaterialPrice != 0 || orderLine.MaterialPricePerUnit != 0 || orderLine.MaterialQuantity != 0)
                            {
                                rateWasChanged = true;
                            }
                            orderLine.MaterialPrice = 0;
                            //orderLine.MaterialPricePerUnit = 0; //Rate change will be handled below
                        }
                        else
                        {
                            if (oldDesignation.MaterialOnly())
                            {
                                rateWasChanged = true;
                                orderLine.FreightQuantity = orderLine.MaterialQuantity;
                                //orderLine.FreightUomId = orderLine.MaterialUomId; //UOM change will be handled below
                            }
                            else if (oldDesignation.FreightOnly())
                            {
                                rateWasChanged = true;
                                //orderLine.MaterialUomId = orderLine.FreightUomId; //UOM change will be handled below
                            }
                        }
                    }

                    if (orderLine.FreightUomId != orderLineModel.FreightUomId)
                    {
                        await UncheckProductionPayOnFreightUomChangeIfNeeded(orderLine, orderLineModel.FreightUomId);
                        var oldUomId = orderLine.FreightUomId;
                        orderLine.FreightUomId = orderLineModel.FreightUomId;
                        orderLineTickets
                            .Where(x => x.FreightUomId == oldUomId)
                            .ToList()
                            .ForEach(t => t.FreightUomId = orderLineModel.FreightUomId);
                    }

                    if (orderLine.MaterialUomId != orderLineModel.MaterialUomId)
                    {
                        var oldUomId = orderLine.MaterialUomId;
                        orderLine.MaterialUomId = orderLineModel.MaterialUomId;
                        orderLineTickets
                            .Where(x => x.MaterialUomId == oldUomId)
                            .ToList()
                            .ForEach(t => t.MaterialUomId = orderLineModel.MaterialUomId);
                    }

                    ItemPricingDto pricing = null;
                    if (orderLine.FreightPricePerUnit != orderLineModel.FreightRate || orderLine.MaterialPricePerUnit != orderLineModel.MaterialRate)
                    {
                        rateWasChanged = true;
                        pricing = await _serviceAppService.GetItemPricing(new GetItemPricingInput
                        {
                            FreightItemId = orderLine.FreightItemId,
                            MaterialItemId = orderLine.MaterialItemId,
                            MaterialUomId = orderLine.MaterialUomId,
                            FreightUomId = orderLine.FreightUomId,
                            QuoteLineId = orderLine.QuoteLineId,
                            DeliverToId = orderLine.DeliverToId,
                            LoadAtId = orderLine.LoadAtId,
                            //todo: are there other fields we need to fill for the new pricing?
                        });
                    }
                    if (orderLine.FreightPricePerUnit != orderLineModel.FreightRate)
                    {
                        orderLine.FreightPricePerUnit = orderLineModel.FreightRate;
                        if (pricing?.QuoteBasedPricing?.FreightRate != null)
                        {
                            orderLine.IsFreightPricePerUnitOverridden = pricing.QuoteBasedPricing.FreightRate != orderLine.FreightPricePerUnit;
                        }
                        else if (pricing?.FreightRate != null && pricing.HasPricing)
                        {
                            orderLine.IsFreightPricePerUnitOverridden = pricing.FreightRate != orderLine.FreightPricePerUnit;
                        }
                        if (!orderLine.IsFreightPriceOverridden)
                        {
                            orderLine.FreightPrice = Math.Round((orderLine.FreightPricePerUnit ?? 0) * (orderLine.FreightQuantity ?? 0), 2);
                        }
                    }
                    if (orderLine.MaterialPricePerUnit != orderLineModel.MaterialRate)
                    {
                        orderLine.MaterialPricePerUnit = orderLineModel.MaterialRate;
                        if (pricing?.QuoteBasedPricing?.PricePerUnit != null)
                        {
                            orderLine.IsMaterialPricePerUnitOverridden = pricing.QuoteBasedPricing.PricePerUnit != orderLine.MaterialPricePerUnit;
                        }
                        else if (pricing?.PricePerUnit != null && pricing.HasPricing)
                        {
                            orderLine.IsMaterialPricePerUnitOverridden = pricing.PricePerUnit != orderLine.MaterialPricePerUnit;
                        }
                        if (!orderLine.IsMaterialPriceOverridden)
                        {
                            orderLine.MaterialPrice = Math.Round((orderLine.MaterialPricePerUnit ?? 0) * (orderLine.MaterialQuantity ?? 0), 2);
                        }
                    }
                    orderLine.FreightRateToPayDrivers = orderLineModel.FreightRateToPayDrivers;

                    if (rateWasChanged)
                    {
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await _orderTaxCalculator.CalculateTotalsAsync(orderLine.OrderId);
                        await _fuelSurchargeCalculator.RecalculateOrderLinesWithTickets(orderLine.Id);
                        orderLineModel.FuelSurchargeRate = orderLine.FuelSurchargeRate;
                    }

                    changedTickets.AddRange(orderHasChanged ? orderTickets : orderLineTickets);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            if (changedTickets.Any())
            {
                var ticketIds = changedTickets.Select(x => x.Id).Distinct().ToList();
                var tickets = await GetTicketsByDriverResultTickets((await _ticketRepository.GetQueryAsync())
                    .Where(x => ticketIds.Contains(x.Id)));

                if (model.Tickets?.Any() != true)
                {
                    model.Tickets = tickets;
                }
                else
                {
                    foreach (var ticket in tickets)
                    {
                        model.Tickets.RemoveAll(x => x.Id == ticket.Id);
                        model.Tickets.Add(ticket);
                    }
                }
            }

            return model;
        }

        private async Task<List<TicketsByDriverResult.TicketDto>> GetTicketsByDriverResultTickets(IQueryable<Ticket> ticketQuery)
        {
            var result = await ticketQuery
                .Select(t => new TicketsByDriverResult.TicketDto
                {
                    Id = t.Id,
                    OrderLineId = t.OrderLineId,
                    CarrierId = t.CarrierId,
                    OfficeId = t.OfficeId,
                    OfficeName = t.Office.Name,
                    TicketNumber = t.TicketNumber,
                    TicketDateTime = t.TicketDateTime,
                    FreightQuantity = t.FreightQuantity,
                    MaterialQuantity = t.MaterialQuantity,
                    FreightUomId = t.FreightUomId,
                    FreightUomName = t.FreightUom.Name,
                    MaterialUomId = t.MaterialUomId,
                    MaterialUomName = t.MaterialUom.Name,
                    FreightItemId = t.FreightItemId,
                    FreightItemName = t.FreightItem.Name,
                    MaterialItemId = t.MaterialItemId,
                    MaterialItemName = t.MaterialItem.Name,
                    TruckId = t.TruckId,
                    TruckCode = t.TruckCode,
                    TrailerId = t.TrailerId,
                    TrailerTruckCode = t.Trailer.TruckCode,
                    DriverId = t.DriverId,
                    TicketPhotoId = t.TicketPhotoId,
                    LoadCount = t.LoadCount,
                    ReceiptLineId = t.ReceiptLineId,
                    NonbillableFreight = t.NonbillableFreight,
                    NonbillableMaterial = t.NonbillableMaterial,
                    IsVerified = t.IsVerified,
                    IsInternal = t.IsInternal,
                    IsInvoiced = t.InvoiceLine != null,
                    HasPayStatementTickets = t.PayStatementTickets.Any(),
                    HasReceipt = t.ReceiptLineId != null,
                    HasLeaseHaulerStatement = t.LeaseHaulerStatementTicket != null,
                })
                .OrderBy(x => x.Id)
                .ToListAsync();

            var timeZone = await GetTimezone();
            foreach (var ticket in result)
            {
                ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(timeZone);
            }

            return result;
        }

        private async Task UncheckProductionPayOnFreightUomChangeIfNeeded(OrderLine orderLine, int? newUomId)
        {
            if (orderLine.FreightUomId == newUomId
                || !orderLine.ProductionPay
                || !newUomId.HasValue
                || !await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs))
            {
                return;
            }

            var uoms = await _listCaches.UnitOfMeasure.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
            var uom = uoms.Find(newUomId);

            if (uom.UomBaseId == UnitOfMeasureBaseEnum.Hours)
            {
                orderLine.ProductionPay = false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public async Task MoveTicketsToOrderLine(MoveTicketsToOrderLineInput input)
        {
            var tickets = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.OrderLineId == input.FromOrderLineId && x.DriverId == input.FromDriverId)
                .ToListAsync();

            var toOrderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.ToOrderLineId)
                .Select(x => new
                {
                    x.Id,
                    x.Order.OfficeId,
                }).FirstAsync();

            foreach (var ticket in tickets)
            {
                ticket.OrderLineId = input.ToOrderLineId;
                ticket.OfficeId = toOrderLine.OfficeId;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public async Task SetIsVerifiedForTickets(List<TicketIsVerifiedDto> models)
        {
            if (models?.Any() == true)
            {
                var ticketIds = models.Select(x => x.Id).ToList();
                var tickets = await (await _ticketRepository.GetQueryAsync()).Where(x => ticketIds.Contains(x.Id)).ToListAsync();
                foreach (var model in models)
                {
                    var ticket = tickets.FirstOrDefault(x => x.Id == model.Id);
                    if (ticket != null)
                    {
                        ticket.IsVerified = model.IsVerified;
                    }
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public async Task<TicketsByDriverResult.DailyFuelCostDto> EditCurrentFuelCost(EditCurrentFuelCostInput input)
        {
            var dailyFuelCost = await (await _dailyFuelCostRepository.GetQueryAsync())
                .Where(x => x.Date == input.Date.AddDays(-1))
                .FirstOrDefaultAsync();

            dailyFuelCost ??= new DailyFuelCost
            {
                Date = input.Date.AddDays(-1),
            };

            var costChanged = dailyFuelCost.Id == 0 || dailyFuelCost.Cost != input.Cost;
            dailyFuelCost.Cost = input.Cost;

            if (dailyFuelCost.Id == 0)
            {
                await _dailyFuelCostRepository.InsertAsync(dailyFuelCost);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            if (costChanged)
            {
                await _fuelSurchargeCalculator.RecalculateOrderLinesWithTickets(input.Date);
            }

            return new TicketsByDriverResult.DailyFuelCostDto
            {
                Date = dailyFuelCost.Date,
                Cost = dailyFuelCost.Cost,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.LeaseHaulerPortal_Tickets, AppPermissions.CustomerPortal_TicketList)]
        public async Task<FileBytesDto> GetTicketPrintOut(GetTicketPrintOutInput input)
        {
            await CheckTicketPrintPermissions(input.TicketId);

            return await GetTicketPrintOutInternal(input);
        }

        private async Task<FileBytesDto> GetTicketPrintOutInternal(GetTicketPrintOutInput input)
        {
            var ticket = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == input.TicketId)
                .Select(x => new
                {
                    TicketPhotoData = new TicketPhotoDataDto
                    {
                        TicketId = x.Id,
                        TicketNumber = x.TicketNumber,
                        TicketDateTime = x.TicketDateTime,
                        TicketPhotoFilename = x.TicketPhotoFilename,
                        TicketPhotoId = x.TicketPhotoId,
                        IsInternal = x.IsInternal,
                        LoadAtName = x.LoadAt.Name,
                    },
                    x.CustomerId,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                }).FirstOrDefaultAsync();

            if (ticket == null)
            {
                return null;
            }

            var data = await GetTicketPrintOutData(input);

            var report = await _ticketPrintOutGenerator.GenerateReport(data);

            //force pdf extension
            ticket.TicketPhotoData.IsInternal = true;
            ticket.TicketPhotoData.TicketPhotoId = null;
            var filename = GenerateTicketFilename(ticket.TicketPhotoData, await GetTimezone());

            return new FileBytesDto
            {
                FileBytes = report.SaveToBytesArray(),
                FileName = filename,
            };
        }

        private async Task CheckTicketPrintPermissions(int ticketId)
        {
            var ticket = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.Id == ticketId)
                .Select(x => new
                {
                    x.CustomerId,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                }).FirstOrDefaultAsync();
            if (ticket != null)
            {
                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_Tickets_View,
                    AppPermissions.LeaseHaulerPortal_Tickets,
                    Session.LeaseHaulerId,
                    new[] { ticket.LeaseHaulerId },
                    async () =>
                    {
                        await CheckCustomerSpecificPermissions(
                            AppPermissions.Pages_Tickets_View,
                            AppPermissions.CustomerPortal_TicketList,
                            ticket?.CustomerId
                        );
                    });
            }
        }

        private async Task<List<TicketPrintOutDto>> GetTicketPrintOutData(GetTicketPrintOutInput input)
        {
            var item = await (await _ticketRepository.GetQueryAsync())
                .Where(x => input.TicketId == x.Id)
                .Select(x => new TicketPrintOutDto
                {
                    TicketNumber = x.TicketNumber,
                    TicketDateTime = x.TicketDateTime,
                    CustomerName = x.Customer.Name,
                    OfficeId = x.OfficeId,
                    FreightItemName = x.FreightItem.Name,
                    MaterialItemName = x.MaterialItem.Name,
                    FreightUomName = x.FreightUom.Name,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    Designation = x.OrderLine.Designation,
                    OrderLineMaterialUomId = x.OrderLine.MaterialUomId,
                    OrderLineFreightUomId = x.OrderLine.FreightUomId,
                    TicketUomId = x.FreightUomId,
                    Note = x.OrderLine.Note,
                }).FirstOrDefaultAsync();

            item.SeparateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            item.LegalName = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingLegalName);
            item.LegalAddress = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingAddress);
            item.BillingPhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingPhoneNumber);
            item.LogoBytes = await _logoProvider.GetReportLogoAsBytesAsync(item.OfficeId);
            item.TicketDateTime = item.TicketDateTime?.ConvertTimeZoneTo(await GetTimezone());
            item.DebugLayout = input.DebugLayout;

            return new List<TicketPrintOutDto> { item };
        }

        private async Task ThrowIfDriverHasTimeOffRequests(int? driverId, DateTime? date)
        {
            if (driverId == null || date == null)
            {
                return;
            }
            if (await (await _timeOffRepository.GetQueryAsync())
                    .AnyAsync(x => x.DriverId == driverId && date <= x.EndDate && date >= x.StartDate))
            {
                throw new UserFriendlyException(L("DriverCantBeAssignedOnDayOff"));
            }
        }

        private async Task EnsureCanAddTickets(int orderLineId, int? officeId)
        {
            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    x.IsMaterialPriceOverridden,
                    x.IsFreightPriceOverridden,
                    OrderOfficeId = x.Order.OfficeId,
                    HasTickets = x.Tickets.Any(),
                    HasTicketsForOffices = x.Tickets.Select(x => x.OfficeId).Distinct().ToList(),
                }).FirstAsync();

            if (orderLine.IsMaterialPriceOverridden || orderLine.IsFreightPriceOverridden)
            {
                //has tickets for other offices
                if (orderLine.HasTicketsForOffices.Any(id => id != (officeId ?? orderLine.OrderOfficeId)))
                {
                    throw new UserFriendlyException("You can't add tickets to a line item with overridden totals for which another office already added tickets");
                }

                if (orderLine.HasTickets)
                {
                    throw new UserFriendlyException(L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"));
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public async Task<bool> IsTicketNumberExisted(string ticketNumber)
        {
            return await (await _ticketRepository.GetQueryAsync()).Where(x => x.TicketDateTime >= DateTime.UtcNow.AddDays(-7)).AnyAsync(x => x.TicketNumber == ticketNumber);
        }
    }
}
