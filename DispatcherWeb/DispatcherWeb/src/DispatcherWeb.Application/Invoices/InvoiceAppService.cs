using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Caching;
using DispatcherWeb.Charges;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Invoices.Reports;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.QuickbooksOnline;
using DispatcherWeb.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Invoices
{
    [AbpAuthorize]
    public class InvoiceAppService : DispatcherWebAppServiceBase, IInvoiceAppService
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<InvoiceBatch> _invoiceBatchRepository;
        private readonly IRepository<InvoiceLine> _invoiceLineRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Charge> _chargeRepository;
        private readonly IRepository<InvoiceEmail> _invoiceEmailRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<TrackableEmail, Guid> _trackableEmailRepository;
        private readonly IRepository<TrackableEmailEvent> _trackableEmailEventRepository;
        private readonly IRepository<TrackableEmailReceiver> _trackableEmailReceiverRepository;
        private readonly ListCacheCollection _listCaches;
        private readonly InvoicePrintOutGenerator _invoicePrintOutGenerator;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;
        private readonly ILogoProvider _logoProvider;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public InvoiceAppService(
            IRepository<Invoice> invoiceRepository,
            IRepository<InvoiceBatch> invoiceBatchRepository,
            IRepository<InvoiceLine> invoiceLineRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Customer> customerRepository,
            IRepository<Charge> chargeRepository,
            IRepository<InvoiceEmail> invoiceEmailRepository,
            IRepository<Item> itemRepository,
            IRepository<TrackableEmail, Guid> trackableEmailRepository,
            IRepository<TrackableEmailEvent> trackableEmailEventRepository,
            IRepository<TrackableEmailReceiver> trackableEmailReceiverRepository,
            ListCacheCollection listCaches,
            InvoicePrintOutGenerator invoicePrintOutGenerator,
            OrderTaxCalculator orderTaxCalculator,
            ITrackableEmailSender trackableEmailSender,
            ICrossTenantOrderSender crossTenantOrderSender,
            ILogoProvider logoProvider,
            IBackgroundJobManager backgroundJobManager
            )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceBatchRepository = invoiceBatchRepository;
            _invoiceLineRepository = invoiceLineRepository;
            _ticketRepository = ticketRepository;
            _customerRepository = customerRepository;
            _chargeRepository = chargeRepository;
            _invoiceEmailRepository = invoiceEmailRepository;
            _itemRepository = itemRepository;
            _trackableEmailRepository = trackableEmailRepository;
            _trackableEmailEventRepository = trackableEmailEventRepository;
            _trackableEmailReceiverRepository = trackableEmailReceiverRepository;
            _listCaches = listCaches;
            _invoicePrintOutGenerator = invoicePrintOutGenerator;
            _orderTaxCalculator = orderTaxCalculator;
            _trackableEmailSender = trackableEmailSender;
            _crossTenantOrderSender = crossTenantOrderSender;
            _logoProvider = logoProvider;
            _backgroundJobManager = backgroundJobManager;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public async Task<PagedResultDto<InvoiceDto>> GetInvoices(GetInvoicesInput input)
        {
            var permissions = new
            {
                ViewAnyInvoices = await IsGrantedAsync(AppPermissions.Pages_Invoices),
                ViewCustomerInvoicesOnly = await IsGrantedAsync(AppPermissions.CustomerPortal_Invoices),
            };

            if (permissions.ViewAnyInvoices)
            {
                //do not additionally filter the data
            }
            else if (permissions.ViewCustomerInvoicesOnly)
            {
                input.CustomerId = Session.GetCustomerIdOrThrow(this);
                input.ApplyCustomerLimitations = true;
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            var query = FilterInvoiceQuery(await _invoiceRepository.GetQueryAsync(), input);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new InvoiceDto
                {
                    Id = x.Id,
                    Status = x.Status,
                    CustomerName = x.Customer.Name,
                    CustomerHasMaterialCompany = x.Customer.MaterialCompanyTenantId != null,
                    JobNumbers = x.InvoiceLines.Select(l => l.JobNumber).Where(j => !string.IsNullOrEmpty(j)).Distinct().ToList(),
                    JobNumberSort = x.InvoiceLines.Select(l => l.JobNumber).Where(j => !string.IsNullOrEmpty(j)).FirstOrDefault(),
                    IssueDate = x.IssueDate,
                    TotalAmount = x.TotalAmount,
                    QuickbooksExportDateTime = x.QuickbooksExportDateTime,
                    EmailDeliveryStatuses = x.InvoiceEmails.Select(e => e.Email.CalculatedDeliveryStatus).ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<InvoiceDto>(
                totalCount,
                items);
        }

        [RemoteService(false)]
        public static IQueryable<Invoice> FilterInvoiceQuery(IQueryable<Invoice> query, GetInvoicesInput input)
        {
            return query
                .WhereIf(input.InvoiceId.HasValue,
                    x => x.Id == input.InvoiceId)
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId)
                .WhereIf(input.Status >= 0,
                    x => x.Status == input.Status)
                .WhereIf(input.ApplyCustomerLimitations,
                    x => new[] { InvoiceStatus.Printed, InvoiceStatus.Sent, InvoiceStatus.Viewed }.Contains(x.Status))
                .WhereIf(input.IssueDateStart.HasValue,
                    x => x.IssueDate >= input.IssueDateStart)
                .WhereIf(input.IssueDateEnd.HasValue,
                    x => x.IssueDate <= input.IssueDateEnd)
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId)
                .WhereIf(input.BatchId.HasValue,
                    x => x.BatchId == input.BatchId)
                .WhereIf(input.UploadBatchId.HasValue,
                    x => x.UploadBatchId == input.UploadBatchId)
                .WhereIf(!input.TicketNumber.IsNullOrEmpty(),
                    x => x.InvoiceLines.Any(l => l.Ticket.TicketNumber == input.TicketNumber));
        }

        public static void ValidateGetInvoicesInputForExport(GetInvoicesInput input)
        {
            if (input.IssueDateStart == null
                || input.IssueDateEnd == null
                || input.IssueDateEnd < input.IssueDateStart
                || (input.IssueDateEnd - input.IssueDateStart)?.TotalDays > 31)
            {
                throw new UserFriendlyException("You must select a date range of less than 31 days to run this export.");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public async Task<InvoiceEditDto> GetInvoiceForEdit(NullableIdDto input)
        {
            InvoiceEditDto invoiceEditDto;

            if (input.Id.HasValue)
            {
                invoiceEditDto = await (await _invoiceRepository.GetQueryAsync())
                    .Select(invoice => new InvoiceEditDto
                    {
                        Id = invoice.Id,
                        BalanceDue = invoice.TotalAmount,
                        Subtotal = invoice.InvoiceLines.Sum(x => x.Subtotal),
                        TaxAmount = invoice.InvoiceLines.Sum(x => x.Tax),
                        TaxRate = invoice.TaxRate,
                        SalesTaxEntityId = invoice.SalesTaxEntityId,
                        SalesTaxEntityName = invoice.SalesTaxEntity.Name,
                        BillingAddress = invoice.BillingAddress,
                        EmailAddress = invoice.EmailAddress,
                        CustomerId = invoice.CustomerId,
                        CustomerName = invoice.Customer.Name,
                        CustomerInvoicingMethod = invoice.Customer.InvoicingMethod,
                        OfficeId = invoice.OfficeId,
                        OfficeName = invoice.Office.Name,
                        DueDate = invoice.DueDate,
                        IssueDate = invoice.IssueDate,
                        Message = invoice.Message,
                        Status = invoice.Status,
                        UploadBatchId = invoice.UploadBatchId,
                        BatchId = invoice.BatchId,
                        Terms = invoice.Terms,
                        JobNumber = invoice.JobNumber,
                        PoNumber = invoice.PoNumber,
                        Description = invoice.Description,
                        ShowFuelSurchargeOnInvoice = invoice.ShowFuelSurchargeOnInvoice,
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                invoiceEditDto = new InvoiceEditDto
                {
                    IssueDate = await GetToday(),
                    Message = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.DefaultMessageOnInvoice),
                    ShowFuelSurchargeOnInvoice = await SettingManager.ShowFuelSurchargeOnInvoice(),
                    OfficeId = Session.OfficeId,
                    OfficeName = Session.OfficeName,
                };
            }

            var fuelItemId = await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            var fuelItem = fuelItemId > 0 ? await (await _itemRepository.GetQueryAsync())
                .Where(x => x.Id == fuelItemId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.IsTaxable,
                })
                .FirstOrDefaultAsync() : null;

            invoiceEditDto.FuelItemId = fuelItem?.Id;
            invoiceEditDto.FuelItemName = fuelItem?.Name;
            invoiceEditDto.FuelItemIsTaxable = fuelItem?.IsTaxable ?? false;

            await CheckInvoicePermissions(invoiceEditDto.CustomerId);

            return invoiceEditDto;
        }

        private async Task CheckInvoicePermissions(int? entityCustomerId)
        {
            await CheckCustomerSpecificPermissions(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices, entityCustomerId);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public async Task<PagedResultDto<InvoiceLineEditDto>> GetInvoiceLines(GetInvoiceLinesInput input)
        {
            var items = await (await _invoiceLineRepository.GetQueryAsync())
                .Where(x => x.InvoiceId == input.InvoiceId)
                .Select(x => new InvoiceLineEditDto
                {
                    Id = x.Id,
                    LineNumber = x.LineNumber,
                    CarrierId = x.CarrierId,
                    CarrierName = x.Carrier.Name,
                    DeliveryDateTime = x.DeliveryDateTime,
                    Description = x.Description,
                    FreightItemId = x.FreightItemId,
                    FreightItemName = x.FreightItem.Name,
                    MaterialItemId = x.MaterialItemId,
                    MaterialItemName = x.MaterialItem.Name,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    MaterialRate = x.MaterialRate,
                    FreightRate = x.FreightRate,
                    IsFreightRateOverridden = x.IsFreightRateOverridden,
                    Subtotal = x.Subtotal,
                    ExtendedAmount = x.ExtendedAmount,
                    FreightExtendedAmount = x.FreightExtendedAmount,
                    LeaseHaulerName = x.Ticket.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                    MaterialExtendedAmount = x.MaterialExtendedAmount,
                    FuelSurcharge = x.FuelSurcharge,
                    Tax = x.Tax,
                    IsFreightTaxable = x.IsFreightTaxable,
                    IsMaterialTaxable = x.IsMaterialTaxable,
                    TicketId = x.TicketId,
                    ChargeId = x.ChargeId,
                    OrderLineId = x.Ticket.OrderLine != null
                        ? x.Ticket.OrderLine.Id
                        : x.Charge.OrderLine.Id,
                    TicketNumber = x.Ticket.TicketNumber,
                    JobNumber = x.JobNumber,
                    PoNumber = x.Ticket.OrderLine != null
                        ? x.Ticket.OrderLine.Order.PONumber
                        : x.Charge.OrderLine.Order.PONumber,
                    TruckCode = x.TruckCode,
                    ChildInvoiceLineKind = x.ChildInvoiceLineKind,
                    ParentInvoiceLineId = x.ParentInvoiceLineId,
                    CustomerId = x.Ticket.OrderLine != null
                        ? x.Ticket.OrderLine.Order.CustomerId
                        : x.Charge.OrderLine.Order.CustomerId,
                    SalesTaxRate = x.Ticket.OrderLine != null
                        ? x.Ticket.OrderLine.Order.SalesTaxRate
                        : x.Charge.OrderLine.Order.SalesTaxRate,
                    SalesTaxEntityId = x.Ticket.OrderLine != null
                        ? x.Ticket.OrderLine.Order.SalesTaxEntityId
                        : x.Charge.OrderLine.Order.SalesTaxEntityId,
                    SalesTaxEntityName = x.Ticket.OrderLine != null
                        ? x.Ticket.OrderLine.Order.SalesTaxEntity.Name
                        : x.Charge.OrderLine.Order.SalesTaxEntity.Name,
                    UseMaterialQuantity = x.Charge.UseMaterialQuantity,
                })
                .OrderBy(x => x.LineNumber)
                .ToListAsync();

            var customerIds = items.Where(x => x.TicketId != null).Select(x => x.CustomerId).Distinct().ToArray();
            if (customerIds.Any())
            {
                await CheckCustomerSpecificPermissions(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices,
                    items.Where(x => x.TicketId != null).Select(x => x.CustomerId).Distinct().ToArray());
            }

            return new PagedResultDto<InvoiceLineEditDto>(items.Count, items);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<bool> GetCustomerHasTickets(GetCustomerTicketsInput input)
        {
            return (await GetCustomerTickets(input)).Items.Any();
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<GetCustomerTicketsResult> GetCustomerTickets(GetCustomerTicketsInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            input.Normalize();

            var query = (await _ticketRepository.GetQueryAsync())
                .Where(x => !x.NonbillableFreight || !x.NonbillableMaterial)
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId)
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId)
                .WhereIf(input.IsBilled.HasValue,
                    x => x.IsBilled == input.IsBilled)
                .WhereIf(input.IsVerified.HasValue,
                    x => x.IsVerified == input.IsVerified)
                .WhereIf(input.HasInvoiceLineId == true,
                    x => x.InvoiceLine != null)
                .WhereIf(input.HasInvoiceLineId == false,
                    x => x.InvoiceLine == null)
                .WhereIf(input.ExcludeTicketIds?.Any() == true,
                    x => !input.ExcludeTicketIds.Contains(x.Id))
                .WhereIf(input.TicketIds != null,
                    x => input.TicketIds.Contains(x.Id))
                .WhereIf(input.JobNumbers?.Any() == true,
                    x => input.JobNumbers.Contains(x.OrderLine.JobNumber))
                .WhereIf(input.OrderLineIds?.Any() == true,
                    x => input.OrderLineIds.Contains(x.OrderLineId))
                .WhereIf(input.SalesTaxRates?.Any() == true,
                    x => input.SalesTaxRates.Contains(x.OrderLine.Order.SalesTaxRate))
                .WhereIf(input.SalesTaxEntityIds?.Any() == true,
                    x => input.SalesTaxEntityIds.Contains(x.OrderLine.Order.SalesTaxEntityId))
                .Select(x => new CustomerTicketDto
                {
                    Id = x.Id,
                    OrderLineId = x.OrderLine.Id,
                    CarrierId = x.CarrierId,
                    CarrierName = x.Carrier.Name,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.Name,
                    DeliverToName = x.DeliverTo.DisplayName,
                    LoadAtName = x.LoadAt.DisplayName,
                    FreightItemName = x.FreightItem.Name,
                    FreightItemId = x.FreightItemId,
                    MaterialItemName = x.MaterialItem.Name,
                    MaterialItemId = x.MaterialItemId,
                    IsFreightTaxable = (x.OrderLine == null || !x.OrderLine.Order.IsTaxExempt)
                                && (x.FreightItem == null || x.FreightItem.IsTaxable),
                    IsMaterialTaxable = (x.OrderLine == null || !x.OrderLine.Order.IsTaxExempt)
                                && (x.MaterialItem == null || x.MaterialItem.IsTaxable),
                    OrderDeliveryDate = x.OrderLine.Order.DeliveryDate,
                    TicketDateTime = x.TicketDateTime,
                    TicketNumber = x.TicketNumber,
                    TruckCode = x.TruckCode,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    Designation = x.OrderLine.Designation,
                    MaterialRate = x.NonbillableMaterial ? 0 : x.OrderLine.MaterialPricePerUnit,
                    FreightRate = x.NonbillableFreight ? 0 : x.OrderLine.FreightPricePerUnit,
                    FreightUomId = x.FreightUomId,
                    MaterialUomId = x.MaterialUomId,
                    FreightUomName = x.FreightUom.Name,
                    MaterialUomName = x.MaterialUom.Name,
                    OrderLineFreightUomId = x.OrderLine.FreightUomId,
                    OrderLineMaterialUomId = x.OrderLine.MaterialUomId,
                    OrderLineFreightUomName = x.OrderLine.FreightUom.Name,
                    OrderLineMaterialUomName = x.OrderLine.MaterialUom.Name,
                    TicketUomId = x.FreightUomId,
                    JobNumber = x.OrderLine.JobNumber,
                    PoNumber = x.OrderLine.Order.PONumber,
                    SalesTaxRate = x.OrderLine.Order.SalesTaxRate,
                    SalesTaxEntityId = x.OrderLine.Order.SalesTaxEntityId,
                    SalesTaxEntityName = x.OrderLine.Order.SalesTaxEntity.Name,
                    IsOrderLineFreightTotalOverridden = x.OrderLine.IsFreightPriceOverridden,
                    IsOrderLineMaterialTotalOverridden = x.OrderLine.IsMaterialPriceOverridden,
                    OrderLineFreightTotal = x.OrderLine.FreightPrice,
                    OrderLineMaterialTotal = x.OrderLine.MaterialPrice,
                    FuelSurcharge = x.FuelSurcharge,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                    InvoiceLineId = x.InvoiceLine.Id,
                    InvoicingMethod = x.Customer.InvoicingMethod,
                });

            var items = await query
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            var hideLoadAtAndDeliverToOnHourlyInvoices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.HideLoadAtAndDeliverToOnHourlyInvoices);

            items.ForEach(x =>
            {
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, x, x.SalesTaxRate ?? 0, separateItems);
                x.HideLoadAtAndDeliverToOnHourlyInvoices = hideLoadAtAndDeliverToOnHourlyInvoices;

                var includeBothQuantities = x.FreightQuantity > 0
                    && x.MaterialQuantity > 0
                    && !string.IsNullOrEmpty(x.MaterialUomName)
                    && !string.IsNullOrEmpty(x.FreightUomName)
                    && x.MaterialUomName != x.FreightUomName;

                var itemNamesSeparator = !string.IsNullOrEmpty(x.FreightItemName) && !string.IsNullOrEmpty(x.MaterialItemName) ? " of " : "";

                var itemDescription = includeBothQuantities
                    ? $"{x.FreightItemName}{itemNamesSeparator}{x.MaterialItemName} - {x.FreightQuantity} {x.FreightUomName} {x.MaterialQuantity} {x.MaterialUomName}"
                    : OrderItemFormatter.GetItemWithQuantityFormatted(x);

                x.Description = $"{itemDescription}{x.LoadAtAndDeliverToDescription}{x.JobNumberAndPoNumberDescription}";
            });

            items = items
                .WhereIf(input.HasRevenue == true, x => x.Total > 0)
                .WhereIf(input.HasRevenue == false, x => x.Total == 0)
                .ToList();

            return new GetCustomerTicketsResult(
                items.Count,
                items);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<bool> GetCustomerHasCharges(GetCustomerChargesInput input)
        {
            return await (await GetCustomerChargesQueryAsync(input)).AnyAsync();
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PagedResultDto<CustomerChargeDto>> GetCustomerCharges(GetCustomerChargesInput input)
        {
            var query = await GetCustomerChargesQueryAsync(input);

            var items = await query
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            return new PagedResultDto<CustomerChargeDto>(
                items.Count,
                items);
        }

        private async Task<IQueryable<CustomerChargeDto>> GetCustomerChargesQueryAsync(GetCustomerChargesInput input)
        {
            input.Normalize();
            var query = (await _chargeRepository.GetQueryAsync())
                .WhereIf(input.CustomerId.HasValue, x => x.OrderLine.Order.CustomerId == input.CustomerId)
                .WhereIf(input.CustomerIds?.Any() == true, x => input.CustomerIds.Contains(x.OrderLine.Order.CustomerId))
                .WhereIf(input.OfficeId.HasValue, x => x.OrderLine.Order.OfficeId == input.OfficeId)
                .WhereIf(input.IsBilled.HasValue, x => x.IsBilled == input.IsBilled)
                .WhereIf(input.HasInvoiceLineId == true, x => x.InvoiceLines.Any())
                .WhereIf(input.HasInvoiceLineId == false, x => !x.InvoiceLines.Any() || x.UseMaterialQuantity)
                .WhereIf(input.ExcludeChargeIds?.Any() == true, x => !input.ExcludeChargeIds.Contains(x.Id))
                .WhereIf(input.ChargeIds != null, x => input.ChargeIds.Contains(x.Id))
                .WhereIf(input.JobNumbers?.Any() == true, x => input.JobNumbers.Contains(x.OrderLine.JobNumber))
                .WhereIf(input.OrderLineIds?.Any() == true, x => input.OrderLineIds.Contains(x.OrderLineId))
                .WhereIf(input.SalesTaxRates?.Any() == true, x => input.SalesTaxRates.Contains(x.OrderLine.Order.SalesTaxRate))
                .WhereIf(input.SalesTaxEntityIds?.Any() == true, x => input.SalesTaxEntityIds.Contains(x.OrderLine.Order.SalesTaxEntityId))
                .WhereIf(input.StartDate.HasValue, x => x.ChargeDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, x => x.ChargeDate <= input.EndDate)
                .Select(x => new CustomerChargeDto
                {
                    Id = x.Id,
                    OrderLineId = x.OrderLineId,
                    CustomerId = x.OrderLine.Order.CustomerId,
                    ChargeDate = x.ChargeDate,
                    Description = x.Description,
                    ItemId = x.ItemId,
                    ItemName = x.Item.Name,
                    ItemType = x.Item.Type,
                    UnitOfMeasureName = x.UnitOfMeasure.Name,
                    Quantity = x.Quantity,
                    UseMaterialQuantity = x.UseMaterialQuantity,
                    Rate = x.Rate,
                    ChargeAmount = x.ChargeAmount,
                    JobNumber = x.OrderLine.JobNumber,
                    PoNumber = x.OrderLine.Order.PONumber,
                    SalesTaxRate = x.OrderLine.Order.SalesTaxRate,
                    SalesTaxEntityId = x.OrderLine.Order.SalesTaxEntityId,
                    SalesTaxEntityName = x.OrderLine.Order.SalesTaxEntity.Name,
                    IsTaxable = !x.OrderLine.Order.IsTaxExempt
                                && x.Item.IsTaxable,
                });

            return query;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PagedResultDto<SelectListDto>> GetActiveCustomersSelectList(GetSelectListInput input)
        {
            var query = (await _customerRepository.GetQueryAsync())
                .Where(x => x.IsActive)
                .Select(x => new SelectListDto<CustomerSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new CustomerSelectListInfoDto
                    {
                        CustomerId = x.Id,
                        InvoiceEmail = x.InvoiceEmail,
                        BillingAddress1 = x.BillingAddress1,
                        BillingAddress2 = x.BillingAddress2,
                        BillingCity = x.BillingCity,
                        BillingState = x.BillingState,
                        BillingZipCode = x.BillingZipCode,
                        Terms = x.Terms,
                        InvoicingMethod = x.InvoicingMethod,
                    },
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<int> EditInvoice(InvoiceEditDto model)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var invoice = model.Id > 0
                ? await (await _invoiceRepository.GetQueryAsync())
                    .Include(x => x.InvoiceLines)
                    .Where(x => x.Id == model.Id.Value)
                    .FirstAsync()
                : new Invoice
                {
                    TenantId = await AbpSession.GetTenantIdAsync(),
                    OfficeId = model.OfficeId ?? OfficeId,
                };

            if (invoice.CustomerId != model.CustomerId)
            {
                if (invoice.InvoiceLines.Any())
                {
                    throw new UserFriendlyException("You can't change the customer after the tickets for the customers were selected");
                }
                if (model.CustomerId.HasValue)
                {
                    invoice.CustomerId = model.CustomerId.Value;
                }
            }

            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            invoice.EmailAddress = model.EmailAddress;
            invoice.BillingAddress = model.BillingAddress;
            invoice.Terms = model.Terms;
            invoice.IssueDate = model.IssueDate;
            invoice.DueDate = model.DueDate;
            invoice.Message = model.Message;
            invoice.TaxRate = model.TaxRate ?? 0;
            invoice.SalesTaxEntityId = model.SalesTaxEntityId;
            invoice.JobNumber = model.JobNumber;
            invoice.PoNumber = model.PoNumber;
            invoice.Description = model.Description;
            invoice.ShowFuelSurchargeOnInvoice = model.ShowFuelSurchargeOnInvoice;

            var newInvoiceLineEntities = new List<InvoiceLine>();
            var removedTicketIds = new List<int>();
            var removedChargeIds = new List<int>();
            if (model.InvoiceLines != null)
            {
                foreach (var invoiceLine in model.InvoiceLines)
                {
                    IOrderLineTaxTotalDetails invoiceLineForTaxCalculation = invoiceLine;
                    if (invoiceLine.ChargeId != null)
                    {
                        //for charges, the entire amount needs to be taxed per spec, so we'll pass the entire amount as material amount to the tax calculator
                        invoiceLineForTaxCalculation = new OrderLineTaxTotalDetailsDto
                        {
                            FreightPrice = 0,
                            MaterialPrice = invoiceLine.MaterialExtendedAmount + invoiceLine.FreightExtendedAmount,
                            Subtotal = invoiceLine.Subtotal,
                            Tax = invoiceLine.Tax,
                            IsTaxable = invoiceLine.IsFreightTaxable,
                            IsFreightTaxable = invoiceLine.IsFreightTaxable,
                            IsMaterialTaxable = invoiceLine.IsMaterialTaxable,
                        };
                    }
                    OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, invoiceLineForTaxCalculation, invoice.TaxRate, separateItems);
                }
                invoice.TotalAmount = model.InvoiceLines.Sum(x => x.ExtendedAmount);
                invoice.Tax = model.InvoiceLines.Sum(x => x.Tax);

                var ticketIds = model.InvoiceLines.Where(x => x.TicketId.HasValue).Select(x => x.TicketId.Value).ToList();
                var takenTicketIds = await (await _ticketRepository.GetQueryAsync())
                    .Where(x => ticketIds.Contains(x.Id) && x.InvoiceLine != null)
                    .Select(x => x.Id).ToListAsync();

                var chargeIds = model.InvoiceLines.Where(x => x.ChargeId.HasValue).Select(x => x.ChargeId.Value).Union(
                    invoice.InvoiceLines.Where(x => x.ChargeId.HasValue).Select(x => x.ChargeId.Value)
                ).ToList();
                var charges = await (await _chargeRepository.GetQueryAsync())
                    .Where(x => chargeIds.Contains(x.Id))
                    .ToListAsync();

                var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));

                var timezone = await GetTimezone();

                model.InvoiceLines = model.InvoiceLines
                        .OrderByDescending(x => x.ChildInvoiceLineKind != ChildInvoiceLineKind.BottomFuelSurchargeLine)
                        .ThenByDescending(x => x.ChargeId == null)
                        .ThenBy(x => x.DeliveryDateTime?.ConvertTimeZoneTo(timezone).Date)
                        .ThenBy(x => x.TruckCode)
                        .ThenBy(x => x.TicketNumber)
                        .ToList();

                short lineNumber = 1;
                foreach (var modelInvoiceLine in model.InvoiceLines)
                {
                    var invoiceLine = modelInvoiceLine.Id == 0 ? null : invoice.InvoiceLines.FirstOrDefault(x => x.Id == modelInvoiceLine.Id);
                    if (invoiceLine == null)
                    {
                        invoiceLine = new InvoiceLine
                        {
                            Invoice = invoice,
                            TenantId = await AbpSession.GetTenantIdAsync(),
                        };
                        //invoice.InvoiceLines.Add(invoiceLine);
                        await _invoiceLineRepository.InsertAsync(invoiceLine);
                        newInvoiceLineEntities.Add(invoiceLine);
                    }

                    invoiceLine.LineNumber = lineNumber++;
                    if (invoiceLine.TicketId != modelInvoiceLine.TicketId)
                    {
                        if (modelInvoiceLine.TicketId.HasValue)
                        {
                            if (takenTicketIds.Contains(modelInvoiceLine.TicketId.Value))
                            {
                                continue;
                            }
                            takenTicketIds.Add(modelInvoiceLine.TicketId.Value);
                        }
                        invoiceLine.TicketId = modelInvoiceLine.TicketId;
                    }
                    invoiceLine.ChargeId = modelInvoiceLine.ChargeId;
                    invoiceLine.CarrierId = modelInvoiceLine.CarrierId;
                    invoiceLine.DeliveryDateTime = modelInvoiceLine.DeliveryDateTime;
                    invoiceLine.TruckCode = modelInvoiceLine.TruckCode;
                    invoiceLine.FreightItemId = modelInvoiceLine.FreightItemId;
                    if (separateItems)
                    {
                        invoiceLine.MaterialItemId = modelInvoiceLine.MaterialItemId;
                    }
                    invoiceLine.JobNumber = modelInvoiceLine.JobNumber;
                    invoiceLine.Description = modelInvoiceLine.Description;
                    invoiceLine.FreightQuantity = modelInvoiceLine.FreightQuantity;
                    invoiceLine.MaterialQuantity = modelInvoiceLine.MaterialQuantity;
                    invoiceLine.MaterialRate = modelInvoiceLine.MaterialRate;
                    invoiceLine.FreightRate = modelInvoiceLine.FreightRate;
                    invoiceLine.IsFreightRateOverridden = modelInvoiceLine.IsFreightRateOverridden;
                    invoiceLine.MaterialExtendedAmount = modelInvoiceLine.MaterialExtendedAmount;
                    invoiceLine.FreightExtendedAmount = modelInvoiceLine.FreightExtendedAmount;
                    invoiceLine.FuelSurcharge = modelInvoiceLine.FuelSurcharge;
                    invoiceLine.Tax = modelInvoiceLine.Tax;
                    invoiceLine.IsFreightTaxable = modelInvoiceLine.IsFreightTaxable ?? false;
                    invoiceLine.IsMaterialTaxable = modelInvoiceLine.IsMaterialTaxable ?? false;
                    invoiceLine.Subtotal = modelInvoiceLine.Subtotal;
                    invoiceLine.ExtendedAmount = modelInvoiceLine.ExtendedAmount;
                    invoiceLine.ChildInvoiceLineKind = modelInvoiceLine.ChildInvoiceLineKind;
                    invoiceLine.ParentInvoiceLineId = modelInvoiceLine.ParentInvoiceLineId;

                    if (invoiceLine.ChargeId != null)
                    {
                        var charge = charges.FirstOrDefault(x => x.Id == invoiceLine.ChargeId);
                        if (charge != null)
                        {
                            var item = items.Find(charge.ItemId);
                            var chargeIsMaterial = CustomerChargeDto.IsChargeMaterial(item?.Type);

                            if (invoiceLine.DeliveryDateTime != null)
                            {
                                charge.ChargeDate = invoiceLine.DeliveryDateTime.Value.Date;
                            }
                            if (invoiceLine.FreightItemId != null)
                            {
                                charge.ItemId = invoiceLine.FreightItemId.Value;
                            }
                            charge.Description = invoiceLine.Description?.TruncateWithPostfix(EntityStringFieldLengths.Charge.Description);
                            charge.Quantity = modelInvoiceLine.UseMaterialQuantity == true
                                ? null
                                : invoiceLine.FreightQuantity;
                            charge.UseMaterialQuantity = modelInvoiceLine.UseMaterialQuantity == true;
                            charge.ChargeAmount = modelInvoiceLine.UseMaterialQuantity == true
                                ? 0
                                : chargeIsMaterial
                                    ? invoiceLine.MaterialExtendedAmount
                                    : invoiceLine.FreightExtendedAmount;
                            charge.Rate = chargeIsMaterial
                                ? invoiceLine.MaterialRate ?? 0
                                : invoiceLine.FreightRate ?? 0;

                            if (!charge.UseMaterialQuantity)
                            {
                                charge.IsBilled = true;
                            }
                        }
                    }
                }

                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (invoiceLine.Id > 0 && !model.InvoiceLines.Any(x => x.Id == invoiceLine.Id))
                    {
                        //invoice.InvoiceLines.Remove(invoiceLine);
                        await _invoiceLineRepository.DeleteAsync(invoiceLine);
                        if (invoiceLine.TicketId.HasValue)
                        {
                            removedTicketIds.Add(invoiceLine.TicketId.Value);
                        }
                        if (invoiceLine.ChargeId.HasValue)
                        {
                            removedChargeIds.Add(invoiceLine.ChargeId.Value);
                        }
                    }
                }

                if (removedTicketIds.Any())
                {
                    var tickets = await (await _ticketRepository.GetQueryAsync())
                        .Where(x => removedTicketIds.Contains(x.Id))
                        .ToListAsync();
                    tickets.ForEach(x => x.IsBilled = false);
                }
                if (removedChargeIds.Any())
                {
                    foreach (var removedCharge in charges.Where(x => removedChargeIds.Contains(x.Id)))
                    {
                        removedCharge.IsBilled = false;
                    }
                }
            }

            var ticketIdsToBill = invoice.InvoiceLines.Where(x => x.TicketId.HasValue).Select(x => x.TicketId.Value).Except(removedTicketIds).ToList();
            var ticketsToBill = await (await _ticketRepository.GetQueryAsync()).Where(x => ticketIdsToBill.Contains(x.Id)).ToListAsync();
            ticketsToBill.ForEach(t => t.IsBilled = true);

            if (invoice.Id == 0)
            {
                await _invoiceRepository.InsertAsync(invoice);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            if (model.InvoiceLines != null)
            {
                foreach (var childInvoiceLineModel in model.InvoiceLines.Where(x => x.ParentInvoiceLineGuid.HasValue))
                {
                    var parentInvoiceLineModel = model.InvoiceLines.FirstOrDefault(x => x.Guid == childInvoiceLineModel.ParentInvoiceLineGuid);
                    var parentEntity = newInvoiceLineEntities.FirstOrDefault(x => x.LineNumber == parentInvoiceLineModel?.LineNumber);
                    var childEntity = newInvoiceLineEntities.FirstOrDefault(x => x.LineNumber == childInvoiceLineModel.LineNumber);
                    if (parentEntity == null || childEntity == null)
                    {
                        continue;
                    }
                    childEntity.ParentInvoiceLineId = parentEntity.Id;
                }
            }

            await ReorderInvoiceLines(invoice);

            await CurrentUnitOfWork.SaveChangesAsync();

            if (invoice.Status != model.Status)
            {
                await UpdateInvoiceStatusInternal(invoice, new UpdateInvoiceStatusInput
                {
                    Id = invoice.Id,
                    Status = model.Status,
                });
            }

            return invoice.Id;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.Pages_Invoices_ApproveInvoices)]
        public async Task UpdateInvoiceStatus(UpdateInvoiceStatusInput input)
        {
            var invoice = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            await UpdateInvoiceStatusInternal(invoice, input);
        }

        private async Task UpdateInvoiceStatusInternal(Invoice invoice, UpdateInvoiceStatusInput input)
        {
            var validationInput = new ValidateInvoiceStatusChangeInput()
            {
                Ids = new[] { invoice.Id },
                Status = input.Status,
            };

            if (invoice.Status != input.Status)
            {
                switch (invoice.Status)
                {
                    case InvoiceStatus.Draft:
                    case InvoiceStatus.Printed:
                        if (input.Status.IsIn(InvoiceStatus.ReadyForExport, InvoiceStatus.Sent, InvoiceStatus.Approved))
                        {
                            await ValidateInvoiceStatusChange(validationInput);
                            invoice.Status = input.Status;
                        }
                        break;
                    case InvoiceStatus.ReadyForExport:
                        if (input.Status.IsIn(InvoiceStatus.Sent, InvoiceStatus.Approved))
                        {
                            await ValidateInvoiceStatusChange(validationInput);
                            invoice.Status = input.Status;
                        }
                        break;
                    case InvoiceStatus.Sent:
                    case InvoiceStatus.Viewed:
                        if (input.Status.IsIn(InvoiceStatus.Approved))
                        {
                            await ValidateInvoiceStatusChange(validationInput);
                            invoice.Status = input.Status;
                        }
                        break;
                    case InvoiceStatus.Approved:
                        if (input.Status.IsIn(InvoiceStatus.Sent, InvoiceStatus.ReadyForExport, InvoiceStatus.Printed))
                        {
                            await ValidateInvoiceStatusChange(validationInput);
                            invoice.Status = input.Status;
                        }
                        break;
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices, AppPermissions.Pages_Invoices_ApproveInvoices)]
        public async Task ValidateInvoiceStatusChange(ValidateInvoiceStatusChangeInput input)
        {
            var invoices = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    InvoiceLines = x.InvoiceLines.Select(l => new
                    {
                        l.TicketId,
                        l.ChargeId,
                        l.FreightQuantity,
                        l.MaterialQuantity,
                    }).ToList(),
                }).ToListAsync();

            foreach (var invoice in invoices)
            {
                if (invoice.Status == input.Status)
                {
                    continue;
                }

                if (invoice.Status == InvoiceStatus.Draft)
                {
                    if (invoice.InvoiceLines.Any(x => !x.TicketId.HasValue //has chargeId or has neither ticketId nor chargeId
                        && x.FreightQuantity is 0 or null
                        && x.MaterialQuantity is 0 or null
                    ))
                    {
                        throw new UserFriendlyException($"Invoice status cannot be changed from Draft until all charge quantities have been entered for invoice #{invoice.Id}.");
                    }
                }
            }
        }

        [RemoteService(false)]
        public async Task ReorderInvoiceLines(Invoice invoice)
        {
            await ReorderInvoiceLines(invoice, SettingManager);
        }

        [RemoteService(false)]
        public static async Task ReorderInvoiceLines(Invoice invoice, ISettingManager settingManager)
        {
            var fuelSurchargeItemId = await settingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            var showFuelSurchargeOnInvoice = await settingManager.ShowFuelSurchargeOnInvoice();
            ReorderInvoiceLines(invoice, fuelSurchargeItemId, showFuelSurchargeOnInvoice);
        }

        [RemoteService(false)]
        public static void ReorderInvoiceLines(Invoice invoice, int fuelSurchargeItemId, ShowFuelSurchargeOnInvoiceEnum showFuelSurchargeOnInvoice)
        {
            var invoiceLinesToReorder = invoice.InvoiceLines.OrderBy(x => x.LineNumber).ToList();
            var regularInvoiceLines = invoiceLinesToReorder.Where(x => x.ChildInvoiceLineKind == null).ToList();
            var bottomLines = invoiceLinesToReorder.Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.BottomFuelSurchargeLine).ToList();
            var perTicketLines = invoiceLinesToReorder.Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.FuelSurchargeLinePerTicket).ToList();

            var reorderedLines = regularInvoiceLines.ToList();
            if (fuelSurchargeItemId != 0 && showFuelSurchargeOnInvoice != ShowFuelSurchargeOnInvoiceEnum.LineItemPerTicket)
            {
                reorderedLines = reorderedLines.OrderByDescending(x => x.FreightItemId != fuelSurchargeItemId).ToList();
            }

            foreach (var perTicketLine in perTicketLines)
            {
                if (perTicketLine.ParentInvoiceLineId != null)
                {
                    var parentLine = reorderedLines.FirstOrDefault(x => x.Id == perTicketLine.ParentInvoiceLineId);
                    if (parentLine != null)
                    {
                        var parentLineIndex = reorderedLines.IndexOf(parentLine);
                        if (parentLineIndex != -1)
                        {
                            reorderedLines.Insert(parentLineIndex + 1, perTicketLine);
                            continue;
                        }
                    }
                }
                reorderedLines.Add(perTicketLine);
            }
            foreach (var bottomLine in bottomLines)
            {
                reorderedLines.Add(bottomLine);
            }

            short lineNumber = 1;
            foreach (var line in reorderedLines)
            {
                line.LineNumber = lineNumber++;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<CreateInvoicesForTicketsResult> CreateInvoicesForTickets(CreateInvoicesForTicketsInput input)
        {
            var tickets = await GetCustomerTickets(new GetCustomerTicketsInput
            {
                IsBilled = false,
                IsVerified = true,
                TicketIds = input.TicketIds,
                HasInvoiceLineId = false,
                HasRevenue = true,
            });

            if (!tickets.Items.Any())
            {
                return new CreateInvoicesForTicketsResult
                {
                    BatchId = null,
                };
            }

            var timezone = await GetTimezone();
            var ticketDates = tickets.Items
                .Select(x => x.OrderDeliveryDate ?? x.TicketDateTime?.ConvertTimeZoneTo(timezone).Date)
                .Distinct()
                .ToList();
            var earliestDate = ticketDates.Min();
            var latestDate = ticketDates.Max();
            var orderLineIds = tickets.Items
                .Where(x => x.OrderLineId.HasValue)
                .Select(x => x.OrderLineId)
                .Distinct()
                .ToList();

            var customerIds = tickets.Items.Select(x => x.CustomerId).Distinct().ToList();
            var customers = await (await _customerRepository.GetQueryAsync())
                .Where(x => customerIds.Contains(x.Id))
                .Select(x => new CustomerSelectListInfoDto
                {
                    CustomerId = x.Id,
                    InvoiceEmail = x.InvoiceEmail,
                    BillingAddress1 = x.BillingAddress1,
                    BillingAddress2 = x.BillingAddress2,
                    BillingCity = x.BillingCity,
                    BillingState = x.BillingState,
                    BillingZipCode = x.BillingZipCode,
                    Terms = x.Terms,
                    InvoicingMethod = x.InvoicingMethod,
                }).ToListAsync();

            if (!customers.Any())
            {
                return new CreateInvoicesForTicketsResult
                {
                    BatchId = null,
                };
            }

            var charges = orderLineIds.Count > 0
                ? await GetCustomerCharges(new GetCustomerChargesInput
                {
                    IsBilled = false,
                    OrderLineIds = orderLineIds,
                    CustomerIds = customerIds,
                    HasInvoiceLineId = false,
                })
                : new PagedResultDto<CustomerChargeDto>(0, new List<CustomerChargeDto>());
            var processedChargeIds = new List<int>();

            var invoiceBatch = new InvoiceBatch { TenantId = await AbpSession.GetTenantIdAsync() };
            await _invoiceBatchRepository.InsertAndGetIdAsync(invoiceBatch);

            var today = await GetToday();
            var defaultMessage = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.DefaultMessageOnInvoice);
            var showFuelSurchargeOnInvoice = await SettingManager.ShowFuelSurchargeOnInvoice();
            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            var fuelItemId = await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            var fuelItem = fuelItemId > 0 ? await (await _itemRepository.GetQueryAsync())
                .Where(x => x.Id == fuelItemId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.IsTaxable,
                })
                .FirstOrDefaultAsync() : null;

            foreach (var customer in customers)
            {
                var customerTicketsGroups = tickets.Items
                    .Where(x => x.CustomerId == customer.CustomerId)
                    .GroupBy(x => new
                    {
                        SaleTaxRate = x.SalesTaxRate ?? 0,
                        x.SalesTaxEntityId,
                    });
                foreach (var customerTickets in customerTicketsGroups)
                {
                    var taxRate = customerTickets.Key.SaleTaxRate;
                    var salesTaxEntityId = customerTickets.Key.SalesTaxEntityId;
                    var customerCharges = charges.Items
                        .Where(x => x.CustomerId == customer.CustomerId
                            && x.SalesTaxRate == taxRate
                            && x.SalesTaxEntityId == salesTaxEntityId)
                        .ToList();

                    switch (customer.InvoicingMethod)
                    {
                        case InvoicingMethodEnum.AggregateAllTickets:
                            await AddInvoiceFromCustomerTickets(
                                customerTickets.ToList(),
                                customerCharges,
                                customer, taxRate, salesTaxEntityId);
                            break;

                        case InvoicingMethodEnum.SeparateTicketsByJobNumber:
                            var ticketsByJobNumber = customerTickets.GroupBy(x => x.JobNumber);
                            foreach (var ticketSubGroup in ticketsByJobNumber)
                            {
                                var orderLineIdsForGroup = ticketSubGroup.Select(t => t.OrderLineId).Distinct().ToList();
                                await AddInvoiceFromCustomerTickets(
                                    ticketSubGroup.ToList(),
                                    customerCharges.Where(x => orderLineIdsForGroup.Contains(x.OrderLineId)).ToList(),
                                    customer, taxRate, salesTaxEntityId);
                            }
                            break;

                        case InvoicingMethodEnum.SeparateTicketsByJob:
                            var ticketsByJob = customerTickets.GroupBy(x => x.OrderLineId);
                            foreach (var ticketSubGroup in ticketsByJob)
                            {
                                await AddInvoiceFromCustomerTickets(
                                    ticketSubGroup.ToList(),
                                    customerCharges.Where(x => x.OrderLineId == ticketSubGroup.Key).ToList(),
                                    customer, taxRate, salesTaxEntityId);
                            }
                            break;

                        case InvoicingMethodEnum.SeparateInvoicePerTicket:
                            foreach (var customerTicket in customerTickets)
                            {
                                await AddInvoiceFromCustomerTickets(
                                    new[] { customerTicket },
                                    customerCharges.Where(x => x.OrderLineId == customerTicket.OrderLineId).ToList(),
                                    customer, taxRate, salesTaxEntityId);
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    var remainingCustomerCharges = customerCharges.Where(x => !processedChargeIds.Contains(x.Id)).ToList();
                    if (remainingCustomerCharges.Any())
                    {
                        switch (customer.InvoicingMethod)
                        {
                            case InvoicingMethodEnum.AggregateAllTickets:
                            case InvoicingMethodEnum.SeparateInvoicePerTicket:
                                await AddInvoiceFromCustomerTickets(
                                    Array.Empty<CustomerTicketDto>(),
                                    remainingCustomerCharges,
                                    customer, taxRate, salesTaxEntityId);
                                break;

                            case InvoicingMethodEnum.SeparateTicketsByJobNumber:
                                var chargesByJobNumber = remainingCustomerCharges.GroupBy(x => x.JobNumber);
                                foreach (var chargeSubGroup in chargesByJobNumber)
                                {
                                    await AddInvoiceFromCustomerTickets(
                                        Array.Empty<CustomerTicketDto>(),
                                        chargeSubGroup.ToList(),
                                        customer, taxRate, salesTaxEntityId);
                                }
                                break;

                            case InvoicingMethodEnum.SeparateTicketsByJob:
                                var chargesByJob = remainingCustomerCharges.GroupBy(x => x.OrderLineId);
                                foreach (var chargeSubGroup in chargesByJob)
                                {
                                    await AddInvoiceFromCustomerTickets(
                                        Array.Empty<CustomerTicketDto>(),
                                        chargeSubGroup.ToList(),
                                        customer, taxRate, salesTaxEntityId);
                                }
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            var ticketIds = tickets.Items.Select(x => x.Id).ToList();
            var ticketsToBill = await (await _ticketRepository.GetQueryAsync()).Where(x => ticketIds.Contains(x.Id)).ToListAsync();
            ticketsToBill.ForEach(t => t.IsBilled = true);

            var chargeIds = charges.Items.Where(x => !x.UseMaterialQuantity).Select(x => x.Id).ToList();
            var chargesToBill = await (await _chargeRepository.GetQueryAsync()).Where(x => chargeIds.Contains(x.Id)).ToListAsync();
            chargesToBill.ForEach(c => c.IsBilled = true);

            return new CreateInvoicesForTicketsResult
            {
                BatchId = invoiceBatch.Id,
            };

            async Task AddInvoiceFromCustomerTickets(
                IReadOnlyCollection<CustomerTicketDto> customerTickets,
                IReadOnlyCollection<CustomerChargeDto> customerCharges,
                CustomerSelectListInfoDto customer, decimal taxRate, int? salesTaxEntityId)
            {

                var dueDate = CalculateDueDate(new CalculateDueDateInput
                {
                    IssueDate = today,
                    Terms = customer.Terms,
                });
                var invoice = new Invoice
                {
                    TenantId = await AbpSession.GetTenantIdAsync(),
                    BatchId = invoiceBatch.Id,
                    EmailAddress = customer.InvoiceEmail,
                    DueDate = dueDate,
                    IssueDate = today,
                    BillingAddress = customer.FullAddress,
                    CustomerId = customer.CustomerId,
                    OfficeId = input.OfficeId ?? OfficeId,
                    Terms = customer.Terms,
                    Status = InvoiceStatus.Draft,
                    TaxRate = taxRate,
                    SalesTaxEntityId = salesTaxEntityId,
                    Message = defaultMessage,
                    ShowFuelSurchargeOnInvoice = showFuelSurchargeOnInvoice,
                };
                await _invoiceRepository.InsertAsync(invoice);

                var jobNumbers = customerTickets.Select(x => x.JobNumber)
                    .Union(customerCharges.Select(x => x.JobNumber))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                if (jobNumbers.Any())
                {
                    invoice.JobNumber = string.Join("; ", jobNumbers).TruncateWithPostfix(EntityStringFieldLengths.Invoice.JobNumber, "…");
                }

                var poNumbers = customerTickets.Select(x => x.PoNumber)
                    .Union(customerCharges.Select(x => x.PoNumber))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                if (poNumbers.Any())
                {
                    invoice.PoNumber = string.Join("; ", poNumbers).TruncateWithPostfix(EntityStringFieldLengths.Invoice.PoNumber, "…");
                }

                customerTickets = customerTickets
                    .OrderBy(x => x.TicketDateTime?.ConvertTimeZoneTo(timezone).Date)
                    .ThenBy(x => x.TruckCode)
                    .ThenBy(x => x.TicketNumber)
                    .ToList();

                short lineNumber = 1;
                decimal totalFuelSurcharge = 0;
                foreach (var ticket in customerTickets)
                {
                    var invoiceLine = new InvoiceLine
                    {
                        LineNumber = lineNumber++,
                        TicketId = ticket.Id,
                        DeliveryDateTime = ticket.TicketDateTime,
                        CarrierId = ticket.CarrierId,
                        TruckCode = ticket.TruckCode,
                        Description = ticket.Description,
                        FreightItemId = ticket.FreightItemId,
                        MaterialItemId = ticket.MaterialItemId,
                        JobNumber = ticket.JobNumber,
                        FreightQuantity = ticket.FreightQuantity,
                        MaterialQuantity = ticket.MaterialQuantity,
                        FreightRate = ticket.FreightRate,
                        MaterialRate = ticket.MaterialRate,
                        MaterialExtendedAmount = ticket.MaterialTotal,
                        FreightExtendedAmount = ticket.FreightTotal,
                        FuelSurcharge = ticket.FuelSurcharge,
                        Tax = ticket.Tax,
                        IsFreightTaxable = ticket.IsFreightTaxable ?? false,
                        IsMaterialTaxable = ticket.IsMaterialTaxable ?? false,
                        Subtotal = ticket.Subtotal,
                        ExtendedAmount = ticket.Total,
                        Invoice = invoice,
                        TenantId = await AbpSession.GetTenantIdAsync(),
                    };
                    await _invoiceLineRepository.InsertAsync(invoiceLine);
                    invoice.TotalAmount += invoiceLine.ExtendedAmount;
                    invoice.Tax += invoiceLine.Tax;

                    if (showFuelSurchargeOnInvoice == ShowFuelSurchargeOnInvoiceEnum.SingleLineItemAtTheBottom)
                    {
                        totalFuelSurcharge += ticket.FuelSurcharge;
                    }
                    else if (showFuelSurchargeOnInvoice == ShowFuelSurchargeOnInvoiceEnum.LineItemPerTicket)
                    {
                        if (ticket.FuelSurcharge != 0)
                        {
                            if (fuelItem == null)
                            {
                                throw new UserFriendlyException(L("PleaseSelectItemToUseForFuelSurchargeOnInvoiceInSettings"));
                            }
                            var fuelLine = new InvoiceLine
                            {
                                LineNumber = lineNumber++,
                                DeliveryDateTime = ticket.TicketDateTime,
                                Description = fuelItem.Name,
                                FreightItemId = fuelItem.Id,
                                MaterialItemId = null,
                                FreightQuantity = 1,
                                MaterialQuantity = 0,
                                FreightRate = ticket.FuelSurcharge,
                                FreightExtendedAmount = ticket.FuelSurcharge,
                                FuelSurcharge = 0,
                                IsFreightTaxable = fuelItem.IsTaxable,
                                IsMaterialTaxable = false,
                                Invoice = invoice,
                                TenantId = await AbpSession.GetTenantIdAsync(),
                                ChildInvoiceLineKind = ChildInvoiceLineKind.FuelSurchargeLinePerTicket,
                                ParentInvoiceLine = invoiceLine,
                            };
                            CalculateInvoiceLineTotals(fuelLine, fuelItem.IsTaxable, invoice.TaxRate, taxCalculationType, separateItems);
                            await _invoiceLineRepository.InsertAsync(fuelLine);
                            invoice.TotalAmount += fuelLine.ExtendedAmount;
                            invoice.Tax += fuelLine.Tax;
                        }
                    }
                }

                customerCharges = customerCharges
                    .OrderBy(x => x.ChargeDate)
                    .ToList();

                foreach (var charge in customerCharges.Where(x => !processedChargeIds.Contains(x.Id)))
                {
                    var chargeQuantity = charge.UseMaterialQuantity
                        ? customerTickets.Where(x => x.OrderLineId == charge.OrderLineId)
                            .Sum(x => x.MaterialQuantity ?? 0)
                        : charge.Quantity;
                    if (charge.UseMaterialQuantity)
                    {
                        charge.Quantity = chargeQuantity;
                        charge.ChargeAmount = (charge.Quantity * charge.Rate) ?? 0;
                    }

                    var invoiceLine = new InvoiceLine
                    {
                        LineNumber = lineNumber++,
                        ChargeId = charge.Id,
                        DeliveryDateTime = charge.ChargeDate.ConvertTimeZoneFrom(timezone),
                        Description = charge.Description,
                        FreightItemId = charge.ItemId,
                        MaterialItemId = null,
                        JobNumber = charge.JobNumber,
                        FreightQuantity = chargeQuantity,
                        MaterialQuantity = chargeQuantity,
                        FreightRate = charge.FreightRate,
                        MaterialRate = charge.MaterialRate,
                        MaterialExtendedAmount = charge.MaterialTotal,
                        FreightExtendedAmount = charge.FreightTotal,
                        FuelSurcharge = charge.FuelSurcharge,
                        Tax = charge.Tax,
                        IsFreightTaxable = charge.IsTaxable,
                        IsMaterialTaxable = false,
                        Subtotal = charge.Subtotal,
                        ExtendedAmount = charge.Total,
                        Invoice = invoice,
                        TenantId = await AbpSession.GetTenantIdAsync(),
                    };
                    await _invoiceLineRepository.InsertAsync(invoiceLine);
                    invoice.TotalAmount += invoiceLine.ExtendedAmount;
                    invoice.Tax += invoiceLine.Tax;

                    processedChargeIds.Add(charge.Id);
                }

                if (showFuelSurchargeOnInvoice == ShowFuelSurchargeOnInvoiceEnum.SingleLineItemAtTheBottom
                    && totalFuelSurcharge != 0)
                {
                    if (fuelItem == null)
                    {
                        throw new UserFriendlyException(L("PleaseSelectItemToUseForFuelSurchargeOnInvoiceInSettings"));
                    }
                    var fuelLine = new InvoiceLine
                    {
                        LineNumber = lineNumber++,
                        DeliveryDateTime = null,
                        Description = fuelItem.Name,
                        FreightItemId = fuelItem.Id,
                        MaterialItemId = null,
                        FreightQuantity = 1,
                        MaterialQuantity = 0,
                        FreightRate = totalFuelSurcharge,
                        FreightExtendedAmount = totalFuelSurcharge,
                        FuelSurcharge = 0,
                        IsFreightTaxable = fuelItem.IsTaxable,
                        IsMaterialTaxable = false,
                        Invoice = invoice,
                        TenantId = await AbpSession.GetTenantIdAsync(),
                        ChildInvoiceLineKind = ChildInvoiceLineKind.BottomFuelSurchargeLine,
                    };
                    CalculateInvoiceLineTotals(fuelLine, fuelItem.IsTaxable, invoice.TaxRate, taxCalculationType, separateItems);
                    await _invoiceLineRepository.InsertAsync(fuelLine);
                    invoice.TotalAmount += fuelLine.ExtendedAmount;
                    invoice.Tax += fuelLine.Tax;
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public DateTime CalculateDueDate(CalculateDueDateInput input)
        {
            switch (input.Terms)
            {
                case BillingTermsEnum.DueOnReceipt: return input.IssueDate;
                case BillingTermsEnum.DueByTheFirstOfTheMonth: return input.IssueDate.AddMonths(1).AddDays(-(input.IssueDate.Day - 1));
                case BillingTermsEnum.Net10: return input.IssueDate.AddDays(10);
                case BillingTermsEnum.Net15: return input.IssueDate.AddDays(15);
                case BillingTermsEnum.Net30: return input.IssueDate.AddDays(30);
                case BillingTermsEnum.Net60: return input.IssueDate.AddDays(60);
                case BillingTermsEnum.Net5: return input.IssueDate.AddDays(5);
                case BillingTermsEnum.Net14: return input.IssueDate.AddDays(14);
                default: return input.IssueDate;
            }
        }

        private void CalculateInvoiceLineTotals(InvoiceLine invoiceLine, bool serviceIsTaxable, decimal taxRate, TaxCalculationType taxCalculationType, bool separateItems)
        {
            var lineDto = new OrderLineTaxTotalDetailsDto
            {
                FreightPrice = invoiceLine.FreightRate ?? 0,
                MaterialPrice = invoiceLine.MaterialRate ?? 0,
                IsTaxable = serviceIsTaxable,
                IsFreightTaxable = serviceIsTaxable,
                IsMaterialTaxable = serviceIsTaxable,
                Subtotal = invoiceLine.Subtotal,
                Tax = invoiceLine.Tax,
                TotalAmount = invoiceLine.ExtendedAmount,
            };
            OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, lineDto, taxRate, separateItems);
            invoiceLine.Subtotal = lineDto.Subtotal;
            invoiceLine.ExtendedAmount = lineDto.TotalAmount;
            invoiceLine.Tax = lineDto.Tax;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task DeleteInvoice(EntityDto input)
        {
            var invoice = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.Status,
                }).FirstOrDefaultAsync();

            //if (invoice != null && invoice.Status != InvoiceStatus.Draft)
            //{
            //    throw new UserFriendlyException(L("InvoiceDeleteErrorNotDraft"));
            //}

            var tickets = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.InvoiceLine.InvoiceId == input.Id && x.IsBilled)
                .ToListAsync();

            foreach (var ticket in tickets)
            {
                ticket.IsBilled = false;
            }

            var charges = await (await _chargeRepository.GetQueryAsync())
                .Where(x => x.InvoiceLines.Any(l => l.InvoiceId == input.Id) && x.IsBilled && !x.UseMaterialQuantity)
                .ToListAsync();

            foreach (var charge in charges)
            {
                charge.IsBilled = false;
            }

            await _invoiceLineRepository.DeleteAsync(x => x.InvoiceId == input.Id);
            await _invoiceRepository.DeleteAsync(input.Id);
            await DeleteTrackableInvoiceEmails(input.Id);
        }

        private async Task DeleteTrackableInvoiceEmails(int invoiceId)
        {
            var invoiceEmails = await (await _invoiceEmailRepository.GetQueryAsync())
                .Include(x => x.Email)
                .ThenInclude(x => x.Events)
                .Include(x => x.Email)
                .ThenInclude(x => x.Receivers)
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync();

            foreach (var invoiceEmail in invoiceEmails)
            {
                if (invoiceEmail.Email != null)
                {
                    foreach (var emailEvent in invoiceEmail.Email.Events)
                    {
                        await _trackableEmailEventRepository.DeleteAsync(emailEvent);
                    }

                    foreach (var emailReceiver in invoiceEmail.Email.Receivers)
                    {
                        await _trackableEmailReceiverRepository.DeleteAsync(emailReceiver);
                    }

                    await _trackableEmailRepository.DeleteAsync(invoiceEmail.Email);
                }

                await _invoiceEmailRepository.DeleteAsync(invoiceEmail);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task UndoInvoiceExport(EntityDto input)
        {
            var invoice = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            invoice.UploadBatchId = null;
            invoice.QuickbooksExportDateTime = null;
            invoice.QuickbooksInvoiceId = null;
            await ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
            {
                Ids = new[] { invoice.Id },
                Status = InvoiceStatus.ReadyForExport,
            });
            invoice.Status = InvoiceStatus.ReadyForExport;
        }


        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices)]
        public async Task<Document> GetInvoicePrintOut(GetInvoicePrintOutInput input)
        {
            var permissions = new
            {
                ApproveInvoices = await IsGrantedAsync(AppPermissions.Pages_Invoices_ApproveInvoices),
            };

            var allowInvoiceApprovalFlow = await FeatureChecker.IsEnabledAsync(AppFeatures.AllowInvoiceApprovalFlow)
                && await SettingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.AllowInvoiceApprovalFlow);

            var invoice = await _invoiceRepository.GetAsync(input.InvoiceId.Value);

            await CheckInvoicePermissions(invoice.CustomerId);

            if (allowInvoiceApprovalFlow && (permissions.ApproveInvoices || invoice.Status == InvoiceStatus.Approved)
                || !allowInvoiceApprovalFlow && invoice.Status == InvoiceStatus.Draft)
            {
                await ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
                {
                    Ids = new[] { invoice.Id },
                    Status = InvoiceStatus.Printed,
                });
                invoice.Status = InvoiceStatus.Printed;
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var data = await GetInvoicePrintOutData(input);

            return await _invoicePrintOutGenerator.GenerateReport(data);
        }

        private async Task<List<InvoicePrintOutDto>> GetInvoicePrintOutData(GetInvoicePrintOutInput input)
        {
            var currentInvoicePrintOutTemplate = (InvoiceTemplateEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.InvoiceTemplate);
            var alwaysShowFreightAndMaterialOnSeparateLines = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInPrintedInvoices);
            var items = await (await _invoiceRepository.GetQueryAsync())
                .WhereIf(input.InvoiceId.HasValue, x => x.Id == input.InvoiceId)
                .WhereIf(input.InvoiceIds?.Any() == true, x => input.InvoiceIds.Contains(x.Id))
                .Select(x => new InvoicePrintOutDto
                {
                    InvoiceId = x.Id,
                    BillingAddress = x.BillingAddress,
                    CustomerName = x.Customer.Name,
                    Terms = x.Terms ?? x.Customer.Terms,
                    CombineTickets = x.Customer.CombineTickets,
                    OfficeId = x.OfficeId,
                    JobNumber = x.JobNumber,
                    PoNumber = x.PoNumber,
                    Description = x.Description,
                    DueDate = x.DueDate,
                    IssueDate = x.IssueDate,
                    Message = x.Message,
                    TaxRate = x.TaxRate,
                    Tax = x.Tax,
                    TotalAmount = x.TotalAmount,
                    InvoiceLines = x.InvoiceLines.Select(l => new InvoicePrintOutLineItemDto
                    {
                        Id = l.Id,
                        DeliveryDateTime = l.DeliveryDateTime,
                        Description = l.Description,
                        Quantity = 0,
                        MaterialQuantity = l.MaterialQuantity,
                        FreightQuantity = l.FreightQuantity,
                        FreightRate = l.FreightRate,
                        MaterialRate = l.MaterialRate,
                        ItemId = l.FreightItemId,
                        ItemName = l.FreightItem.Name,
                        MaterialItemId = l.MaterialItemId,
                        MaterialItemName = l.MaterialItem.Name,
                        JobNumber = l.JobNumber,
                        PoNumber = l.Ticket.OrderLine.Order.PONumber,
                        Subtotal = l.Subtotal,
                        ExtendedAmount = l.ExtendedAmount,
                        FreightExtendedAmount = l.FreightExtendedAmount,
                        MaterialExtendedAmount = l.MaterialExtendedAmount,
                        Tax = l.Tax,
                        LeaseHaulerName = l.Ticket.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                        LineNumber = l.LineNumber,
                        TicketNumber = l.Ticket.TicketNumber,
                        TicketId = l.TicketId,
                        ChargeId = l.ChargeId,
                        TruckCode = l.TruckCode,
                        ParentInvoiceLineId = l.ParentInvoiceLineId,
                        ChildInvoiceLineKind = l.ChildInvoiceLineKind,
                        DeliverToStreetAddress = l.Ticket.DeliverTo.StreetAddress,
                        DeliverToName = l.Ticket.DeliverTo.Name,
                        DeliverToDisplayName = l.Ticket.DeliverTo.DisplayName,
                        LoadAtName = l.Ticket.LoadAt.Name,
                        LoadAtDisplayName = l.Ticket.LoadAt.DisplayName,
                        FuelSurcharge = l.FuelSurcharge,
                    }).ToList(),
                }).ToListAsync();

            foreach (var item in items)
            {
                var timezone = await GetTimezone();
                item.InvoiceLines = item.InvoiceLines.OrderBy(x => x.LineNumber ?? 0).ToList();
                item.InvoiceLines.ForEach(x => x.DeliveryDateTime = x.DeliveryDateTime?.ConvertTimeZoneTo(timezone));

                item.InvoiceLines.RemoveAll(l =>
                        l.ChildInvoiceLineKind.IsIn(ChildInvoiceLineKind.BottomFuelSurchargeLine, ChildInvoiceLineKind.FuelSurchargeLinePerTicket)
                        && l.ExtendedAmount == 0);

                if (currentInvoicePrintOutTemplate == InvoiceTemplateEnum.MinimalDescription)
                {
                    var fuelSurchargeLines = item.InvoiceLines
                        .Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.BottomFuelSurchargeLine
                            || x.ChildInvoiceLineKind == ChildInvoiceLineKind.FuelSurchargeLinePerTicket)
                        .ToList();
                    fuelSurchargeLines.ForEach(x => x.JobNumber = x.ItemName);

                    if (item.CombineTickets)
                    {
                        var ticketsGroups = item.InvoiceLines
                            .Where(x => x.TicketId.HasValue)
                            .GroupBy(x => new
                            {
                                DeliveryDate = x.DeliveryDateTime?.Date,
                                x.TruckCode,
                                x.JobNumber,
                            });
                        foreach (var ticketGroup in ticketsGroups)
                        {
                            var ticketLines = ticketGroup.ToList();
                            if (ticketLines.Count > 1)
                            {
                                var combinedLine = ticketLines.First();
                                combinedLine.Quantity = ticketLines.Sum(x => x.Quantity);
                                combinedLine.FreightQuantity = ticketLines.Sum(x => x.FreightQuantity);
                                combinedLine.MaterialQuantity = ticketLines.Sum(x => x.MaterialQuantity);
                                combinedLine.Subtotal = ticketLines.Sum(x => x.Subtotal);
                                var linesToDelete = ticketLines.Except(new[] { combinedLine });
                                item.InvoiceLines.RemoveAll(l => linesToDelete.Contains(l));
                            }
                        }
                    }
                }

                if (currentInvoicePrintOutTemplate == InvoiceTemplateEnum.Invoice8
                    || currentInvoicePrintOutTemplate == InvoiceTemplateEnum.Invoice9)
                {
                    item.InvoiceLines.RemoveAll(l => l.ChildInvoiceLineKind.IsIn(ChildInvoiceLineKind.FuelSurchargeLinePerTicket, ChildInvoiceLineKind.BottomFuelSurchargeLine));
                }

                var quickbooksIntegrationKind = (QuickbooksIntegrationKind)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.Quickbooks.IntegrationKind);
                var invoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);

                if (quickbooksIntegrationKind == QuickbooksIntegrationKind.Desktop
                    || quickbooksIntegrationKind == QuickbooksIntegrationKind.QboExport
                    || quickbooksIntegrationKind == QuickbooksIntegrationKind.TransactionProExport)
                {
                    item.NumberPrefix = invoiceNumberPrefix;
                }

                item.LegalName = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingLegalName);
                item.LegalAddress = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingAddress);
                item.LegalPhoneNumber = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingPhoneNumber);
                item.RemitToInformation = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.RemitToInformation);

                item.TimeZone = await GetTimezone();
                item.CurrencyCulture = await SettingManager.GetCurrencyCultureAsync();

                item.CompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
                item.TermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.TermsAndConditions);
                item.TermsAndConditions = item.TermsAndConditions
                    .Replace("{CompanyName}", item.CompanyName)
                    .Replace("{CompanyNameUpperCase}", item.CompanyName.ToUpper());

                item.DebugLayout = input.DebugLayout;
                item.DebugInput = input;
            }

            foreach (var itemGroup in items.GroupBy(x => x.OfficeId))
            {
                var officeId = itemGroup.Key;
                var logoBytes = await _logoProvider.GetReportLogoAsBytesAsync(officeId);
                foreach (var item in itemGroup)
                {
                    item.LogoBytes = logoBytes;
                }
            }

            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            items.SplitMaterialAndFreightLines(separateItems, alwaysShowFreightAndMaterialOnSeparateLines);

            return items;
        }

        private async Task<EmailInvoicePrintOutBaseDto> GetEmailInvoicePrintOutBaseModel()
        {
            var user = await (await UserManager.GetQueryAsync())
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                })
                .FirstAsync();

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = ReplaceEmailSubjectTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailSubjectTemplate), companyName);

            var body = ReplaceEmailBodyTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailBodyTemplate), user.FirstName, user.LastName);

            var ccMeOnInvoices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.CCMeOnInvoices);

            return new EmailInvoicePrintOutBaseDto
            {
                From = user.Email,
                CC = ccMeOnInvoices ? user.Email : null,
                Subject = subject,
                Body = body,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<EmailInvoicePrintOutDto> GetEmailInvoicePrintOutModel(EntityDto input)
        {
            var invoice = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.EmailAddress,
                })
                .FirstAsync();

            var model = await GetEmailInvoicePrintOutBaseModel();
            return model.CopyTo(new EmailInvoicePrintOutDto
            {
                InvoiceId = input.Id,
                To = invoice.EmailAddress,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<EmailApprovedInvoicesInput> GetEmailOrPrintApprovedInvoicesModalModel()
        {
            var model = await GetEmailInvoicePrintOutBaseModel();
            return model.CopyTo(new EmailApprovedInvoicesInput());
        }

        public static string ReplaceEmailSubjectTemplateTokens(string subjectTemplate, string companyName)
        {
            return subjectTemplate
                .Replace("{CompanyName}", companyName);
        }

        public static string ReplaceEmailBodyTemplateTokens(string bodyTemplate, string userFirstName, string userLastName)
        {
            return bodyTemplate
                .Replace("{User.FirstName}", userFirstName)
                .Replace("{User.LastName}", userLastName);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<EmailInvoicePrintOutResult> EmailInvoicePrintOut(EmailInvoicePrintOutDto input)
        {
            await ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
            {
                Ids = new[] { input.InvoiceId },
                Status = InvoiceStatus.Sent,
            });
            var report = await GetInvoicePrintOut(new GetInvoicePrintOutInput
            {
                InvoiceId = input.InvoiceId,
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

            var filename = "Invoice";

            filename = Utilities.RemoveInvalidFileNameChars(filename);
            filename += ".pdf";

            using (var stream = new MemoryStream())
            {
                report.SaveToMemoryStream(stream);
                message.Attachments.Add(new Attachment(stream, filename));

                try
                {
                    var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                    await _invoiceEmailRepository.InsertAsync(new InvoiceEmail
                    {
                        EmailId = trackableEmailId,
                        InvoiceId = input.InvoiceId,
                    });

                    var invoice = await _invoiceRepository.GetAsync(input.InvoiceId);
                    if (invoice.Status.IsIn(InvoiceStatus.Draft, InvoiceStatus.ReadyForExport, InvoiceStatus.Printed))
                    {
                        invoice.Status = InvoiceStatus.Sent;
                    }
                }
                catch (SmtpException ex)
                {
                    if (ex.Message.Contains("The from address does not match a verified Sender Identity"))
                    {
                        return new EmailInvoicePrintOutResult
                        {
                            FromEmailAddressIsNotVerifiedError = true,
                        };
                    }
                    else
                    {
                        throw;
                    }
                }

                return new EmailInvoicePrintOutResult
                {
                    Success = true,
                };
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<bool> HasDraftInvoices(PrintDraftInvoicesInput input)
        {
            var count = await (await GetDraftInvoicesQueryAsync(input)).CountAsync();

            await CheckInvoicePrintLimit(count);

            return count > 0;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<HasApprovedInvoicesResult> HasApprovedInvoices()
        {
            return new HasApprovedInvoicesResult
            {
                HasApprovedInvoicesToPrint = await (await GetApprovedInvoicesToPrintQueryAsync()).AnyAsync(),
                HasApprovedInvoicesToEmail = await (await GetApprovedInvoicesToEmailQueryAsync()).AnyAsync(),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task EnqueueEmailApprovedInvoicesJob(EmailApprovedInvoicesInput input)
        {
            await _backgroundJobManager.EnqueueAsync<EmailApprovedInvoicesBackgroundJob, EmailApprovedInvoicesBackgroundJobArgs>(new EmailApprovedInvoicesBackgroundJobArgs
            {
                TenantId = await Session.GetTenantIdAsync(),
                RequestorUser = await Session.ToUserIdentifierAsync(),
                Input = input,
            });
        }

        private async Task<IQueryable<Invoice>> GetApprovedInvoicesToEmailQueryAsync()
        {
            return (await _invoiceRepository.GetQueryAsync())
                .Where(i => i.Status == InvoiceStatus.Approved
                    && i.Customer.PreferredDeliveryMethod == PreferredBillingDeliveryMethodEnum.Email
                    && !string.IsNullOrWhiteSpace(i.EmailAddress)
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<EmailInvoicePrintOutResult> EmailApprovedInvoices(EmailApprovedInvoicesInput input)
        {
            var invoices = await (await GetApprovedInvoicesToEmailQueryAsync())
                .ToListAsync();

            await ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
            {
                Ids = invoices.Select(x => x.Id).ToArray(),
                Status = InvoiceStatus.Sent,
            });

            var someEmailsWereNotSentError = false;
            foreach (var invoice in invoices)
            {
                var emailInvoicePrintoutDto = input.CopyTo(new EmailInvoicePrintOutDto
                {
                    InvoiceId = invoice.Id,
                    To = invoice.EmailAddress,
                });

                try
                {
                    var emailResult = await EmailInvoicePrintOut(emailInvoicePrintoutDto);
                    if (emailResult.FromEmailAddressIsNotVerifiedError)
                    {
                        return new EmailInvoicePrintOutResult
                        {
                            FromEmailAddressIsNotVerifiedError = true,
                        };
                    }
                    invoice.Status = InvoiceStatus.Sent;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error during EmailApprovedInvoices", ex);
                    someEmailsWereNotSentError = true;
                }
            }

            if (someEmailsWereNotSentError)
            {
                return new EmailInvoicePrintOutResult
                {
                    SomeEmailsWereNotSentError = true,
                };
            }

            return new EmailInvoicePrintOutResult
            {
                Success = true,
            };
        }

        private async Task<IQueryable<Invoice>> GetApprovedInvoicesToPrintQueryAsync()
        {
            return (await _invoiceRepository.GetQueryAsync())
                .Where(i => i.Status == InvoiceStatus.Approved
                    && i.Customer.PreferredDeliveryMethod == PreferredBillingDeliveryMethodEnum.Print
                );
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task EnsureCanPrintApprovedInvoices()
        {
            var invoiceIds = await (await GetApprovedInvoicesToPrintQueryAsync())
                .Select(x => x.Id)
                .ToArrayAsync();

            await ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
            {
                Ids = invoiceIds,
                Status = InvoiceStatus.Sent,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<Document> PrintApprovedInvoices()
        {
            var invoices = await (await GetApprovedInvoicesToPrintQueryAsync())
                .ToListAsync();

            var invoiceIds = invoices.Select(x => x.Id).ToArray();
            await ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
            {
                Ids = invoiceIds,
                Status = InvoiceStatus.Sent,
            });

            if (!invoices.Any())
            {
                return null;
            }

            foreach (var invoice in invoices)
            {
                invoice.Status = InvoiceStatus.Sent;
            }

            var data = await GetInvoicePrintOutData(new GetInvoicePrintOutInput { InvoiceIds = invoiceIds });
            return await _invoicePrintOutGenerator.GenerateReport(data);
        }

        private async Task CheckInvoicePrintLimit(int invoiceCount)
        {
            var invoicePrintLimit = await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.InvoicePrintLimit);
            if (invoiceCount > invoicePrintLimit)
            {
                throw new UserFriendlyException(L("InvoicePrintLimitExceededError", invoicePrintLimit));
            }
        }

        private async Task<IQueryable<Invoice>> GetDraftInvoicesQueryAsync(PrintDraftInvoicesInput input)
        {
            var query = (await _invoiceRepository.GetQueryAsync())
                .Where(i => i.Status == InvoiceStatus.Draft);

            query = FilterInvoiceQuery(query, input);

            return query;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices, AppPermissions.Pages_Invoices_ApproveInvoices)]
        public async Task<Document> PrintDraftInvoices(PrintDraftInvoicesInput input)
        {
            var invoices = await (await GetDraftInvoicesQueryAsync(input))
                .Select(x => new
                {
                    x.Id,
                }).ToListAsync();

            if (!invoices.Any())
            {
                return null;
            }

            await CheckInvoicePrintLimit(invoices.Count);

            var invoiceIds = invoices.Select(x => x.Id).ToArray();
            var data = await GetInvoicePrintOutData(new GetInvoicePrintOutInput { InvoiceIds = invoiceIds });
            return await _invoicePrintOutGenerator.GenerateReport(data);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task SendInvoiceTicketsToCustomerTenant(EntityDto input)
        {
            await _crossTenantOrderSender.SendInvoiceTicketsToCustomerTenant(input);
        }
    }
}
