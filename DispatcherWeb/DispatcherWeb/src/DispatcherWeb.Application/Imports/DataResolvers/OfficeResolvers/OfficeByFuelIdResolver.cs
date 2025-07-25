using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Offices;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public class OfficeByFuelIdResolver : OfficeResolverBase, ITransientDependency, IOfficeResolver
    {
        protected readonly IRepository<Office> _officeRepository;
        public OfficeByFuelIdResolver(IRepository<Office> officeRepository)
        {
            _officeRepository = officeRepository;
        }

        protected override async Task<Dictionary<string, int>> GetOfficeStringValueIdDictionaryAsync()
        {
            return (await (await _officeRepository.GetQueryAsync())
                .Where(o => !o.FuelIds.IsNullOrEmpty())
                .Select(o => new { o.Id, o.FuelIds })
                .ToListAsync())
                .Select(o => new { o.Id, FuelIds = o.FuelIds.Split('|') })
                .SelectMany(o => o.FuelIds, (o, fuelId) => new { Id = o.Id, FuelId = fuelId })
                .ToDictionary(o => o.FuelId, o => o.Id);
        }
    }
}
