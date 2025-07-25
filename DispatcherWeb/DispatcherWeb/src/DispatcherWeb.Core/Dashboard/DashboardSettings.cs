using System.Linq;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;

namespace DispatcherWeb.Dashboard
{
    public static class DashboardSettings
    {
        public static class ScheduledTruckCounters
        {
            public static DashboardSetting TrucksRequestedToday = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.TrucksRequestedToday,
                SettingLocalizableLabel = "TrucksRequestedToday",
                PermissionName = AppPermissions.Pages_Dashboard_Dispatching,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting TrucksScheduledToday = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.TrucksScheduledToday,
                SettingLocalizableLabel = "TrucksScheduledToday",
                PermissionName = AppPermissions.Pages_Dashboard_Dispatching,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting TrucksRequestedTomorrow = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.TrucksRequestedTomorrow,
                SettingLocalizableLabel = "TrucksRequestedTomorrow",
                PermissionName = AppPermissions.Pages_Dashboard_Dispatching,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting TrucksScheduledTomorrow = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.TrucksScheduledTomorrow,
                SettingLocalizableLabel = "TrucksScheduledTomorrow",
                PermissionName = AppPermissions.Pages_Dashboard_Dispatching,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting[] All => new[]
            {
                TrucksRequestedToday,
                TrucksScheduledToday,
                TrucksRequestedTomorrow,
                TrucksScheduledTomorrow,
            };
        }

        public static DashboardSetting TruckAvailabilityChart = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.TruckAvailabilityChart,
            SettingLocalizableLabel = "TruckAvailabilityChart",
            PermissionName = AppPermissions.Pages_Dashboard_TruckMaintenance,
        };

        public static DashboardSetting ServiceStatusChart = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.ServiceStatusChart,
            SettingLocalizableLabel = "ServiceStatusChart",
            PermissionName = AppPermissions.Pages_Dashboard_TruckMaintenance,
        };

        public static DashboardSetting LicensePlateStatusChart = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.LicensePlateStatusChart,
            SettingLocalizableLabel = "LicensePlateStatusChart",
            PermissionName = AppPermissions.Pages_Dashboard_TruckMaintenance,
        };

        public static DashboardSetting DriverLicenseStatusChart = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.DriverLicenseStatusChart,
            SettingLocalizableLabel = "DriverLicenseStatusChart",
            PermissionName = AppPermissions.Pages_Dashboard_DriverDotRequirements,
        };

        public static DashboardSetting PhysicalStatusChart = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.PhysicalStatusChart,
            SettingLocalizableLabel = "PhysicalStatusChart",
            PermissionName = AppPermissions.Pages_Dashboard_DriverDotRequirements,
        };

        public static DashboardSetting DriverMvrStatusChart = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.DriverMvrStatusChart,
            SettingLocalizableLabel = "DriverMvrStatusChart",
            PermissionName = AppPermissions.Pages_Dashboard_DriverDotRequirements,
        };

        public static class RevenueCharts
        {
            public static DashboardSetting Revenue = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.Revenue,
                SettingLocalizableLabel = "Revenue",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting RevenuePerTruck = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.RevenuePerTruck,
                SettingLocalizableLabel = "RevenuePerTruck",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting FuelCost = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.FuelCost,
                SettingLocalizableLabel = "FuelCost",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting AdjustedRevenue = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.AdjustedRevenue,
                SettingLocalizableLabel = "AdjustedRevenue",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting AverageAdjustedRevenuePerTruck = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.AverageAdjustedRevenuePerTruck,
                SettingLocalizableLabel = "AverageAdjustedRevenuePerTruck",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting AverageFuelCostPerMile = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.AverageFuelCostPerMile,
                SettingLocalizableLabel = "AverageFuelCostPerMile",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting AverageRevenuePerHour = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.AverageRevenuePerHour,
                SettingLocalizableLabel = "AverageRevenuePerHour",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting AverageRevenuePerMile = new DashboardSetting
            {
                SettingName = AppSettings.Dashboard.AverageRevenuePerMile,
                SettingLocalizableLabel = "AverageRevenuePerMile",
                PermissionName = AppPermissions.Pages_Dashboard_Revenue,
                FeatureName = AppFeatures.PaidFunctionality,
            };

            public static DashboardSetting[] All => new[]
            {
                Revenue,
                RevenuePerTruck,
                FuelCost,
                AdjustedRevenue,
                AverageAdjustedRevenuePerTruck,
                AverageFuelCostPerMile,
                AverageRevenuePerHour,
                AverageRevenuePerMile,
            };
        }

        public static DashboardSetting RevenuePerTruckByDateGraph = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.RevenuePerTruckByDateGraph,
            SettingLocalizableLabel = "RevenuePerTruckByDateGraph",
            PermissionName = AppPermissions.Pages_Dashboard_Revenue,
            FeatureName = AppFeatures.PaidFunctionality,
        };

        public static DashboardSetting RevenueByDateGraph = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.RevenueByDateGraph,
            SettingLocalizableLabel = "RevenueByDateGraph",
            PermissionName = AppPermissions.Pages_Dashboard_Revenue,
            FeatureName = AppFeatures.PaidFunctionality,
        };

        public static DashboardSetting TruckUtilizationGraph = new DashboardSetting
        {
            SettingName = AppSettings.Dashboard.TruckUtilizationGraph,
            SettingLocalizableLabel = "TruckUtilizationGraph",
            PermissionName = AppPermissions.Pages_Dashboard_TruckUtilization,
            FeatureName = AppFeatures.PaidFunctionality,
        };

        public static DashboardSetting[] All =>
            ScheduledTruckCounters.All
            .Union(new[]
            {
                TruckAvailabilityChart,
                ServiceStatusChart,
                LicensePlateStatusChart,
                DriverLicenseStatusChart,
                PhysicalStatusChart,
                DriverMvrStatusChart,
            })
            .Union(RevenueCharts.All)
            .Union(new[]
            {
                RevenuePerTruckByDateGraph,
                RevenueByDateGraph,
                TruckUtilizationGraph,
            })
            .ToArray();
    }
}
