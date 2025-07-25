using DispatcherWeb.TaxRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
    {
        public void Configure(EntityTypeBuilder<TaxRate> builder)
        {
            builder
                .Property(e => e.Rate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
