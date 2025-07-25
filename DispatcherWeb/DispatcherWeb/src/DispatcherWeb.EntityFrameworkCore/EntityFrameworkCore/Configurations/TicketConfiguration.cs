using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasIndex(e => e.DeferredTicketPhotoId);

#pragma warning disable CS0618 // Type or member is obsolete
            builder
                .Property(e => e.Quantity)
                .HasColumnType("decimal(18, 4)");
#pragma warning restore CS0618 // Type or member is obsolete

            builder
                .Property(e => e.FreightQuantity)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.MaterialQuantity)
                .HasColumnType("decimal(18, 4)");

            builder
              .Property(e => e.TareWeight)
              .HasColumnType("decimal(18, 4)");

            builder.HasOne(e => e.OrderLine)
                .WithMany(e => e.Tickets)
                .HasForeignKey(e => e.OrderLineId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ReceiptLine)
                .WithMany(e => e.Tickets)
                .HasForeignKey(e => e.ReceiptLineId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.FreightItem)
                .WithMany(e => e.FreightTickets)
                .HasForeignKey(e => e.FreightItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.MaterialItem)
                .WithMany(e => e.MaterialTickets)
                .HasForeignKey(e => e.MaterialItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.FreightUom)
                .WithMany()
                .HasForeignKey(e => e.FreightUomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.MaterialUom)
                .WithMany()
                .HasForeignKey(e => e.MaterialUomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.LoadAt)
                .WithMany()
                .HasForeignKey(e => e.LoadAtId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.DeliverTo)
                .WithMany()
                .HasForeignKey(e => e.DeliverToId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Truck)
                .WithMany(e => e.TicketsOfTruck)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Trailer)
                .WithMany(e => e.TicketsOfTrailer)
                .HasForeignKey(e => e.TrailerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
