using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Data.EntityConfigurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Number)
                .IsRequired();

            // Уникальность номера заказа
            builder.HasIndex(o => o.Number)
                .IsUnique();

            builder.Property(o => o.OrderDate)
                .IsRequired();

            builder.Property(o => o.DeliveryDate)
                .IsRequired();

            builder.Property(o => o.ReceiptCode)
                .IsRequired();

            // Связь с PickUpPoint
            builder.HasOne(o => o.PickUpPoint)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PickUpPointId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь с User
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь со Status
            builder.HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}