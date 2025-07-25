using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public abstract class ImportTruckDataBaseAppService<T> : ImportDataBaseAppService<T>, IImportDataBaseAppService where T : ITruckImportRow
    {
        private readonly IRepository<Truck> _truckRepository;

        protected ImportTruckDataBaseAppService(
            IRepository<Truck> truckRepository
        )
        {
            _truckRepository = truckRepository;
        }

        protected override async Task<bool> ImportRowAsync(T row)
        {
            int? officeId = null;
            if (!row.Office.IsNullOrEmpty())
            {
                officeId = await OfficeResolver.GetOfficeIdAsync(row.Office);
                if (officeId == null)
                {
                    _result.NotFoundOffices.Add(row.Office);
                    return false;
                }
            }
            else
            {
                var truckInOffices = await (await _truckRepository.GetQueryAsync())
                    .Where(t => t.TruckCode == row.TruckNumber && t.OfficeId != null)
                    .Select(t => new
                    {
                        t.TruckCode,
                        OfficeName = t.Office.Name,
                    })
                    .Distinct()
                    .ToListAsync();
                if (truckInOffices.Count > 1)
                {
                    _result.TruckCodeInOffices.AddRange(
                        truckInOffices.GroupBy(t => t.TruckCode).Select(g => (g.Key, g.Select(t => t.OfficeName).ToList())).ToList()
                    );
                    return false;
                }
            }

            var truckId = await GetTruckIdAsync(row.TruckNumber, officeId);
            if (truckId == null)
            {
                _result.NotFoundTrucks.Add(row.TruckNumber);
                return false;
            }

            return await ImportRowAsync(row, truckId.Value);
        }

        protected abstract Task<bool> ImportRowAsync(T row, int truckId);

        protected async Task<int?> GetTruckIdAsync(string truckNumber, int? officeId)
        {
            return await (await _truckRepository.GetQueryAsync())
                .Where(t => t.OfficeId != null) // Exclude Lease Hauler trucks
                .WhereIf(officeId.HasValue, t => t.OfficeId == officeId.Value)
                .Where(t => t.TruckCode == truckNumber)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();
        }
    }
}
