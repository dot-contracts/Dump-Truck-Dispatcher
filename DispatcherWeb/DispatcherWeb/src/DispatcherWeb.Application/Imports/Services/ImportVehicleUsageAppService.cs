using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [RemoteService(false)]
    public class ImportVehicleUsageAppService : ImportTruckDataBaseAppService<ImportVehicleUsageRow>, IImportVehicleUsageAppService
    {
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;

        public ImportVehicleUsageAppService(
            IRepository<Truck> truckRepository,
            IRepository<VehicleUsage> vehicleUsageRepository
        ) : base(truckRepository)
        {
            _vehicleUsageRepository = vehicleUsageRepository;
        }

        protected override bool IsRowEmpty(ImportVehicleUsageRow row)
        {
            return row.TruckNumber.IsNullOrWhiteSpace()
                || row.ReadingDateTime == null
                || row.EngineHours == null && row.OdometerReading == null;
        }

        protected override async Task<bool> ImportRowAsync(ImportVehicleUsageRow row, int truckId)
        {
            var isImported = false;
            if (row.OdometerReading.HasValue)
            {
                await CreateOrUpdateVehicleUsageAsync(ReadingType.Miles, row, truckId);
                isImported = true;
            }
            if (row.EngineHours.HasValue)
            {
                await CreateOrUpdateVehicleUsageAsync(ReadingType.Hours, row, truckId);
                isImported = true;
            }

            return isImported;

        }

        private async Task CreateOrUpdateVehicleUsageAsync(ReadingType readingType, ImportVehicleUsageRow row, int truckId)
        {
            Debug.Assert(row.ReadingDateTime != null, "row.ReadingDateTime != null");
            var utcReadingDateTime = ConvertLocalDateTimeToUtcDateTime(row.ReadingDateTime.Value);

            var entity = await (await _vehicleUsageRepository.GetQueryAsync())
                .FirstOrDefaultAsync(vu =>
                    vu.TruckId == truckId
                    && vu.ReadingDateTime == utcReadingDateTime
                    && vu.ReadingType == readingType
                );

            entity ??= new VehicleUsage
            {
                TruckId = truckId,
                ReadingDateTime = utcReadingDateTime,
            };

            UpdateFields(entity, GetReadingByType(readingType, row), readingType, row);

            await _vehicleUsageRepository.InsertOrUpdateAsync(entity);
        }

        private static decimal GetReadingByType(ReadingType readingType, ImportVehicleUsageRow row)
        {
            switch (readingType)
            {
                case ReadingType.Miles:
                    Debug.Assert(row.OdometerReading != null, "row.OdometerReading != null");
                    return row.OdometerReading.Value;
                case ReadingType.Hours:
                    Debug.Assert(row.EngineHours != null, "row.EngineHours != null");
                    return row.EngineHours.Value;
                default:
                    throw new ArgumentException($"Usupported ReadingType: {readingType}");
            }
        }

        private static void UpdateFields(VehicleUsage entity, decimal reaingValue, ReadingType readingType, ImportVehicleUsageRow row)
        {
            Debug.Assert(row.OdometerReading != null || row.EngineHours != null);
            Debug.Assert(readingType == ReadingType.Miles && row.OdometerReading != null || readingType == ReadingType.Hours && row.EngineHours != null);
            entity.ReadingType = readingType;
            entity.Reading = reaingValue;
        }
    }
}
