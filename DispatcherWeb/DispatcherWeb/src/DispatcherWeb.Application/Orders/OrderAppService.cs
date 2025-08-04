using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Templates;
using DispatcherWeb.Items;
using DispatcherWeb.Localization;
using DispatcherWeb.Locations;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Orders.Reports;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Payments;
using DispatcherWeb.Payments.Dto;
using DispatcherWeb.Quotes;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Sessions;
using DispatcherWeb.Storage;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Orders
{
    [AbpAuthorize]
    public class OrderAppService : DispatcherWebAppServiceBase, IOrderAppService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<OrderLineVehicleCategory> _orderLineVehicleCategoryRepository;
        private readonly IRepository<BilledOrder> _billedOrderRepository;
        private readonly IRepository<QuoteLine> _quoteLineRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<OrderEmail> _orderEmailRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<OrderPayment> _orderPaymentRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<UnitOfMeasure> _unitOfMeasureRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TrackableEmail, Guid> _trackableEmailRepository;
        private readonly IRepository<TrackableEmailEvent> _trackableEmailEventRepository;
        private readonly IRepository<TrackableEmailReceiver> _trackableEmailReceiverRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly DateCacheInvalidator _dateCacheInvalidator;
        private readonly ListCacheCollection _listCaches;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IReceiptsExcelExporter _receiptsExcelExporter;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IEncryptionService _encryptionService;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IPaymentAppService _paymentAppService;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly ILogoProvider _logoProvider;

        public OrderAppService(
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<OrderLineVehicleCategory> orderLineVehicleCategoryRepository,
            IRepository<BilledOrder> billedOrderRepository,
            IRepository<QuoteLine> quoteLineRepository,
            IRepository<User, long> userRepository,
            IRepository<OrderEmail> orderEmailRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Payment> paymentRepository,
            IRepository<OrderPayment> orderPaymentRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<Item> itemRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository,
            IRepository<Location> locationRepository,
            IRepository<UnitOfMeasure> unitOfMeasureRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<TrackableEmail, Guid> trackableEmailRepository,
            IRepository<TrackableEmailEvent> trackableEmailEventRepository,
            IRepository<TrackableEmailReceiver> trackableEmailReceiverRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            DateCacheInvalidator dateCacheInvalidator,
            ListCacheCollection listCaches,
            ISyncRequestSender syncRequestSender,
            IReceiptsExcelExporter receiptsExcelExporter,
            ITrackableEmailSender trackableEmailSender,
            IWebHostEnvironment hostingEnvironment,
            ISingleOfficeAppService singleOfficeService,
            IEncryptionService encryptionService,
            OrderTaxCalculator orderTaxCalculator,
            IPaymentAppService paymentAppService,
            IBinaryObjectManager binaryObjectManager,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            ILogoProvider logoProvider
            )
        {
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _orderLineVehicleCategoryRepository = orderLineVehicleCategoryRepository;
            _billedOrderRepository = billedOrderRepository;
            _quoteLineRepository = quoteLineRepository;
            _userRepository = userRepository;
            _orderEmailRepository = orderEmailRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
            _locationRepository = locationRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _ticketRepository = ticketRepository;
            _trackableEmailRepository = trackableEmailRepository;
            _trackableEmailEventRepository = trackableEmailEventRepository;
            _trackableEmailReceiverRepository = trackableEmailReceiverRepository;
            _paymentRepository = paymentRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _receiptRepository = receiptRepository;
            _itemRepository = itemRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _dateCacheInvalidator = dateCacheInvalidator;
            _listCaches = listCaches;
            _syncRequestSender = syncRequestSender;
            _receiptsExcelExporter = receiptsExcelExporter;
            _trackableEmailSender = trackableEmailSender;
            _hostingEnvironment = hostingEnvironment;
            _singleOfficeService = singleOfficeService;
            _encryptionService = encryptionService;
            _orderTaxCalculator = orderTaxCalculator;
            _paymentAppService = paymentAppService;
            _binaryObjectManager = binaryObjectManager;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _logoProvider = logoProvider;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PagedResultDto<OrderDto>> GetOrders(GetOrdersInput input)
        {
            var query = (await _orderRepository.GetQueryAsync())
                .Include(x => x.Office) // Prevent N+1 queries for Office.Name
                .Include(x => x.Customer) // Prevent N+1 queries for Customer.Name
                .Include(x => x.Quote) // Prevent N+1 queries for Quote.Name
                .Include(x => x.CustomerContact) // Prevent N+1 queries for CustomerContact.Name
                .Include(x => x.OrderLines) // Prevent N+1 queries for OrderLines
                .Include(x => x.OrderEmails) // Prevent N+1 queries for OrderEmails
                .ThenInclude(x => x.Email) // Prevent N+1 queries for Email.CalculatedDeliveryStatus
                .WhereIf(input.StartDate.HasValue,
                         x => x.DeliveryDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue,
                         x => x.DeliveryDate <= input.EndDate)
                .WhereIf(input.OfficeId.HasValue,
                         x => x.OfficeId == input.OfficeId)
                .WhereIf(input.CustomerId.HasValue,
                         x => x.CustomerId == input.CustomerId)
                .WhereIf(input.ItemId.HasValue,
                         x => x.OrderLines.Any(ol => ol.FreightItemId == input.ItemId || ol.MaterialItemId == input.ItemId))
                .WhereIf(!string.IsNullOrEmpty(input.JobNumber),
                        x => x.OrderLines.Any(ol => ol.JobNumber == input.JobNumber))
                .WhereIf(!string.IsNullOrEmpty(input.Misc),
                         x => x.Quote.Name.Contains(input.Misc)
                         || x.ChargeTo.Contains(input.Misc))
                .WhereIf(input.LoadAtId.HasValue,
                         x => x.OrderLines.Any(ol => ol.LoadAtId == input.LoadAtId))
                .WhereIf(input.DeliverToId.HasValue,
                         x => x.OrderLines.Any(ol => ol.DeliverToId == input.DeliverToId))
                 .Where(x => x.IsPending == input.ShowPendingOrders);

            var totalCount = await query.CountAsync();
            var items = await query
                .AsNoTracking() // Improve performance for read-only operations
                .Select(x => new OrderDto
                {
                    Id = x.Id,
                    DeliveryDate = x.DeliveryDate,
                    OfficeId = x.OfficeId,
                    OfficeName = x.Office.Name,
                    CustomerName = x.Customer.Name,
                    QuoteName = x.Quote.Name,
                    PONumber = x.PONumber,
                    ContactName = x.CustomerContact.Name,
                    ChargeTo = x.ChargeTo,
                    CODTotal = x.CODTotal,
                    NumberOfTrucks = Math.Round(x.OrderLines.Sum(ol => ol.NumberOfTrucks ?? 0), 2),
                    EmailDeliveryStatuses = x.OrderEmails.Select(y => y.Email.CalculatedDeliveryStatus).ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OrderDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_IdDropdown, AppPermissions.CustomerPortal_Orders_IdDropdown)]
        public async Task<ListResultDto<SelectListDto>> GetOrderIdsSelectList(GetSelectListInput input)
        {
            int? customerId = null;
            var permissions = new
            {
                AllOrderIdsDropdown = await IsGrantedAsync(AppPermissions.Pages_Orders_IdDropdown),
                OnlyCustomerOrderIdsDropdown = await IsGrantedAsync(AppPermissions.CustomerPortal_Orders_IdDropdown),
            };

            if (permissions.AllOrderIdsDropdown)
            {
                //do not additionally filter the data
            }
            else if (permissions.OnlyCustomerOrderIdsDropdown)
            {
                customerId = Session.GetCustomerIdOrThrow(this);
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            var ordersQuery = (await _orderRepository.GetQueryAsync())
                .WhereIf(customerId.HasValue, x => x.CustomerId == customerId)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Id.ToString(),
                });

            return await ordersQuery.GetSelectListResult(input);
        }

        [DontWrapResult]
        [AbpAuthorize(AppPermissions.Pages_Orders_View, AppPermissions.LeaseHaulerPortal_Jobs_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<OrderEditDto> GetOrderForEdit(NullableIdDto input)
        {
            var permissions = new
            {
                ViewOrders = await IsGrantedAsync(AppPermissions.Pages_Orders_View),
                EditLeaseHaulerScheduledJobs = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Jobs_Edit),
                LeaseHaulerTruckRequest = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Truck_Request),
            };

            var result = await GetOrderForEditInternal(input);
            if (permissions.ViewOrders)
            {
                // show all
            }
            else if (permissions.EditLeaseHaulerScheduledJobs || permissions.LeaseHaulerTruckRequest)
            {
                // remove secret values
                result.SalesTaxRate = 0;
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            return result;
        }

        private async Task<OrderEditDto> GetOrderForEditInternal(NullableIdDto input)
        {
            int? leaseHaulerIdFilter = null;
            int? officeIdFilter = null;
            bool editOrdersPermission;

            using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdAsync()))
            {
                var permissions = new
                {
                    ViewOrders = await IsGrantedAsync(AppPermissions.Pages_Orders_View),
                    EditOrders = await IsGrantedAsync(AppPermissions.Pages_Orders_Edit),
                    EditLeaseHaulerScheduledJobs = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Jobs_Edit),
                    LeaseHaulerTruckRequest = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Truck_Request),
                };
                editOrdersPermission = permissions.EditOrders;

                if (permissions.ViewOrders || permissions.EditOrders)
                {
                    officeIdFilter = Session.GetOfficeIdOrThrow();
                }
                else if (permissions.EditLeaseHaulerScheduledJobs || permissions.LeaseHaulerTruckRequest)
                {
                    leaseHaulerIdFilter = Session.GetLeaseHaulerIdOrThrow(this);
                }
                else
                {
                    throw new AbpAuthorizationException();
                }
            }

            OrderEditDto orderEditDto;

            if (input.Id.HasValue)
            {
                orderEditDto = await (await _orderRepository.GetQueryAsync())
                    .Select(order => new OrderEditDto
                    {
                        Id = order.Id,
                        CreationTime = order.CreationTime,
                        CreatorName = order.CreatorUser.Name + " " + order.CreatorUser.Surname,
                        LastModificationTime = order.LastModificationTime,
                        LastModifierName = order.LastModifierUser.Name + " " + order.LastModifierUser.Surname,
                        MaterialCompanyOrderId = order.MaterialCompanyOrderId,
                        CODTotal = order.CODTotal,
                        ContactId = order.ContactId,
                        ContactName = order.CustomerContact.Name,
                        ContactPhone = order.CustomerContact.PhoneNumber,
                        ChargeTo = order.ChargeTo,
                        CustomerId = order.CustomerId,
                        CustomerName = order.Customer.Name,
                        CustomerAccountNumber = order.Customer.AccountNumber,
                        CustomerIsCod = order.Customer.IsCod,
                        DeliveryDate = order.DeliveryDate,
                        Shift = order.Shift,
                        IsPending = order.IsPending,
                        Directions = order.Directions,
                        FreightTotal = order.FreightTotal,
                        IsClosed = order.IsClosed,
                        //IsFreightTotalOverridden = order.IsFreightTotalOverridden,
                        //IsMaterialTotalOverridden = order.IsMaterialTotalOverridden,
                        OfficeId = order.OfficeId,
                        MaterialTotal = order.MaterialTotal,
                        OfficeName = order.Office.Name,
                        PONumber = order.PONumber,
                        PricingTierId = order.Customer.PricingTierId,
                        SpectrumNumber = order.SpectrumNumber,
                        QuoteId = order.QuoteId,
                        QuoteName = order.Quote.Name,
                        IsTaxExempt = order.IsTaxExempt,
                        CustomerIsTaxExempt = order.Customer.IsTaxExempt,
                        QuoteIsTaxExempt = order.Quote.IsTaxExempt,
                        SalesTax = order.SalesTax,
                        SalesTaxRate = order.SalesTaxRate,
                        SalesTaxEntityId = order.SalesTaxEntityId,
                        SalesTaxEntityName = order.SalesTaxEntity.Name,
                        Priority = order.Priority,
                        BaseFuelCost = order.BaseFuelCost,
                        FuelSurchargeCalculationId = order.FuelSurchargeCalculationId,
                        FuelSurchargeCalculationName = order.FuelSurchargeCalculation.Name,
                        CanChangeBaseFuelCost = order.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                        Receipts = order.Receipts
                            .Select(r => new ReceiptDto
                            {
                                Id = r.Id,
                                ReceiptDate = r.ReceiptDate,
                                Total = r.Total,
                            }).ToList(),
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);

                if (orderEditDto == null)
                {
                    throw await GetOrderNotFoundException(new EntityDto(input.Id.Value));
                }

                // check if this order has been assigned to this lease hauler
                if (leaseHaulerIdFilter.HasValue)
                {
                    await CheckLeaseHaulerEditOrderPermission(input.Id.Value);
                }

                var payment = officeIdFilter.HasValue
                    ? await (await _orderRepository.GetQueryAsync())
                        .Where(x => x.Id == input.Id)
                        .SelectMany(x => x.OrderPayments)
                        .Where(x => x.OfficeId == officeIdFilter)
                        .Select(x => x.Payment)
                        .Where(x => !x.IsCancelledOrRefunded)
                        .Select(x => new
                        {
                            x.AuthorizationDateTime,
                            x.AuthorizationCaptureDateTime,
                        }).FirstOrDefaultAsync()
                    : null;

                orderEditDto.AuthorizationDateTime = payment?.AuthorizationDateTime;
                orderEditDto.AuthorizationCaptureDateTime = payment?.AuthorizationCaptureDateTime;

                var timeZone = await GetTimezone();
                orderEditDto.CreationTime = orderEditDto.CreationTime?.ConvertTimeZoneTo(timeZone);
                orderEditDto.LastModificationTime = orderEditDto.LastModificationTime?.ConvertTimeZoneTo(timeZone);
            }
            else
            {
                if (!editOrdersPermission)
                {
                    throw new AbpAuthorizationException("You do not have permission to create orders");
                }
                orderEditDto = new OrderEditDto
                {
                    OfficeId = OfficeId,
                    OfficeName = Session.OfficeName,
                    Priority = OrderPriority.Medium,
                };
                if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.AutopopulateDefaultTaxRate)
                    && (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType) != TaxCalculationType.NoCalculation)
                {
                    orderEditDto.SalesTaxRate = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.DefaultTaxRate);
                }
            }
            orderEditDto.CanEditAnyOrderDirections = await CanEditAnyOrderDirectionsAsync();
            await _singleOfficeService.FillSingleOffice(orderEditDto);

            var defaultFuelSurchargeCalculationId = await SettingManager.GetDefaultFuelSurchargeCalculationId();
            if (defaultFuelSurchargeCalculationId > 0)
            {
                var defaultFuelSurchargeCalculation = await (await _fuelSurchargeCalculationRepository.GetQueryAsync())
                    .Where(x => x.Id == defaultFuelSurchargeCalculationId)
                    .Select(x => new
                    {
                        x.Name,
                        x.CanChangeBaseFuelCost,
                        x.BaseFuelCost,
                    })
                    .FirstOrDefaultAsync();

                orderEditDto.DefaultFuelSurchargeCalculationName = defaultFuelSurchargeCalculation.Name;
                orderEditDto.DefaultBaseFuelCost = defaultFuelSurchargeCalculation.BaseFuelCost;
                orderEditDto.DefaultCanChangeBaseFuelCost = defaultFuelSurchargeCalculation.CanChangeBaseFuelCost;

                if (!input.Id.HasValue)
                {
                    orderEditDto.FuelSurchargeCalculationId = defaultFuelSurchargeCalculationId;
                    orderEditDto.FuelSurchargeCalculationName = defaultFuelSurchargeCalculation.Name;
                    orderEditDto.CanChangeBaseFuelCost = defaultFuelSurchargeCalculation.CanChangeBaseFuelCost;
                    orderEditDto.BaseFuelCost = defaultFuelSurchargeCalculation.BaseFuelCost;
                }
            }
            else
            {
                orderEditDto.DefaultFuelSurchargeCalculationName = AppConsts.FuelSurchargeCalculationBlankName;
            }

            return orderEditDto;
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit)]
        public async Task<EditOrderResult> EditJob(JobEditDto model)
        {
            var permissions = new
            {
                EditOrders = await IsGrantedAsync(AppPermissions.Pages_Orders_Edit),
                EditLeaseHaulerScheduledJobs = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Jobs_Edit),
                CounterSales = await IsGrantedAsync(AppPermissions.Pages_CounterSales),
            };

            var editOrderModel = await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = true }, async () =>
            {
                if (permissions.EditOrders)
                {
                    // do nothing
                }
                else if (permissions.EditLeaseHaulerScheduledJobs)
                {
                    await CheckLeaseHaulerEditOrderLinePermission(model.OrderLineId);
                }
                else
                {
                    throw new AbpAuthorizationException();
                }

                return await GetOrderForEditInternal(new NullableIdDto(model.OrderId));
            });

            if (permissions.EditOrders)
            {
                editOrderModel.DeliveryDate = model.DeliveryDate;
                editOrderModel.CustomerId = model.CustomerId;
                editOrderModel.QuoteId = model.QuoteId;
                editOrderModel.Shift = model.Shift;
                editOrderModel.OfficeId = model.OfficeId;
                editOrderModel.ContactId = model.ContactId;
                editOrderModel.MaterialCompanyOrderId = model.MaterialCompanyOrderId;
                editOrderModel.PONumber = model.PONumber;
                editOrderModel.SpectrumNumber = model.SpectrumNumber;
                editOrderModel.Directions = model.Directions;
                editOrderModel.FuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
                editOrderModel.BaseFuelCost = model.BaseFuelCost;
                editOrderModel.IsTaxExempt = model.IsTaxExempt;

                if ((TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType) == TaxCalculationType.NoCalculation)
                {
                    editOrderModel.SalesTax = model.SalesTax;
                }
                else
                {
                    editOrderModel.SalesTaxRate = model.SalesTaxRate;
                    editOrderModel.SalesTaxEntityId = model.SalesTaxEntityId;
                }
            }

            editOrderModel.ChargeTo = model.ChargeTo;
            editOrderModel.Priority = model.Priority;

            var editOrderResult = await EditOrder(editOrderModel);
            if (!editOrderResult.Completed)
            {
                return editOrderResult;
            }

            return await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = true }, async () =>
            {
                var editOrderLineModel = await GetOrderLineForEditInternal(new GetOrderLineForEditInput(model.OrderLineId, editOrderResult.Id));
                if (permissions.EditOrders)
                {
                    editOrderLineModel.JobNumber = model.JobNumber;
                    editOrderLineModel.Designation = model.Designation;
                    editOrderLineModel.LoadAtId = model.LoadAtId;
                    editOrderLineModel.DeliverToId = model.DeliverToId;
                    editOrderLineModel.FreightItemId = model.FreightItemId;
                    editOrderLineModel.MaterialItemId = model.MaterialItemId;
                    editOrderLineModel.FreightUomId = model.FreightUomId;
                    editOrderLineModel.MaterialUomId = model.MaterialUomId;
                    editOrderLineModel.FreightPricePerUnit = model.FreightPricePerUnit;
                    editOrderLineModel.IsFreightPricePerUnitOverridden = model.IsFreightPricePerUnitOverridden;
                    editOrderLineModel.IsFreightRateToPayDriversOverridden = model.IsFreightRateToPayDriversOverridden;
                    editOrderLineModel.IsLeaseHaulerPriceOverridden = model.IsLeaseHaulerPriceOverridden;
                    editOrderLineModel.IsFreightPriceOverridden = model.IsFreightPriceOverridden;
                    editOrderLineModel.MaterialPricePerUnit = model.MaterialPricePerUnit;
                    editOrderLineModel.MaterialCostRate = model.MaterialCostRate;
                    editOrderLineModel.IsMaterialPricePerUnitOverridden = model.IsMaterialPricePerUnitOverridden;
                    editOrderLineModel.IsMaterialPriceOverridden = model.IsMaterialPriceOverridden;
                    editOrderLineModel.FreightQuantity = model.FreightQuantity;
                    editOrderLineModel.MaterialQuantity = model.MaterialQuantity;
                    editOrderLineModel.FreightPrice = model.FreightPrice;
                    editOrderLineModel.MaterialPrice = model.MaterialPrice;
                    editOrderLineModel.LeaseHaulerRate = model.LeaseHaulerRate;
                    editOrderLineModel.FreightRateToPayDrivers = model.FreightRateToPayDrivers;
                    editOrderLineModel.DriverPayTimeClassificationId = model.DriverPayTimeClassificationId;
                    editOrderLineModel.HourlyDriverPayRate = model.HourlyDriverPayRate;
                    editOrderLineModel.TravelTime = model.TravelTime;
                    editOrderLineModel.LoadBased = model.LoadBased;
                    editOrderLineModel.NumberOfTrucks = model.NumberOfTrucks;
                    editOrderLineModel.IsMultipleLoads = model.IsMultipleLoads;
                    editOrderLineModel.ProductionPay = model.ProductionPay;
                    editOrderLineModel.RequireTicket = model.RequireTicket;
                    editOrderLineModel.QuoteLineId = model.QuoteLineId;
                    editOrderLineModel.VehicleCategories = model.VehicleCategories;
                    editOrderLineModel.BedConstruction = model.BedConstruction;
                }
                editOrderLineModel.TimeOnJob = model.TimeOnJob;
                editOrderLineModel.UpdateOrderLineTrucksTimeOnJob = model.UpdateOrderLineTrucksTimeOnJob;
                editOrderLineModel.UpdateDispatchesTimeOnJob = model.UpdateDispatchesTimeOnJob;
                editOrderLineModel.Note = model.Note;
                editOrderLineModel.RequiresCustomerNotification = model.RequiresCustomerNotification;
                editOrderLineModel.CustomerNotificationContactName = model.CustomerNotificationContactName;
                editOrderLineModel.CustomerNotificationPhoneNumber = model.CustomerNotificationPhoneNumber;

                var orderLine = await EditOrderLine(editOrderLineModel);
                await CurrentUnitOfWork.SaveChangesAsync();

                var allowCounterSales = await SettingManager.AllowCounterSales();

                if (model.Designation == DesignationEnum.MaterialOnly && permissions.CounterSales && allowCounterSales)
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLine.OrderLineId);
                    await orderLineUpdater.UpdateFieldAsync(x => x.IsComplete, true);
                    await orderLineUpdater.SaveChangesAsync();
                }

                int? ticketId = null;

                if (model.Designation == DesignationEnum.MaterialOnly && permissions.CounterSales && allowCounterSales
                    && (model.TicketId != null || !string.IsNullOrEmpty(model.TicketNumber) || model.AutoGenerateTicketNumber))
                {
                    var ticket = model.TicketId != null ? await (await _ticketRepository.GetQueryAsync())
                        .Where(t => t.Id == model.TicketId)
                        .FirstOrDefaultAsync() : null;

                    if (ticket == null)
                    {
                        ticket = new Ticket
                        {
                            OrderLineId = orderLine.OrderLineId,
                        };

                        await _ticketRepository.InsertAndGetIdAsync(ticket);
                    }
                    ticket.TicketNumber = model.TicketNumber;
                    ticket.FreightQuantity = 0;
                    ticket.MaterialQuantity = model.MaterialQuantity;
                    ticket.TicketDateTime = editOrderModel.DeliveryDate?.ConvertTimeZoneFrom(await GetTimezone());
                    ticket.CustomerId = editOrderModel.CustomerId;
                    ticket.FreightItemId = editOrderLineModel.FreightItemId;
                    ticket.MaterialItemId = editOrderLineModel.MaterialItemId;
                    ticket.LoadAtId = editOrderLineModel.LoadAtId;
                    ticket.DeliverToId = editOrderLineModel.DeliverToId;
                    ticket.OfficeId = model.OfficeId;
                    ticket.IsInternal = true;
                    ticket.NonbillableFreight = true;
                    ticket.FreightUomId = editOrderLineModel.FreightUomId;
                    ticket.MaterialUomId = editOrderLineModel.MaterialUomId;

                    ticketId = ticket.Id;

                    if (model.AutoGenerateTicketNumber)
                    {
                        ticket.TicketNumber = "G-" + ticket.Id;
                    }
                }

                return new EditJobResult
                {
                    Id = editOrderResult.Id,
                    Completed = editOrderResult.Completed,
                    HasZeroQuantityItems = editOrderResult.HasZeroQuantityItems,
                    NotAvailableTrucks = editOrderResult.NotAvailableTrucks,
                    OrderLineId = orderLine.OrderLineId,
                    TicketId = ticketId,
                };
            });
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit)]
        public async Task<EditOrderResult> EditOrder(OrderEditDto model)
        {
            return await UnitOfWorkManager.WithUnitOfWorkNoCompleteAsync(new UnitOfWorkOptions { IsTransactional = true }, async unitOfWork =>
            {
                await CheckOrderEditPermissions(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit,
                    _orderLineRepository, model.Id);

                var result = await EditOrderInternal(model);

                if (!result.Completed)
                {
                    return result;
                }

                if (model.Id > 0)
                {
                    var failedEditResult = await SyncLinkedOrders(model.Id.Value);
                    if (failedEditResult != null && !failedEditResult.Completed)
                    {
                        return failedEditResult;
                    }
                }

                await unitOfWork.CompleteAsync();
                return result;
            });
        }

        private async Task CheckLeaseHaulerEditOrderLinePermission(int? orderLineId)
        {
            await CheckLeaseHaulerEditOrderLinePermission(_orderLineRepository, orderLineId);
        }

        private async Task CheckLeaseHaulerEditOrderPermission(int? orderId)
        {
            await CheckLeaseHaulerEditOrderPermission(_orderLineRepository, orderId);
        }

        private async Task<EditOrderResult> EditOrderInternal(OrderEditDto model)
        {
            var order = model.Id.HasValue ? await _orderRepository.GetAsync(model.Id.Value) : new Order();

            if (model.DeliveryDate == null)
            {
                throw new UserFriendlyException("Order Delivery Date is a required field");
            }
            var modelDeliveryDate = model.DeliveryDate.Value.Date;

            await ThrowUserFriendlyExceptionIfStatusIsChangedToPendingAndThereArePrerequisites(order, model);
            // #7551 prevent transferring to another office when there are trucks scheduled against it
            //await ClearTrucksIfOfficeIsChanged(order, model);
            if (model.Id.HasValue && model.OfficeId != order.OfficeId)
            {
                await ThrowIfOrderOfficeCannotBeChanged(model.Id.Value);
            }

            var dispatchRelatedFieldWasChanged = false;

            if (model.Id.HasValue)
            {
                if (order.DeliveryDate != model.DeliveryDate
                    || order.Shift != model.Shift
                    || order.QuoteId != model.QuoteId)
                {
                    await ThrowIfOrderLinesHaveTickets(model.Id.Value);
                }

                if (order.CustomerId != model.CustomerId)
                {
                    if (await HasLoadedDispatchesForOrder(model.Id.Value) || await HasManualTicketsForOrder(model.Id.Value))
                    {
                        throw new UserFriendlyException(L("Order_Edit_Error_HasDispatches"));
                    }
                    dispatchRelatedFieldWasChanged = true;
                }
            }

            var deliveryDateWasChanged = model.DeliveryDate != order.DeliveryDate;

            if (!model.Id.HasValue)
            {
                // no need to call invalidator for new orders
                //_dateCacheInvalidator.ChangeDateOrShift(order, model.DeliveryDate.Value, model.Shift);
                order.DeliveryDate = model.DeliveryDate.Value;
                order.Shift = model.Shift;
            }
            else if (order.DeliveryDate != model.DeliveryDate || order.Shift != model.Shift)
            {
                var setOrderDateResult = await SetOrderDateInternal(new SetOrderDateInput
                {
                    Date = model.DeliveryDate.Value,
                    Shift = model.Shift,
                    KeepTrucks = true,
                    OrderId = model.Id.Value,
                    OrderLineId = null,
                    RemoveNotAvailableTrucks = model.RemoveNotAvailableTrucks,
                });
                if (!setOrderDateResult.Completed)
                {
                    return new EditOrderResult
                    {
                        Completed = false,
                        NotAvailableTrucks = setOrderDateResult.NotAvailableTrucks,
                    };
                }
            }

            if (order.OfficeId != model.OfficeId && model.Id.HasValue)
            {
                var ticketQuery = (await _ticketRepository.GetQueryAsync()).Where(t => t.OrderLine.OrderId == model.Id);
                await ThrowIfTicketsOfficeCannotBeChanged(ticketQuery, this);

                var tickets = await ticketQuery.ToListAsync();
                foreach (var ticket in tickets)
                {
                    ticket.OfficeId = model.OfficeId;
                }
            }

            var needToRecalculateFuelSurcharge = order.FuelSurchargeCalculationId != model.FuelSurchargeCalculationId || order.BaseFuelCost != model.BaseFuelCost;

            order.CODTotal = model.CODTotal;
            order.ContactId = model.ContactId;
            order.ChargeTo = model.ChargeTo;
            order.CustomerId = model.CustomerId;
            order.IsPending = model.IsPending;
            order.Directions = model.Directions;
            order.FreightTotal = model.FreightTotal;
            order.IsClosed = model.IsClosed;
            //order.IsFreightTotalOverridden = model.IsFreightTotalOverridden;
            //order.IsMaterialTotalOverridden = model.IsMaterialTotalOverridden;
            order.OfficeId = model.OfficeId;
            order.MaterialTotal = model.MaterialTotal;
            order.PONumber = model.PONumber;
            order.SpectrumNumber = model.SpectrumNumber;
            order.QuoteId = model.QuoteId;
            order.IsTaxExempt = model.IsTaxExempt;
            order.SalesTax = model.SalesTax;
            order.SalesTaxRate = model.SalesTaxRate;
            order.SalesTaxEntityId = model.SalesTaxEntityId;
            order.FuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
            order.BaseFuelCost = model.BaseFuelCost;
            order.Priority = model.Priority;

            if (!model.Id.HasValue && model.OrderLines != null)
            {
                model.Id = await _orderRepository.InsertAndGetIdAsync(order);

                foreach (var orderLineModel in model.OrderLines)
                {
                    orderLineModel.OrderId = order.Id;
                    orderLineModel.UpdateStaggeredTime = true;

                    if (orderLineModel.FirstStaggeredTimeOnJob.HasValue)
                    {
                        orderLineModel.FirstStaggeredTimeOnJob = modelDeliveryDate.Add(orderLineModel.FirstStaggeredTimeOnJob.Value.TimeOfDay);
                    }
                    if (orderLineModel.TimeOnJob.HasValue)
                    {
                        orderLineModel.TimeOnJob = modelDeliveryDate.Add(orderLineModel.TimeOnJob.Value.TimeOfDay);
                    }
                    await EditOrderLineInternal(orderLineModel);
                }

                var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));

                await _orderTaxCalculator.CalculateTotalsAsync(order, model.OrderLines.Select(x => new OrderLineTaxDetailsDto
                {
                    IsTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? true,
                    IsFreightTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? true,
                    IsMaterialTaxable = items.Find(x.MaterialItemId)?.IsTaxable ?? true,
                    FreightPrice = x.FreightPrice,
                    MaterialPrice = x.MaterialPrice,
                }));
            }
            else if (model.Id > 0)
            {
                var orderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.OrderId == model.Id)
                    .Select(x => new OrderLineTaxDetailsDto
                    {
                        FreightPrice = x.FreightPrice,
                        MaterialPrice = x.MaterialPrice,
                        IsTaxable = x.FreightItem.IsTaxable,
                        IsMaterialTaxable = x.MaterialItem.IsTaxable,
                        IsFreightTaxable = x.FreightItem.IsTaxable,
                    }).ToListAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order, orderLines);
            }

            if (model.Id > 0 && deliveryDateWasChanged)
            {
                await UpdateOrderLineDatesIfNeeded(model.DeliveryDate.Value, model.Id);
            }

            var result = new EditOrderResult
            {
                Completed = true,
                Id = model.Id ?? await _orderRepository.InsertAndGetIdAsync(order),
            };
            model.Id = result.Id;
            await CurrentUnitOfWork.SaveChangesAsync();

            if (needToRecalculateFuelSurcharge)
            {
                await _fuelSurchargeCalculator.RecalculateOrderLinesWithTicketsForOrder(result.Id);
            }

            result.HasZeroQuantityItems = await (await _orderLineRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderId == model.Id
                    && (x.MaterialQuantity == null || x.MaterialQuantity == 0)
                    && (x.FreightQuantity == null || x.FreightQuantity == 0)
                    && (x.NumberOfTrucks == null || x.NumberOfTrucks == 0));

            if (dispatchRelatedFieldWasChanged)
            {
                await UpdateAssociatedDispatchesOnOrderChange(model.Id.Value);
            }

            return result;
        }

        /// <summary>
        /// Syncs destination orders linked to the provided source order
        /// </summary>
        /// <returns>Returns null in case of success, or EditOrderResult of a failed order in case of a failure</returns>
        private async Task<EditOrderResult> SyncLinkedOrders(int sourceOrderId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var sourceOrder = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id == sourceOrderId)
                .Select(x => new
                {
                    x.HasLinkedHaulingCompanyOrders,
                    x.DeliveryDate,
                    x.Directions,
                    x.IsClosed,
                    x.IsPending,
                    x.PONumber,
                    x.Priority,
                    x.SpectrumNumber,
                })
                .FirstAsync();

            if (sourceOrder.HasLinkedHaulingCompanyOrders)
            {
                var destinationOrderIds = new List<MustHaveTenantDto>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
                {
                    destinationOrderIds = await (await _orderRepository.GetQueryAsync())
                        .Where(x => x.MaterialCompanyOrderId == sourceOrderId)
                        .Select(x => new MustHaveTenantDto
                        {
                            Id = x.Id,
                            TenantId = x.TenantId,
                        })
                        .ToListAsync();
                }

                foreach (var destinationOrderId in destinationOrderIds)
                {
                    using (CurrentUnitOfWork.SetTenantId(destinationOrderId.TenantId))
                    {
                        var admin = await UserManager.GetAdminAsync();
                        using (Session.Use(admin.ToUserIdentifier()))
                        {
                            var destinationOrderEditDto = await GetOrderForEditInternal(new NullableIdDto(destinationOrderId.Id));
                            destinationOrderEditDto.DeliveryDate = sourceOrder.DeliveryDate;
                            destinationOrderEditDto.Directions = sourceOrder.Directions;
                            destinationOrderEditDto.IsClosed = sourceOrder.IsClosed;
                            destinationOrderEditDto.IsPending = sourceOrder.IsPending;
                            destinationOrderEditDto.PONumber = sourceOrder.PONumber;
                            destinationOrderEditDto.Priority = sourceOrder.Priority;
                            destinationOrderEditDto.SpectrumNumber = sourceOrder.SpectrumNumber;
                            var destinationEditResult = await EditOrderInternal(destinationOrderEditDto);
                            if (!destinationEditResult.Completed)
                            {
                                return destinationEditResult;
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                }
            }

            return null;
        }

        private async Task UpdateAssociatedDispatchesOnOrderChange(int orderId)
        {
            var dispatches = await (await _dispatchRepository.GetQueryAsync())
                                .Where(x => x.OrderLine.OrderId == orderId)
                                .ToListAsync();

            if (dispatches.Any())
            {
                dispatches.ForEach(d => d.LastModificationTime = Clock.Now);

                await CurrentUnitOfWork.SaveChangesAsync();

                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage("Updated Order has affected dispatch(es)"));
            }
        }

        private async Task ThrowUserFriendlyExceptionIfStatusIsChangedToPendingAndThereArePrerequisites(Order order, OrderEditDto model)
        {
            if (model.Id.HasValue && model.IsPending && order.IsPending != model.IsPending)
            {
                if (await (await _orderLineRepository.GetQueryAsync())
                    .AnyAsync(ol => ol.OrderId == order.Id
                        && (
                            ol.IsComplete
                            || ol.Tickets.Any()
                            || ol.OrderLineTrucks.Any()
                        )
                    ))
                {
                    throw new UserFriendlyException("Cannot change status to Pending.");
                }
            }
        }

        private async Task ThrowIfOrderLinesHaveTickets(int orderId)
        {
            var orderLineDetails = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.OrderId == orderId)
                .Select(x => new
                {
                    HasTickets = x.Tickets.Any(),
                })
                .ToListAsync();

            if (orderLineDetails.Any(x => x.HasTickets))
            {
                throw new UserFriendlyException(L("Order_Edit_Error_HasTickets"));
            }
        }

        private async Task ThrowIfOrderLineHasTicketsOrActualAmounts(int orderLineId)
        {
            var orderLineDetails = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    HasTickets = x.Tickets.Any(),
                    HasOpenDispatches = x.Dispatches.Any(d => !Dispatch.ClosedDispatchStatuses.Contains(d.Status)),
                })
                .FirstAsync();

            if (orderLineDetails.HasTickets || orderLineDetails.HasOpenDispatches)
            {
                throw new UserFriendlyException(L("OrderLine_Edit_Error_HasTickets"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)] //TicketsByDriver is the only view this method is currently being used from. If we ever need to use it from somewhere else we can try to find a better permission
        public async Task ThrowIfOrderOfficeCannotBeChanged(int orderId)
        {
            await ThrowIfTicketsOfficeCannotBeChanged((await _ticketRepository.GetQueryAsync())
                .Where(x => x.OrderLine.OrderId == orderId), this);
        }

        public static async Task ThrowIfTicketsOfficeCannotBeChanged(IQueryable<Ticket> tickets, ILocalizationHelperProvider localizationHelperProvider)
        {
            var localizationHelper = localizationHelperProvider.LocalizationHelper;
            if (await tickets.AnyAsync(t => t.InvoiceLine != null || t.LeaseHaulerStatementTicket != null))
            {
                throw new UserFriendlyException(localizationHelper.L("TicketsHaveBeenInvoicesdOrOnLHPayStatements_TicketsCantBeMovedToDifferentOffice"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<OrderLastModifiedDatesDto> GetOrderLastModifiedDates(int orderId)
        {
            var result = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id == orderId)
                .Select(x => new OrderLastModifiedDatesDto
                {
                    LastModificationTime = x.LastModificationTime,
                    LastModifierName = x.LastModifierUser.Name + " " + x.LastModifierUser.Surname,
                    CreationTime = x.CreationTime,
                    CreatorName = x.CreatorUser.Name + " " + x.CreatorUser.Surname,
                }).FirstAsync();

            var timeZone = await GetTimezone();
            result.CreationTime = result.CreationTime.ConvertTimeZoneTo(timeZone);
            result.LastModificationTime = result.LastModificationTime?.ConvertTimeZoneTo(timeZone);

            return result;
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<SetOrderDateResult> SetOrderDate(SetOrderDateInput input)
        {
            return await UnitOfWorkManager.WithUnitOfWorkNoCompleteAsync(new UnitOfWorkOptions { IsTransactional = true }, async unitOfWork =>
            {
                var result = await SetOrderDateInternal(input);
                if (result.Completed)
                {
                    if (input.OrderLineId.HasValue)
                    {
                        var newOrder = await (await _orderLineRepository.GetQueryAsync())
                            .Where(x => x.Id == input.OrderLineId)
                            .Select(x => new
                            {
                                x.OrderId,
                            }).FirstAsync();
                        await UpdateOrderLineDatesIfNeeded(input.Date, newOrder.OrderId);
                    }
                    else
                    {
                        await UpdateOrderLineDatesIfNeeded(input.Date, input.OrderId);
                    }

                    await unitOfWork.CompleteAsync();
                }
                return result;
            });
        }

        private async Task<SetOrderDateResult> SetOrderDateInternal(SetOrderDateInput input)
        {
            await CheckUseShiftSettingCorrespondsInput(input.Shift);
            var result = new SetOrderDateResult();
            var order = await _orderRepository.GetAsync(input.OrderId);
            if (order.DeliveryDate == input.Date && order.Shift == input.Shift)
            {
                result.Completed = true;
                return result;
            }
            if (input.OrderLineId == null)
            {
                var hasCompletedOrderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.OrderId == input.OrderId)
                    .AnyAsync(x => x.IsComplete);

                if (hasCompletedOrderLines)
                {
                    throw new UserFriendlyException(L("Order_Edit_Error_HasCompletedOrderLines"));
                }

                await CheckOpenDispatchesForOrder(input.OrderId);
                await ThrowIfOrderLinesHaveTickets(input.OrderId);

                if (!input.KeepTrucks)
                {
                    await DeleteOrderLineTrucks(x => x.OrderLine.OrderId == input.OrderId);
                }
                else
                {
                    await DeleteOrderLineTrucks(x => x.OrderLine.OrderId == input.OrderId
                        && (!x.Truck.IsActive
                            || x.Truck.IsOutOfService
                            || x.Truck.OfficeId == null
                            || x.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule), input.Date);
                    var notAvailableTrucks = await GetOrderTrucksNotAvailableForDateShift(input.OrderId, input.Date, input.Shift);
                    notAvailableTrucks.AddRange(await GetOrderTrucksWithoutDriverOnDateShift(input.OrderId, input.Date, input.Shift));
                    if (await ShouldReturnResultOrDeleteNotAvailableTrucks(notAvailableTrucks, input.RemoveNotAvailableTrucks, result))
                    {
                        return result;
                    }

                    await UpdateDriverAssignmentsDateShift(order.Id, order.DeliveryDate, order.Shift, input.Date.Date, input.Shift);
                }
                _dateCacheInvalidator.ChangeDateOrShift(order, input.Date.Date, input.Shift);
            }
            else
            {
                var orderLine = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == input.OrderLineId)
                    .Select(x => new
                    {
                        x.IsComplete,
                    }).SingleAsync();

                if (orderLine.IsComplete)
                {
                    throw new UserFriendlyException(L("Order_Edit_Error_HasCompletedOrderLines"));
                }

                await CheckOpenDispatchesForOrderLine(input.OrderLineId.Value);
                await ThrowIfOrderLineHasTicketsOrActualAmounts(input.OrderLineId.Value);

                if (!input.KeepTrucks)
                {
                    await DeleteOrderLineTrucks(x => x.OrderLineId == input.OrderLineId);
                }
                else
                {
                    await DeleteOrderLineTrucks(x => x.OrderLineId == input.OrderLineId
                        && (!x.Truck.IsActive
                            || x.Truck.IsOutOfService
                            || x.Truck.OfficeId == null
                            || x.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule), input.Date);
                    var notAvailableTrucks = await GetOrderLineTrucksNotAvailableForDateShift(input.OrderLineId.Value, input.Date, input.Shift);
                    notAvailableTrucks.AddRange(await GetOrderLineTrucksWithoutDriverOnDateShift(input.OrderLineId.Value, input.Date, input.Shift));
                    if (await ShouldReturnResultOrDeleteNotAvailableTrucks(notAvailableTrucks, input.RemoveNotAvailableTrucks, result))
                    {
                        return result;
                    }

                    await CopyDriverAssignmentsForOrderLineWithNewDateShift(input.OrderLineId.Value, order.DeliveryDate, order.Shift, input.Date.Date, input.Shift);
                }
                var newOrder = await CreateOrderCopyAndAssignOrderLineToIt(order, input.OrderLineId.Value);
                newOrder.DeliveryDate = input.Date.Date;
                newOrder.Shift = input.Shift;
            }
            result.Completed = true;
            return result;

            // Local functions
            async Task<List<NotAvailableOrderLineTruck>> GetOrderLineTrucksWithoutDriverOnDateShift(int orderLineId, DateTime date, Shift? shift) =>
                await GetTrucksWithoutDriverOnDateShift(null, orderLineId, date, shift);
            async Task<List<NotAvailableOrderLineTruck>> GetOrderTrucksWithoutDriverOnDateShift(int orderId, DateTime date, Shift? shift) =>
                await GetTrucksWithoutDriverOnDateShift(orderId, null, date, shift);
            async Task<List<NotAvailableOrderLineTruck>> GetTrucksWithoutDriverOnDateShift(int? orderId, int? orderLineId, DateTime date, Shift? shift)
            {
                return await (await _orderLineTruckRepository.GetQueryAsync())
                    .WhereIf(orderId.HasValue, olt => olt.OrderLine.OrderId == orderId)
                    .WhereIf(orderLineId.HasValue, olt => olt.OrderLineId == orderLineId.Value)
                    .Where(olt => olt.Truck.DriverAssignments.Any(da => da.Date == date && da.Shift == shift && da.DriverId == null))
                    .Select(olt => new NotAvailableOrderLineTruck(olt.TruckId, olt.OrderLineId, olt.Truck.TruckCode, olt.Utilization))
                    .ToListAsync();
            }

            async Task UpdateDriverAssignmentsDateShift(int orderId, DateTime date, Shift? shift, DateTime newDate, Shift? newShift)
            {
                await UpdateOrCopyDriverAssignments(orderId, null, date, shift, newDate, newShift);
            }

            async Task CopyDriverAssignmentsForOrderLineWithNewDateShift(int orderLineId, DateTime date, Shift? shift, DateTime newDate, Shift? newShift)
            {
                await UpdateOrCopyDriverAssignments(null, orderLineId, date, shift, newDate, newShift);
            }

            async Task UpdateOrCopyDriverAssignments(int? orderId, int? orderLineId, DateTime date, Shift? shift, DateTime newDate, Shift? newShift)
            {
                var timezone = await GetTimezone();
                var driverAssignmentsToCopy = await GetDriverAssignmentsForOrderOrOrderLineTrucksOnDateShift(orderId, orderLineId, date, shift);
                var existingDriverAssignmentsOnNewDateShift = await GetDriverAssignmentsForOrderOrOrderLineTrucksOnDateShift(orderId, orderLineId, newDate, newShift);
                foreach (var driverAssignment in driverAssignmentsToCopy)
                {
                    if (existingDriverAssignmentsOnNewDateShift.Any(da =>
                            da.TruckId == driverAssignment.TruckId
                            && da.Date == newDate
                            && da.Shift == newShift))
                    {
                        continue;
                    }

                    if (orderId.HasValue)
                    {
                        _dateCacheInvalidator.ChangeDateOrShift(driverAssignment, newDate, newShift);
                    }
                    else
                    {
                        var startTime = driverAssignment.StartTime?.ConvertTimeZoneTo(timezone);
                        var driverAssignmentToCopy = new DriverAssignment
                        {
                            Date = newDate,
                            Shift = newShift,
                            DriverId = driverAssignment.DriverId,
                            TruckId = driverAssignment.TruckId,
                            OfficeId = driverAssignment.OfficeId,
                            StartTime = (startTime == null ? (DateTime?)null : newDate.Date.Add(startTime.Value.TimeOfDay))?.ConvertTimeZoneFrom(timezone),
                        };
                        existingDriverAssignmentsOnNewDateShift.Add(driverAssignmentToCopy);
                        await _driverAssignmentRepository.InsertAsync(driverAssignmentToCopy);
                    }
                }

            }

            async Task<IList<DriverAssignment>> GetDriverAssignmentsForOrderOrOrderLineTrucksOnDateShift(int? orderId, int? orderLineId, DateTime date, Shift? shift)
            {
                Debug.Assert(orderId.HasValue || orderLineId.HasValue);
                Debug.Assert(!orderId.HasValue || !orderLineId.HasValue);
                return await (await _orderLineTruckRepository.GetQueryAsync())
                    .WhereIf(orderId.HasValue, olt => olt.OrderLine.OrderId == orderId)
                    .WhereIf(orderLineId.HasValue, olt => olt.OrderLineId == orderLineId)
                    .SelectMany(olt => olt.Truck.DriverAssignments)
                    .Where(da => da.Date == date && da.Shift == shift)
                    .Select(da => da)
                    .ToListAsync();
            }
        }

        private async Task DeleteOrderLineTrucks(Expression<Func<OrderLineTruck, bool>> filter, DateTime? deliveryDate = null)
        {
            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(filter)
                .ToListAsync();

            await _orderLineTruckRepository.DeleteRangeAsync(orderLineTrucks);

            var today = await GetToday();
            var orderLineIds = orderLineTrucks.Select(x => x.OrderLineId).Distinct().ToList();
            var orderLineDates = await (await _orderLineRepository.GetQueryAsync())
                .Select(x => new
                {
                    x.Id,
                    x.Order.DeliveryDate,
                }).ToListAsync();

            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var orderLineId in orderLineIds)
            {
                if ((deliveryDate ?? orderLineDates.FirstOrDefault(x => x.Id == orderLineId)?.DeliveryDate) >= today)
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                    orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                    await orderLineUpdater.SaveChangesAsync();
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task UpdateOrderLineDatesIfNeeded(DateTime deliveryDate, int? orderId)
        {
            if (!(orderId > 0))
            {
                return;
            }

            var orderLines = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            var timezone = await GetTimezone();

            var notStaggeredOrderLineIds = new List<int>();

            foreach (var orderLine in orderLines)
            {
                var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLine.Id);
                await orderLineUpdater.UpdateFieldAsync(x => x.FirstStaggeredTimeOnJob, deliveryDate.AddTimeOrNull(orderLine.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone));
                await orderLineUpdater.UpdateFieldAsync(x => x.TimeOnJob, deliveryDate.AddTimeOrNull(orderLine.TimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone));
                await orderLineUpdater.SaveChangesAsync();

                //staggered OLT time will be recalculated when FirstStaggeredTimeOnJob changes above, so we only need to handle non-staggered ones
                if (orderLine.StaggeredTimeKind == StaggeredTimeKind.None)
                {
                    notStaggeredOrderLineIds.Add(orderLine.Id);
                }
            }

            if (notStaggeredOrderLineIds.Any())
            {
                var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => notStaggeredOrderLineIds.Contains(x.OrderLineId))
                    .ToListAsync();

                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.TimeOnJob = deliveryDate.AddTimeOrNull(orderLineTruck.TimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task CheckOpenDispatchesForOrderLine(int orderLineId)
        {
            if (await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == orderLineId)
                    .AnyAsync(x => x.Dispatches.Any(d => Dispatch.OpenStatuses.Contains(d.Status))))
            {
                throw new UserFriendlyException(L("Order_Edit_Error_HasDispatches"));
            }
        }

        private async Task CheckOpenDispatchesForOrder(int orderId)
        {
            if (await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.OrderId == orderId)
                    .AnyAsync(x => x.Dispatches.Any(d => Dispatch.OpenStatuses.Contains(d.Status))))
            {
                throw new UserFriendlyException(L("Order_Edit_Error_HasDispatches"));
            }
        }

        private async Task<bool> HasLoadedDispatchesForOrder(int orderId)
        {
            return await (await _orderLineRepository.GetQueryAsync())
                    .AnyAsync(ol => ol.OrderId == orderId && ol.Dispatches.Any(d => d.Status == DispatchStatus.Loaded || d.Status == DispatchStatus.Completed));
        }

        private async Task<bool> HasManualTicketsForOrder(int orderId)
        {
            return await (await _orderLineRepository.GetQueryAsync())
                .AnyAsync(ol => ol.OrderId == orderId
                    && ol.Tickets.Any(t => t.Load == null));
        }

        private async Task<bool> ShouldReturnResultOrDeleteNotAvailableTrucks(
            List<NotAvailableOrderLineTruck> notAvailableTrucks,
            bool removeNotAvailableTrucks,
            SetOrderDateResult result
        )
        {
            if (notAvailableTrucks.Count > 0)
            {
                if (removeNotAvailableTrucks)
                {
                    var truckIdsPerOrderLineId = notAvailableTrucks.GroupBy(x => x.OrderLineId);
                    foreach (var truckIdGroup in truckIdsPerOrderLineId)
                    {
                        var orderLineId = truckIdGroup.Key;
                        var truckIds = truckIdGroup.Select(x => x.TruckId).Distinct().ToList();
                        await DeleteOrderLineTrucks(x => x.OrderLineId == orderLineId && truckIds.Contains(x.TruckId));
                    }
                }
                else
                {
                    result.NotAvailableTrucks = notAvailableTrucks.Select(x => x.TruckCode).ToList();
                    return true;
                }
            }
            return false;
        }
        private async Task<List<NotAvailableOrderLineTruck>> GetOrderTrucksNotAvailableForDateShift(int orderId, DateTime date, Shift? shift)
        {
            return await GetTrucksNotAvailableForDateShift(orderId, null, date, shift);
        }
        private async Task<List<NotAvailableOrderLineTruck>> GetOrderLineTrucksNotAvailableForDateShift(int orderLineId, DateTime date, Shift? shift)
        {
            return await GetTrucksNotAvailableForDateShift(null, orderLineId, date, shift);
        }
        private async Task<List<NotAvailableOrderLineTruck>> GetTrucksNotAvailableForDateShift(int? orderId, int? orderLineId, DateTime date, Shift? shift)
        {
            Debug.Assert(orderId.HasValue && !orderLineId.HasValue || !orderId.HasValue && orderLineId.HasValue);
            var orderTrucks = (await _orderLineTruckRepository.GetQueryAsync())
                .WhereIf(orderId.HasValue, olt => olt.OrderLine.OrderId == orderId.Value)
                .WhereIf(orderLineId.HasValue, olt => olt.OrderLineId == orderLineId.Value)
                .Select(olt => new { olt.TruckId, olt.OrderLineId, olt.Truck.TruckCode, olt.Utilization });
            var anotherDateOrdersTrucks = (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLine.Order.DeliveryDate == date && olt.OrderLine.Order.Shift == shift)
                .Select(olt => new { olt.TruckId, olt.Utilization });
            var notAvailableTrucks =
                from ot in orderTrucks
                join aot in anotherDateOrdersTrucks on ot.TruckId equals aot.TruckId
                where (ot.Utilization + aot.Utilization) > 1
                select new NotAvailableOrderLineTruck(ot.TruckId, ot.OrderLineId, ot.TruckCode, ot.Utilization + aot.Utilization);
            return await notAvailableTrucks.ToListAsync();
        }
        private class NotAvailableOrderLineTruck
        {
            public NotAvailableOrderLineTruck(int truckId, int orderLineId, string truckCode, decimal utilization)
            {
                TruckId = truckId;
                OrderLineId = orderLineId;
                TruckCode = truckCode;
                Utilization = utilization;
            }
            public int TruckId { get; }
            public int OrderLineId { get; }
            public string TruckCode { get; }
            public decimal Utilization { get; }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task SetOrderOfficeId(SetOrderOfficeIdInput input)
        {
            var order = await _orderRepository.GetAsync(input.OrderId);
            if (order.OfficeId == input.OfficeId)
            {
                return;
            }
            if (input.OrderLineId == null)
            {
                var ticketQuery = (await _ticketRepository.GetQueryAsync()).Where(t => t.OrderLine.OrderId == input.OrderId);
                await ThrowIfTicketsOfficeCannotBeChanged(ticketQuery, this);

                order.OfficeId = input.OfficeId;

                await _orderLineRepository.DeleteAsync(x => x.OrderId == input.OrderId
                    && (x.MaterialQuantity == null || x.MaterialQuantity == 0)
                    && (x.FreightQuantity == null || x.FreightQuantity == 0)
                    && (x.NumberOfTrucks == null || x.NumberOfTrucks < 0.01));

                var tickets = await ticketQuery.ToListAsync();
                foreach (var ticket in tickets)
                {
                    ticket.OfficeId = input.OfficeId;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);
            }
            else
            {
                var ticketQuery = (await _ticketRepository.GetQueryAsync()).Where(t => t.OrderLineId == input.OrderLineId);
                await ThrowIfTicketsOfficeCannotBeChanged(ticketQuery, this);

                var newOrder = await CreateOrderCopyAndAssignOrderLineToIt(order, input.OrderLineId.Value);
                newOrder.OfficeId = input.OfficeId;

                var tickets = await ticketQuery.ToListAsync();
                foreach (var ticket in tickets)
                {
                    ticket.OfficeId = input.OfficeId;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsAsync(newOrder.Id);
                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);
            }
        }

        private async Task<Order> CreateOrderCopyAndAssignOrderLineToIt(Order order, int orderLineId)
        {
            var newOrder = order.CreateCopy();
            await _orderRepository.InsertAsync(newOrder);
            var orderLine = await _orderLineRepository.GetAsync(orderLineId);
            orderLine.Order = newOrder;

            await DecrementOrderLineNumbers(order.Id, orderLine.LineNumber);
            orderLine.LineNumber = 1;

            await CurrentUnitOfWork.SaveChangesAsync();

            await _orderTaxCalculator.CalculateTotalsAsync(order.Id);
            await _orderTaxCalculator.CalculateTotalsAsync(newOrder.Id);

            return newOrder;
        }



        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<bool> DoesOrderHaveOtherOrderLines(int orderId, int orderLineId)
        {
            return await (await _orderLineRepository.GetQueryAsync()).AnyAsync(ol => ol.OrderId == orderId && ol.Id != orderLineId);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<int[]> CopyOrder(CopyOrderInput input)
        {
            if (input.DateBegin > input.DateEnd)
            {
                throw new ArgumentException($"{nameof(input.DateBegin)} must be less or equal than {nameof(input.DateEnd)}");
            }

            if ((input.DateEnd - input.DateBegin).Days + 1 > 7)
            {
                throw new ArgumentException("You cannot copy order for more than 7 days.");
            }

            await CheckUseShiftSettingCorrespondsInput(input.Shifts);
            Shift?[] shifts = input.Shifts.ToNullableArrayWithNullElementIfEmpty();

            IQueryable<Order> query = (await _orderRepository.GetQueryAsync())
                .AsNoTracking()
                .Include(x => x.OrderLines)
                    .ThenInclude(x => x.OrderLineVehicleCategories);

            if (input.CarryUnfinishedPortionForward)
            {
                query = query
                    .Include(x => x.OrderLines)
                        .ThenInclude(x => x.Tickets);
            }

            var order = await query
                .FirstOrDefaultAsync(x => x.Id == input.OrderId);

            if (order == null)
            {
                throw new UserFriendlyException("The order you're trying to copy wasn't found, it might have been already deleted.");
            }

            if (!order.OrderLines.Any())
            {
                throw new UserFriendlyException("This order doesn't have any line items, so it can't be copied.");
            }

            var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));

            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            var allowLoadBasedRates = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowLoadBasedRates);
            var hideFreightRateToPayDrivers = !allowProductionPay
                || !await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature)
                || !await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate);

            var timezone = await GetTimezone();

            var createdOrderIds = new List<int>();
            var currentDate = input.DateBegin;
            while (currentDate <= input.DateEnd)
            {
                foreach (var shift in shifts)
                {
                    var newOrder = order.CreateCopy();
                    newOrder.IsClosed = false;
                    newOrder.DeliveryDate = currentDate.Date;
                    newOrder.Shift = shift;

                    var newOrderLines = order.OrderLines.ToList();

                    bool copySingleOrderLine = newOrderLines
                        .WhereIf(input.OrderLineId.HasValue, ol => ol.Id == input.OrderLineId)
                        .WhereIf(input.OrderLineId.HasValue, ol => ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                        .Count() == 1;

                    newOrderLines = order.OrderLines
                        .WhereIf(input.OrderLineId.HasValue, ol => ol.Id == input.OrderLineId)
                        .WhereIf(!input.OrderLineId.HasValue, ol => ol.MaterialQuantity > 0 || ol.FreightQuantity > 0 || ol.NumberOfTrucks > 0)
                        .Select(x =>
                        {
                            var newOrderLine = new OrderLine
                            {
                                LineNumber = copySingleOrderLine ? 1 : x.LineNumber,
                                QuoteLineId = x.QuoteLineId,
                                MaterialQuantity = x.MaterialQuantity,
                                FreightQuantity = x.FreightQuantity,
                                NumberOfTrucks = x.NumberOfTrucks,
                                ScheduledTrucks = input.CopyTrucks ? x.ScheduledTrucks : null,
                                MaterialPricePerUnit = x.MaterialPricePerUnit,
                                MaterialCostRate = x.MaterialCostRate,
                                FreightPricePerUnit = x.FreightPricePerUnit,
                                IsMaterialPricePerUnitOverridden = x.IsMaterialPricePerUnitOverridden,
                                IsFreightPricePerUnitOverridden = x.IsFreightPricePerUnitOverridden,
                                IsFreightRateToPayDriversOverridden = x.IsFreightRateToPayDriversOverridden,
                                IsLeaseHaulerPriceOverridden = x.IsLeaseHaulerPriceOverridden,
                                FreightItemId = x.FreightItemId,
                                MaterialItemId = x.MaterialItemId,
                                LoadAtId = x.LoadAtId,
                                DeliverToId = x.DeliverToId,
                                FreightUomId = x.FreightUomId,
                                MaterialUomId = x.MaterialUomId,
                                Designation = x.Designation,
                                MaterialPrice = x.MaterialPrice,
                                FreightPrice = x.FreightPrice,
                                IsMaterialPriceOverridden = x.IsMaterialPriceOverridden,
                                IsFreightPriceOverridden = x.IsFreightPriceOverridden,
                                LeaseHaulerRate = x.LeaseHaulerRate,
                                FreightRateToPayDrivers = hideFreightRateToPayDrivers ? x.FreightPricePerUnit : x.FreightRateToPayDrivers,
                                DriverPayTimeClassificationId = x.DriverPayTimeClassificationId,
                                HourlyDriverPayRate = x.HourlyDriverPayRate,
                                TravelTime = x.TravelTime,
                                TimeOnJob = x.TimeOnJob == null ? null : (currentDate.Date.Add(x.TimeOnJob.Value.ConvertTimeZoneTo(timezone).TimeOfDay)).ConvertTimeZoneFrom(timezone),
                                FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob == null ? null : (currentDate.Date.Add(x.FirstStaggeredTimeOnJob.Value.ConvertTimeZoneTo(timezone).TimeOfDay)).ConvertTimeZoneFrom(timezone),
                                StaggeredTimeKind = x.StaggeredTimeKind,
                                StaggeredTimeInterval = x.StaggeredTimeInterval,
                                JobNumber = x.JobNumber,
                                Note = x.Note,
                                IsMultipleLoads = x.IsMultipleLoads,
                                BedConstruction = x.BedConstruction,
                                ProductionPay = allowProductionPay && x.ProductionPay,
                                RequireTicket = x.RequireTicket,
                                LoadBased = allowLoadBasedRates && x.LoadBased,
                                RequiresCustomerNotification = x.RequiresCustomerNotification,
                                CustomerNotificationContactName = x.CustomerNotificationContactName,
                                CustomerNotificationPhoneNumber = x.CustomerNotificationPhoneNumber,
                                Order = newOrder,
                            };
                            foreach (var vehicleCategory in x.OrderLineVehicleCategories)
                            {
                                newOrderLine.OrderLineVehicleCategories.Add(new OrderLineVehicleCategory
                                {
                                    OrderLine = newOrderLine,
                                    VehicleCategoryId = vehicleCategory.VehicleCategoryId,
                                });
                            }

                            if (input.CarryUnfinishedPortionForward)
                            {
                                var deliveredFreightQuantity = x.Tickets.Sum(t => t.FreightQuantity ?? 0);
                                var deliveredMaterialQuantity = x.Tickets.Sum(t => t.MaterialQuantity ?? 0);

                                if (newOrderLine.FreightUomId == newOrderLine.MaterialUomId)
                                {
                                    if (newOrderLine.MaterialQuantity > 0 && newOrderLine.MaterialQuantity < newOrderLine.FreightQuantity)
                                    {
                                        //for 'minimum freight amount' functionality keep freight quantity unchanged
                                        deliveredFreightQuantity = 0;
                                    }
                                    else
                                    {
                                        //otherwise, for matching UOMs, use material quantity as delivered freight quantity
                                        deliveredFreightQuantity = deliveredMaterialQuantity;
                                    }
                                }

                                newOrderLine.FreightQuantity = Math.Max(0, (x.FreightQuantity ?? 0) - deliveredFreightQuantity);
                                newOrderLine.MaterialQuantity = Math.Max(0, (x.MaterialQuantity ?? 0) - deliveredMaterialQuantity);
                            }

                            return newOrderLine;
                        }).ToList();

                    if (input.CarryUnfinishedPortionForward)
                    {
                        foreach (var newOrderLine in newOrderLines.ToList())
                        {
                            if (newOrderLine.MaterialQuantity == 0 && newOrderLine.FreightQuantity == 0)
                            {
                                newOrderLines.Remove(newOrderLine);
                            }
                        }
                        if (!newOrderLines.Any())
                        {
                            throw new UserFriendlyException("Order cannot be carried forward as all quantities have been fulfilled. No remaining materials or freight to process.");
                        }
                    }

                    foreach (var newOrderLine in newOrderLines)
                    {
                        await _orderLineRepository.InsertAsync(newOrderLine);
                    }

                    var newOrderLinesTaxDetails = newOrderLines.Select(x => new OrderLineTaxDetailsDto
                    {
                        MaterialPrice = x.MaterialPrice,
                        FreightPrice = x.FreightPrice,
                        IsTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? false,
                        IsMaterialTaxable = items.Find(x.MaterialItemId)?.IsTaxable ?? false,
                        IsFreightTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? false,
                    });
                    await _orderTaxCalculator.CalculateTotalsAsync(newOrder, newOrderLinesTaxDetails);

                    var newId = await _orderRepository.InsertAndGetIdAsync(newOrder);
                    newOrder.Id = newId;

                    await _fuelSurchargeCalculator.RecalculateOrderLinesWithTicketsForOrder(newOrder.Id);
                    createdOrderIds.Add(newId);
                }
                currentDate = currentDate.AddDays(1);
            }

            return createdOrderIds.ToArray();
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task RecalculateStaggeredTimeForOrders(RecalculateStaggeredTimeForOrdersInput input)
        {
            var today = await GetToday();
            var orderLines = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => input.OrderIds.Contains(x.OrderId)
                    && x.Order.DeliveryDate >= today
                    && x.StaggeredTimeKind != StaggeredTimeKind.None
                )
                .Select(x => new
                {
                    x.Id,
                    x.Order.DeliveryDate,
                })
                .ToListAsync();

            foreach (var orderLine in orderLines)
            {
                var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLine.Id);
                orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                await orderLineUpdater.SaveChangesAsync();
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<OrderInternalNotesDto> GetOrderInternalNotes(EntityDto input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.Id,
                    x.EncryptedInternalNotes,
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var result = new OrderInternalNotesDto
            {
                OrderId = order.Id,
            };

            result.InternalNotes = _encryptionService.DecryptIfNotEmpty(order.EncryptedInternalNotes);

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task SetOrderInternalNotes(OrderInternalNotesDto input)
        {
            var order = await _orderRepository.GetAsync(input.OrderId);

            order.EncryptedInternalNotes = _encryptionService.EncryptIfNotEmpty(input.InternalNotes);

            if (order.EncryptedInternalNotes?.Length > EntityStringFieldLengths.Order.EncryptedInternalNotes)
            {
                throw new UserFriendlyException("The internal notes are too long.");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<int> GetOrderDuplicateCount(GetOrderDuplicateCountInput input)
        {
            return await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id != input.Id
                        && x.CustomerId == input.CustomerId
                        && x.QuoteId == input.QuoteId
                        && x.DeliveryDate == input.DeliveryDate)
                .CountAsync();
        }

        private async Task EnsureOrderCanBeDeletedAsync(EntityDto input)
        {
            var record = await (await _orderRepository.GetQueryAsync())
                .Where(o => o.Id == input.Id)
                .Select(o => new
                {
                    IsClosed = o.IsClosed,
                    HasCompletedOrderLines = o.OrderLines.Any(ol => ol.IsComplete),
                    HasRelatedData =
                        o.BilledOrders.Any()
                        || o.OrderEmails.Any()
                        || o.OrderLines.Any(ol => ol.Tickets.Any())
                        || o.OrderLines.Any(ol => ol.ReceiptLines.Any())
                        || o.OrderLines.Any(ol => ol.Dispatches.Any())
                        || o.OrderLines.Any(ol => ol.OrderLineTrucks.Any())
                        || o.HasLinkedHaulingCompanyOrders
                        || o.OrderLines.Any(ol => ol.HaulingCompanyOrderLineId != null),
                })
                .SingleAsync();

            if (record.IsClosed)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_OrderClosed"));
            }

            if (record.HasCompletedOrderLines)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasCompletedOrderLines"));
            }

            if (record.HasRelatedData)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasRelatedData"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task DeleteOrder(EntityDto input)
        {
            await EnsureOrderCanBeDeletedAsync(input);

            var order = await (await _orderRepository.GetQueryAsync())
                .Include(x => x.OrderLines)
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            var orderLines = order.OrderLines.ToList();

            foreach (var orderLine in orderLines)
            {
                await _orderLineRepository.DeleteAsync(orderLine);
            }
            await _orderRepository.DeleteAsync(order);
            await DeleteTrackableOrderEmails(input.Id);
            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var orderLine in orderLines)
            {
                if (orderLine.MaterialCompanyOrderLineId.HasValue
                    && orderLine.MaterialCompanyTenantId.HasValue)
                {
                    using (CurrentUnitOfWork.SetTenantId(orderLine.MaterialCompanyTenantId))
                    {
                        var materialOrderLine = await _orderLineRepository.GetAsync(orderLine.MaterialCompanyOrderLineId.Value);
                        materialOrderLine.HaulingCompanyOrderLineId = null;
                        materialOrderLine.HaulingCompanyTenantId = null;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
            if (order.MaterialCompanyOrderId.HasValue
                && order.MaterialCompanyTenantId.HasValue)
            {
                var otherHaulingOrdersExist = false;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
                {
                    otherHaulingOrdersExist = await (await _orderRepository.GetQueryAsync())
                        .AnyAsync(x => x.MaterialCompanyOrderId == order.MaterialCompanyOrderId && x.Id != order.Id);
                }
                if (!otherHaulingOrdersExist)
                {
                    using (CurrentUnitOfWork.SetTenantId(order.MaterialCompanyTenantId))
                    {
                        var materialOrder = await _orderRepository.GetAsync(order.MaterialCompanyOrderId.Value);
                        materialOrder.HasLinkedHaulingCompanyOrders = false;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task DeleteTrackableOrderEmails(int orderId)
        {
            var orderEmails = await (await _orderEmailRepository.GetQueryAsync())
                .Include(x => x.Email)
                .ThenInclude(x => x.Events)
                .Include(x => x.Email)
                .ThenInclude(x => x.Receivers)
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            foreach (var orderEmail in orderEmails)
            {
                if (orderEmail.Email != null)
                {
                    foreach (var emailEvent in orderEmail.Email.Events)
                    {
                        await _trackableEmailEventRepository.DeleteAsync(emailEvent);
                    }

                    foreach (var emailReceiver in orderEmail.Email.Receivers)
                    {
                        await _trackableEmailReceiverRepository.DeleteAsync(emailReceiver);
                    }

                    await _trackableEmailRepository.DeleteAsync(orderEmail.Email);
                }

                await _orderEmailRepository.DeleteAsync(orderEmail);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PagedResultDto<OrderLineEditDto>> GetOrderLines(GetOrderLinesInput input)
        {
            if (input.OrderId.HasValue)
            {
                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
                var allowLoadBasedRates = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowLoadBasedRates);

                var orderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.OrderId == input.OrderId)
                    .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters,
                             x => x.LoadAtId == input.LoadAtId)
                    .WhereIf(input.ItemId.HasValue,
                             x => x.FreightItemId == input.ItemId || x.MaterialItemId == input.ItemId)
                    .WhereIf(input.MaterialUomId.HasValue,
                             x => x.MaterialUomId == input.MaterialUomId)
                    .WhereIf(input.FreightUomId.HasValue,
                             x => x.FreightUomId == input.FreightUomId)
                    .WhereIf(input.Designation.HasValue,
                             x => x.Designation == input.Designation)
                    .Select(x => new OrderLineEditDto
                    {
                        Id = x.Id,
                        LineNumber = x.LineNumber,
                        QuoteLineId = x.QuoteLineId,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        //Tickets = x.Tickets.Select(t => new TicketDto
                        //{
                        //    OfficeId = t.OfficeId,
                        //    MaterialQuantity = t.MaterialQuantity,
                        //    FreightQuantity = t.FreightQuantity
                        //}).ToList(),
                        //SharedOrderLines = x.SharedOrderLines.Select(s => new OrderLineShareDto { OfficeId = s.OfficeId }).ToList(),
                        MaterialPricePerUnit = x.MaterialPricePerUnit,
                        MaterialCostRate = x.MaterialCostRate,
                        FreightPricePerUnit = x.FreightPricePerUnit,
                        IsMaterialPricePerUnitOverridden = x.IsMaterialPricePerUnitOverridden,
                        IsFreightPricePerUnitOverridden = x.IsFreightPricePerUnitOverridden,
                        IsFreightRateToPayDriversOverridden = x.IsFreightRateToPayDriversOverridden,
                        IsLeaseHaulerPriceOverridden = x.IsLeaseHaulerPriceOverridden,
                        CanOverrideTotals = x.Tickets.Any(t => t.OfficeId != OfficeId),
                        QuoteId = x.Order.QuoteId,
                        HasQuoteBasedPricing = x.FreightItem.QuoteFreightItems.Any(y => y.QuoteId == x.Order.QuoteId
                            && (y.MaterialUomId == x.MaterialUomId || y.FreightUomId == x.FreightUomId) && y.LoadAtId == x.LoadAtId),
                        FreightItemId = x.FreightItemId,
                        FreightItemName = x.FreightItem.Name,
                        MaterialItemId = x.MaterialItemId,
                        MaterialItemName = x.MaterialItem.Name,
                        IsTaxable = x.FreightItem.IsTaxable,
                        IsMaterialTaxable = x.MaterialItem.IsTaxable,
                        IsFreightTaxable = x.FreightItem.IsTaxable,
                        LoadAtId = x.LoadAtId,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToId = x.DeliverToId,
                        DeliverToName = x.DeliverTo.DisplayName,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        FreightUomBaseId = (UnitOfMeasureBaseEnum?)x.FreightUom.UnitOfMeasureBaseId,
                        Designation = x.Designation,
                        MaterialPrice = x.MaterialPrice,
                        FreightPrice = x.FreightPrice,
                        IsMaterialPriceOverridden = x.IsMaterialPriceOverridden,
                        IsFreightPriceOverridden = x.IsFreightPriceOverridden,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                        DriverPayTimeClassificationId = x.DriverPayTimeClassificationId,
                        DriverPayTimeClassificationName = x.DriverPayTimeClassification.Name,
                        HourlyDriverPayRate = x.HourlyDriverPayRate,
                        TravelTime = x.TravelTime,
                        LoadBased = allowLoadBasedRates && x.LoadBased,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        IsMultipleLoads = x.IsMultipleLoads,
                        NumberOfTrucks = x.NumberOfTrucks,
                        OrderId = x.OrderId,
                        ProductionPay = allowProductionPay && x.ProductionPay,
                        RequireTicket = x.RequireTicket,
                        StaggeredTimeKind = x.StaggeredTimeKind,
                        StaggeredTimeInterval = x.StaggeredTimeInterval,
                        FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob,
                        TimeOnJob = x.TimeOnJob,
                        HasTickets = x.Tickets.Any(),
                        HasOpenDispatches = x.Dispatches.Any(d => !Dispatch.ClosedDispatchStatuses.Contains(d.Status)),
                        BedConstruction = x.BedConstruction,
                        VehicleCategories = x.OrderLineVehicleCategories.Select(vc => new OrderLineVehicleCategoryDto
                        {
                            Id = vc.VehicleCategoryId,
                            Name = vc.VehicleCategory.Name,
                        }).ToList(),
                    })
                    .OrderBy(input.Sorting)
                    //.PageBy(input)
                    .ToListAsync();

                var totalCount = orderLines.Count;

                var timezone = await GetTimezone();

                foreach (var orderLine in orderLines)
                {
                    orderLine.TimeOnJob = orderLine.TimeOnJob?.ConvertTimeZoneTo(timezone);
                    orderLine.FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone);
                    //previously, it was also setting the date portion of the above two fields to match Order.DeliveryDate. We might need to implement that again if any historical data is incorrect.
                }

                return new PagedResultDto<OrderLineEditDto>(
                    totalCount,
                    orderLines);
            }
            else if (input.QuoteId.HasValue)
            {
                var orderLines = await (await _quoteLineRepository.GetQueryAsync())
                    .Where(x => x.QuoteId == input.QuoteId)
                    .WhereIf(input.LoadAtId.HasValue, x => x.LoadAtId == input.LoadAtId)
                    .WhereIf(input.ItemId.HasValue, x => x.FreightItemId == input.ItemId || x.MaterialItemId == input.ItemId)
                    .WhereIf(input.DeliverToId.HasValue, x => x.DeliverToId == input.DeliverToId)
                    .Select(x => new OrderLineEditDto
                    {
                        Id = x.Id,
                        QuoteLineId = x.Id,
                        LoadAtId = x.LoadAtId,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToId = x.DeliverToId,
                        DeliverToName = x.DeliverTo.DisplayName,
                        FreightItemId = x.FreightItemId,
                        FreightItemName = x.FreightItem.Name,
                        MaterialItemId = x.MaterialItemId,
                        MaterialItemName = x.MaterialItem.Name,
                        IsTaxable = x.FreightItem.IsTaxable,
                        IsMaterialTaxable = x.MaterialItem.IsTaxable,
                        IsFreightTaxable = x.FreightItem.IsTaxable,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        FreightUomBaseId = (UnitOfMeasureBaseEnum?)x.FreightUom.UnitOfMeasureBaseId,
                        Designation = x.Designation,
                        MaterialPricePerUnit = x.PricePerUnit,
                        MaterialCostRate = x.MaterialCostRate,
                        FreightPricePerUnit = x.FreightRate,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                        DriverPayTimeClassificationId = x.DriverPayTimeClassificationId,
                        DriverPayTimeClassificationName = x.DriverPayTimeClassification.Name,
                        HourlyDriverPayRate = x.HourlyDriverPayRate,
                        TravelTime = x.TravelTime,
                        ProductionPay = x.ProductionPay,
                        RequireTicket = x.RequireTicket,
                        LoadBased = x.LoadBased,
                        //Quantity = x.Quantity, //Do not default quantities. They will have to fill that in.
                        //MaterialQuantity = x.MaterialQuantity,
                        //FreightQuantity = x.FreightQuantity,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        CanOverrideTotals = true,
                        QuoteId = input.QuoteId.Value,
                        HasQuoteBasedPricing = x.FreightItem.QuoteFreightItems.Any(y => y.QuoteId == input.QuoteId
                            && (y.MaterialUomId == x.MaterialUomId || y.FreightUomId == x.FreightUomId) && y.LoadAtId == x.LoadAtId),
                        BedConstruction = x.BedConstruction,
                        VehicleCategories = x.QuoteLineVehicleCategories.Select(vc => new OrderLineVehicleCategoryDto
                        {
                            Id = vc.VehicleCategoryId,
                            Name = vc.VehicleCategory.Name,
                        }).ToList(),
                    })
                    .OrderBy(input.Sorting)
                    .ToListAsync();

                var preventProductionPayOnHourlyJobs = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.PreventProductionPayOnHourlyJobs);

                var i = 1;
                foreach (var orderLine in orderLines)
                {
                    orderLine.Id = null;
                    orderLine.LineNumber = i++;
                    orderLine.ProductionPay = orderLine.ProductionPay && (!preventProductionPayOnHourlyJobs || orderLine.FreightUomName?.ToLower().TrimEnd('s') != "hour");
                }

                return new PagedResultDto<OrderLineEditDto>(orderLines.Count, orderLines);
            }
            else
            {
                throw new ArgumentNullException(nameof(input.OrderId));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View, AppPermissions.LeaseHaulerPortal_Jobs_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<OrderLineEditDto> GetOrderLineForEdit(GetOrderLineForEditInput input)
        {
            var permissions = new
            {
                ViewOrderForEdit = await IsGrantedAsync(AppPermissions.Pages_Orders_View),
                EditLeaseHaulerJob = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Jobs_Edit),
                LeaseHaulerTruckRequest = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Truck_Request),
            };

            var result = await GetOrderLineForEditInternal(input);
            if (permissions.ViewOrderForEdit)
            {
                // do nothing, show all
            }
            else if (permissions.EditLeaseHaulerJob || permissions.LeaseHaulerTruckRequest)
            {
                // remove secret values
                result.FreightPricePerUnit = null;
                result.FreightPrice = 0;
                result.MaterialPricePerUnit = null;
                result.MaterialCostRate = null;
                result.MaterialPrice = 0;
                result.FreightRateToPayDrivers = null;
                result.HourlyDriverPayRate = null;
                result.Tax = 0;
                result.TotalAmount = 0;
                result.Subtotal = 0;
            }
            else
            {
                throw new AbpAuthorizationException();
            }


            return result;
        }

        private async Task<OrderLineEditDto> GetOrderLineForEditInternal(GetOrderLineForEditInput input)
        {
            var permissions = new
            {
                ViewOrderForEdit = await IsGrantedAsync(AppPermissions.Pages_Orders_View),
                EditLeaseHaulerJob = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Jobs_Edit),
                LeaseHaulerTruckRequest = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Truck_Request),
            };

            int? leaseHaulerIdFilter = null;
            if (permissions.ViewOrderForEdit)
            {
                // do nothing, show all
            }
            else if (permissions.EditLeaseHaulerJob || permissions.LeaseHaulerTruckRequest)
            {
                leaseHaulerIdFilter = Session.GetLeaseHaulerIdOrThrow(this);
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();

            OrderLineEditDto orderLineEditDto;

            if (input.Id.HasValue)
            {
                var canOverrideTotals = permissions.ViewOrderForEdit && await _orderLineRepository.CanOverrideTotals(input.Id.Value, OfficeId);
                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
                var allowLoadBasedRates = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowLoadBasedRates);

                orderLineEditDto = await (await _orderLineRepository.GetQueryAsync())
                    .Select(x => new OrderLineEditDto
                    {
                        Id = x.Id,
                        OrderId = x.OrderId,
                        QuoteId = x.Order.QuoteId,
                        QuoteLineId = x.QuoteLineId,
                        LineNumber = x.LineNumber,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        MaterialPricePerUnit = x.MaterialPricePerUnit,
                        MaterialCostRate = x.MaterialCostRate,
                        FreightPricePerUnit = x.FreightPricePerUnit,
                        IsMaterialTaxable = x.MaterialItem.IsTaxable,
                        IsFreightTaxable = x.FreightItem.IsTaxable,
                        IsTaxable = x.FreightItem.IsTaxable,
                        OrderSalesTaxRate = x.Order.SalesTaxRate,
                        IsMaterialPricePerUnitOverridden = x.IsMaterialPricePerUnitOverridden,
                        IsFreightPricePerUnitOverridden = x.IsFreightPricePerUnitOverridden,
                        IsFreightRateToPayDriversOverridden = x.IsFreightRateToPayDriversOverridden,
                        IsLeaseHaulerPriceOverridden = x.IsLeaseHaulerPriceOverridden,
                        FreightItemId = x.FreightItemId,
                        FreightItemName = x.FreightItem.Name,
                        UseZoneBasedRates = x.FreightItem.UseZoneBasedRates,
                        MaterialItemId = x.MaterialItemId,
                        MaterialItemName = x.MaterialItem.Name,
                        LoadAtId = x.LoadAtId,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToId = x.DeliverToId,
                        DeliverToName = x.DeliverTo.DisplayName,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        FreightUomBaseId = (UnitOfMeasureBaseEnum?)x.FreightUom.UnitOfMeasureBaseId,
                        Designation = x.Designation,
                        MaterialPrice = x.MaterialPrice,
                        FreightPrice = x.FreightPrice,
                        IsMaterialPriceOverridden = x.IsMaterialPriceOverridden,
                        IsFreightPriceOverridden = x.IsFreightPriceOverridden,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                        DriverPayTimeClassificationId = x.DriverPayTimeClassificationId,
                        DriverPayTimeClassificationName = x.DriverPayTimeClassification.Name,
                        HourlyDriverPayRate = x.HourlyDriverPayRate,
                        TravelTime = x.TravelTime,
                        LoadBased = allowLoadBasedRates && x.LoadBased,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        NumberOfTrucks = x.NumberOfTrucks,
                        TimeOnJob = x.TimeOnJob,
                        StaggeredTimeKind = x.StaggeredTimeKind,
                        IsMultipleLoads = x.IsMultipleLoads,
                        ProductionPay = allowProductionPay && x.ProductionPay,
                        RequireTicket = x.RequireTicket,
                        CanOverrideTotals = canOverrideTotals,
                        RequiresCustomerNotification = x.RequiresCustomerNotification,
                        CustomerNotificationContactName = x.CustomerNotificationContactName,
                        CustomerNotificationPhoneNumber = x.CustomerNotificationPhoneNumber,
                        BedConstruction = x.BedConstruction,
                        PricingTierId = x.Order.Customer.PricingTierId,
                        CustomerIsCod = x.Order.Customer.IsCod,
                        VehicleCategories = x.OrderLineVehicleCategories.Select(vc => new OrderLineVehicleCategoryDto
                        {
                            Id = vc.VehicleCategory.Id,
                            Name = vc.VehicleCategory.Name,
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);

                if (orderLineEditDto == null)
                {
                    throw await GetOrderLineNotFoundException(new EntityDto(input.Id.Value));
                }

                // check if this order has been assigned to this lease hauler
                if (leaseHaulerIdFilter.HasValue)
                {
                    await CheckLeaseHaulerEditOrderLinePermission(input.Id.Value);
                }
            }
            else if (input.OrderId.HasValue)
            {
                var order = await (await _orderRepository.GetQueryAsync())
                    .Select(x => new
                    {
                        x.Id,
                        x.QuoteId,
                        x.Customer.PricingTierId,
                        CustomerIsCod = x.Customer.IsCod,
                        OrderLinesCount = x.OrderLines.Count,
                        x.SalesTaxRate,
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.OrderId);

                if (order == null)
                {
                    throw await GetOrderNotFoundException(new EntityDto(input.OrderId.Value));
                }

                orderLineEditDto = new OrderLineEditDto
                {
                    OrderId = order.Id,
                    QuoteId = order.QuoteId,
                    PricingTierId = order.PricingTierId,
                    CustomerIsCod = order.CustomerIsCod,
                    LineNumber = order.OrderLinesCount + 1,
                    CanOverrideTotals = true,
                    OrderSalesTaxRate = order.SalesTaxRate,
                    ProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay),
                    RequireTicket = requiredTicketEntry.GetRequireTicketDefaultValue(),
                };
            }
            else
            {
                return new OrderLineEditDto
                {
                    CanOverrideTotals = true,
                    RequireTicket = requiredTicketEntry.GetRequireTicketDefaultValue(),
                };
            }

            var timezone = await GetTimezone();
            orderLineEditDto.TimeOnJob = orderLineEditDto.TimeOnJob?.ConvertTimeZoneTo(timezone);
            orderLineEditDto.FirstStaggeredTimeOnJob = orderLineEditDto.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(timezone);

            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, orderLineEditDto, orderLineEditDto.OrderSalesTaxRate, separateItems);

            return orderLineEditDto;
        }

        private async Task<Exception> GetOrderNotFoundException(EntityDto input)
        {
            if (await IsOrderDeleted(input))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can't be edited");
            }

            return new Exception($"Order with id {input.Id} wasn't found and is not deleted");
        }

        private async Task<Exception> GetOrderLineNotFoundException(EntityDto input)
        {
            var deletedOrderLine = await _orderLineRepository.GetDeletedEntity(input, CurrentUnitOfWork);
            if (deletedOrderLine == null)
            {
                return new Exception($"OrderLine with id {input.Id} wasn't found and is not deleted");
            }

            if (await IsOrderDeleted(new EntityDto(deletedOrderLine.OrderId)))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can't be edited");
            }

            return new EntityDeletedException("OrderLine", "This order line has been deleted and can't be edited");
        }

        private async Task<bool> IsOrderDeleted(EntityDto input)
        {
            return await _orderRepository.IsEntityDeleted(input, CurrentUnitOfWork);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View, AppPermissions.LeaseHaulerPortal_Jobs_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<JobEditDto> GetJobForEdit(GetJobForEditInput input)
        {
            var orderLine = await GetOrderLineForEdit(new GetOrderLineForEditInput
            {
                Id = input.OrderLineId,
            });

            var order = await GetOrderForEdit(new NullableIdDto
            {
                Id = orderLine.OrderId == 0 ? null : orderLine.OrderId,
            });

            if (input.OrderLineId == null)
            {
                if (input.DeliveryDate.HasValue)
                {
                    order.DeliveryDate = input.DeliveryDate;
                }
                if (input.Shift.HasValue)
                {
                    order.Shift = input.Shift;
                }
                if (input.OfficeId.HasValue)
                {
                    order.OfficeId = input.OfficeId.Value;
                    order.OfficeName = input.OfficeName;
                }
            }

            var result = new JobEditDto
            {
                OrderId = order.Id,
                OrderLineId = orderLine.Id,
                DeliveryDate = order.DeliveryDate,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerIsCod = order.CustomerIsCod,
                ContactId = order.ContactId,
                ContactName = order.ContactName,
                ContactPhone = order.ContactPhone,
                ChargeTo = order.ChargeTo,
                PONumber = order.PONumber,
                SpectrumNumber = order.SpectrumNumber,
                Directions = order.Directions,
                Priority = order.Priority,
                QuoteId = order.QuoteId,
                QuoteName = order.QuoteName,
                Shift = order.Shift,
                OfficeId = order.OfficeId,
                OfficeName = order.OfficeName,
                SalesTaxRate = order.SalesTaxRate,
                SalesTax = order.SalesTax,
                SalesTaxEntityId = order.SalesTaxEntityId,
                SalesTaxEntityName = order.SalesTaxEntityName,
                FocusFieldId = input.FocusFieldId,
                BaseFuelCost = order.BaseFuelCost,
                FuelSurchargeCalculationId = order.FuelSurchargeCalculationId,
                FuelSurchargeCalculationName = order.FuelSurchargeCalculationName,
                CanChangeBaseFuelCost = order.CanChangeBaseFuelCost,
                DefaultFuelSurchargeCalculationName = order.DefaultFuelSurchargeCalculationName,
                DefaultBaseFuelCost = order.DefaultBaseFuelCost,
                DefaultCanChangeBaseFuelCost = order.DefaultCanChangeBaseFuelCost,
                PricingTierId = order.PricingTierId,
                CustomerIsTaxExempt = order.CustomerIsTaxExempt,
                QuoteIsTaxExempt = order.QuoteIsTaxExempt,
                IsTaxExempt = order.IsTaxExempt,
                TaxAmount = order.SalesTax,
                Total = order.CODTotal,

                DeliverToId = orderLine.DeliverToId,
                DeliverToName = orderLine.DeliverToName,
                OrderLineTotal = orderLine.TotalAmount,
                OrderLineSalesTax = orderLine.Tax,
                LoadAtId = orderLine.LoadAtId,
                LoadAtName = orderLine.LoadAtName,
                CanOverrideTotals = orderLine.CanOverrideTotals,
                IsTaxable = orderLine.IsTaxable,
                IsMaterialTaxable = orderLine.IsMaterialTaxable,
                IsFreightTaxable = orderLine.IsFreightTaxable,
                JobNumber = orderLine.JobNumber,
                IsFreightPriceOverridden = orderLine.IsFreightPriceOverridden,
                Designation = orderLine.Designation,
                MaterialPrice = orderLine.MaterialPrice,
                MaterialUomId = orderLine.MaterialUomId,
                MaterialUomName = orderLine.MaterialUomName,
                MaterialQuantity = orderLine.MaterialQuantity,
                MaterialPricePerUnit = orderLine.MaterialPricePerUnit,
                MaterialCostRate = orderLine.MaterialCostRate,
                MaterialItemId = orderLine.MaterialItemId,
                MaterialItemName = orderLine.MaterialItemName,
                IsMaterialPriceOverridden = orderLine.IsMaterialPriceOverridden,
                IsMaterialPricePerUnitOverridden = orderLine.IsMaterialPricePerUnitOverridden,
                FreightPrice = orderLine.FreightPrice,
                FreightUomId = orderLine.FreightUomId,
                FreightUomName = orderLine.FreightUomName,
                FreightUomBaseId = orderLine.FreightUomBaseId,
                FreightQuantity = orderLine.FreightQuantity,
                FreightPricePerUnit = orderLine.FreightPricePerUnit,
                IsFreightPricePerUnitOverridden = orderLine.IsFreightPricePerUnitOverridden,
                IsFreightRateToPayDriversOverridden = orderLine.IsFreightRateToPayDriversOverridden,
                IsLeaseHaulerPriceOverridden = orderLine.IsLeaseHaulerPriceOverridden,
                FirstStaggeredTimeOnJob = orderLine.FirstStaggeredTimeOnJob,
                HasOpenDispatches = orderLine.HasOpenDispatches,
                HasQuoteBasedPricing = orderLine.HasQuoteBasedPricing,
                HasTickets = orderLine.HasTickets,
                IsMultipleLoads = orderLine.IsMultipleLoads,
                LeaseHaulerRate = orderLine.LeaseHaulerRate,
                FreightRateToPayDrivers = orderLine.FreightRateToPayDrivers,
                DriverPayTimeClassificationId = orderLine.DriverPayTimeClassificationId,
                DriverPayTimeClassificationName = orderLine.DriverPayTimeClassificationName,
                HourlyDriverPayRate = orderLine.HourlyDriverPayRate,
                TravelTime = orderLine.TravelTime,
                LoadBased = orderLine.LoadBased,
                Note = orderLine.Note,
                NumberOfTrucks = orderLine.NumberOfTrucks,
                ProductionPay = orderLine.ProductionPay,
                RequireTicket = orderLine.RequireTicket,
                FreightItemId = orderLine.FreightItemId,
                FreightItemName = orderLine.FreightItemName,
                UseZoneBasedRates = orderLine.UseZoneBasedRates,
                StaggeredTimeInterval = orderLine.StaggeredTimeInterval,
                StaggeredTimeKind = orderLine.StaggeredTimeKind,
                TimeOnJob = orderLine.TimeOnJob,
                UpdateStaggeredTime = orderLine.UpdateStaggeredTime,
                QuoteLineId = orderLine.QuoteLineId,
                RequiresCustomerNotification = orderLine.RequiresCustomerNotification,
                CustomerNotificationContactName = orderLine.CustomerNotificationContactName,
                CustomerNotificationPhoneNumber = orderLine.CustomerNotificationPhoneNumber,
                BedConstruction = orderLine.BedConstruction,
                VehicleCategories = orderLine.VehicleCategories,
            };

            if (input.OrderLineId != null)
            {
                var ticket = await (await _ticketRepository.GetQueryAsync())
                    .Select(t => new
                    {
                        t.Id,
                        t.OrderLineId,
                        t.TicketNumber,
                    })
                    .OrderBy(t => t.Id)
                    .FirstOrDefaultAsync(t => t.OrderLineId == input.OrderLineId);

                if (ticket != null)
                {
                    result.TicketId = ticket.Id;
                    result.TicketNumber = ticket.TicketNumber;
                }
                else
                {
                    result.AutoGenerateTicketNumber = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber);
                }
            }
            else
            {
                result.AutoGenerateTicketNumber = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber);
                if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSalesForTenant)
                    && await IsGrantedAsync(AppPermissions.Pages_CounterSales))
                {
                    if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.DefaultDesignationToMaterialOnly))
                    {
                        result.Designation = DesignationEnum.MaterialOnly;
                    }

                    var defaultLoadAtLocationId = await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DefaultLoadAtLocationId);
                    if (defaultLoadAtLocationId > 0)
                    {
                        var location = await (await _locationRepository.GetQueryAsync())
                            .Where(x => x.Id == defaultLoadAtLocationId)
                            .Select(x => new
                            {
                                x.DisplayName,
                            })
                            .FirstOrDefaultAsync();

                        if (location != null)
                        {
                            result.DefaultLoadAtLocationId = defaultLoadAtLocationId;
                            result.DefaultLoadAtLocationName = location.DisplayName;
                        }
                    }

                    var defaultMaterialItemId = await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DefaultMaterialItemId);
                    if (defaultMaterialItemId > 0)
                    {
                        var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                        var item = items.Find(defaultMaterialItemId);
                        if (item != null)
                        {
                            result.DefaultMaterialItemId = defaultMaterialItemId;
                            result.DefaultMaterialItemName = item.Name;
                        }
                    }

                    var defaultMaterialUomId = await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DefaultMaterialUomId);
                    if (defaultMaterialUomId > 0)
                    {
                        var uoms = await _listCaches.UnitOfMeasure.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
                        var uom = uoms.Find(defaultMaterialUomId);
                        if (uom != null)
                        {
                            result.DefaultMaterialUomId = defaultMaterialUomId;
                            result.DefaultMaterialUomName = uom.Name;
                        }
                    }
                }
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<ResetOverriddenOrderLineValuesOutput> ResetOverriddenOrderLineValues(ResetOverriddenOrderLineValuesInput input)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.Id);
            var orderLine = await orderLineUpdater.GetEntityAsync();
            if (input.OverrideReadOnlyState && await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_TicketsByDriver_EditTicketsOnInvoicesOrPayStatements))
            {
                orderLineUpdater.SuppressReadOnlyChecker();
            }
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMaterialPriceOverridden, false);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightPriceOverridden, false);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialPrice, orderLine.MaterialPricePerUnit * orderLine.MaterialQuantity ?? 0);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightPrice, orderLine.FreightPricePerUnit * orderLine.FreightQuantity ?? 0);

            await orderLineUpdater.SaveChangesAsync();
            await CurrentUnitOfWork.SaveChangesAsync();

            var orderTaxDetails = await _orderTaxCalculator.CalculateTotalsAsync(orderLine.OrderId);

            return new ResetOverriddenOrderLineValuesOutput
            {
                MaterialTotal = orderLine.MaterialPrice,
                FreightTotal = orderLine.FreightPrice,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<EditOrderLineOutput> EditOrderLine(OrderLineEditDto model)
        {
            var orderLine = await EditOrderLineInternal(model);

            await CurrentUnitOfWork.SaveChangesAsync();
            model.Id = orderLine.Id;

            var orderTaxDetails = await _orderTaxCalculator.CalculateTotalsAsync(model.OrderId);

            return new EditOrderLineOutput
            {
                OrderLineId = orderLine.Id,
                OrderTaxDetails = new OrderTaxDetailsDto(orderTaxDetails),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task EditOrderLines(List<OrderLineEditDto> modelList)
        {
            foreach (var model in modelList)
            {
                await EditOrderLineInternal(model);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            if (modelList.Any())
            {
                await _orderTaxCalculator.CalculateTotalsAsync(modelList.First().OrderId);
            }
        }

        private async Task<OrderLine> EditOrderLineInternal(OrderLineEditDto model)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(model.Id ?? 0);
            var orderLine = await orderLineUpdater.GetEntityAsync();

            if (orderLine.Id == 0)
            {
                await orderLineUpdater.UpdateFieldAsync(o => o.OrderId, model.OrderId);
            }
            var order = await orderLineUpdater.GetOrderAsync();
            var date = order.DeliveryDate;
            var timezone = await GetTimezone();

            if (await IsGrantedAsync(AppPermissions.Pages_Orders_Edit))
            {
                if (model.UpdateOrderLineTrucksTimeOnJob.HasValue)
                {
                    orderLineUpdater.UpdateOrderLineTrucksTimeOnJobIfNeeded(model.UpdateOrderLineTrucksTimeOnJob.Value);
                }
                if (model.UpdateDispatchesTimeOnJob.HasValue)
                {
                    orderLineUpdater.UpdateDispatchesTimeOnJobIfNeeded(model.UpdateDispatchesTimeOnJob.Value);
                }
            }

            if (model.UpdateStaggeredTime)
            {
                await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeKind, model.StaggeredTimeKind);
                await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeInterval, model.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? model.StaggeredTimeInterval : null);

                var firstStaggeredTimeOnJobUtc = model.StaggeredTimeKind == StaggeredTimeKind.SetInterval
                    ? date.AddTimeOrNull(model.FirstStaggeredTimeOnJob)?.ConvertTimeZoneFrom(timezone)
                    : (DateTime?)null;
                await orderLineUpdater.UpdateFieldAsync(o => o.FirstStaggeredTimeOnJob, firstStaggeredTimeOnJobUtc);

                if (model.StaggeredTimeKind != StaggeredTimeKind.None)
                {
                    model.TimeOnJob = null;
                    await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, null);
                }
            }

            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialQuantity, model.MaterialQuantity);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightQuantity, model.FreightQuantity);
            await orderLineUpdater.UpdateFieldAsync(o => o.TravelTime, model.TravelTime);
            await orderLineUpdater.UpdateFieldAsync(o => o.LineNumber, model.LineNumber);
            await orderLineUpdater.UpdateFieldAsync(o => o.QuoteLineId, model.QuoteLineId);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialPricePerUnit, model.MaterialPricePerUnit);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialCostRate, model.MaterialCostRate);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightPricePerUnit, model.FreightPricePerUnit);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMaterialPricePerUnitOverridden, model.IsMaterialPricePerUnitOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightPricePerUnitOverridden, model.IsFreightPricePerUnitOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightRateToPayDriversOverridden, model.IsFreightRateToPayDriversOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsLeaseHaulerPriceOverridden, model.IsLeaseHaulerPriceOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightItemId, model.FreightItemId);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialItemId, model.MaterialItemId);
            await orderLineUpdater.UpdateFieldAsync(o => o.LoadAtId, model.LoadAtId);
            await orderLineUpdater.UpdateFieldAsync(o => o.DeliverToId, model.DeliverToId);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialUomId, model.MaterialUomId);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightUomId, model.FreightUomId);
            await orderLineUpdater.UpdateFieldAsync(o => o.Designation, model.Designation);
            await orderLineUpdater.UpdateFieldAsync(o => o.MaterialPrice, model.MaterialPrice);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightPrice, model.FreightPrice);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMaterialPriceOverridden, model.IsMaterialPriceOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.IsFreightPriceOverridden, model.IsFreightPriceOverridden);
            await orderLineUpdater.UpdateFieldAsync(o => o.LeaseHaulerRate, model.LeaseHaulerRate);
            await orderLineUpdater.UpdateFieldAsync(o => o.FreightRateToPayDrivers, model.FreightRateToPayDrivers);
            await orderLineUpdater.UpdateFieldAsync(o => o.DriverPayTimeClassificationId, model.DriverPayTimeClassificationId);
            await orderLineUpdater.UpdateFieldAsync(o => o.HourlyDriverPayRate, model.HourlyDriverPayRate);
            await orderLineUpdater.UpdateFieldAsync(o => o.LoadBased, model.LoadBased);
            await orderLineUpdater.UpdateFieldAsync(o => o.JobNumber, model.JobNumber);
            await orderLineUpdater.UpdateFieldAsync(o => o.Note, model.Note);

            await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, date.AddTimeOrNull(model.TimeOnJob)?.ConvertTimeZoneFrom(timezone));

            await orderLineUpdater.UpdateFieldAsync(o => o.NumberOfTrucks, model.NumberOfTrucks.Round(2));
            await orderLineUpdater.UpdateFieldAsync(o => o.IsMultipleLoads, model.IsMultipleLoads);
            await orderLineUpdater.UpdateFieldAsync(o => o.ProductionPay, model.ProductionPay);
            await orderLineUpdater.UpdateFieldAsync(o => o.RequireTicket, model.RequireTicket);

            await orderLineUpdater.UpdateFieldAsync(o => o.RequiresCustomerNotification, model.RequiresCustomerNotification);
            await orderLineUpdater.UpdateFieldAsync(o => o.CustomerNotificationContactName, model.CustomerNotificationContactName);
            await orderLineUpdater.UpdateFieldAsync(o => o.CustomerNotificationPhoneNumber, model.CustomerNotificationPhoneNumber);

            await orderLineUpdater.UpdateFieldAsync(o => o.BedConstruction, model.BedConstruction);

            await orderLineUpdater.SaveChangesAsync();

            var existingVehicleCategories = model.Id.HasValue ? await (await _orderLineVehicleCategoryRepository.GetQueryAsync())
                .Where(vc => vc.OrderLineId == model.Id)
                .ToListAsync() : new List<OrderLineVehicleCategory>();

            await _orderLineVehicleCategoryRepository.DeleteRangeAsync(
                existingVehicleCategories
                    .Where(e => !model.VehicleCategories.Any(m => m.Id == e.VehicleCategoryId))
                    .ToList()
            );

            await _orderLineVehicleCategoryRepository.InsertRangeAsync(
                model.VehicleCategories
                    .Where(m => !existingVehicleCategories.Any(e => e.VehicleCategoryId == m.Id))
                    .Select(x => new OrderLineVehicleCategory
                    {
                        OrderLine = orderLine,
                        VehicleCategoryId = x.Id,
                    })
                    .ToList()
            );

            return orderLine;
        }

        private async Task EnsureOrderLineCanBeDeletedAsync(EntityDto input)
        {
            var record = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == input.Id)
                .Select(ol => new
                {
                    IsComplete = ol.IsComplete,
                    HasRelatedData = ol.Order.IsClosed
                        || ol.OrderLineTrucks.Any()
                        || ol.Tickets.Any()
                        || ol.ReceiptLines.Any()
                        || ol.Dispatches.Any()
                        || ol.PayStatementTimes.Any()
                        || ol.EmployeeTimes.Any()
                        || ol.HaulingCompanyOrderLineId != null,
                })
                .SingleAsync();

            if (record.HasRelatedData)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasRelatedData"));
            }

            if (record.IsComplete)
            {
                throw new UserFriendlyException(L("Order_Delete_Error_HasCompletedOrderLines"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<DeleteOrderLineOutput> DeleteOrderLine(DeleteOrderLineInput input)
        {
            await EnsureOrderLineCanBeDeletedAsync(input);

            var orderLine = await _orderLineRepository.GetAsync(input.Id);

            await _orderLineRepository.DeleteAsync(orderLine);
            await CurrentUnitOfWork.SaveChangesAsync();

            var order = await _orderTaxCalculator.CalculateTotalsAsync(input.OrderId);

            var remainingOrderLines = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id != input.Id && ol.OrderId == input.OrderId)
                .ToListAsync();

            int i = 1;
            remainingOrderLines.ForEach(x => x.LineNumber = i++);

            if (orderLine.MaterialCompanyOrderLineId.HasValue
                && orderLine.MaterialCompanyTenantId.HasValue)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                using (CurrentUnitOfWork.SetTenantId(orderLine.MaterialCompanyTenantId))
                {
                    var materialOrderLine = await _orderLineRepository.GetAsync(orderLine.MaterialCompanyOrderLineId.Value);
                    materialOrderLine.HaulingCompanyOrderLineId = null;
                    materialOrderLine.HaulingCompanyTenantId = null;
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            return new DeleteOrderLineOutput
            {
                OrderTaxDetails = new OrderTaxDetailsDto(order),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<IOrderTaxDetails> CalculateOrderTotals(OrderTaxDetailsDto orderTaxDetails)
        {
            List<OrderLineTaxDetailsDto> orderLines;

            if (orderTaxDetails.Id != 0)
            {
                orderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.OrderId == orderTaxDetails.Id)
                    .Select(x => new OrderLineTaxDetailsDto
                    {
                        FreightPrice = x.FreightPrice,
                        MaterialPrice = x.MaterialPrice,
                        IsTaxable = x.FreightItem.IsTaxable,
                        IsFreightTaxable = x.FreightItem.IsTaxable,
                        IsMaterialTaxable = x.MaterialItem.IsTaxable,
                    })
                    .ToListAsync();
            }
            else if (orderTaxDetails.OrderLines != null)
            {
                orderLines = orderTaxDetails.OrderLines;
            }
            else
            {
                orderLines = new List<OrderLineTaxDetailsDto>();
            }

            await _orderTaxCalculator.CalculateTotalsAsync(orderTaxDetails, orderLines);

            return orderTaxDetails;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View, AppPermissions.LeaseHaulerPortal_Jobs_Edit)]
        public async Task<StaggeredTimesDto> GetStaggeredTimesForEdit(NullableIdDto input)
        {
            if (!input.Id.HasValue)
            {
                return new StaggeredTimesDto();
            }

            await CheckOrderLineEditPermissions(AppPermissions.Pages_Orders_View, AppPermissions.LeaseHaulerPortal_Jobs_Edit,
                _orderLineRepository, input.Id.Value);

            var model = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new StaggeredTimesDto
                {
                    OrderLineId = x.Id,
                    StaggeredTimeKind = x.StaggeredTimeKind,
                    StaggeredTimeInterval = x.StaggeredTimeInterval,
                    FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob == null && x.StaggeredTimeKind == StaggeredTimeKind.None ? x.TimeOnJob : x.FirstStaggeredTimeOnJob,
                }).FirstAsync();

            model.FirstStaggeredTimeOnJob = model.FirstStaggeredTimeOnJob?.ConvertTimeZoneTo(await GetTimezone());

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit)]
        public async Task<StaggeredTimesDto> SetStaggeredTimes(StaggeredTimesDto model)
        {
            if (model.OrderLineId == null)
            {
                return model;
            }

            await CheckOrderLineEditPermissions(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit,
                _orderLineRepository, model.OrderLineId.Value);

            var orderLineUpdater = _orderLineUpdaterFactory.Create(model.OrderLineId.Value);
            var orderLine = await orderLineUpdater.GetEntityAsync();
            var order = await orderLineUpdater.GetOrderAsync();
            var date = order.DeliveryDate;
            var timezone = await GetTimezone();

            await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeKind, model.StaggeredTimeKind);
            await orderLineUpdater.UpdateFieldAsync(o => o.StaggeredTimeInterval, model.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? model.StaggeredTimeInterval : null);

            var firstStaggeredTimeOnJobUtc = model.StaggeredTimeKind == StaggeredTimeKind.SetInterval
                    ? date.AddTimeOrNull(model.FirstStaggeredTimeOnJob)?.ConvertTimeZoneFrom(timezone)
                    : (DateTime?)null;
            await orderLineUpdater.UpdateFieldAsync(o => o.FirstStaggeredTimeOnJob, firstStaggeredTimeOnJobUtc);

            if (model.StaggeredTimeKind != StaggeredTimeKind.None)
            {
                await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, null);
            }

            await orderLineUpdater.SaveChangesAsync();
            await CurrentUnitOfWork.SaveChangesAsync();

            return model;
        }

        private async Task<IQueryable<Receipt>> GetReceiptsQuery(GetReceiptReportInput input)
        {
            var officeIds = await GetOfficeIds();

            return (await _receiptRepository.GetQueryAsync())
                .WhereIf(input.StartDate.HasValue,
                    x => x.DeliveryDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue,
                    x => x.DeliveryDate <= input.EndDate)
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId)
                .WhereIf(!input.OfficeId.HasValue,
                    x => officeIds.Contains(x.OfficeId))
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId);
            //.Where(x => x.ReceiptLines.Any(l => l.Tickets.Any(a => a.OfficeId == OfficeId)) || x.Receipts.Any(r => r.OfficeId == OfficeId)); //&& a.ActualQuantity != null
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Receipts)]
        public async Task<PagedResultDto<ReceiptReportDto>> GetReceipts(GetReceiptReportInput input)
        {
            var query = await GetReceiptsQuery(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new ReceiptReportDto
                {
                    ReceiptId = x.Id,
                    OrderId = x.OrderId,
                    DeliveryDate = x.DeliveryDate,
                    CustomerName = x.Customer.Name,
                    SalesTaxRate = x.SalesTaxRate,
                    CODTotal = x.Total,
                    FreightTotal = x.FreightTotal,
                    MaterialTotal = x.MaterialTotal,
                    SalesTax = x.SalesTax,
                    Items = x.ReceiptLines.Select(l => new ReceiptReportItemDto
                    {
                        IsTaxable = l.FreightItem.IsTaxable,
                        IsFreightTaxable = l.MaterialItem.IsTaxable,
                        IsMaterialTaxable = l.FreightItem.IsTaxable,
                        ActualMaterialQuantity = l.MaterialQuantity,
                        ActualFreightQuantity = l.FreightQuantity,
                        FreightPricePerUnit = l.FreightRate,
                        MaterialPricePerUnit = l.MaterialRate,
                        ReceiptLineMaterialPrice = l.MaterialAmount,
                        ReceiptLineFreightPrice = l.FreightAmount,
                        OrderLineMaterialPrice = l.OrderLine == null ? 0 : l.OrderLine.MaterialPrice,
                        OrderLineFreightPrice = l.OrderLine == null ? 0 : l.OrderLine.FreightPrice,
                        IsOrderLineMaterialPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsMaterialPriceOverridden,
                        IsOrderLineFreightPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsFreightPriceOverridden,
                    }).ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            //await CalculateTotalsAsync<ReceiptsDto<ReceiptsItemDto>, ReceiptsItemDto>(items);

            return new PagedResultDto<ReceiptReportDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Receipts)]
        public async Task<FileDto> ExportReceiptsToExcel(GetReceiptReportInput input)
        {
            var query = await GetReceiptsQuery(input);

            var items = await query
                .Select(x => new ReceiptExcelReportDto
                {
                    ReceiptId = x.Id,
                    OrderId = x.OrderId,
                    DeliveryDate = x.DeliveryDate,
                    CustomerName = x.Customer.Name,
                    SalesTaxRate = x.SalesTaxRate,
                    CODTotal = x.Total,
                    FreightTotal = x.FreightTotal,
                    MaterialTotal = x.MaterialTotal,
                    SalesTax = x.SalesTax,
                    Items = x.ReceiptLines.Select(l => new ReceiptExcelReportItemDto
                    {
                        IsTaxable = l.FreightItem.IsTaxable,
                        IsFreightTaxable = l.FreightItem.IsTaxable,
                        IsMaterialTaxable = l.MaterialItem.IsTaxable,
                        ActualMaterialQuantity = l.MaterialQuantity,
                        ActualFreightQuantity = l.FreightQuantity,
                        FreightPricePerUnit = l.FreightRate,
                        MaterialPricePerUnit = l.MaterialRate,
                        ReceiptLineMaterialPrice = l.MaterialAmount,
                        ReceiptLineFreightPrice = l.FreightAmount,
                        OrderLineMaterialPrice = l.OrderLine == null ? 0 : l.OrderLine.MaterialPrice,
                        OrderLineFreightPrice = l.OrderLine == null ? 0 : l.OrderLine.FreightPrice,
                        IsOrderLineMaterialPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsMaterialPriceOverridden,
                        IsOrderLineFreightPriceOverridden = l.OrderLine == null ? false : l.OrderLine.IsFreightPriceOverridden,
                        FreightItemName = l.FreightItem.Name,
                        MaterialItemName = l.MaterialItem.Name,
                        Designation = l.Designation,
                        LoadAtName = l.LoadAt.DisplayName,
                        DeliverToName = l.DeliverTo.DisplayName,
                        MaterialUomName = l.MaterialUom.Name,
                        FreightUomName = l.FreightUom.Name,
                    }).ToList(),
                })
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            if (items.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export.");
            }

            //await CalculateTotalsAsync<ReceiptsDto<ReceiptsReportItemDto>, ReceiptsReportItemDto>(items);

            return await _receiptsExcelExporter.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task SetOrderIsBilled(SetOrderIsBilledInput input)
        {
            var existingBillingRecords = await (await _billedOrderRepository.GetQueryAsync())
                .Where(x => x.OrderId == input.OrderId && x.OfficeId == OfficeId)
                .ToListAsync();
            //only one record is expected, but we'll want to delete both records in case a duplicate occurs somehow

            if (input.IsBilled)
            {
                if (!existingBillingRecords.Any())
                {
                    await _billedOrderRepository.InsertAsync(new BilledOrder
                    {
                        OrderId = input.OrderId,
                        OfficeId = OfficeId,
                    });
                }
            }
            else
            {
                if (existingBillingRecords.Any())
                {
                    await _billedOrderRepository.DeleteRangeAsync(existingBillingRecords);
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<Document> GetWorkOrderReport(GetWorkOrderReportInput input)
        {
            if (input.Id == null && input.Date == null && input.Ids?.Any() != true)
            {
                throw new ArgumentNullException(nameof(input.Id), "At least one of (Id, Date) should be set");
            }

            var paidImagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Common/Images/Paid.png");
            var paidImageBytes = await File.ReadAllBytesAsync(paidImagePath);
            var staggeredTimeImagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Common/Images/far-clock.png");
            var staggeredTimeImageBytes = await File.ReadAllBytesAsync(staggeredTimeImagePath);
            var timeZone = await GetTimezone();
            var showDriverNamesOnPrintedOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowDriverNamesOnPrintedOrder);
            var showLoadAtOnPrintedOrder = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.ShowLoadAtOnPrintedOrder);
            var spectrumNumberLabel = await SettingManager.GetSettingValueAsync(AppSettings.General.UserDefinedField1);
            var showSignatureColumn = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia) == DispatchVia.DriverApplication
                    && !await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.HideTicketControlsInDriverApp);
            var shiftDictionary = await SettingManager.GetShiftDictionary();
            var currentCulture = await SettingManager.GetCurrencyCultureAsync();
            var showTruckCategories = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders);
            var includeTravelTime = await FeatureChecker.IsEnabledAsync(AppFeatures.IncludeTravelTime);
            input.Date = input.Date?.Date;

            var data = await (await GetWorkOrderReportQueryAsync(input)).ToListAsync();

            foreach (var officeGroup in data.GroupBy(x => x.OfficeId))
            {
                var officeLogoBytes = await _logoProvider.GetReportLogoAsBytesAsync(officeGroup.Key);
                foreach (var item in officeGroup)
                {
                    item.LogoBytes = officeLogoBytes;
                }
            }

            data.ForEach(x =>
            {
                //TODO remove items with actual quantity of 0 when UseActualAmount is true
                //if (x.Items.Count > 1)
                //    // ReSharper disable once CompareOfFloatsByEqualityOperator
                //    x.Items.RemoveAll(s =>
                //        (s.MaterialQuantity ?? 0) == 0 && (s.FreightQuantity ?? 0) == 0 && s.NumberOfTrucks == 0 &&
                //        (!input.UseActualAmount || (s.ActualQuantity ?? 0) == 0)
                //    );
                x.HidePrices = input.HidePrices;
                x.SplitRateColumn = input.SplitRateColumn;
                x.ShowPaymentStatus = input.ShowPaymentStatus;
                x.ShowSpectrumNumber = input.ShowSpectrumNumber && !spectrumNumberLabel.IsNullOrEmpty();
                x.SpectrumNumberLabel = spectrumNumberLabel;
                x.ShowOfficeName = input.ShowOfficeName;
                x.UseActualAmount = input.UseActualAmount;
                x.UseReceipts = input.UseReceipts;
                x.ShowDeliveryInfo = input.ShowDeliveryInfo;
                x.IncludeTickets = input.IncludeTickets;
                x.TimeZone = timeZone;
                x.ShowDriverNamesOnPrintedOrder = showDriverNamesOnPrintedOrder;
                x.ShowLoadAtOnPrintedOrder = showLoadAtOnPrintedOrder;
                x.OrderShiftName = x.OrderShift.HasValue && shiftDictionary.TryGetValue(x.OrderShift.Value, out var value) ? value : "";
                x.ShowSignatureColumn = showSignatureColumn;
                x.ShowTruckCategories = showTruckCategories;
                x.IncludeTravelTime = includeTravelTime;
                x.CurrencyCulture = currentCulture;
                x.DebugLayout = input.DebugLayout;
            });

            if (input.ShowDeliveryInfo)
            {
                foreach (var load in data
                    .Where(x => x.DeliveryInfoItems != null)
                    .SelectMany(x => x.DeliveryInfoItems)
                    .Where(x => x.Load != null)
                    .Select(x => x.Load))
                {
                    if (load.SignatureId.HasValue && load.SignatureBytes == null)
                    {
                        load.SignatureBytes = await _binaryObjectManager.GetImageAsBytesAsync(load.SignatureId.Value);
                    }
                }
            }

            if (input.IncludeTickets)
            {
                foreach (var ticket in data
                    .Where(x => x.DeliveryInfoItems != null)
                    .SelectMany(x => x.DeliveryInfoItems))
                {
                    if (ticket.TicketPhotoId.HasValue)
                    {
                        ticket.TicketPhotoBytes = await _binaryObjectManager.GetImageAsBytesAsync(ticket.TicketPhotoId.Value);
                    }
                }
            }

            var collectionModel = new WorkOrderReportCollectionDto
            {
                WorkOrderReports = data,
                PaidImageBytes = paidImageBytes,
                StaggeredTimeImageBytes = staggeredTimeImageBytes,
                ConvertPdfTicketImages = await FeatureChecker.IsEnabledAsync(AppFeatures.PrintAlreadyUploadedPdfTicketImages),
                SeparateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems),
            };

            var workOrderReportGenerator = new WorkOrderReportGenerator(_orderTaxCalculator);

            return await workOrderReportGenerator.GenerateReport(collectionModel);
        }

        [AbpAuthorize(AppPermissions.Pages_PrintOrders)]
        public async Task<bool> DoesWorkOrderReportHaveData(GetWorkOrderReportInput input)
        {
            return await (await GetWorkOrderReportQueryAsync(input)).AnyAsync();
        }

        private async Task<IQueryable<WorkOrderReportDto>> GetWorkOrderReportQueryAsync(GetWorkOrderReportInput input)
        {
            if (input.UseReceipts)
            {
                return (await _receiptRepository.GetQueryAsync())
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(input.Ids?.Any() == true, x => input.Ids.Contains(x.Id))
                    .WhereIf(input.Date.HasValue, x => x.Order.DeliveryDate == input.Date)
                    //.WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                    .GetWorkOrderReportDtoQuery(input, OfficeId);
            }
            else
            {
                return (await _orderRepository.GetQueryAsync())
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(input.Ids?.Any() == true, x => input.Ids.Contains(x.Id))
                    .WhereIf(input.Date.HasValue, x => x.DeliveryDate == input.Date)
                    .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                    .GetWorkOrderReportDtoQuery(input, OfficeId); //not input.OfficeId, this one is used for payments
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<byte[]> GetOrderSummaryReport(GetOrderSummaryReportInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            input.Date = input.Date.Date;

            var items = await (await GetOrderSummaryReportQueryAsync(input))
                .GetOrderSummaryReportItems(await SettingManager.GetShiftDictionary(), _orderTaxCalculator, SettingManager, separateItems);

            var data = new OrderSummaryReportDto
            {
                Date = input.Date,
                HidePrices = input.HidePrices,
                Items = items,
                UseShifts = await SettingManager.UseShifts(),
                CurrencyCulture = await SettingManager.GetCurrencyCultureAsync(),
            };

            return OrderSummaryReportGenerator.GenerateReport(data);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<bool> DoesOrderSummaryReportHaveData(GetOrderSummaryReportInput input)
        {
            return await (await GetOrderSummaryReportQueryAsync(input)).AnyAsync();
        }

        private async Task<IQueryable<OrderLine>> GetOrderSummaryReportQueryAsync(GetOrderSummaryReportInput input)
        {
            return (await _orderLineRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, x => x.Order.OfficeId == input.OfficeId)
                .Where(x =>
                    x.Order.DeliveryDate == input.Date
                    && !x.Order.IsPending
                    && (x.MaterialQuantity > 0 || x.FreightQuantity > 0 || x.NumberOfTrucks > 0)
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_PaymentReconciliation)]
        public async Task<byte[]> GetPaymentReconciliationReport(GetPaymentReconciliationReportInput input)
        {
            var timeZone = await GetTimezone();
            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();
            var startDateFilter = input.StartDate.ConvertTimeZoneFrom(timeZone);
            var endDateFilter = input.EndDate.ConvertTimeZoneFrom(timeZone);
            var heartlandPublicKeyId = await _paymentAppService.GetHeartlandPublicKeyIdAsync();

            try
            {
                await _paymentAppService.UpdatePaymentsFromHeartland(new UpdatePaymentsFromHeartlandInput
                {
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    AllOffices = input.AllOffices,
                });
            }
            catch (Exception e)
            {
                Logger.Error("Exception on UpdatePyamentsFromHeartland", e);
                throw new UserFriendlyException("Error", "Unable to receive transactions from Heartland. Please try again later.", e);
            }

            var items = await (await _paymentRepository.GetQueryAsync())
                .WhereIf(!input.AllOffices,
                    x => x.PaymentHeartlandKeyId == heartlandPublicKeyId
                        || x.OrderPayments.Any(o => o.OfficeId == OfficeId))
                .Where(x => x.AuthorizationCaptureDateTime >= startDateFilter && x.AuthorizationCaptureDateTime < endDateFilter.AddDays(1))
                .Where(x => !x.IsCancelledOrRefunded)
                .Select(x => new PaymentReconciliationReportItemDto
                {
                    PaymentId = x.Id,
                    CaptureAmount = x.AuthorizationCaptureAmount,
                    TransactionId = x.AuthorizationCaptureTransactionId,
                    TransactionDate = x.AuthorizationCaptureDateTime,
                    TransactionType = x.TransactionType,
                    CardLast4 = x.CardLast4,
                    CardType = x.CardType,
                    AuthorizationAmount = x.AuthorizationAmount,
                    BatchSummaryId = x.BatchSummaryId,
                }).ToListAsync();

            var paymentIds = items.Select(x => x.PaymentId).ToList();

            var orderDetails = await (await _orderPaymentRepository.GetQueryAsync())
                .Where(x => paymentIds.Contains(x.PaymentId))
                .Select(x => new
                {
                    PaymentId = x.PaymentId,
                    OrderId = x.Order.Id,
                    OfficeId = x.Order.OfficeId,
                    OfficeName = x.Order.Office.Name,
                    CustomerName = x.Order.Customer.Name,
                    DeliveryDate = x.Order.DeliveryDate,
                })
                .ToListAsync();

            foreach (var order in orderDetails)
            {
                var payment = items.First(x => x.PaymentId == order.PaymentId);
                payment.PaymentId = order.PaymentId;
                payment.OrderId = order.OrderId;
                payment.OfficeId = order.OfficeId;
                payment.OfficeName = order.OfficeName;
                payment.CustomerName = order.CustomerName;
                payment.DeliveryDate = order.DeliveryDate;
            }

            items.ForEach(x =>
            {
                x.TimeZone = timeZone;
                x.CurrencyCulture = currencyCulture;
            });

            return PaymentReconciliationReportGenerator.GenerateReport(new PaymentReconciliationReportDto
            {
                Items = items,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                AllOffices = input.AllOffices,
                OfficeName = Session.OfficeName,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<EmailOrderReportDto> GetEmailReceiptReportModel(EntityDto input)
        {
            var user = await (await _userRepository.GetQueryAsync())
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    PhoneNumber = x.PhoneNumber,
                })
                .FirstAsync();

            var receipt = await (await _receiptRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.ReceiptDate,
                    x.Shift,
                    ContactEmail = x.Order.CustomerContact.Email,
                })
                .FirstOrDefaultAsync();

            if (receipt == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailSubjectTemplate);

            var body = (await SettingManager.GetSettingValueAsync(AppSettings.Receipt.EmailBodyTemplate))
                    .ReplaceTokensInTemplate(new TemplateTokenDto
                    {
                        DeliveryDate = receipt.DeliveryDate.ToShortDateString(),
                        Shift = await SettingManager.GetShiftName(receipt.Shift) ?? "",
                        CompanyName = companyName,
                    })
                    .Replace("{ReceiptDate}", receipt.ReceiptDate.ToShortDateString())
                    .Replace("{Order.DateTime}", receipt.DeliveryDate.ToShortDateString()) // Support both the new {DeliveryDate} and old {Order.DateTime}
                    ;

            return new EmailOrderReportDto
            {
                Id = input.Id,
                UseReceipts = true,
                From = user.Email,
                To = receipt.ContactEmail,
                CC = user.Email,
                Subject = subject,
                Body = body,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<EmailOrderReportDto> GetEmailOrderReportModel(EntityDto input)
        {
            var user = await (await _userRepository.GetQueryAsync())
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    PhoneNumber = x.PhoneNumber,
                })
                .FirstAsync();

            var order = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.DeliveryDate,
                    x.Shift,
                    ContactEmail = x.CustomerContact.Email,
                    x.IsPending,
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = order.IsPending
                ? await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate)
                : await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailSubjectTemplate);

            var body = order.IsPending
                ? QuoteAppService.ReplaceEmailBodyTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailBodyTemplate), user.FirstName, user.LastName, user.PhoneNumber, companyName)
                : (await SettingManager.GetSettingValueAsync(AppSettings.Order.EmailBodyTemplate))
                    .ReplaceTokensInTemplate(new TemplateTokenDto
                    {
                        DeliveryDate = order.DeliveryDate.ToShortDateString(),
                        Shift = await SettingManager.GetShiftName(order.Shift) ?? "",
                        CompanyName = companyName,
                    })
                    .Replace("{Order.DateTime}", order.DeliveryDate.ToShortDateString()) // Support both the new {DeliveryDate} and old {Order.DateTime}
                    ;

            return new EmailOrderReportDto
            {
                Id = input.Id,
                From = user.Email,
                To = order.ContactEmail,
                CC = user.Email,
                Subject = subject,
                Body = body,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<EmailOrderReportResult> EmailOrderReport(EmailOrderReportDto input)
        {
            var report = input.UseReceipts
                ? await GetWorkOrderReport(new GetWorkOrderReportInput
                {
                    Id = input.Id,
                    UseReceipts = true,
                    ShowPaymentStatus = true,
                    ShowSpectrumNumber = true,
                    ShowOfficeName = true,
                })
                : await GetWorkOrderReport(new GetWorkOrderReportInput
                {
                    Id = input.Id,
                    ShowPaymentStatus = true,
                });
            var message = new MailMessage
            {
                From = new MailAddress(input.From),
                Subject = input.Subject,
                Body = input.Body,
                IsBodyHtml = false,
            };
            foreach (var to in EmailHelper.SplitEmailAddresses(input.To))
            {
                message.To.Add(to);
            }
            foreach (var cc in EmailHelper.SplitEmailAddresses(input.CC))
            {
                message.CC.Add(cc);
            }
            string filename;
            if (input.UseReceipts)
            {
                filename = "Receipt";
            }
            else
            {
                var orderDetails = await (await _orderRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new
                    {
                        x.IsPending,
                    })
                    .FirstAsync();

                filename = !orderDetails.IsPending
                    ? "Order"
                    : "Quote";
            }
            filename = Utilities.RemoveInvalidFileNameChars(filename);
            filename += ".pdf";

            using (var stream = new MemoryStream())
            {
                report.SaveToMemoryStream(stream);
                message.Attachments.Add(new Attachment(stream, filename));

                try
                {
                    var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                    if (!input.UseReceipts)
                    {
                        var order = await _orderRepository.GetAsync(input.Id);
                        order.LastQuoteEmailId = trackableEmailId;
                        await _orderEmailRepository.InsertAsync(new OrderEmail
                        {
                            EmailId = trackableEmailId,
                            OrderId = order.Id,
                        });
                    }
                }
                catch (SmtpException ex)
                {
                    if (ex.Message.Contains("The from address does not match a verified Sender Identity"))
                    {
                        return new EmailOrderReportResult
                        {
                            FromEmailAddressIsNotVerifiedError = true,
                        };
                    }
                    else
                    {
                        throw;
                    }
                }

                return new EmailOrderReportResult
                {
                    Success = true,
                };
            }
        }

        private async Task DecrementOrderLineNumbers(int orderId, int aboveLineNumber)
        {
            var orderLines = await (await _orderLineRepository.GetQueryAsync())
                       .Where(x => x.OrderId == orderId && x.LineNumber > aboveLineNumber)
                       .ToListAsync();

            foreach (var item in orderLines)
            {
                item.LineNumber -= 1;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public async Task<PagedResultDto<SelectListDto>> GetOrderLinesSelectListToMoveTicketsByDriverTo(GetOrderLinesSelectListToMoveTicketsByDriverToInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var query = (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Order.CustomerId == input.CustomerId && x.Order.DeliveryDate == input.DeliveryDate)
                .Select(x => new OrderLinesSelectListToMoveTicketsByDriverToDto
                {
                    OrderLineId = x.Id,
                    OfficeName = x.Order.Office.Name,
                    OrderId = x.OrderId,
                    LoadAtName = x.LoadAt.DisplayName,
                    DeliverToName = x.DeliverTo.DisplayName,
                    FreightItemName = x.FreightItem.Name,
                    Designation = x.Designation,
                    MaterialItemName = x.MaterialItem.Name,
                })
                .Select(x => new SelectListDto<OrderLinesSelectListToMoveTicketsByDriverToDto>
                {
                    Id = x.OrderLineId.ToString(),
                    Name = x.OfficeName + " "
                        + x.OrderId + " "
                        + x.LoadAtName + " "
                        + x.DeliverToName + " "
                        + (separateItems
                            ? x.Designation == DesignationEnum.MaterialOnly
                                ? x.MaterialItemName
                                : x.FreightItemName + ", " + x.MaterialItemName
                            : x.FreightItemName),
                    Item = x,
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<ValidateOrderLineTimeOnJobResult> ValidateOrderLineTimeOnJob(ValidateOrderLineTimeOnJobInput input)
        {
            var result = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineId)
                .Select(ol => new ValidateOrderLineTimeOnJobResult
                {
                    HasOrderLineTrucks = ol.OrderLineTrucks.Any(olt => !olt.IsDone),
                    HasDisagreeingOrderLineTrucks = ol.OrderLineTrucks.Any(olt => !olt.IsDone && olt.TimeOnJob != ol.TimeOnJob),
                    HasOpenDispatches = ol.Dispatches.Any(d => Dispatch.OpenStatuses.Contains(d.Status)),
                }).FirstAsync();

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<ValidateOrderLineTruckTimeOnJobResult> ValidateOrderLineTruckTimeOnJob(ValidateOrderLineTruckTimeOnJobInput input)
        {
            var result = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineTruckId)
                .Select(olt => new ValidateOrderLineTruckTimeOnJobResult
                {
                    HasOpenDispatches = olt.Dispatches.Any(d => Dispatch.OpenStatuses.Contains(d.Status)),
                }).FirstAsync();

            return result;
        }
    }
}
