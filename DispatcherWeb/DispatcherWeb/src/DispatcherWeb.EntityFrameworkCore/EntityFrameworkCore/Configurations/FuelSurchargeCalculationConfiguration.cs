using DispatcherWeb.FuelSurchargeCalculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class FuelSurchargeCalculationConfiguration : IEntityTypeConfiguration<FuelSurchargeCalculation>
    {
        public void Configure(EntityTypeBuilder<FuelSurchargeCalculation> builder)
        {
            builder
                .Property(e => e.FreightRatePercent)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
