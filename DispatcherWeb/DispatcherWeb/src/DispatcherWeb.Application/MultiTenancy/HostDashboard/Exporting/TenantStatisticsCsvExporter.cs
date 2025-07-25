using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Exporting
{
    public class TenantStatisticsCsvExporter : CsvExporterBase, ITenantStatisticsCsvExporter
    {
        public TenantStatisticsCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(GetTenantStatisticsResult tenantStatistics)
        {
            return await CreateCsvFileAsync(
                "TenantStatistics.csv",
                () =>
                {
                    AddHeaderAndDataAndFooter(
                        tenantStatistics.Items.ToList(),
                        ("Tenant name", x => x.TenantName, "Total"),
                        ("Edition", x => x.TenantEditionName, ""),
                        ("Created Date", x => x.TenantCreationDate.ToString("d"), ""),
                        ("# Trucks", x => x.NumberOfTrucks.ToString("N0"), tenantStatistics.Total.NumberOfTrucks.ToString("N0")),
                        ("# Users", x => x.NumberOfUsers.ToString("N0"), tenantStatistics.Total.NumberOfUsers.ToString("N0")),
                        ("# Users Active", x => x.NumberOfUsersActive.ToString("N0"), tenantStatistics.Total.NumberOfUsersActive.ToString("N0")),
                        ("Order lines", x => x.OrderLines.ToString("N0"), tenantStatistics.Total.OrderLines.ToString("N0")),
                        ("Trucks Sched", x => x.TrucksScheduled.ToString("N0"), tenantStatistics.Total.TrucksScheduled.ToString("N0")),
                        ("LH Trucks Sched", x => x.LeaseHaulersOrderLines.ToString("N0"), tenantStatistics.Total.LeaseHaulersOrderLines.ToString("N0")),
                        ("Tickets", x => x.TicketsCreated.ToString("N0"), tenantStatistics.Total.TicketsCreated.ToString("N0")),
                        ("SMS", x => x.SmsSent.ToString("N0"), tenantStatistics.Total.SmsSent.ToString("N0")),
                        ("Invoices", x => x.InvoicesCreated.ToString("N0"), tenantStatistics.Total.InvoicesCreated.ToString("N0")),
                        ("Pay Statements", x => x.PayStatementsCreated.ToString("N0"), tenantStatistics.Total.PayStatementsCreated.ToString("N0"))
                    );
                }
            );
        }

    }
}
