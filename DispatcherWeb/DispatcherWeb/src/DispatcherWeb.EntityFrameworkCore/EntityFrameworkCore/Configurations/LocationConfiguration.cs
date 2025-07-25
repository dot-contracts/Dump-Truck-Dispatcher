using DispatcherWeb.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder
                .HasMany(e => e.LocationContacts)
                .WithOne(e => e.Location)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.Latitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);

            builder
                .Property(e => e.Longitude)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimalLocation);
        }
    }
}
