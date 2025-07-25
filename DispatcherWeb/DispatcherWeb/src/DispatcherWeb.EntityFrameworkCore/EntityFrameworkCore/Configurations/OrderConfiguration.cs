using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder
                .HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.CustomerContact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Office)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.OrderLines)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.SalesTax)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.CODTotal)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightTotal)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MaterialTotal)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
