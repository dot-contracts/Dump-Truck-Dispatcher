using DispatcherWeb.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PaymentHeartlandKeyConfiguration : IEntityTypeConfiguration<PaymentHeartlandKey>
    {
        public void Configure(EntityTypeBuilder<PaymentHeartlandKey> builder)
        {
            builder
                .HasIndex(x => x.PublicKey)
                .IsUnique();
        }
    }
}
