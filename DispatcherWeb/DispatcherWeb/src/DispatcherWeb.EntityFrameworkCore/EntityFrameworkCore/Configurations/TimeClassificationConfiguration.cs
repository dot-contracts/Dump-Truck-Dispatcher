using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TimeClassificationConfiguration : IEntityTypeConfiguration<TimeClassification>
    {
        public void Configure(EntityTypeBuilder<TimeClassification> builder)
        {
            builder
                .Property(e => e.DefaultRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
