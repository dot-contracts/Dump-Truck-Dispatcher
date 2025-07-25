using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Offices;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public class OfficeByNameResolver : OfficeResolverBase, ITransientDependency, IOfficeResolver
    {
        private readonly IRepository<Office> _officeRepository;

        public OfficeByNameResolver(
            IRepository<Office> officeRepository
        )
        {
            _officeRepository = officeRepository;
        }

        protected override async Task<Dictionary<string, int>> GetOfficeStringValueIdDictionaryAsync()
        {
            return _officeStringValueIdDictionary = await (await _officeRepository.GetQueryAsync())
                .GroupBy(o => o.Name) // In case there are offices with the same name
                .Select(o => new { o.First().Id, Name = o.Key })
                .ToDictionaryAsync(o => o.Name.ToLowerInvariant(), o => o.Id);
        }
    }
}
