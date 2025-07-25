using DispatcherWeb.Auditing;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.Debugging;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.Trucks;
using Hangfire;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Infrastructure.RecurringJobs
{
    public static class StaticRecurringJobs
    {
        private const string EveryDay = "{0} {1} * * *";
        private const string WeeklyOnSunday = "{0} {1} * * 0";
        public static void CreateAll(IConfigurationRoot appConfiguration)
        {
            if (DebugHelper.IsDebug)
            {
                return;
            }

            //If you're getting an exception here, make sure you created an empty database (default name is DispatcherWebDb)
            RecurringJob.AddOrUpdate<ITruckTelematicsAppService>(
                JobIds.UpdateTrucksMileageAndHours,
                x => x.UpdateMileageForAllTenantsAsync(),
                string.Format(EveryDay, "30", "2") //2:30 AM UTC
            );

            RecurringJob.AddOrUpdate<ITruckTelematicsAppService>(
                JobIds.SyncWialonDeviceTypes,
                x => x.SyncWialonDeviceTypesInternal(),
                string.Format(WeeklyOnSunday, "20", "2") //2:20 AM UTC Sun
            );

            RecurringJob.AddOrUpdate<IDailyHistoryAppService>(
                JobIds.UpdateDailyHistoryTables,
                x => x.FillDailyHistoriesAsync(),
                string.Format(EveryDay, "30", "7") //03:30 EST / 07:30 UTC
            );

            RecurringJob.AddOrUpdate<IDriverApplicationAppService>(
                JobIds.RemoveOldDriverAppLogs,
                x => x.RemoveOldDriverApplicationLogs(),
                string.Format(EveryDay, "0", "6") //6:00 AM UTC, 2AM EST
                                                  //string.Format(EveryDay, "*/5", "*") //debug, every 5 minutes
            );

            RecurringJob.AddOrUpdate<IAuditLogAppService>(
                JobIds.RemoveOldAuditLogs,
                x => x.RemoveOldAuditLogsAsync(),
                string.Format(EveryDay, "0", "7") //7:00 AM UTC, 3AM EST
                                                  //string.Format(EveryDay, "*/5", "*") //debug, every 5 minutes
            );

            //commented out since this should only run on qa4, not on prod
            //RecurringJob.AddOrUpdate<ITruckMileageAndHoursAppService>(
            //    JobIds.AddDemoTrucksMileageAndHours,
            //    x => x.AddTrucksMileageAndHourForDayBeforeTickets(),
            //    string.Format(EveryDay, "00", "9") //9:00 AM UTC
            //    //string.Format(EveryDay, "*/2", "*") //debug, every 2 minutes          
            //);

            var uploadTruckPositionsJobCronExpression = appConfiguration["DtdTracker:UploadTruckPositionsJobCronExpression"];
            var dtdTrackerAccessToken = appConfiguration["DtdTracker:AccessToken"];
            if (!string.IsNullOrEmpty(uploadTruckPositionsJobCronExpression)
                && !string.IsNullOrEmpty(dtdTrackerAccessToken))
            {
                RecurringJob.AddOrUpdate<ITruckTelematicsAppService>(
                    JobIds.UploadTruckPositionsToWialon,
                    x => x.UploadTruckPositionsToWialonAsync(),
                    uploadTruckPositionsJobCronExpression
                );
            }
            else
            {
                RecurringJob.RemoveIfExists(JobIds.UploadTruckPositionsToWialon);
            }
        }

        private static class JobIds
        {
            public const string UpdateTrucksMileageAndHours = "Job_UpdateTrucksMileageAndHours";
            public const string SyncWialonDeviceTypes = "Job_SyncWialonDeviceTypes";
            public const string UpdateDailyHistoryTables = "Job_UpdateDailyHistoryTables";
            public const string RemoveOldDriverAppLogs = "Job_RemoveOldDriverAppLogs";
            public const string RemoveOldAuditLogs = "Job_RemoveOldAuditLogs";
            public const string AddDemoTrucksMileageAndHours = "Job_AddDemoTrucksMileageAndHours";
            public const string UploadTruckPositionsToWialon = "Job_UploadTruckPositionsToWialon";
        }
    }
}
