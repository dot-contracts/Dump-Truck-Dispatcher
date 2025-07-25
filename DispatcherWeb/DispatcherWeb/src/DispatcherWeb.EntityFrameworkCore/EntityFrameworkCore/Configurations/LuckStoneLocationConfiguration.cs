using DispatcherWeb.LuckStone;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LuckStoneLocationConfiguration : IEntityTypeConfiguration<LuckStoneLocation>
    {
        public void Configure(EntityTypeBuilder<LuckStoneLocation> builder)
        {
            builder
                .Property(e => e.Latitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);

            builder
                .Property(e => e.Longitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);
        }
    }
}
