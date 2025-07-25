using DispatcherWeb.Dispatching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LoadConfiguration : IEntityTypeConfiguration<Load>
    {
        public void Configure(EntityTypeBuilder<Load> builder)
        {
            builder.HasIndex(e => e.DeferredSignatureId);

            builder.HasMany(e => e.Tickets)
                .WithOne(e => e.Load)
                .HasForeignKey(e => e.LoadId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
