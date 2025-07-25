using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [RemoteService(false)]
    public class ImportFuelUsageAppService : ImportTruckDataBaseAppService<ImportFuelUsageRow>, IImportFuelUsageAppService
    {
        private readonly IRepository<FuelPurchase> _fuelPurchaseRepository;
        private List<DateTime> _affectedDateList = new List<DateTime>();

        public ImportFuelUsageAppService(
            IRepository<Truck> truckRepository,
            IRepository<FuelPurchase> fuelPurchaseRepository
        ) : base(truckRepository)
        {
            _fuelPurchaseRepository = fuelPurchaseRepository;
        }

        protected override bool IsRowEmpty(ImportFuelUsageRow row)
        {
            return row.TruckNumber.IsNullOrWhiteSpace() || !row.FuelDateTime.HasValue || !row.Amount.HasValue;
        }

        protected override async Task<bool> ImportRowAsync(ImportFuelUsageRow row, int truckId)
        {
            Debug.Assert(row.FuelDateTime != null, "row.FuelDateTime != null");
            var utcFuelDateTime = ConvertLocalDateTimeToUtcDateTime(row.FuelDateTime.Value);

            var entity = await (await _fuelPurchaseRepository.GetQueryAsync())
                .FirstOrDefaultAsync(fp => fp.TruckId == truckId && fp.FuelDateTime == utcFuelDateTime);

            if (entity == null)
            {
                entity = new FuelPurchase
                {
                    TruckId = truckId,
                    FuelDateTime = utcFuelDateTime,
                };
                await _fuelPurchaseRepository.InsertAsync(entity);
            }
            UpdateFields(entity, row);

            _affectedDateList.AddIfNotContains(row.FuelDateTime.Value.Date);

            return true;
        }

        private static void UpdateFields(FuelPurchase entity, ImportFuelUsageRow row)
        {
            entity.Odometer = row.Odometer;
            entity.Rate = row.FuelRate;
            entity.Amount = row.Amount;
            entity.TicketNumber = row.TicketNumber;
        }
    }
}
