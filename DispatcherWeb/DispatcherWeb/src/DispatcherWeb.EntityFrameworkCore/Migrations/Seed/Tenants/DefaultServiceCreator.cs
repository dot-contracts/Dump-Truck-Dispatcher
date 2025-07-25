using System.Threading.Tasks;
using DispatcherWeb.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultServiceCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultServiceCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            //if (!_context.Services.Any(x => x.TenantId == _tenantId && x.PredefinedServiceKind == PredefinedServiceKind.TemporarySite))
            //{
            //    _context.Services.Add(new Service { TenantId = _tenantId, Service1 = "Temporary site – see note", IsActive = true, PredefinedServiceKind = PredefinedServiceKind.TemporarySite });
            //    _context.SaveChanges();
            //}

            //if (!_context.Services.Any(x => x.TenantId == _tenantId && x.PredefinedServiceKind == PredefinedServiceKind.TemporarySupplier))
            //{
            //    _context.Services.Add(new Service { TenantId = _tenantId, Service1 = "Temporary supplier – see note", IsActive = true, PredefinedServiceKind = PredefinedServiceKind.TemporarySupplier });
            //    _context.SaveChanges();
            //}
        }

        public Task CreateAsync()
        {
            return Task.CompletedTask;
        }
    }
}
