using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Application.Infrastructure.Utilities;
using DispatcherWeb.Authorization;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Infrastructure.Reports.Dto;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.ScheduledReports.Dto;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.ScheduledReports
{
    [AbpAuthorize(AppPermissions.Pages_Reports_ScheduledReports)]
    public class ScheduledReportAppService : DispatcherWebAppServiceBase, IScheduledReportAppService
    {
        private readonly IRepository<ScheduledReport> _scheduledReportRepository;

        public ScheduledReportAppService(
            IRepository<ScheduledReport> scheduledReportRepository
        )
        {
            _scheduledReportRepository = scheduledReportRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_ScheduledReports)]
        public async Task<PagedResultDto<ScheduledReportDto>> GetScheduledReportPagedList(GetScheduledReportPagedListInput input)
        {
            var query = await _scheduledReportRepository.GetQueryAsync()
                ;

            var totalCount = await query.CountAsync();
            var rawItems = await query
                .Select(x => new
                {
                    x.Id,
                    ReportName = x.ReportType,
                    x.SendTo,
                    x.ReportFormat,
                    x.ScheduleTime,
                    x.SendOnDaysOfWeek,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var todayUtc = Clock.Now.Date;
            var timeZone = await GetTimezone();

            var items = rawItems
                .Select(x => new ScheduledReportDto
                {
                    Id = x.Id,
                    ReportName = x.ReportName.GetDisplayName(),
                    SendTo = x.SendTo,
                    ReportFormat = x.ReportFormat.GetDisplayName(),
                    ScheduleTime = todayUtc.Add(x.ScheduleTime).ConvertTimeZoneTo(timeZone).ToString("HH:mm"),
                    SendOn = x.SendOnDaysOfWeek.ToDayOfWeekEnumerable().Select(d => d.GetDisplayName()).JoinAsString(", "),
                })
                .ToList();

            return new PagedResultDto<ScheduledReportDto>(totalCount, items);

        }

        [AbpAuthorize(AppPermissions.Pages_Reports_ScheduledReports)]
        public async Task<ScheduledReportEditDto> GetScheduledReportForEdit(NullableIdDto input)
        {
            ScheduledReportEditDto dto;
            if (input.Id.HasValue)
            {
                var entity = await (await _scheduledReportRepository.GetQueryAsync())
                    .Select(x => new
                    {
                        x.Id,
                        x.ReportType,
                        x.ReportFormat,
                        x.ScheduleTime,
                        x.SendOnDaysOfWeek,
                        x.SendTo,
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);

                var todayUtc = Clock.Now.Date;

                dto = new ScheduledReportEditDto
                {
                    Id = entity.Id,
                    ReportType = entity.ReportType,
                    ReportFormat = entity.ReportFormat,
                    ScheduleTime = todayUtc.Add(entity.ScheduleTime).ConvertTimeZoneTo(await GetTimezone()).ToString("HH:mm"),
                    SendOnDaysOfWeek = entity.SendOnDaysOfWeek.ToDayOfWeekEnumerable().Select(d => (int)d).ToArray(),
                    SendTo = entity.SendTo,
                };
            }
            else
            {
                dto = new ScheduledReportEditDto
                {
                };

            }
            return dto;
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_ScheduledReports)]
        public async Task<ScheduledReportEditDto> SaveScheduledReport(ScheduledReportEditDto model)
        {
            DateTime dt;
            if (!DateTime.TryParseExact(model.ScheduleTime, "h:mm tt", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dt))
            {
                throw new ValidationException($"Wrong format of the ScheduleTime: {model.ScheduleTime}");
            }
            dt = dt.ConvertTimeZoneFrom(await GetTimezone());
            TimeSpan scheduleTime = dt.TimeOfDay;

            ScheduledReport entity = model.Id != 0 ? await _scheduledReportRepository.GetAsync(model.Id) : new ScheduledReport();

            entity.ReportType = model.ReportType;
            entity.SendTo = model.SendTo;
            entity.ReportFormat = model.ReportFormat;
            entity.ScheduleTime = scheduleTime;
            entity.SendOnDaysOfWeek = model.SendOnDaysOfWeek.GetDayOfWeekBitFlag();

            if (model.Id != 0)
            {
                RemoveSheduleReport(entity.Id);
            }

            model.Id = await _scheduledReportRepository.InsertOrUpdateAndGetIdAsync(entity);
            await AddScheduledReportAsync(entity);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_ScheduledReports)]
        public async Task DeleteScheduledReport(EntityDto input)
        {
            RemoveSheduleReport(input.Id);
            await _scheduledReportRepository.DeleteAsync(input.Id);
        }

        private async Task AddScheduledReportAsync(ScheduledReport scheduledReport)
        {
            if (!await PermissionChecker.IsGrantedAsync(scheduledReport.ReportType.GetPermissionName()))
            {
                throw new AbpAuthorizationException($"You need have the {scheduledReport.ReportType.GetPermissionName()} permission for the {scheduledReport.ReportType} report.");
            }
            var scheduledReportGeneratorInput = new ScheduledReportGeneratorInput
            {
                ReportFormat = scheduledReport.ReportFormat,
                ReportType = scheduledReport.ReportType,
                EmailAddresses = EmailHelper.SplitEmailAddresses(scheduledReport.SendTo),
                CustomSession = new CustomSession(await AbpSession.GetTenantIdAsync(), AbpSession.GetUserId()),
            };

            RecurringJob.AddOrUpdate<IScheduledReportGeneratorAppService>(
                GetJobId(scheduledReport.Id),
                x => x.GenerateReport(scheduledReportGeneratorInput),
                Utility.GetCronString(scheduledReport.SendOnDaysOfWeek, scheduledReport.ScheduleTime)
            );
        }

        private void RemoveSheduleReport(int id)
        {
            RecurringJob.RemoveIfExists(GetJobId(id));
        }

        private string GetJobId(int id)
        {
            return $"ScheduledReport_{id}";
        }
    }
}
