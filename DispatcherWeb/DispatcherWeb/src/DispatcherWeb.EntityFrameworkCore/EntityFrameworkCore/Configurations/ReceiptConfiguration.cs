using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder
                .HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.ReceiptLines)
                .WithOne(e => e.Receipt)
                .HasForeignKey(e => e.ReceiptId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.SalesTaxRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Order)
                .WithMany(e => e.Receipts)
                .HasForeignKey(e => e.OrderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.SalesTax)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Total)
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
