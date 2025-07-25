using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks.Exporting
{
    public class TruckListCsvExporter : CsvExporterBase, ITruckListCsvExporter
    {
        public TruckListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<TruckEditDto> truckEditDtos)
        {
            return await CreateCsvFileAsync(
                "TruckList.csv",
                () =>
                {
                    var headersAndData = new List<(string header, Func<TruckEditDto, string> dataSelector)>
                    {
                        (L("TruckCode"), x => x.TruckCode),
                        (L("Office"), x => x.OfficeName),
                        (L("Category"), x => x.VehicleCategoryName),
                        (L("Apportioned"), x => x.IsApportioned.ToYesNoString()),
                        (L("BedConstruction"), x => x.BedConstruction.GetDisplayName()),
                        (L("CanPullTrailer"), x => x.CanPullTrailer.ToYesNoString()),
                        (L("DefaultDriver"), x => x.DefaultDriverName),
                        (L("CurrentTrailer"), x => x.CurrentTrailerCode),
                        (L("IsActive"), x => x.IsActive.ToYesNoString()),
                        (L("InactivationDate"), x => x.InactivationDate?.ToShortDateString()),
                        (L("OutOfService"), x => x.IsOutOfService.ToYesNoString()),
                        (L("Reason"), x => x.Reason),
                        (L("AlwaysShowOnSchedule"), x => x.AlwaysShowOnSchedule.ToYesNoString()),
                        (L("Year"), x => x.Year?.ToString("N0")),
                        (L("Make"), x => x.Make),
                        (L("Model"), x => x.Model),
                        (L("InServiceDate"), x => x.InServiceDate.ToShortDateString()),
                        (L("VIN"), x => x.Vin),
                        (L("Plate"), x => x.Plate),
                        (L("PlateExpiration"), x => x.PlateExpiration?.ToShortDateString()),
                        ("Ave Load(Tons)", x => x.CargoCapacity?.ToString("N0")),
                        ("Ave Load(Cyds)", x => x.CargoCapacityCyds?.ToString("N0")),
                        (L("InsurancePolicyNumber"), x => x.InsurancePolicyNumber),
                        (L("InsuranceValidUntil"), x => x.InsuranceValidUntil?.ToShortDateString()),
                        (L("PurchaseDate"), x => x.PurchaseDate?.ToShortDateString()),
                        (L("PurchasePrice"), x => x.PurchasePrice?.ToString("N")),
                        (L("SoldDate"), x => x.SoldDate?.ToShortDateString()),
                        (L("SoldPrice"), x => x.SoldPrice?.ToString("N")),
                        (L("FuelType"), x => x.FuelType?.GetDisplayName()),
                        (L("FuelCapacity"), x => x.FuelCapacity?.ToString("N0")),
                        (L("SteerTires"), x => x.SteerTires),
                        (L("DriveAxleTires"), x => x.DriveAxleTires),
                        (L("DropAxleTires"), x => x.DropAxleTires),
                        (L("TrailerTires"), x => x.TrailerTires),
                        (L("Transmission"), x => x.Transmission),
                        (L("Engine"), x => x.Engine),
                        (L("RearEnd"), x => x.RearEnd),
                        (L("CurrentMileage"), x => x.CurrentMileage.ToString("N0")),
                        (L("CurrentHours"), x => x.CurrentHours.ToString("N0")),
                        (L("TruxTruckId"), x => x.TruxTruckId),
                        (L("UniqueId"), x => x.DtdTrackerUniqueId),
                        (L("Notes"), x => x.Notes),
                    };

                    foreach (var deviceType in Enum.GetValues(typeof(DeviceTypeEnum)).Cast<DeviceTypeEnum>())
                    {
                        var headerPrefix = deviceType.GetDisplayName() + " ";
                        headersAndData.Add((headerPrefix + L("Make"), x => GetMobileDevice(x, deviceType).Make));
                        headersAndData.Add((headerPrefix + L("Model"), x => GetMobileDevice(x, deviceType).Model));
                        headersAndData.Add((headerPrefix + L("Imei"), x => GetMobileDevice(x, deviceType).Imei));
                        headersAndData.Add((headerPrefix + L("SimId"), x => GetMobileDevice(x, deviceType).SimId));
                    }

                    AddHeaderAndData(
                        truckEditDtos,
                        headersAndData.ToArray()
                    );
                }
            );
        }

        private static MobileDeviceEditDto GetMobileDevice(TruckEditDto model, DeviceTypeEnum deviceType)
        {
            return model.MobileDevices.FirstOrDefault(x => x.DeviceType == deviceType) ?? new();
        }

    }
}
