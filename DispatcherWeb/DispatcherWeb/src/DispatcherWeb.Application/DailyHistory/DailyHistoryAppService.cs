using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Customers;
using DispatcherWeb.Invoices;
using DispatcherWeb.Orders;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Sms;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DailyHistory
{
    public class DailyHistoryAppService : DispatcherWebAppServiceBase, IDailyHistoryAppService
    {
        private readonly IRepository<TenantDailyHistory> _tenantDailyHistoryRepository;
        private readonly IRepository<UserDailyHistory> _userDailyHistoryRepository;
        private readonly IRepository<TransactionDailyHistory> _transactionDailyHistoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<AuditLog, long> _auditLogRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<SentSms> _sentSmsRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<PayStatement> _payStatementRepository;

        public DailyHistoryAppService(
            IRepository<TenantDailyHistory> tenantDailyHistoryRepository,
            IRepository<UserDailyHistory> userDailyHistoryRepository,
            IRepository<TransactionDailyHistory> transactionDailyHistoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<User, long> userRepository,
            IRepository<AuditLog, long> auditLogRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Customer> customerRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<SentSms> sentSmsRepository,
            IRepository<Invoice> invoiceRepository,
            IRepository<PayStatement> payStatementRepository
        )
        {
            _tenantDailyHistoryRepository = tenantDailyHistoryRepository;
            _userDailyHistoryRepository = userDailyHistoryRepository;
            _transactionDailyHistoryRepository = transactionDailyHistoryRepository;
            _truckRepository = truckRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _orderLineRepository = orderLineRepository;
            _customerRepository = customerRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _ticketRepository = ticketRepository;
            _sentSmsRepository = sentSmsRepository;
            _invoiceRepository = invoiceRepository;
            _payStatementRepository = payStatementRepository;
        }

        [RemoteService(false)]
        public async Task FillDailyHistoriesAsync()
        {
            var todayUtc = Clock.Now.Date;

            await FillTenantDailyHistoryAsync(todayUtc);
            await FillUserDailyHistoryAsync(todayUtc);
            await FillTransactionDailyHistoryAsync(todayUtc);
        }

        [HttpGet]
        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Settings)]
        public async Task FillDailyHistoriesForMonthAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var dayUtc = DateTime.UtcNow.Date.AddMonths(-1);
            while (dayUtc <= todayUtc)
            {
                await FillTenantDailyHistoryAsync(dayUtc);
                await FillUserDailyHistoryAsync(dayUtc);
                await FillTransactionDailyHistoryAsync(dayUtc);

                dayUtc = dayUtc.AddDays(1);
            }
        }

        [RemoteService(false)]
        public async Task FillTenantDailyHistoryAsync(DateTime todayUtc)
        {
            Logger.Info($"FillTenantDailyHistory() started at {DateTime.UtcNow:s}");
            var yesterdayUtc = todayUtc.AddDays(-1);

            await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions
            {
                IsTransactional = true,
                Timeout = TimeSpan.FromMinutes(10),
            }, async () =>
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var tenants = await (await TenantManager.GetQueryAsync()).Select(t => t.Id).ToListAsync();

                    var activeTrucks = await (await _truckRepository.GetQueryAsync())
                        .Where(t => t.CreationTime < todayUtc
                            && t.IsActive
                            && t.AlwaysShowOnSchedule
                            && t.VehicleCategory.IsPowered
                            && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true
                            && t.OfficeId != null
                        )
                        .GroupBy(t => t.TenantId)
                        .Select(t => new TenantDailyHistoryField
                        {
                            TenantId = t.Key,
                            Value = t.Count(),
                        })
                        .ToListAsync();

                    var activeUsers = await (await _userRepository.GetQueryAsync())
                        .Where(u => u.CreationTime < todayUtc && u.IsActive && u.TenantId != null)
                        .GroupBy(u => u.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key.Value,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var usersWithActivity = await (await _auditLogRepository.GetQueryAsync())
                        .Where(al =>
                            al.ExecutionTime >= yesterdayUtc
                            && al.ExecutionTime < todayUtc
                            && al.TenantId != null
                            && al.ImpersonatorUserId == null
                        )
                        .GroupBy(al => new { al.TenantId, al.UserId })
                        .Select(g => new
                        {
                            g.Key.TenantId,
                            g.Key.UserId,
                        })
                        .GroupBy(x => x.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key.Value,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var orderLinesScheduled = await (await _orderLineRepository.GetQueryAsync())
                        .Where(ol =>
                            ol.Order.DeliveryDate >= yesterdayUtc
                            && ol.Order.DeliveryDate < todayUtc
                            && ol.NumberOfTrucks > 0
                            && (ol.MaterialQuantity > 0 || ol.FreightQuantity > 0)
                        )
                        .GroupBy(ol => ol.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var orderLinesCreated = await (await _orderLineRepository.GetQueryAsync())
                        .Where(ol => ol.CreationTime >= yesterdayUtc && ol.CreationTime < todayUtc)
                        .GroupBy(ol => ol.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var activeCustomers = await (await _customerRepository.GetQueryAsync())
                        .Where(c => c.CreationTime < todayUtc && c.IsActive)
                        .GroupBy(c => c.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var internalTrucksScheduled = await (await _orderLineTruckRepository.GetQueryAsync())
                        .Where(olt =>
                            olt.CreationTime >= yesterdayUtc
                            && olt.CreationTime < todayUtc
                            && olt.Truck.VehicleCategory.IsPowered
                            && olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule != true
                            && olt.Truck.OfficeId != null
                        )
                        .GroupBy(olt => new { olt.TenantId, olt.TruckId })
                        .Select(g => new
                        {
                            g.Key.TenantId,
                            g.Key.TruckId,
                        })
                        .GroupBy(x => x.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var internalScheduledDeliveries = await (await _orderLineTruckRepository.GetQueryAsync())
                        .Where(olt =>
                            olt.OrderLine.Order.DeliveryDate >= yesterdayUtc
                            && olt.OrderLine.Order.DeliveryDate < todayUtc
                            && olt.Truck.VehicleCategory.IsPowered
                            && olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule != true
                            && olt.Truck.OfficeId != null
                        )
                        .GroupBy(olt => olt.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var leaseHaulerScheduledDeliveries = await (await _orderLineTruckRepository.GetQueryAsync())
                        .Where(olt =>
                            olt.CreationTime >= yesterdayUtc
                            && olt.CreationTime < todayUtc
                            && (olt.Truck.OfficeId == null || olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule)
                        )
                        .GroupBy(olt => olt.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var ticketsCreated = await (await _ticketRepository.GetQueryAsync())
                        .Where(t => t.CreationTime >= yesterdayUtc && t.CreationTime < todayUtc)
                        .GroupBy(t => t.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var smsSent = await (await _sentSmsRepository.GetQueryAsync())
                        .Where(x => x.CreationTime >= yesterdayUtc && x.CreationTime < todayUtc && x.TenantId != null)
                        .GroupBy(x => x.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key.Value,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var invoicesCreated = await (await _invoiceRepository.GetQueryAsync())
                        .Where(t => t.CreationTime >= yesterdayUtc && t.CreationTime < todayUtc)
                        .GroupBy(t => t.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    var payStatementsCreated = await (await _payStatementRepository.GetQueryAsync())
                        .Where(t => t.CreationTime >= yesterdayUtc && t.CreationTime < todayUtc)
                        .GroupBy(t => t.TenantId)
                        .Select(g => new TenantDailyHistoryField
                        {
                            TenantId = g.Key,
                            Value = g.Count(),
                        })
                        .ToListAsync();

                    await _tenantDailyHistoryRepository.DeleteAsync(tdh => tdh.Date == yesterdayUtc);
                    foreach (var tenant in tenants)
                    {
                        await _tenantDailyHistoryRepository.InsertAsync(new TenantDailyHistory
                        {
                            TenantId = tenant,
                            Date = yesterdayUtc,
                            ActiveTrucks = activeTrucks.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            ActiveUsers = activeUsers.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            UsersWithActivity = usersWithActivity.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            OrderLinesScheduled = orderLinesScheduled.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            OrderLinesCreated = orderLinesCreated.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            ActiveCustomers = activeCustomers.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            InternalTrucksScheduled = internalTrucksScheduled.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            InternalScheduledDeliveries = internalScheduledDeliveries.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            LeaseHaulerScheduledDeliveries = leaseHaulerScheduledDeliveries.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            TicketsCreated = ticketsCreated.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            SmsSent = smsSent.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            InvoicesCreated = invoicesCreated.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                            PayStatementsCreated = payStatementsCreated.SingleOrDefault(x => x.TenantId == tenant)?.Value ?? 0,
                        });
                    }
                }
            });
            Logger.Info($"FillTenantDailyHistory() ended at {DateTime.UtcNow:s}");
        }

        private class TenantDailyHistoryField
        {
            public int TenantId { get; set; }
            public int Value { get; set; }
        }

        [RemoteService(false)]
        public async Task FillUserDailyHistoryAsync(DateTime todayUtc)
        {
            Logger.Info($"FillUserDailyHistory() started at {DateTime.UtcNow:s}");

            var yesterdayUtc = todayUtc.AddDays(-1);

            await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions
            {
                IsTransactional = true,
                Timeout = TimeSpan.FromMinutes(10),
            }, async () =>
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var users = await (
                            from a in await _auditLogRepository.GetQueryAsync()
                            where a.TenantId.HasValue && a.ImpersonatorUserId == null && a.ExecutionTime < todayUtc && a.ExecutionTime >= yesterdayUtc
                            group a by a.UserId into u
                            where u.Key.HasValue
                            select new
                            {
                                UserId = u.Key.Value,
                                TenantId = u.First().TenantId,
                                NumberOfTransactions = u.Count(),
                            })
                        .ToListAsync();

                    await _userDailyHistoryRepository.DeleteAsync(tdh => tdh.Date == yesterdayUtc);
                    foreach (var user in users)
                    {
                        await _userDailyHistoryRepository.InsertAsync(new UserDailyHistory
                        {
                            Date = yesterdayUtc,
                            UserId = user.UserId,
                            TenantId = user.TenantId,
                            NumberOfTransactions = user.NumberOfTransactions,
                        });
                    }
                }
            });


            Logger.Info($"FillUserDailyHistory() ended at {DateTime.UtcNow:s}");
        }

        [RemoteService(false)]
        public async Task FillTransactionDailyHistoryAsync(DateTime todayUtc)
        {
            Logger.Info($"FillTransactionDailyHistory() started at {DateTime.UtcNow:s}");

            var yesterdayUtc = todayUtc.AddDays(-1);

            await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions
            {
                IsTransactional = true,
                Timeout = TimeSpan.FromMinutes(10),
            }, async () =>
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var transactions = await (
                            from a in await _auditLogRepository.GetQueryAsync()
                            where a.TenantId.HasValue && a.ImpersonatorUserId == null && a.ExecutionTime < todayUtc && a.ExecutionTime >= yesterdayUtc
                            group a by new { a.ServiceName, a.MethodName } into g
                            select new
                            {
                                SeviceName = g.Key.ServiceName,
                                MethodName = g.Key.MethodName,
                                NumberOfTransactions = g.Count(),
                                AverageExecutionDuration = g.Average(x => x.ExecutionDuration),
                            })
                        .ToListAsync();

                    await _transactionDailyHistoryRepository.DeleteAsync(tdh => tdh.Date == yesterdayUtc);
                    foreach (var transaction in transactions)
                    {
                        await _transactionDailyHistoryRepository.InsertAsync(new TransactionDailyHistory
                        {
                            Date = yesterdayUtc,
                            ServiceName = transaction.SeviceName,
                            MethodName = transaction.MethodName,
                            NumberOfTransactions = transaction.NumberOfTransactions,
                            AverageExecutionDuration = (int)transaction.AverageExecutionDuration,
                        });
                    }
                }
            });

            Logger.Info($"FillTransactionDailyHistory() ended at {DateTime.UtcNow:s}");
        }

    }
}
