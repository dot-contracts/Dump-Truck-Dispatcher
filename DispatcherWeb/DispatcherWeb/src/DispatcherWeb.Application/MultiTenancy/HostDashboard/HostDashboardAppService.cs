using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Linq.Extensions;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;
using DispatcherWeb.MultiTenancy.HostDashboard.Exporting;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.SignalR;
using DispatcherWeb.SignalR.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.MultiTenancy.HostDashboard
{
    [DisableAuditing]
    [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
    public class HostDashboardAppService : DispatcherWebAppServiceBase, IHostDashboardAppService
    {
        private const int Top20Number = 20;
        private const int SubscriptionEndAlertDayCount = 30;
        private const int MaxExpiringTenantsShownCount = 10;
        private const int MaxRecentTenantsShownCount = 10;
        private const int RecentTenantsDayCount = 7;

        private readonly ISignalRCommunicator _signalRCommunicator;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Setting, long> _settingRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<TenantDailyHistory> _tenantDailyHistoryRepository;
        private readonly IRepository<UserDailyHistory> _userDailyHistoryRepository;
        private readonly IRepository<TransactionDailyHistory> _transactionDailyHistoryRepository;
        private readonly IRequestsCsvExporter _requestsCsvExporter;
        private readonly ITenantStatisticsCsvExporter _tenantStatisticsCsvExporter;
        private readonly IRepository<AuditLog, long> _auditLogRepository;

        public HostDashboardAppService(
            ISignalRCommunicator signalRCommunicator,
            IRepository<Order> orderRepository,
            IRepository<Setting, long> settingRepository,
            IRepository<Office> officeRepository,
            IEncryptionService encryptionService,
            ISubscriptionPaymentRepository subscriptionPaymentRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<TenantDailyHistory> tenantDailyHistoryRepository,
            IRepository<UserDailyHistory> userDailyHistoryRepository,
            IRepository<TransactionDailyHistory> transactionDailyHistoryRepository,
            IRequestsCsvExporter requestsCsvExporter,
            ITenantStatisticsCsvExporter tenantStatisticsCsvExporter,
            IRepository<AuditLog, long> auditLogRepository
        )
        {
            _signalRCommunicator = signalRCommunicator;
            _orderRepository = orderRepository;
            _settingRepository = settingRepository;
            _officeRepository = officeRepository;
            _encryptionService = encryptionService;
            _subscriptionPaymentRepository = subscriptionPaymentRepository;
            _tenantRepository = tenantRepository;
            _tenantDailyHistoryRepository = tenantDailyHistoryRepository;
            _userDailyHistoryRepository = userDailyHistoryRepository;
            _transactionDailyHistoryRepository = transactionDailyHistoryRepository;
            _requestsCsvExporter = requestsCsvExporter;
            _tenantStatisticsCsvExporter = tenantStatisticsCsvExporter;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<HostDashboardData> GetDashboardStatisticsData(GetDashboardDataInput input)
        {
            var subscriptionEndDateEndUtc = Clock.Now.ToUniversalTime().AddDays(SubscriptionEndAlertDayCount);
            var subscriptionEndDateStartUtc = Clock.Now.ToUniversalTime();
            var tenantCreationStartDate = Clock.Now.ToUniversalTime().AddDays(-RecentTenantsDayCount);

            return new HostDashboardData
            {
                DashboardPlaceholder1 = 125,
                DashboardPlaceholder2 = 830,
                NewTenantsCount = await GetTenantsCountByDate(input.StartDate, input.EndDate),
                NewSubscriptionAmount = await GetNewSubscriptionAmount(input.StartDate, input.EndDate),
                ExpiringTenants = await GetExpiringTenantsData(subscriptionEndDateStartUtc, subscriptionEndDateEndUtc, MaxExpiringTenantsShownCount),
                RecentTenants = await GetRecentTenantsData(tenantCreationStartDate, MaxRecentTenantsShownCount),
                MaxExpiringTenantsShownCount = MaxExpiringTenantsShownCount,
                MaxRecentTenantsShownCount = MaxRecentTenantsShownCount,
                SubscriptionEndAlertDayCount = SubscriptionEndAlertDayCount,
                RecentTenantsDayCount = RecentTenantsDayCount,
                SubscriptionEndDateStart = subscriptionEndDateStartUtc,
                SubscriptionEndDateEnd = subscriptionEndDateEndUtc,
                TenantCreationStartDate = tenantCreationStartDate,
            };
        }

        [HttpPost]
        public async Task<GetTenantStatisticsResult> GetTenantStatistics(GetDashboardDataInput input)
        {
            var startDate = input.StartDate;
            var endDate = input.EndDate;
            var yesterday = (await GetToday()).AddDays(-1);

            var tenants = await (await _tenantRepository.GetQueryAsync())
                .WhereIf(input.EditionIds?.Any() == true, t => input.EditionIds.Contains(t.EditionId))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .Select(t => new
                {
                    t.Id,
                    t.TenancyName,
                    EditionName = t.Edition.DisplayName,
                    t.CreationTime,
                }).ToListAsync();

            using var _ = UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);
            var periodUserStatisticsDtoList = await (await _userDailyHistoryRepository.GetQueryAsync())
                .Where(udh => udh.Date >= startDate && udh.Date <= endDate && udh.TenantId != null)
                .GroupBy(udh => new { udh.UserId, udh.TenantId })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    TenantId = g.Key.TenantId,
                })
                .GroupBy(u => u.TenantId)
                .Select(u => new
                {
                    TenantId = u.Key.Value,
                    NumberOfUsersActive = u.Count(),
                })
                .ToListAsync();

            var periodTenantStatisticsDtoList = await (await _tenantDailyHistoryRepository.GetQueryAsync())
                .Include(a => a.Tenant)
                .Where(tdh => tdh.Date >= startDate && tdh.Date <= endDate)
                .Select(x => new
                {
                    x.TenantId,
                    x.Tenant.TenancyName,
                    EditionName = x.Tenant.Edition.DisplayName,
                    x.Tenant.CreationTime,
                    x.OrderLinesCreated,
                    x.InternalTrucksScheduled,
                    x.LeaseHaulerScheduledDeliveries,
                    x.TicketsCreated,
                    x.SmsSent,
                    x.InvoicesCreated,
                    x.PayStatementsCreated,
                })
                .GroupBy(tdh => new { tdh.TenantId, tdh.TenancyName, tdh.EditionName })
                .Select(g => new
                {
                    TenantId = g.Key.TenantId,
                    TenantName = g.Key.TenancyName,
                    EditionName = g.Key.EditionName,
                    OrderLines = g.Sum(tdh => tdh.OrderLinesCreated),
                    TrucksScheduled = g.Sum(tdh => tdh.InternalTrucksScheduled),
                    LeaseHaulersOrderLines = g.Sum(tdh => tdh.LeaseHaulerScheduledDeliveries),
                    TicketsCreated = g.Sum(tdh => tdh.TicketsCreated),
                    SmsSent = g.Sum(tdh => tdh.SmsSent),
                    InvoicesCreated = g.Sum(tdh => tdh.InvoicesCreated),
                    PayStatementsCreated = g.Sum(tdh => tdh.PayStatementsCreated),
                }).ToListAsync();

            var yesterdayTenantStatisticsDtoList = await (await _tenantDailyHistoryRepository.GetQueryAsync())
                .Where(tdh => tdh.Date == yesterday)
                .GroupBy(tdh => new { tdh.TenantId, tdh.Tenant.TenancyName })
                .Select(g => new
                {
                    TenantId = g.Key.TenantId,
                    NumberOfTrucks = g.Sum(tdh => tdh.ActiveTrucks),
                    NumberOfUsers = g.Sum(tdh => tdh.ActiveUsers),
                })
                .ToListAsync();

            var combinedDtoQuery = (
                from t in tenants
                join pt in periodTenantStatisticsDtoList on t.Id equals pt.TenantId into ptj
                from pt in ptj.DefaultIfEmpty()
                join pu in periodUserStatisticsDtoList on t.Id equals pu.TenantId into puj
                from pu in puj.DefaultIfEmpty()
                join yt in yesterdayTenantStatisticsDtoList on t.Id equals yt.TenantId into ytj
                from yt in ytj.DefaultIfEmpty()
                select new TenantStatisticsDto
                {
                    TenantId = t.Id,
                    TenantName = t.TenancyName,
                    TenantEditionName = t.EditionName,
                    TenantCreationDate = t.CreationTime,
                    NumberOfTrucks = yt != null ? yt.NumberOfTrucks : 0,
                    NumberOfUsers = yt != null ? yt.NumberOfUsers : 0,
                    NumberOfUsersActive = pu != null ? pu.NumberOfUsersActive : 0,
                    OrderLines = pt != null ? pt.OrderLines : 0,
                    TrucksScheduled = pt != null ? pt.TrucksScheduled : 0,
                    LeaseHaulersOrderLines = pt != null ? pt.LeaseHaulersOrderLines : 0,
                    TicketsCreated = pt != null ? pt.TicketsCreated : 0,
                    SmsSent = pt != null ? pt.SmsSent : 0,
                    InvoicesCreated = pt != null ? pt.InvoicesCreated : 0,
                    PayStatementsCreated = pt != null ? pt.PayStatementsCreated : 0,
                }).ToList();

            var count = combinedDtoQuery.Count;

            var combinedDtoQueryable = (IQueryable<TenantStatisticsDto>)combinedDtoQuery
                .AsQueryable()
                .OrderBy(input.Sorting);

            if (!input.SuppressPaging)
            {
                combinedDtoQueryable = combinedDtoQueryable.PageBy(input);
            }

            var combinedDtoList = combinedDtoQueryable.ToList();

            var total = combinedDtoQuery
                .GroupBy(x => 1)
                .Select(x => new TenantStatisticsDto
                {
                    LeaseHaulersOrderLines = x.Sum(y => y.LeaseHaulersOrderLines),
                    NumberOfTrucks = x.Sum(y => y.NumberOfTrucks),
                    NumberOfUsers = x.Sum(y => y.NumberOfUsers),
                    NumberOfUsersActive = x.Sum(y => y.NumberOfUsersActive),
                    OrderLines = x.Sum(y => y.OrderLines),
                    SmsSent = x.Sum(y => y.SmsSent),
                    TrucksScheduled = x.Sum(y => y.TrucksScheduled),
                    TicketsCreated = x.Sum(y => y.TicketsCreated),
                    InvoicesCreated = x.Sum(y => y.InvoicesCreated),
                    PayStatementsCreated = x.Sum(y => y.PayStatementsCreated),
                }).FirstOrDefault() ?? new();

            return new GetTenantStatisticsResult(count, combinedDtoList)
            {
                Total = total,
            };
        }

        public async Task<PagedResultDto<RequestDto>> GetRequests(GetRequestsInput input)
        {
            var requests = await GetRequests(false, input);
            return requests;
        }

        private async Task<PagedResultDto<RequestDto>> GetRequests(bool getAll, GetRequestsInput input)
        {
            var today = await GetToday();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var startDate = input.StartDate ?? today.AddDays(-1);
                var endDate = input.EndDate ?? today.AddDays(-1);

                IQueryable<RequestDto> query = (await _transactionDailyHistoryRepository.GetQueryAsync())
                    .Where(tdh => tdh.Date >= startDate && tdh.Date <= endDate)
                    .GroupBy(tdh => new { tdh.ServiceName, tdh.MethodName })
                    .Select(g => new RequestDto
                    {
                        ServiceName = g.Key.ServiceName,
                        MethodName = g.Key.MethodName,
                        ServiceAndMethodName = g.Key.ServiceName + "." + g.Key.MethodName,
                        AverageExecutionDuration = g.Sum(x => x.NumberOfTransactions * x.AverageExecutionDuration) / g.Sum(x => x.NumberOfTransactions),
                        NumberOfTransactions = g.Sum(x => x.NumberOfTransactions),
                    })
                    .OrderBy(input.Sorting);

                var totalCount = await query.CountAsync();

                if (!getAll)
                {
                    query = query.PageBy(input);
                }

                var items = await query.ToListAsync();

                return new PagedResultDto<RequestDto>(totalCount, items);
            }
        }

        [HttpPost]
        public async Task<FileDto> GetRequestsToCsv(GetRequestsInput input)
        {
            var requests = await GetRequests(true, input);
            return await _requestsCsvExporter.ExportToFileAsync(requests.Items.ToList());
        }

        [HttpPost]
        public async Task<FileDto> GetTenantStatisticsToCsv(GetDashboardDataInput input)
        {
            var tenantStatistics = await GetTenantStatistics(input);

            var timezone = await GetTimezone();
            foreach (var item in tenantStatistics.Items)
            {
                item.TenantCreationDate = item.TenantCreationDate.ConvertTimeZoneTo(timezone);
            }

            return await _tenantStatisticsCsvExporter.ExportToFileAsync(tenantStatistics);
        }

        public async Task<PagedResultDto<MostRecentActiveUserDto>> GetMostRecentActiveUsers()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var hourAgoUtc = DateTime.UtcNow.AddHours(-1);
                var activeUsers = await (await _auditLogRepository.GetQueryAsync())
                    .OrderByDescending(a => a.ExecutionTime)
                    .Where(a => a.UserId.HasValue && a.TenantId.HasValue && a.ImpersonatorUserId == null && a.ExecutionTime > hourAgoUtc)
                    .GroupBy(a => new { a.UserId, a.TenantId })
                    .Select(g => new
                    {
                        g.Key.TenantId,
                        g.Key.UserId,
                        LastTransaction = g.Max(x => x.ExecutionTime),
                        NumberOfTransactions = g.Count(),
                    })
                    .Take(Top20Number)
                    .ToListAsync();

                var userIds = activeUsers.Select(x => x.UserId).Distinct().ToList();
                var tenantIds = activeUsers.Select(x => x.TenantId).Distinct().ToList();
                var users = await (await UserManager.GetQueryAsync())
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new
                    {
                        u.Id,
                        FullName = u.Name + " " + u.Surname,
                    }).ToListAsync();
                var tenants = await (await TenantManager.GetQueryAsync())
                    .Where(t => tenantIds.Contains(t.Id))
                    .Select(t => new
                    {
                        t.Id,
                        t.TenancyName,
                    }).ToListAsync();

                var userList = activeUsers
                    .Select(a => new MostRecentActiveUserDto
                    {
                        TenancyName = tenants.FirstOrDefault(t => t.Id == a.TenantId)?.TenancyName,
                        FullName = users.FirstOrDefault(u => u.Id == a.UserId)?.FullName,
                        LastTransaction = a.LastTransaction,
                        NumberOfTransactions = a.NumberOfTransactions,
                    })
                    .Where(a => a.LastTransaction > hourAgoUtc)
                    .ToList();

                return new PagedResultDto<MostRecentActiveUserDto>(userList.Count, userList);
            }
        }

        public async Task<HostDashboardKpiDto> GetDashboardKpiData(GetDashboardDataInput input)
        {
            var startDate = input.StartDate;
            var endDate = input.EndDate;
            var yesterday = Clock.Now.ToUniversalTime().Date.AddDays(-1);
            var tdhYesterdayQuery = (await _tenantDailyHistoryRepository.GetQueryAsync()).Where(tdh => tdh.Date == yesterday && !tdh.Tenant.IsDeleted);
            var yesterdayKpiDto = await tdhYesterdayQuery
                .GroupBy(tdh => 1)
                .Select(g => new
                {
                    ActiveTenants = g.Count(t => t.Tenant.IsActive),
                    ActiveTrucks = g.Sum(tdh => tdh.ActiveTrucks),
                    ActiveUsers = g.Sum(tdh => tdh.ActiveUsers),
                })
                .FirstOrDefaultAsync();

            var tdhIntervalQuery = (await _tenantDailyHistoryRepository.GetQueryAsync()).Where(tdh => tdh.Date >= startDate && tdh.Date <= endDate && !tdh.Tenant.IsDeleted);
            var intervalKpiDto = await tdhIntervalQuery
                .GroupBy(tdh => 1)
                .Select(g => new
                {
                    OrderLinesCreated = g.Sum(tdh => tdh.OrderLinesCreated),
                    InternalTrucksScheduled = g.Sum(tdh => tdh.InternalTrucksScheduled),
                    InternalScheduledDeliveries = g.Sum(tdh => tdh.InternalScheduledDeliveries),
                    LeaseHaulerScheduledDeliveries = g.Sum(tdh => tdh.LeaseHaulerScheduledDeliveries),
                    TicketsCreated = g.Sum(tdh => tdh.TicketsCreated),
                    SmsSent = g.Sum(tdh => tdh.SmsSent),
                })
                .FirstOrDefaultAsync();

            var intervalUserKpiDto = await (await _userDailyHistoryRepository.GetQueryAsync())
                .Where(udh => udh.Date >= startDate && udh.Date <= endDate && udh.TenantId != null)
                .GroupBy(udh => udh.UserId)
                .CountAsync();

            return new HostDashboardKpiDto
            {
                ActiveTenants = yesterdayKpiDto?.ActiveTenants ?? 0,
                ActiveTrucks = yesterdayKpiDto?.ActiveTrucks ?? 0,
                ActiveUsers = yesterdayKpiDto?.ActiveUsers ?? 0,

                OrderLinesCreated = intervalKpiDto?.OrderLinesCreated ?? 0,
                InternalTrucksScheduled = intervalKpiDto?.InternalTrucksScheduled ?? 0,
                InternalScheduledDeliveries = intervalKpiDto?.InternalScheduledDeliveries ?? 0,
                LeaseHaulerScheduledDeliveries = intervalKpiDto?.LeaseHaulerScheduledDeliveries ?? 0,
                TicketsCreated = intervalKpiDto?.TicketsCreated ?? 0,
                SmsSent = intervalKpiDto?.SmsSent ?? 0,

                UsersWithActivity = intervalUserKpiDto,
            };
        }

        private async Task<decimal> GetNewSubscriptionAmount(DateTime startDate, DateTime endDate)
        {
            return await (await _subscriptionPaymentRepository.GetQueryAsync())
                .Where(s => s.CreationTime >= startDate
                            && s.CreationTime <= endDate
                            && s.Status == SubscriptionPaymentStatus.Completed)
                .Select(x => x.Amount)
                .SumAsync();
        }

        private async Task<int> GetTenantsCountByDate(DateTime startDate, DateTime endDate)
        {
            return await (await _tenantRepository.GetQueryAsync())
                .Where(t => t.CreationTime >= startDate && t.CreationTime <= endDate)
                .CountAsync();
        }

        private async Task<List<ExpiringTenant>> GetExpiringTenantsData(DateTime subscriptionEndDateStartUtc, DateTime subscriptionEndDateEndUtc, int? maxExpiringTenantsShownCount = null)
        {
            var query = (await _tenantRepository.GetQueryAsync()).Where(t =>
                    t.SubscriptionEndDateUtc.HasValue
                    && t.SubscriptionEndDateUtc.Value >= subscriptionEndDateStartUtc
                    && t.SubscriptionEndDateUtc.Value <= subscriptionEndDateEndUtc)
                .Select(t => new
                {
                    t.Name,
                    t.SubscriptionEndDateUtc,
                });

            if (maxExpiringTenantsShownCount.HasValue)
            {
                query = query.Take(maxExpiringTenantsShownCount.Value);
            }

            var rawData = await query.ToListAsync();

            return rawData
                .Select(t => new ExpiringTenant
                {

                    TenantName = t.Name,
                    RemainingDayCount = Convert.ToInt32(t.SubscriptionEndDateUtc.Value.Subtract(subscriptionEndDateStartUtc).TotalDays),
                })
                .OrderBy(t => t.RemainingDayCount)
                .ThenBy(t => t.TenantName)
                .ToList();
        }

        private async Task<List<RecentTenant>> GetRecentTenantsData(DateTime creationDateStart, int? maxRecentTenantsShownCount = null)
        {
            var query = (await _tenantRepository.GetQueryAsync())
                .Where(t => t.CreationTime >= creationDateStart)
                .OrderByDescending(t => t.CreationTime);

            if (maxRecentTenantsShownCount.HasValue)
            {
                query = (IOrderedQueryable<Tenant>)query.Take(maxRecentTenantsShownCount.Value);
            }

            return await query
                .Select(t => new RecentTenant
                {
                    Id = t.Id,
                    Name = t.Name,
                    CreationTime = t.CreationTime,
                })
                .ToListAsync();
        }

        public async Task<TopStatsData> GetTopStatsData(GetTopStatsInput input)
        {
            return new TopStatsData
            {
                DashboardPlaceholder1 = 125,
                DashboardPlaceholder2 = 830,
                NewTenantsCount = await GetTenantsCountByDate(input.StartDate, input.EndDate),
                NewSubscriptionAmount = await GetNewSubscriptionAmount(input.StartDate, input.EndDate),
            };
        }

        public async Task<GetRecentTenantsOutput> GetRecentTenantsData()
        {
            var tenantCreationStartDate = Clock.Now.ToUniversalTime().AddDays(-RecentTenantsDayCount);

            var recentTenants = await GetRecentTenantsData(tenantCreationStartDate, MaxRecentTenantsShownCount);

            return new GetRecentTenantsOutput
            {
                RecentTenants = recentTenants,
                TenantCreationStartDate = tenantCreationStartDate,
                RecentTenantsDayCount = RecentTenantsDayCount,
                MaxRecentTenantsShownCount = MaxRecentTenantsShownCount,
            };
        }

        public async Task<GetExpiringTenantsOutput> GetSubscriptionExpiringTenantsData()
        {
            var subscriptionEndDateEndUtc = Clock.Now.ToUniversalTime().AddDays(SubscriptionEndAlertDayCount);
            var subscriptionEndDateStartUtc = Clock.Now.ToUniversalTime();

            var expiringTenants = await GetExpiringTenantsData(subscriptionEndDateStartUtc, subscriptionEndDateEndUtc,
                MaxExpiringTenantsShownCount);

            return new GetExpiringTenantsOutput
            {
                ExpiringTenants = expiringTenants,
                MaxExpiringTenantsShownCount = MaxExpiringTenantsShownCount,
                SubscriptionEndAlertDayCount = SubscriptionEndAlertDayCount,
                SubscriptionEndDateStart = subscriptionEndDateStartUtc,
                SubscriptionEndDateEnd = subscriptionEndDateEndUtc,
            };
        }

        private async Task<string> SendDebugMessage(string message, LogLevel logLevel)
        {
            message = $"{Clock.Now:O} - {message}";
            await _signalRCommunicator.SendDebugMessage(new DebugMessage(logLevel, message));
            return message;
        }

        private async Task<string> Debug(string message)
        {
            return await SendDebugMessage(message, LogLevel.Debug);
        }

        private async Task<string> Info(string message)
        {
            return await SendDebugMessage(message, LogLevel.Information);
        }

        private async Task<string> Warn(string message)
        {
            return await SendDebugMessage(message, LogLevel.Warning);
        }

        private async Task<string> Error(string message)
        {
            return await SendDebugMessage(message, LogLevel.Error);
        }

        [UnitOfWork(IsDisabled = true)]
        public async Task MigratePlaintextSettings()
        {
            const string obsoleteEncryptedSettingSuffix = "Obsolete";

            await Info("Step 1: Encrypting plaintext AbpSettings");
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                try
                {
                    using var _ = CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant);
                    var settingNamesToMigrate = new[]
                    {
                        AppSettings.Sms.AccountSid,
                        AppSettings.Sms.AuthToken,
                        AppSettings.GpsIntegration.Geotab.Password,
                        AppSettings.GpsIntegration.Samsara.ApiToken,
                        AppSettings.Invoice.Quickbooks.CsrfToken,
                        AppSettings.GpsIntegration.IntelliShift.Password,
                        AppSettings.FulcrumIntegration.Password,
                    };

                    foreach (var settingName in settingNamesToMigrate)
                    {
                        var settingNameObsolete = settingName + obsoleteEncryptedSettingSuffix;
                        await Debug($"Receiving settings for {settingName}...");
                        var settings = await (await _settingRepository.GetQueryAsync())
                            .Where(s => s.Name == settingNameObsolete && !string.IsNullOrEmpty(s.Value))
                            .ToListAsync();
                        await Debug($"Processing {settings.Count} settings for {settingName}...");

                        foreach (var setting in settings)
                        {
                            bool alreadyEncrypted;
                            try
                            {
                                var decryptedValue = _encryptionService.DecryptIfNotEmpty(setting.Value);
                                alreadyEncrypted = !string.IsNullOrEmpty(decryptedValue);
                            }
                            catch
                            {
                                alreadyEncrypted = false;
                            }
                            if (alreadyEncrypted)
                            {
                                await Warn($"Setting {setting.Value} for tenant {setting.TenantId} was already encrypted");
                            }
                            else
                            {
                                setting.Value = _encryptionService.EncryptIfNotEmpty(setting.Value);
                            }
                            //remove the obsolete suffix from the setting name
                            setting.Name = settingName;
                        }
                        await Debug("Saving changes...");
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await Debug("Saved changes");
                    }
                }
                catch (Exception ex)
                {
                    await Error($"Error during AbpSettings migration: {ex.Message}");
                    throw;
                }
            });
            await Info("Step 1 completed.");

            await Info("Migration completed.");
        }
    }
}
