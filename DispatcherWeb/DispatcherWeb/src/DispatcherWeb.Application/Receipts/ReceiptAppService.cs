using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Charges;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Receipts.Dto;
using DispatcherWeb.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Receipts
{
    [AbpAuthorize]
    public class ReceiptAppService : DispatcherWebAppServiceBase, IReceiptAppService
    {
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<Charge> _chargeRepository;
        private readonly ListCacheCollection _listCaches;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly OrderTaxCalculator _orderTaxCalculator;

        public ReceiptAppService(
            IRepository<Receipt> receiptRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Item> itemRepository,
            IRepository<Charge> chargeRepository,
            ListCacheCollection listCaches,
            ISingleOfficeAppService singleOfficeService,
            OrderTaxCalculator orderTaxCalculator
            )
        {
            _receiptRepository = receiptRepository;
            _receiptLineRepository = receiptLineRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _ticketRepository = ticketRepository;
            _itemRepository = itemRepository;
            _chargeRepository = chargeRepository;
            _listCaches = listCaches;
            _singleOfficeService = singleOfficeService;
            _orderTaxCalculator = orderTaxCalculator;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<ReceiptEditDto> GetReceiptForEdit(GetReceiptForEditInput input)
        {
            ReceiptEditDto receiptEditDto;

            if (input.Id.HasValue)
            {
                receiptEditDto = await (await _receiptRepository.GetQueryAsync())
                    .Select(receipt => new ReceiptEditDto
                    {
                        Id = receipt.Id,
                        OrderId = receipt.OrderId,
                        OfficeId = receipt.OfficeId,
                        OfficeName = receipt.Office.Name,
                        MaterialTotal = receipt.MaterialTotal,
                        FreightTotal = receipt.FreightTotal,
                        PoNumber = receipt.PoNumber,
                        CustomerId = receipt.CustomerId,
                        CustomerName = receipt.Customer.Name,
                        DeliveryDate = receipt.DeliveryDate,
                        IsFreightTotalOverridden = receipt.IsFreightTotalOverridden,
                        IsMaterialTotalOverridden = receipt.IsMaterialTotalOverridden,
                        QuoteId = receipt.QuoteId,
                        QuoteName = receipt.Quote.Name,
                        ReceiptDate = receipt.ReceiptDate,
                        SalesTax = receipt.SalesTax,
                        SalesTaxRate = receipt.SalesTaxRate,
                        Shift = receipt.Shift,
                        Total = receipt.Total,
                    })
                    .FirstAsync(x => x.Id == input.Id);

                var payment = await (await _orderRepository.GetQueryAsync())
                    .Where(x => x.Id == receiptEditDto.OrderId)
                    .SelectMany(x => x.OrderPayments)
                    .Where(x => x.OfficeId == OfficeId)
                    .Select(x => x.Payment)
                    .Where(x => !x.IsCancelledOrRefunded)
                    .Select(x => new
                    {
                        x.AuthorizationDateTime,
                        x.AuthorizationCaptureDateTime,
                    }).FirstOrDefaultAsync();

                receiptEditDto.AuthorizationDateTime = payment?.AuthorizationDateTime;
                receiptEditDto.AuthorizationCaptureDateTime = payment?.AuthorizationCaptureDateTime;
            }
            else if (input.OrderId.HasValue)
            {
                var today = await GetToday();

                receiptEditDto = await (await _orderRepository.GetQueryAsync())
                    .Where(x => x.Id == input.OrderId)
                    .Select(order => new ReceiptEditDto
                    {
                        CustomerId = order.CustomerId,
                        CustomerName = order.Customer.Name,
                        DeliveryDate = order.DeliveryDate,
                        OrderId = order.Id,
                        PoNumber = order.PONumber,
                        QuoteId = order.QuoteId,
                        QuoteName = order.Quote.Name,
                        Shift = order.Shift,
                        SalesTax = order.SalesTax,
                        SalesTaxRate = order.SalesTaxRate,
                        //MaterialTotal
                        //FreightTotal
                    })
                    .FirstAsync();

                receiptEditDto.ReceiptDate = today;
                receiptEditDto.OfficeId = OfficeId;
                receiptEditDto.OfficeName = Session.OfficeName;
            }
            else
            {
                throw new ArgumentNullException(nameof(input.OrderId));
            }

            await _singleOfficeService.FillSingleOffice(receiptEditDto);

            return receiptEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<int> EditReceipt(ReceiptEditDto model)
        {
            var receipt = model.Id > 0 ? await _receiptRepository.GetAsync(model.Id.Value) : new Receipt();

            receipt.CustomerId = model.CustomerId;
            receipt.DeliveryDate = model.DeliveryDate;
            receipt.FreightTotal = model.FreightTotal;
            receipt.MaterialTotal = model.MaterialTotal;
            receipt.OfficeId = model.OfficeId;
            receipt.PoNumber = model.PoNumber;
            receipt.QuoteId = model.QuoteId;
            receipt.ReceiptDate = model.ReceiptDate;
            receipt.SalesTax = model.SalesTax;
            receipt.SalesTaxRate = model.SalesTaxRate;
            receipt.Shift = model.Shift;
            receipt.Total = model.Total;

            if (receipt.Id == 0)
            {
                receipt.OrderId = model.OrderId;
                model.Id = await _receiptRepository.InsertAndGetIdAsync(receipt);

                if (model.ReceiptLines != null)
                {
                    var ticketIds = model.ReceiptLines.SelectMany(x => x.TicketIds ?? new List<int>()).Distinct().ToList();
                    var tickets = await (await _ticketRepository.GetQueryAsync()).Where(x => ticketIds.Contains(x.Id) && x.ReceiptLineId == null && x.IsBilled == false).ToListAsync();
                    var chargeIds = model.ReceiptLines.Where(x => x.ChargeId.HasValue).Select(x => x.ChargeId).ToList();
                    var charges = await (await _chargeRepository.GetQueryAsync()).Where(x => chargeIds.Contains(x.Id) && !x.ReceiptLines.Any() && x.IsBilled == false && !x.UseMaterialQuantity).ToListAsync();

                    foreach (var receiptLineModel in model.ReceiptLines)
                    {
                        receiptLineModel.ReceiptId = receipt.Id;
                        var receiptLine = await EditReceiptLineInternal(receiptLineModel);

                        if (receiptLineModel.TicketIds?.Any() == true)
                        {
                            tickets.Where(x => receiptLineModel.TicketIds.Contains(x.Id)).ToList()
                                .ForEach(x => x.ReceiptLine = receiptLine);
                        }
                    }
                    tickets.ForEach(x => x.IsBilled = true);
                    charges.ForEach(x => x.IsBilled = true);

                    var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));

                    await _orderTaxCalculator.CalculateTotalsAsync(receipt, model.ReceiptLines.Select(x => new ReceiptLineTaxDetailsDto
                    {
                        IsTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? true,
                        IsFreightTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? true,
                        IsMaterialTaxable = items.Find(x.MaterialItemId)?.IsTaxable ?? true,
                        FreightAmount = x.FreightAmount,
                        MaterialAmount = x.MaterialAmount,
                    }));
                }
            }
            else
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateReceiptTotalsAsync(receipt.Id);
            }

            return receipt.Id;
        }

        //*********************//

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PagedResultDto<ReceiptLineEditDto>> GetReceiptLines(GetReceiptLinesInput input)
        {
            if (input.ReceiptId.HasValue)
            {
                var query = await _receiptLineRepository.GetQueryAsync();

                var totalCount = await query.CountAsync();

                var items = await query
                    .Where(x => x.ReceiptId == input.ReceiptId)
                    .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters,
                             x => x.LoadAtId == input.LoadAtId)
                    .WhereIf(input.DeliverToId.HasValue || input.ForceDuplicateFilters,
                             x => x.DeliverToId == input.DeliverToId)
                    .WhereIf(input.ItemId.HasValue,
                             x => x.FreightItemId == input.ItemId || x.MaterialItemId == input.ItemId)
                    .WhereIf(input.FreightUomId.HasValue,
                             x => x.FreightUomId == input.FreightUomId)
                    .WhereIf(input.MaterialUomId.HasValue,
                             x => x.MaterialUomId == input.MaterialUomId)
                    .WhereIf(input.Designation.HasValue,
                             x => x.Designation == input.Designation)
                    .Select(x => new ReceiptLineEditDto
                    {
                        Id = x.Id,
                        LoadAtId = x.LoadAtId,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToId = x.DeliverToId,
                        DeliverToName = x.DeliverTo.DisplayName,
                        FreightItemName = x.FreightItem.Name,
                        MaterialItemName = x.MaterialItem.Name,
                        Designation = x.Designation,
                        FreightAmount = x.FreightAmount,
                        FreightItemId = x.FreightItemId,
                        MaterialItemId = x.MaterialItemId,
                        FreightRate = x.FreightRate,
                        FreightQuantity = x.FreightQuantity,
                        FreightUomId = x.FreightUomId,
                        IsFreightAmountOverridden = x.IsFreightAmountOverridden,
                        IsMaterialAmountOverridden = x.IsMaterialAmountOverridden,
                        IsFreightRateOverridden = x.IsFreightRateOverridden,
                        IsMaterialRateOverridden = x.IsMaterialRateOverridden,
                        OrderLineId = x.OrderLineId,
                        MaterialUomId = x.MaterialUomId,
                        LineNumber = x.LineNumber,
                        MaterialRate = x.MaterialRate,
                        MaterialAmount = x.MaterialAmount,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightUomName = x.FreightUom.Name,
                        MaterialUomName = x.MaterialUom.Name,
                        JobNumber = x.JobNumber,
                        ReceiptId = x.ReceiptId,
                    })
                    .OrderBy(input.Sorting)
                    //.PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<ReceiptLineEditDto>(
                    totalCount,
                    items);
            }
            else if (input.OrderId.HasValue)
            {
                var splitBillingByOffices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices);
                var orderLines = await (await _orderLineRepository.GetQueryAsync())
                        .Where(x => x.OrderId == input.OrderId)
                        .Select(x => new
                        {
                            ReceiptLine = new ReceiptLineEditDto
                            {
                                Id = 0,
                                LoadAtId = x.LoadAtId,
                                LoadAtName = x.LoadAt.DisplayName,
                                DeliverToId = x.DeliverToId,
                                DeliverToName = x.DeliverTo.DisplayName,
                                FreightItemName = x.FreightItem.Name,
                                MaterialItemName = x.MaterialItem.Name,
                                Designation = x.Designation,
                                FreightAmount = x.IsFreightPriceOverridden ? x.FreightPrice : 0,
                                FreightItemId = x.FreightItemId,
                                MaterialItemId = x.MaterialItemId,
                                FreightRate = x.FreightPricePerUnit == null ? 0 : x.FreightPricePerUnit.Value,
                                //FreightQuantity = x.FreightQuantity,
                                FreightUomId = x.FreightUomId,
                                IsFreightAmountOverridden = x.IsFreightPriceOverridden,
                                IsMaterialAmountOverridden = x.IsMaterialPriceOverridden,
                                IsFreightRateOverridden = x.IsFreightPricePerUnitOverridden,
                                IsMaterialRateOverridden = x.IsMaterialPricePerUnitOverridden,
                                OrderLineId = x.Id,
                                MaterialUomId = x.MaterialUomId,
                                LineNumber = x.LineNumber,
                                MaterialRate = x.MaterialPricePerUnit == null ? 0 : x.MaterialPricePerUnit.Value,
                                MaterialAmount = x.IsMaterialPriceOverridden ? x.MaterialPrice : 0,
                                //MaterialQuantity = x.MaterialQuantity,
                                FreightUomName = x.FreightUom.Name,
                                MaterialUomName = x.MaterialUom.Name,
                                JobNumber = x.JobNumber,
                                //ReceiptId = x.ReceiptId
                            },
                            Tickets = x.Tickets
                                .Where(t => !splitBillingByOffices || t.OfficeId == OfficeId)
                                .Select(t => new
                                {
                                    t.Id,
                                    t.FreightQuantity,
                                    t.MaterialQuantity,
                                    t.FreightUomId,
                                    t.ReceiptLineId,
                                    t.IsBilled,
                                }).ToList(),
                            Charges = x.Charges
                                .Where(c => !c.ReceiptLines.Any())
                                .Select(c => new ReceiptLineEditDto
                                {
                                    Id = 0,
                                    LoadAtId = x.LoadAtId,
                                    LoadAtName = x.LoadAt.DisplayName,
                                    DeliverToId = x.DeliverToId,
                                    DeliverToName = x.DeliverTo.DisplayName,
                                    FreightItemId = c.ItemId,
                                    FreightItemName = c.Item.Name,
                                    MaterialItemId = null,
                                    MaterialItemName = null,
                                    FreightUomId = c.UnitOfMeasureId,
                                    FreightUomName = c.UnitOfMeasure.Name,
                                    MaterialUomId = null,
                                    MaterialUomName = null,
                                    Designation = DesignationEnum.FreightOnly,
                                    FreightAmount = c.ChargeAmount,
                                    FreightRate = c.Rate,
                                    FreightQuantity = c.Quantity,
                                    UseMaterialQuantity = c.UseMaterialQuantity,
                                    MaterialRate = 0,
                                    MaterialQuantity = 0,
                                    MaterialAmount = 0,
                                    IsFreightAmountOverridden = false,
                                    IsMaterialAmountOverridden = false,
                                    IsFreightRateOverridden = false,
                                    IsMaterialRateOverridden = false,
                                    OrderLineId = c.OrderLineId,
                                    ChargeId = c.Id,
                                    LineNumber = x.LineNumber,
                                    JobNumber = x.JobNumber,
                                }),
                            HasPreviousReceiptLines = x.ReceiptLines.Any(r => r.Receipt.OfficeId == OfficeId),
                        })
                        .ToListAsync();

                var receiptLines = orderLines
                    .Where(x =>
                    {
                        if (x.ReceiptLine.IsMaterialAmountOverridden && x.ReceiptLine.IsFreightAmountOverridden && x.HasPreviousReceiptLines)
                        {
                            return false;
                        }
                        return true;
                    })
                    .Select(x =>
                    {
                        x.ReceiptLine.TicketIds = new List<int>();
                        if (x.ReceiptLine.IsMaterialAmountOverridden || x.ReceiptLine.IsFreightAmountOverridden)
                        {
                            //only one ticket is allowed for the overridden values
                            var ticket = x.Tickets.OrderBy(t => t.Id).FirstOrDefault();
                            if (ticket != null)
                            {
                                var ticketAmount = new TicketQuantityDto
                                {
                                    FreightQuantity = ticket.FreightQuantity,
                                    MaterialQuantity = ticket.MaterialQuantity,
                                    Designation = x.ReceiptLine.Designation,
                                    OrderLineMaterialUomId = x.ReceiptLine.MaterialUomId,
                                    OrderLineFreightUomId = x.ReceiptLine.FreightUomId,
                                    TicketUomId = ticket.FreightUomId,
                                };
                                //the single allowed ticket hasn't been used up by another receipt line yet
                                if (ticket.ReceiptLineId == null && !ticket.IsBilled)
                                {
                                    x.ReceiptLine.TicketIds.Add(ticket.Id);
                                    if (x.ReceiptLine.IsMaterialAmountOverridden)
                                    {
                                        x.ReceiptLine.MaterialQuantity = ticketAmount.GetMaterialQuantity();
                                    }
                                    if (x.ReceiptLine.IsFreightAmountOverridden)
                                    {
                                        x.ReceiptLine.FreightQuantity = ticketAmount.GetFreightQuantity();
                                    }
                                }
                                else
                                {
                                    if (x.ReceiptLine.IsMaterialAmountOverridden)
                                    {
                                        x.ReceiptLine.MaterialQuantity = 0;
                                        x.ReceiptLine.MaterialAmount = 0;
                                    }
                                    if (x.ReceiptLine.IsFreightAmountOverridden)
                                    {
                                        x.ReceiptLine.FreightQuantity = 0;
                                        x.ReceiptLine.FreightAmount = 0;
                                    }
                                }
                            }
                        }
                        if (!x.ReceiptLine.IsMaterialAmountOverridden || !x.ReceiptLine.IsFreightAmountOverridden)
                        {
                            //all new tickets are allowed for non-overridden values
                            var tickets = x.Tickets.Where(t => t.ReceiptLineId == null && !t.IsBilled).ToList();
                            var ticketQuantities = tickets.Select(t => new TicketQuantityDto
                            {
                                FreightQuantity = t.FreightQuantity,
                                MaterialQuantity = t.MaterialQuantity,
                                Designation = x.ReceiptLine.Designation,
                                OrderLineMaterialUomId = x.ReceiptLine.MaterialUomId,
                                OrderLineFreightUomId = x.ReceiptLine.FreightUomId,
                                TicketUomId = t.FreightUomId,
                            }).ToList();
                            x.ReceiptLine.TicketIds.AddRange(tickets.Select(t => t.Id).Except(x.ReceiptLine.TicketIds));
                            if (!x.ReceiptLine.IsMaterialAmountOverridden)
                            {
                                x.ReceiptLine.MaterialQuantity = ticketQuantities.Any()
                                    ? ticketQuantities.Sum(t => t.GetMaterialQuantity())
                                    : (decimal?)null;
                                x.ReceiptLine.MaterialAmount = (x.ReceiptLine.MaterialQuantity ?? 0) * x.ReceiptLine.MaterialRate;
                            }
                            if (!x.ReceiptLine.IsFreightAmountOverridden)
                            {
                                x.ReceiptLine.FreightQuantity = ticketQuantities.Any()
                                    ? ticketQuantities.Sum(t => t.GetFreightQuantity())
                                    : (decimal?)null;
                                x.ReceiptLine.FreightAmount = (x.ReceiptLine.FreightQuantity ?? 0) * x.ReceiptLine.FreightRate;
                            }
                        }
                        return x.ReceiptLine;
                    })
                    .ToList();

                var nextLineNumber = receiptLines.Max(x => x.LineNumber) + 1;
                foreach (var orderLine in orderLines)
                {
                    foreach (var charge in orderLine.Charges)
                    {
                        if (charge.UseMaterialQuantity == true)
                        {
                            charge.FreightQuantity = orderLine.Tickets
                                .Sum(x => x.MaterialQuantity ?? 0);
                        }

                        charge.LineNumber = nextLineNumber++;
                        receiptLines.Add(charge);
                    }
                }

                return new PagedResultDto<ReceiptLineEditDto>(
                    receiptLines.Count,
                    receiptLines);
            }
            else
            {
                throw new ArgumentNullException(nameof(input.OrderId));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<ReceiptLineEditDto> GetReceiptLineForEdit(GetReceiptLineForEditInput input)
        {
            ReceiptLineEditDto receiptLineEditDto;

            if (input.Id.HasValue)
            {
                receiptLineEditDto = await (await _receiptLineRepository.GetQueryAsync())
                    .Select(x => new ReceiptLineEditDto
                    {
                        Id = x.Id,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToName = x.DeliverTo.DisplayName,
                        FreightItemName = x.FreightItem.Name,
                        MaterialItemName = x.MaterialItem.Name,
                        Designation = x.Designation,
                        FreightAmount = x.FreightAmount,
                        LoadAtId = x.LoadAtId,
                        DeliverToId = x.DeliverToId,
                        FreightItemId = x.FreightItemId,
                        MaterialItemId = x.MaterialItemId,
                        FreightRate = x.FreightRate,
                        FreightQuantity = x.FreightQuantity,
                        FreightUomId = x.FreightUomId,
                        IsFreightRateOverridden = x.IsFreightRateOverridden,
                        IsMaterialRateOverridden = x.IsMaterialRateOverridden,
                        IsFreightAmountOverridden = x.IsFreightAmountOverridden,
                        IsMaterialAmountOverridden = x.IsMaterialAmountOverridden,
                        OrderLineId = x.Id,
                        MaterialUomId = x.MaterialUomId,
                        LineNumber = x.LineNumber,
                        MaterialRate = x.MaterialRate,
                        MaterialAmount = x.MaterialAmount,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightUomName = x.FreightUom.Name,
                        MaterialUomName = x.MaterialUom.Name,
                        JobNumber = x.JobNumber,
                        ReceiptId = x.ReceiptId,
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else if (input.ReceiptId.HasValue)
            {
                var lastReceiptLine = await (await _receiptLineRepository.GetQueryAsync())
                    .Where(x => x.ReceiptId == input.ReceiptId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        Id = x.Id,
                        LoadAtId = x.LoadAtId,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToId = x.DeliverToId,
                        DeliverToName = x.DeliverTo.DisplayName,
                    })
                    .FirstOrDefaultAsync();

                var receipt = await (await _receiptRepository.GetQueryAsync())
                    .Where(x => x.Id == input.ReceiptId)
                    .Select(x => new
                    {
                        ReceiptLinesCount = x.ReceiptLines.Count,
                    })
                    .FirstOrDefaultAsync();

                receiptLineEditDto = new ReceiptLineEditDto
                {
                    ReceiptId = input.ReceiptId.Value,
                    LoadAtId = lastReceiptLine?.LoadAtId,
                    LoadAtName = lastReceiptLine?.LoadAtName,
                    DeliverToId = lastReceiptLine?.DeliverToId,
                    DeliverToName = lastReceiptLine?.DeliverToName,
                    LineNumber = receipt.ReceiptLinesCount + 1,
                };
            }
            else
            {
                receiptLineEditDto = new ReceiptLineEditDto();
            }

            return receiptLineEditDto;
        }

        private async Task<ReceiptLine> EditReceiptLineInternal(ReceiptLineEditDto model)
        {
            var isNew = model.Id == 0 || model.Id == null;
            var receiptLine = !isNew ? await _receiptLineRepository.GetAsync(model.Id.Value) : new ReceiptLine();

            if (!isNew)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Orders_Edit);
            }

            if (isNew)
            {
                receiptLine.ReceiptId = model.ReceiptId;
                receiptLine.OrderLineId = model.OrderLineId;
                receiptLine.ChargeId = model.ChargeId;
            }

            receiptLine.Designation = model.Designation;
            receiptLine.FreightAmount = model.FreightAmount;
            receiptLine.LoadAtId = model.LoadAtId;
            receiptLine.DeliverToId = model.DeliverToId;
            receiptLine.FreightItemId = model.FreightItemId;
            receiptLine.MaterialItemId = model.MaterialItemId;
            receiptLine.FreightRate = model.FreightRate;
            receiptLine.FreightQuantity = model.FreightQuantity;
            receiptLine.FreightUomId = model.FreightUomId;
            receiptLine.IsFreightAmountOverridden = model.IsFreightAmountOverridden;
            receiptLine.IsMaterialAmountOverridden = model.IsMaterialAmountOverridden;
            receiptLine.IsFreightRateOverridden = model.IsFreightRateOverridden;
            receiptLine.IsMaterialRateOverridden = model.IsMaterialRateOverridden;
            receiptLine.MaterialUomId = model.MaterialUomId;
            receiptLine.LineNumber = model.LineNumber;
            receiptLine.MaterialRate = model.MaterialRate;
            receiptLine.MaterialAmount = model.MaterialAmount;
            receiptLine.MaterialQuantity = model.MaterialQuantity;
            receiptLine.JobNumber = model.JobNumber;

            if (isNew)
            {
                await _receiptLineRepository.InsertAsync(receiptLine);
            }

            return receiptLine;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<EditReceiptLineOutput> EditReceiptLine(ReceiptLineEditDto model)
        {
            var receiptLine = await EditReceiptLineInternal(model);

            await CurrentUnitOfWork.SaveChangesAsync();
            model.Id = receiptLine.Id;

            var taxDetails = await _orderTaxCalculator.CalculateReceiptTotalsAsync(model.ReceiptId);

            return new EditReceiptLineOutput
            {
                ReceiptLineId = receiptLine.Id,
                OrderTaxDetails = new ReceiptTaxDetailsDto(taxDetails),
            };
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<DeleteReceiptLineOutput> DeleteReceiptLines(IdListInput input)
        {
            var receipts = await (await _receiptRepository.GetQueryAsync())
                .Include(x => x.ReceiptLines)
                    .ThenInclude(x => x.Tickets)
                .Include(x => x.ReceiptLines)
                    .ThenInclude(x => x.Charge)
                .Where(x => x.ReceiptLines.Any(r => input.Ids.Contains(r.Id)))
                .ToListAsync();

            var receiptLinesToDelete = receipts.SelectMany(x => x.ReceiptLines).Where(x => input.Ids.Contains(x.Id)).ToList();

            foreach (var receiptLine in receiptLinesToDelete)
            {
                await _receiptLineRepository.DeleteAsync(receiptLine);
                foreach (var ticket in receiptLine.Tickets)
                {
                    ticket.IsBilled = false;
                    ticket.ReceiptLineId = null;
                }
                if (receiptLine.Charge != null)
                {
                    receiptLine.Charge.IsBilled = false;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            var items = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));

            foreach (var receipt in receipts)
            {
                var nextLineNumber = 1;
                foreach (var receiptLine in receipt.ReceiptLines.Where(x => !input.Ids.Contains(x.Id)).OrderBy(x => x.LineNumber))
                {
                    receiptLine.LineNumber = nextLineNumber++;
                }

                OrderTaxCalculator.CalculateTotals(taxCalculationType, receipt,
                    receipt.ReceiptLines
                        .Except(receiptLinesToDelete)
                        .Select(x => new ReceiptLineTaxDetailsDto
                        {
                            IsTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? true,
                            IsFreightTaxable = items.Find(x.FreightItemId)?.IsTaxable ?? true,
                            IsMaterialTaxable = items.Find(x.MaterialItemId)?.IsTaxable ?? true,
                            FreightAmount = x.FreightAmount,
                            MaterialAmount = x.MaterialAmount,
                        })
                        .ToList(), separateItems);
            }

            return new DeleteReceiptLineOutput
            {
                OrderTaxDetails = receipts.Any() ? new ReceiptTaxDetailsDto(receipts.FirstOrDefault()) : null,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<IOrderTaxDetails> CalculateReceiptTotals(ReceiptTaxDetailsDto receiptTaxDetails)
        {
            List<ReceiptLineTaxDetailsDto> receiptLines;

            if (receiptTaxDetails.Id != 0)
            {
                receiptLines = await (await _receiptLineRepository.GetQueryAsync())
                    .Where(x => x.ReceiptId == receiptTaxDetails.Id)
                    .Select(x => new ReceiptLineTaxDetailsDto
                    {
                        FreightAmount = x.FreightAmount,
                        MaterialAmount = x.MaterialAmount,
                        IsTaxable = x.FreightItem.IsTaxable,
                        IsFreightTaxable = x.FreightItem.IsTaxable,
                        IsMaterialTaxable = x.MaterialItem.IsTaxable,
                    })
                    .ToListAsync();
            }
            else if (receiptTaxDetails.ReceiptLines != null)
            {
                receiptLines = receiptTaxDetails.ReceiptLines;
            }
            else
            {
                receiptLines = new List<ReceiptLineTaxDetailsDto>();
            }

            await _orderTaxCalculator.CalculateTotalsAsync(receiptTaxDetails, receiptLines);

            return receiptTaxDetails;
        }
    }
}
