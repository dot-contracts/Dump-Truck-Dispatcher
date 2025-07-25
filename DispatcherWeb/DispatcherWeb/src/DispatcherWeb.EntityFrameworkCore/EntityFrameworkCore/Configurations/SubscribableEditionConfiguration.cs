using DispatcherWeb.Editions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class SubscribableEditionConfiguration : IEntityTypeConfiguration<SubscribableEdition>
    {
        public void Configure(EntityTypeBuilder<SubscribableEdition> builder)
        {
            builder
                .Property(e => e.AnnualPrice)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.DailyPrice)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MonthlyPrice)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.WeeklyPrice)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
