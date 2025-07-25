using DispatcherWeb.Emailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TrackableEmailConfiguration : IEntityTypeConfiguration<TrackableEmail>
    {
        public void Configure(EntityTypeBuilder<TrackableEmail> builder)
        {
            builder
                .HasMany(e => e.Events)
                .WithOne(e => e.TrackableEmail)
                .HasForeignKey(e => e.TrackableEmailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Receivers)
                .WithOne(e => e.TrackableEmail)
                .HasForeignKey(e => e.TrackableEmailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
