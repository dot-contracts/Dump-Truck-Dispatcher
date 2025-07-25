using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Imports.Dto;

namespace DispatcherWeb.Imports
{
    [AbpAuthorize(AppPermissions.Pages_Imports)]
    public class ImportScheduleAppService : DispatcherWebAppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public ImportScheduleAppService(
            IBackgroundJobManager backgroundJobManager
        )
        {
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task ScheduleImport(ScheduleImportInput input)
        {
            if (input.ImportType != ImportType.FuelUsage && input.JacobusEnergy)
            {
                throw new ArgumentException("The ImportType must be FuelUsage when the JacobusEnergy is true!");
            }

            await _backgroundJobManager.EnqueueAsync<ImportJob, ImportJobArgs>(
                new ImportJobArgs
                {
                    RequestorUser = await AbpSession.ToUserIdentifierAsync(),
                    File = input.BlobName,
                    FieldMap = input.JacobusEnergy ? JacobusEnergyFieldMap : input.FieldMap,
                    ImportType = input.ImportType,
                    JacobusEnergy = input.JacobusEnergy,
                }
            );
        }

        private static FieldMapItem[] JacobusEnergyFieldMap =>
            new[]
            {
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.Office,
                    UserField = FuelUsageFromJacobusEnergyColumn.Office,
                },
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.TruckNumber,
                    UserField = FuelUsageFromJacobusEnergyColumn.TruckNumber,
                },
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.FuelDateTime,
                    UserField = FuelUsageFromJacobusEnergyColumn.FuelDateTime,
                },
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.Amount,
                    UserField = FuelUsageFromJacobusEnergyColumn.Amount,
                },
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.FuelRate,
                    UserField = FuelUsageFromJacobusEnergyColumn.FuelRate,
                },
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.Odometer,
                    UserField = FuelUsageFromJacobusEnergyColumn.Odometer,
                },
                new FieldMapItem
                {
                    StandardField = FuelUsageColumn.TicketNumber,
                    UserField = FuelUsageFromJacobusEnergyColumn.TicketNumber,
                },
            };
    }
}
