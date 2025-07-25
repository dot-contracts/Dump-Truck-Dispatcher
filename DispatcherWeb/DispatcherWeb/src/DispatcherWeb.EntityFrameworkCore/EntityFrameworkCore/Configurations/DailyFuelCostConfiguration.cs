using DispatcherWeb.DailyFuelCosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class DailyFuelCostConfiguration : IEntityTypeConfiguration<DailyFuelCost>
    {
        public void Configure(EntityTypeBuilder<DailyFuelCost> builder)
        {
            builder
                .HasIndex(e => new { e.TenantId, e.Date })
                .IsUnique()
                .HasFilter($"[{nameof(DailyFuelCost.IsDeleted)}] = 0");
        }
    }
}
