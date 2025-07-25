using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Locations;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultLocationsCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultLocationsCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            if (!_context.LocationCategories.Any(x => x.TenantId == _tenantId))
            {
                var categories = GetDefaultCategories();
                categories.ToList().ForEach(x => _context.LocationCategories.Add(x));
                _context.SaveChanges();
            }
        }

        public async Task CreateAsync()
        {
            if (!await _context.LocationCategories.AnyAsync(x => x.TenantId == _tenantId))
            {
                var categories = GetDefaultCategories();
                categories.ToList().ForEach(x => _context.LocationCategories.Add(x));
                await _context.SaveChangesAsync();
            }
        }

        private LocationCategory[] GetDefaultCategories()
        {
            var categories = new[]
            {
                new LocationCategory { TenantId = _tenantId, Name = "Asphalt Plant", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.AsphaltPlant },
                new LocationCategory { TenantId = _tenantId, Name = "Concrete Plant", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.ConcretePlant },
                new LocationCategory { TenantId = _tenantId, Name = "Landfill/Recycling", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.LandfillOrRecycling },
                new LocationCategory { TenantId = _tenantId, Name = "Miscellaneous", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Miscellaneous },
                new LocationCategory { TenantId = _tenantId, Name = "Yard", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Yard },
                new LocationCategory { TenantId = _tenantId, Name = "Quarry", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Quarry },
                new LocationCategory { TenantId = _tenantId, Name = "Sand Pit", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.SandPit },
                new LocationCategory { TenantId = _tenantId, Name = "Project Site", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.ProjectSite },
                new LocationCategory { TenantId = _tenantId, Name = "Temporary", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Temporary },
                new LocationCategory { TenantId = _tenantId, Name = "Unknown Load Site", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.UnknownLoadSite },
                new LocationCategory { TenantId = _tenantId, Name = "Unknown Delivery Site", PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.UnknownDeliverySite },
            };
            return categories;
        }
    }
}
