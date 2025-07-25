using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Items;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets.JobsMissingTicketsReport;
using DispatcherWeb.Tickets.JobsMissingTicketsReport.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Tickets.Reports;

[AbpAuthorize(AppPermissions.Pages_Reports_JobsMissingTickets)]
public class JobsMissingTicketsReportAppService : ReportAppServiceBase<JobsMissingTicketsReportInput>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderLine> _orderLineRepository;
    private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Item> _itemRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<Truck> _truckRepository;
    private readonly IRepository<Driver> _driverRepository;
    private readonly IRepository<Ticket> _ticketRepository;

    public JobsMissingTicketsReportAppService(
        IAttachmentHelper attachmentHelper,
        IRepository<Order> orderRepository,
        IRepository<OrderLine> orderLineRepository,
        IRepository<OrderLineTruck> orderLineTruckRepository,
        IRepository<Customer> customerRepository,
        IRepository<Item> itemRepository,
        IRepository<Location> locationRepository,
        IRepository<Truck> truckRepository,
        IRepository<Driver> driverRepository,
        IRepository<Ticket> ticketRepository
    ) : base(attachmentHelper)
    {
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _orderLineTruckRepository = orderLineTruckRepository;
        _customerRepository = customerRepository;
        _itemRepository = itemRepository;
        _locationRepository = locationRepository;
        _truckRepository = truckRepository;
        _driverRepository = driverRepository;
        _ticketRepository = ticketRepository;
    }

    protected override string ReportPermission => AppPermissions.Pages_Reports_JobsMissingTickets;

    protected override string ReportFileName => "JobsMissingTickets";

    protected override Task<string> GetReportFilename(string extension, JobsMissingTicketsReportInput input)
    {
        return Task.FromResult(
            $"{ReportFileName}_{input.DeliveryDateBegin:yyyyMMdd}to{input.DeliveryDateEnd:yyyyMMdd}.{extension}");
    }

    protected override void InitPdfReport(PdfReport report)
    { }

    protected override Task<bool> CreatePdfReport(PdfReport report, JobsMissingTicketsReportInput input)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> CreateCsvReport(CsvReport report, JobsMissingTicketsReportInput input)
    {
        return CreateReport(report, input, () => new JobsMissingTicketsTableCsv(report.CsvWriter));
    }

    private async Task<bool> CreateReport(
        IReport report,
        JobsMissingTicketsReportInput input,
        Func<IJobsMissingTicketsTable> createJobsMissingTicketsTable)
    {
        report.AddReportHeader($"Jobs Missing Tickets Report for {input.DeliveryDateBegin:d} - {input.DeliveryDateEnd:d}");

        var missingTickets = await GetJobsMissingTickets(input);
        if (missingTickets.Count == 0)
        {
            return false;
        }

        var jobsMissingTicketsTable = createJobsMissingTicketsTable();

        jobsMissingTicketsTable.AddColumnHeaders(
            "Customer Name",
            "Item Name",
            "Delivery Date",
            "Order Id",
            "Deliver To",
            "Truck",
            "Driver");

        var currencyCulture = await SettingManager.GetCurrencyCultureAsync();

        foreach (var missingTicket in missingTickets)
        {
            jobsMissingTicketsTable.AddRow(
                missingTicket.CustomerName,
                missingTicket.ItemName,
                missingTicket.DeliveryDate.ToString("d"),
                missingTicket.OrderId.ToString(),
                missingTicket.DeliverTo,
                missingTicket.TruckCode,
                missingTicket.Driver);
        }

        return true;
    }

    private async Task<List<JobsMissingTicket>> GetJobsMissingTickets(JobsMissingTicketsReportInput input)
    {
        var tenantId = await AbpSession.GetTenantIdAsync();
        var query = (await _orderLineRepository.GetQueryAsync())
            .Join(await _orderLineTruckRepository.GetQueryAsync(),
                ol => ol.Id,
                truck => truck.OrderLineId,
                (ol, truck) => new { OrderLine = ol, OrderLineTruck = truck })
            .Join(await _orderRepository.GetQueryAsync(),
                ol => ol.OrderLine.OrderId,
                o => o.Id,
                (x, o) => new { x.OrderLine, x.OrderLineTruck, Order = o })
            .Join(await _customerRepository.GetQueryAsync(),
                x => x.Order.CustomerId,
                c => c.Id,
                (x, c) => new { x.OrderLine, x.OrderLineTruck, x.Order, Customer = c })
            .Join(await _itemRepository.GetQueryAsync(),
                x => x.OrderLine.FreightItemId,
                i => i.Id,
                (x, i) => new { x.OrderLine, x.OrderLineTruck, x.Order, x.Customer, FreightItem = i })
            .Join(await _locationRepository.GetQueryAsync(),
                x => x.OrderLine.DeliverToId,
                l => l.Id,
                (x, l) => new { x.OrderLine, x.OrderLineTruck, x.Order, x.Customer, x.FreightItem, Location = l })
            .Join(await _truckRepository.GetQueryAsync(),
                x => x.OrderLineTruck.TruckId,
                tr => tr.Id,
                (x, tr) => new { x.OrderLine, x.OrderLineTruck, x.Order, x.Customer, x.FreightItem, x.Location, Truck = tr })
            .Join(await _driverRepository.GetQueryAsync(),
                x => x.OrderLineTruck.DriverId,
                d => d.Id,
                (x, d) => new { x.OrderLine, x.OrderLineTruck, x.Order, x.Customer, x.FreightItem, x.Location, x.Truck, Driver = d })
            .GroupJoin(await _ticketRepository.GetQueryAsync(),
                x => new { OrderLineId = (int?)x.OrderLine.Id, TruckId = (int?)x.OrderLineTruck.TruckId },
                t => new { t.OrderLineId, t.TruckId },
                (x, tickets) => new { x.OrderLine, x.OrderLineTruck, x.Order, x.Customer, x.FreightItem, x.Location, x.Truck, x.Driver, Tickets = tickets })
            .SelectMany(x =>
                x.Tickets.DefaultIfEmpty(),
                (x, t) => new { x.OrderLine, x.Order, x.Customer, x.FreightItem, x.Location, x.Truck, x.Driver, Ticket = t })
            .Where(x =>
                x.Ticket == default
                && x.OrderLine.TenantId == tenantId
                && x.Order.DeliveryDate >= input.DeliveryDateBegin && x.Order.DeliveryDate <= input.DeliveryDateEnd
                && !x.OrderLine.IsCancelled
                && !x.OrderLine.IsDeleted
                && !x.Order.IsDeleted)
            .OrderBy(x => x.Customer.Name)
            .Select(s => new JobsMissingTicket
            {
                CustomerName = s.Customer.Name,
                ItemName = s.FreightItem.Name,
                DeliveryDate = s.Order.DeliveryDate,
                OrderId = s.OrderLine.OrderId,
                DeliverTo = s.Location.DisplayName,
                TruckCode = s.Truck.TruckCode,
                Driver = $"{s.Driver.LastName}, {s.Driver.FirstName}",
            });

        return await query.ToListAsync();
    }
}
