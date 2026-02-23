using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Data.EntityConfigurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            builder.Property(oi => oi.Price)
                .IsRequired()
                .HasPrecision(18, 2); // для совместимости с другими БД

            // Связь с Order
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении заказа удаляются и его позиции

            // Связь с Article
            builder.HasOne(oi => oi.Article)
                .WithMany(a => a.OrderItems)
                .HasForeignKey(oi => oi.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}