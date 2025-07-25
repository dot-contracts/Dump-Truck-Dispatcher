using DispatcherWeb.MultiTenancy.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
        {
            builder
                .Property(e => e.Amount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
