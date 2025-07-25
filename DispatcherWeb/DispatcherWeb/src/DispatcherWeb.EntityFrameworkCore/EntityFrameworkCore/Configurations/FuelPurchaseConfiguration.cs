using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class FuelPurchaseConfiguration : IEntityTypeConfiguration<FuelPurchase>
    {
        public void Configure(EntityTypeBuilder<FuelPurchase> builder)
        {
            builder
                .Property(e => e.Amount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Rate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
