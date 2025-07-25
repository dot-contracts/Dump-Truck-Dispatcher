using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Infrastructure.Reports.Dto;
using DispatcherWeb.Offices;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    [AbpAuthorize(AppPermissions.Pages_Reports_OutOfServiceTrucks)]
    public class OutOfServiceTrucksReportAppService : ReportAppServiceBase<EmptyInput>
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Office> _officeRepository;

        public OutOfServiceTrucksReportAppService(
            IAttachmentHelper attachmentHelper,
            IRepository<Truck> truckRepository,
            IRepository<Office> officeRepository
        ) : base(attachmentHelper)
        {
            _truckRepository = truckRepository;
            _officeRepository = officeRepository;
        }

        protected override string ReportPermission => AppPermissions.Pages_Reports_OutOfServiceTrucks;
        protected override string ReportFileName => "OutOfServiceTrucks";
        protected override void InitPdfReport(PdfReport report)
        {
        }

        protected override Task<bool> CreatePdfReport(PdfReport report, EmptyInput input)
        {
            report.AddSection();
            return CreateReport(
                report,
                () => new InactiveTrucksTablePdf(report.Section),
                () => new OutOfServiceTrucksTablePdf(report.Section)
            );
        }

        protected override Task<bool> CreateCsvReport(CsvReport report, EmptyInput input)
        {
            return CreateReport(
                report,
                () => new InactiveTrucksTableCsv(report.CsvWriter),
                () => new OutOfServiceTrucksTableCsv(report.CsvWriter)
            );
        }

        [UnitOfWork]
        private async Task<bool> CreateReport(
            IReport report,
            Func<IInactiveTrucksTable> createInactiveTrucksTable,
            Func<IOutOfServiceTrucksTable> createOutOfServiceTrucksTable
        )
        {
            var today = (await GetLocalDateTimeNow()).Date;
            report.AddReportHeader($"Out of Service Trucks for {today:d}");

            var inactiveTrucks = await GetInactiveTrucks(today);
            var inactiveTrucksTable = createInactiveTrucksTable();
            inactiveTrucksTable.AddColumnHeaders("Division", "Inactive Trucks Due to Lack of Drivers");
            foreach (var row in inactiveTrucks)
            {
                inactiveTrucksTable.AddRow(row.DivisionName, row.InactiveTrucks.ToString("N0"));
            }
            report.AddEmptyLine();

            var outOfServiceTrucks = await GetOutOfServiceTrucks(today);
            foreach (var office in outOfServiceTrucks)
            {
                report.AddEmptyLine();
                report.AddHeader(office.OfficeName);
                if (office.OutOfServiceTrucks.Count == 0)
                {
                    report.AddText("All trucks are in service");
                    continue;
                }

                var outOfServiceTrucksTable = createOutOfServiceTrucksTable();
                outOfServiceTrucksTable.AddColumnHeaders("Truck number", "Out of service date", "# of days out of service", "Reason");
                foreach (var row in office.OutOfServiceTrucks)
                {
                    outOfServiceTrucksTable.AddRow(
                        row.TruckCode,
                        row.OutOfServiceDate.ToString("d"),
                        row.NumberOfDaysOutOfService.ToString("N0"),
                        row.Reason
                    );
                }
            }

            return true;
        }

        private async Task<List<InactiveTrucksDto>> GetInactiveTrucks(DateTime today)
        {
            var allOffices = await (await _officeRepository.GetQueryAsync())
                .Where(x => x.Trucks.Any())
                .Select(x => new InactiveTrucksDto
                {
                    DivisionName = x.Name,
                }).ToListAsync();

            var items = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.OfficeId != null && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true && x.IsActive && !x.IsOutOfService && x.VehicleCategory.IsPowered
                    && (x.DriverAssignments.Any(da => da.Date == today && da.DriverId == null) || !x.DriverAssignments.Any(da => da.Date == today))
                 )
                .GroupBy(x => x.OfficeId)
                .Select(x => new InactiveTrucksDto
                {
                    DivisionName = x.First().Office.Name,
                    InactiveTrucks = x.Count(),
                }).ToListAsync();

            foreach (var office in allOffices)
            {
                if (!items.Any(x => x.DivisionName == office.DivisionName))
                {
                    items.Add(office);
                }
            }

            items = items.OrderBy(x => x.DivisionName).ToList();

            return items;
        }

        private class InactiveTrucksDto
        {
            public string DivisionName { get; set; }
            public int InactiveTrucks { get; set; }
        }

        private async Task<List<OfficeDto>> GetOutOfServiceTrucks(DateTime today)
        {
            var timezone = await GetTimezone();
            var tomorrow = today.AddDays(1);
            var tomorrowInUtc = tomorrow.ConvertTimeZoneFrom(timezone);
            var query = (await _truckRepository.GetQueryAsync())
                .Where(x => x.OfficeId != null && x.IsActive)
                .GroupBy(x => x.OfficeId)
                .Select(g => new
                {
                    OfficeId = g.Key,
                    OfficeName = g.FirstOrDefault().Office.Name,
                    OutOfService = g
                        .Where(t =>
                            t.OutOfServiceHistories.Any(oosh => oosh.OutOfServiceDate < tomorrowInUtc && oosh.InServiceDate == null))
                        .Select(t => new
                        {
                            TruckCode = t.TruckCode,
                            OutOfServiceHistory = t.OutOfServiceHistories
                                .Where(oosh => oosh.OutOfServiceDate < tomorrowInUtc && oosh.InServiceDate == null)
                                .OrderBy(oosh => oosh.OutOfServiceDate)
                                .Select(oosh => new
                                {
                                    oosh.Id,
                                    oosh.OutOfServiceDate,
                                    oosh.Reason,
                                })
                                .FirstOrDefault(),
                        }),
                });
            var utcDateTime = Clock.Now;
            var result = (await query.ToListAsync())
                .Select(x => new OfficeDto
                {
                    OfficeName = x.OfficeName,
                    OutOfServiceTrucks = x.OutOfService.Select(t => new OutOfServiceTrucksDto
                    {
                        TruckCode = t.TruckCode,
                        OutOfServiceDate = t.OutOfServiceHistory.OutOfServiceDate,
                        NumberOfDaysOutOfService = (utcDateTime - t.OutOfServiceHistory.OutOfServiceDate).Days,
                        Reason = t.OutOfServiceHistory.Reason,
                    }).ToList(),
                })
                .OrderBy(x => x.OfficeName)
                .ToList();

            result.ForEach(x => x.OutOfServiceTrucks.ForEach(y => y.OutOfServiceDate = y.OutOfServiceDate.ConvertTimeZoneTo(timezone)));

            return result;
        }

        private class OfficeDto
        {
            public string OfficeName { get; set; }
            public List<OutOfServiceTrucksDto> OutOfServiceTrucks { get; set; }
        }

        private class OutOfServiceTrucksDto
        {
            public string TruckCode { get; set; }
            public DateTime OutOfServiceDate { get; set; }
            public int NumberOfDaysOutOfService { get; set; }
            public string Reason { get; set; }
        }

    }
}
