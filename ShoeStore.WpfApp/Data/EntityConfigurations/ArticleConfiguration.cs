using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Data.EntityConfigurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.ToTable("Articles");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(a => a.Title)
                .IsUnique();

            // Связь с Product (один-ко-многим, но фактически один-к-одному через уникальность ArticleId в Product)
            builder.HasMany(a => a.Products)
                .WithOne(p => p.Article)
                .HasForeignKey(p => p.ArticleId)
                .OnDelete(DeleteBehavior.Restrict); // Запрет каскадного удаления

            // Связь с OrderItem (один-ко-многим)
            builder.HasMany(a => a.OrderItems)
                .WithOne(oi => oi.Article)
                .HasForeignKey(oi => oi.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}