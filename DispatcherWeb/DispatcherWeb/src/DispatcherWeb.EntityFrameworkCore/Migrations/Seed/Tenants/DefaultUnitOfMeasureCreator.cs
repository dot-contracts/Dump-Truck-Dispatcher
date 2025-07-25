using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultUnitOfMeasureCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultUnitOfMeasureCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            if (!_context.UnitsOfMeasure.Any(x => x.TenantId == _tenantId))
            {
                var uoms = GetDefaultUoms();

                foreach (var uom in uoms)
                {
                    uom.TenantId = _tenantId;
                    _context.UnitsOfMeasure.Add(uom);
                }

                _context.SaveChanges();
            }
        }

        public async Task CreateAsync()
        {
            if (!await _context.UnitsOfMeasure.AnyAsync(x => x.TenantId == _tenantId))
            {
                var uoms = GetDefaultUoms();

                foreach (var uom in uoms)
                {
                    uom.TenantId = _tenantId;
                    _context.UnitsOfMeasure.Add(uom);
                }

                await _context.SaveChangesAsync();
            }
        }

        private List<UnitOfMeasure> GetDefaultUoms()
        {
            return new[]
            {
                new UnitOfMeasure
                {
                    Name = "Hours",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.Hours,
                },
                new UnitOfMeasure
                {
                    Name = "Tons",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.Tons,
                },
                new UnitOfMeasure
                {
                    Name = "Loads",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.Loads,
                },
                new UnitOfMeasure
                {
                    Name = "Cubic Yards",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.CubicYards,
                },
                new UnitOfMeasure
                {
                    Name = "Each",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.Each,
                },
                new UnitOfMeasure
                {
                    Name = "Cubic Meters",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.CubicMeters,
                },
                new UnitOfMeasure
                {
                    Name = "Miles",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.Miles,
                },
                new UnitOfMeasure
                {
                    Name = "Drive Miles",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.DriveMiles,
                },
                new UnitOfMeasure
                {
                    Name = "Air Miles",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.AirMiles,
                },
                new UnitOfMeasure
                {
                    Name = "Drive KMs",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.DriveKMs,
                },
                new UnitOfMeasure
                {
                    Name = "Air KMs",
                    UnitOfMeasureBaseId = (int)UnitOfMeasureBaseEnum.AirKMs,
                },
            }.ToList();
        }
    }
}
