using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerStatements.Dto;
using DispatcherWeb.LeaseHaulerStatements.Exporting;
using DispatcherWeb.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerStatements
{
    [AbpAuthorize]
    public class LeaseHaulerStatementAppService : DispatcherWebAppServiceBase, ILeaseHaulerStatementAppService
    {
        private readonly IRepository<LeaseHaulerStatement> _leaseHaulerStatementRepository;
        private readonly ILeaseHaulerStatementTicketRepository _leaseHaulerStatementTicketRepository;
        private readonly IRepository<Ticket> _tickerRepository;
        private readonly ILeaseHaulerStatementCsvExporter _leaseHaulerStatementCsvExporter;

        public LeaseHaulerStatementAppService(
            IRepository<LeaseHaulerStatement> leaseHaulerStatementRepository,
            ILeaseHaulerStatementTicketRepository leaseHaulerStatementTicketRepository,
            IRepository<Ticket> tickerRepository,
            ILeaseHaulerStatementCsvExporter leaseHaulerStatementCsvExporter
            )
        {
            _leaseHaulerStatementRepository = leaseHaulerStatementRepository;
            _leaseHaulerStatementTicketRepository = leaseHaulerStatementTicketRepository;
            _tickerRepository = tickerRepository;
            _leaseHaulerStatementCsvExporter = leaseHaulerStatementCsvExporter;
        }


        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public async Task<PagedResultDto<LeaseHaulerStatementDto>> GetLeaseHaulerStatements(GetLeaseHaulerStatementsInput input)
        {
            var query = (await _leaseHaulerStatementRepository.GetQueryAsync())
                .WhereIf(input.StatementId.HasValue, x => x.Id == input.StatementId)
                .WhereIf(input.StatementDateBegin.HasValue, x => x.StatementDate >= input.StatementDateBegin)
                .WhereIf(input.StatementDateEnd.HasValue, x => x.StatementDate <= input.StatementDateEnd);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LeaseHaulerStatementDto
                {
                    Id = x.Id,
                    StatementDate = x.StatementDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Customers = x.LeaseHaulerStatementTickets
                        .Where(t => t.Ticket.OrderLineId != null)
                        .Select(t => t.Ticket.OrderLine.Order.Customer)
                        .Select(c => new LeaseHaulerStatementCustomerDto
                        {
                            CustomerId = c.Id,
                            CustomerName = c.Name,
                        })
                        .ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            items.ForEach(x => x.Customers = x.Customers.GroupBy(c => c.CustomerId).Select(c => c.First()).OrderBy(c => c.CustomerName).ToList());

            return new PagedResultDto<LeaseHaulerStatementDto>(
                totalCount,
                items);
        }

        private async Task<IQueryable<Ticket>> GetTicketQueryForNewLeaseHaulerStatement(AddLeaseHaulerStatementInput input)
        {
            var timezone = await GetTimezone();
            var startDateInUtc = input.StartDate?.ConvertTimeZoneFrom(timezone);
            var endDateInUtc = input.EndDate?.AddDays(1).ConvertTimeZoneFrom(timezone);

            var query = (await _tickerRepository.GetQueryAsync())
                .WhereIf(startDateInUtc.HasValue, x => x.TicketDateTime >= startDateInUtc)
                .WhereIf(endDateInUtc.HasValue, x => x.TicketDateTime < endDateInUtc)
                .Where(x =>
                    x.CarrierId.HasValue
                    && x.Driver.IsExternal
                    && x.LeaseHaulerStatementTicket == null
                    && x.IsVerified
                    && !x.NonbillableFreight
                )
                .WhereIf(input.LeaseHaulerIds?.Any() == true, x => input.LeaseHaulerIds.Contains(x.CarrierId.Value));

            return query;
        }

        private async Task<LeaseHaulerStatement> GetNewLeaseHaulerStatementEntity(GetNewLeaseHaulerStatementEntityInput input)
        {
            var today = await GetToday();
            var timezone = await GetTimezone();
            var ticketsWithDateTime = input.Tickets.Where(x => x.TicketDateTime.HasValue).ToList();

            var leaseHaulerStatement = new LeaseHaulerStatement
            {
                StartDate = input.StartDate ?? ticketsWithDateTime.Min(x => x.TicketDateTime)?.ConvertTimeZoneTo(timezone).Date ?? today,
                EndDate = input.EndDate ?? ticketsWithDateTime.Max(x => x.TicketDateTime)?.ConvertTimeZoneTo(timezone).Date ?? today,
                StatementDate = today,
            };
            return leaseHaulerStatement;
        }

        private async Task<LeaseHaulerStatementTicket> GetNewLeaseHaulerStatementTicketEntity(INewLeaseHaulerStatementTicketDetailsDto ticket)
        {
            var brokerageFeeRate = await SettingManager.GetSettingValueAsync<decimal>(AppSettings.LeaseHaulers.BrokerFee) / 100;

            var statementTicket = new LeaseHaulerStatementTicket
            {
                TicketId = ticket.TicketId,
                LeaseHaulerId = ticket.LeaseHaulerId,
                TruckId = ticket.TruckId,
                Quantity = ticket.Quantity,
                Rate = ticket.LeaseHaulerRate,
                FuelSurcharge = ticket.FuelSurcharge,
            };

            var freightRate = ticket.FreightRate == 0 ? null : ticket.FreightRate;
            var extendedFreightAmount = ticket.IsFreightTotalOverridden
                ? (ticket.FreightTotal / freightRate * ticket.LeaseHaulerRate) ?? 0
                : ticket.Quantity * (ticket.LeaseHaulerRate ?? 0);

            statementTicket.BrokerFee = extendedFreightAmount * brokerageFeeRate;
            statementTicket.ExtendedAmount = extendedFreightAmount - statementTicket.BrokerFee + statementTicket.FuelSurcharge;
            return statementTicket;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public async Task AddLeaseHaulerStatement(AddLeaseHaulerStatementInput input)
        {
            var timezone = await GetTimezone();

            var query = await GetTicketQueryForNewLeaseHaulerStatement(input);
            var tickets = await query
                .Select(x => new NewLeaseHaulerStatementTicketDetailsDto
                {
                    TicketDateTime = x.TicketDateTime,
                    TicketId = x.Id,
                    LeaseHaulerId = x.Driver.LeaseHaulerDriver.LeaseHaulerId,
                    LeaseHaulerName = x.Driver.LeaseHaulerDriver.LeaseHauler.Name,
                    TruckId = x.TruckId,
                    TruckCode = x.TruckId.HasValue ? x.Truck.TruckCode : x.TruckCode,
                    Quantity = x.FreightQuantity ?? 0,
                    FreightRate = x.OrderLine.FreightPricePerUnit,
                    LeaseHaulerRate = x.OrderLine.LeaseHaulerRate,
                    FuelSurcharge = x.FuelSurcharge,
                    IsFreightTotalOverridden = x.OrderLine.IsFreightPriceOverridden,
                    FreightTotal = x.OrderLine.FreightPrice,
                }).ToListAsync();

            if (!tickets.Any())
            {
                throw new UserFriendlyException(L("NoDataForSelectedPeriod"));
            }

            await ValidateMissingLhRates(tickets);

            var leaseHaulerStatement = await GetNewLeaseHaulerStatementEntity(input.CopyTo(new GetNewLeaseHaulerStatementEntityInput
            {
                Tickets = tickets,
            }));
            await _leaseHaulerStatementRepository.InsertAsync(leaseHaulerStatement);

            foreach (var ticket in tickets)
            {
                var statementTicket = await GetNewLeaseHaulerStatementTicketEntity(ticket);
                statementTicket.LeaseHaulerStatement = leaseHaulerStatement;
                await _leaseHaulerStatementTicketRepository.InsertAsync(statementTicket);
            }
        }

        private async Task ValidateMissingLhRates(IEnumerable<INewLeaseHaulerStatementTicketDetailsDto> tickets)
        {
            var missingLhRateTickets = tickets
                .Where(t => t.LeaseHaulerRate is null or 0)
                .ToList();

            if (missingLhRateTickets.Any())
            {
                var timezone = await GetTimezone();
                var leaseHaulersWithTrucks = missingLhRateTickets
                    .GroupBy(x => x.LeaseHaulerName)
                    .Select(x => $"{x.Key} {string.Join(", ", x.Select(t => t.TruckCode).Distinct().OrderBy(t => t))}")
                    .OrderBy(x => x)
                    .ToList();
                var dates = missingLhRateTickets
                    .Select(t => t.TicketDateTime?.ConvertTimeZoneTo(timezone).Date)
                    .Where(x => x.HasValue)
                    .OrderBy(x => x.Value)
                    .Select(x => x.Value.ToString("d"))
                    .Distinct()
                    .ToList();
                throw new UserFriendlyException(L("LeaseHaulerRateIsMissingOnTicketDates", string.Join(", ", leaseHaulersWithTrucks), string.Join(", ", dates)));
            }
        }

        private async Task<LeaseHaulerStatementReportDto> GetLeaseHaulerStatementReportDto(ExportLeaseHaulerStatementByIdInput input)
        {
            var item = await (await _leaseHaulerStatementRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new LeaseHaulerStatementReportDto
                {
                    Id = x.Id,
                    StatementDate = x.StatementDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Tickets = x.LeaseHaulerStatementTickets.Select(t => new LeaseHaulerStatementTicketReportDto
                    {
                        OrderDate = t.Ticket.OrderLine.Order.DeliveryDate,
                        Shift = t.Ticket.OrderLine.Order.Shift,
                        CustomerName = t.Ticket.Customer.Name,
                        FreightItemName = t.Ticket.FreightItem.Name,
                        MaterialItemName = t.Ticket.MaterialItem.Name,
                        TicketNumber = t.Ticket.TicketNumber,
                        TicketDateTime = t.Ticket.TicketDateTime,
                        CarrierName = t.Ticket.Carrier.Name,
                        TruckCode = t.TruckId.HasValue ? t.Truck.TruckCode : t.Ticket.TruckCode,
                        DriverName = t.Ticket.Driver.FirstName + " " + t.Ticket.Driver.LastName,
                        LoadAtName = t.Ticket.LoadAt.DisplayName,
                        DeliverToName = t.Ticket.DeliverTo.DisplayName,
                        TicketId = t.TicketId,
                        LeaseHaulerId = t.LeaseHaulerId,
                        LeaseHaulerName = t.LeaseHauler.Name,
                        TruckId = t.TruckId,
                        FreightUomName = t.Ticket.FreightUom.Name,
                        MaterialUomName = t.Ticket.MaterialUom.Name,
                        Quantity = t.Quantity,
                        FreightRate = t.Ticket.OrderLine.FreightPricePerUnit,
                        LeaseHaulerRate = t.Rate,
                        BrokerFee = t.BrokerFee,
                        FuelSurcharge = t.FuelSurcharge,
                        IsFreightTotalOverridden = t.Ticket.OrderLine.IsFreightPriceOverridden,
                        FreightTotal = t.Ticket.OrderLine.FreightPrice,
                        ExtendedAmount = t.ExtendedAmount,
                    }).ToList(),
                }).FirstAsync();

            var timezone = await GetTimezone();
            foreach (var ticket in item.Tickets)
            {
                ticket.ShiftName = await SettingManager.GetShiftName(ticket.Shift);
                ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(timezone);
            }

            return item;
        }

        private async Task<LeaseHaulerStatementReportDto> GetLeaseHaulerStatementReportDto(ExportLeaseHaulerStatementIntermediatelyByDatesInput input)
        {
            var timezone = await GetTimezone();

            var ticketsQuery = await GetTicketQueryForNewLeaseHaulerStatement(input);
            var tickets = await ticketsQuery
                .Select(t => new LeaseHaulerStatementTicketReportDto
                {
                    OrderDate = t.OrderLine.Order.DeliveryDate,
                    Shift = t.OrderLine.Order.Shift,
                    CustomerName = t.Customer.Name,
                    FreightItemName = t.FreightItem.Name,
                    MaterialItemName = t.MaterialItem.Name,
                    TicketNumber = t.TicketNumber,
                    TicketDateTime = t.TicketDateTime,
                    CarrierName = t.Carrier.Name,
                    TruckCode = t.TruckId.HasValue ? t.Truck.TruckCode : t.TruckCode,
                    DriverName = t.Driver.FirstName + " " + t.Driver.LastName,
                    LoadAtName = t.LoadAt.DisplayName,
                    DeliverToName = t.DeliverTo.DisplayName,
                    FreightUomName = t.FreightUom.Name,
                    MaterialUomName = t.MaterialUom.Name,
                    Quantity = t.FreightQuantity ?? 0,
                    FreightRate = t.OrderLine.FreightPricePerUnit,
                    LeaseHaulerRate = t.OrderLine.LeaseHaulerRate,
                    //BrokerFee will be calculated below
                    FuelSurcharge = t.FuelSurcharge,
                    IsFreightTotalOverridden = t.OrderLine.IsFreightPriceOverridden,
                    FreightTotal = t.OrderLine.FreightPrice,
                    //ExtendedAmount will be calculated below
                    LeaseHaulerId = t.CarrierId.Value,
                    LeaseHaulerName = t.Carrier.Name,
                    TicketId = t.Id,
                    TruckId = t.TruckId,
                }).ToListAsync();

            if (!tickets.Any())
            {
                throw new UserFriendlyException(L("NoDataForSelectedPeriod"));
            }

            await ValidateMissingLhRates(tickets);

            var leaseHaulerStatement = await GetNewLeaseHaulerStatementEntity(input.CopyTo(new GetNewLeaseHaulerStatementEntityInput
            {
                Tickets = tickets,
            }));

            foreach (var ticket in tickets)
            {
                var statementTicket = await GetNewLeaseHaulerStatementTicketEntity(ticket);
                ticket.BrokerFee = statementTicket.BrokerFee;
                ticket.ExtendedAmount = statementTicket.ExtendedAmount;

                ticket.ShiftName = await SettingManager.GetShiftName(ticket.Shift);
                ticket.TicketDateTime = ticket.TicketDateTime?.ConvertTimeZoneTo(timezone);
            }

            var result = new LeaseHaulerStatementReportDto
            {
                Id = null,
                StatementDate = leaseHaulerStatement.StatementDate,
                StartDate = leaseHaulerStatement.StartDate,
                EndDate = leaseHaulerStatement.EndDate,
                Tickets = tickets,
            };

            return result;
        }


        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        [HttpPost]
        public async Task<FileDto> ExportLeaseHaulerStatementIntermediatelyByDates(ExportLeaseHaulerStatementIntermediatelyByDatesInput input)
        {
            var data = await GetLeaseHaulerStatementReportDto(input);
            return await GetCsvFilesFromReportDtoAsync(new GetCsvFilesFromReportDtoInput
            {
                SplitByLeaseHauler = input.SplitByLeaseHauler,
                Report = data,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        [HttpPost]
        public async Task<FileDto> ExportLeaseHaulerStatementById(ExportLeaseHaulerStatementByIdInput input)
        {
            var data = await GetLeaseHaulerStatementReportDto(input);
            return await GetCsvFilesFromReportDtoAsync(new GetCsvFilesFromReportDtoInput
            {
                SplitByLeaseHauler = input.SplitByLeaseHauler,
                Report = data,
            });
        }

        private async Task<FileDto> GetCsvFilesFromReportDtoAsync(GetCsvFilesFromReportDtoInput input)
        {
            var data = input.Report;
            var filename = $"LeaseHaulerStatement{data.Id}-{data.StartDate:yyyyMMdd}-{data.EndDate:yyyyMMdd}";
            if (input.SplitByLeaseHauler)
            {
                var csvList = new List<FileBytesDto>();
                foreach (var group in data.Tickets
                    .GroupBy(x => x.CarrierName))
                {
                    var carrierData = data.Clone();
                    carrierData.FileName = $"{filename}-{group.Key}.csv";
                    carrierData.Tickets = group.ToList();
                    csvList.Add(await _leaseHaulerStatementCsvExporter.ExportToFileBytes(carrierData));
                }

                var zipFile = csvList.ToZipFile(filename + ".zip", CompressionLevel.Optimal);
                return await _leaseHaulerStatementCsvExporter.StoreTempFileAsync(zipFile);
            }
            else
            {
                data.FileName = $"{filename}.csv";
                return await _leaseHaulerStatementCsvExporter.ExportToFileAsync(data);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public async Task DeleteLeaseHaulerStatement(EntityDto input)
        {
            await _leaseHaulerStatementTicketRepository.DeleteAllForLeaseHaulerStatementIdAsync(input.Id, await Session.GetTenantIdAsync());
            await _leaseHaulerStatementRepository.DeleteAsync(input.Id);
        }
    }
}
