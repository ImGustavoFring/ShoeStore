using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Data.EntityConfigurations
{
    public class PickUpPointConfiguration : IEntityTypeConfiguration<PickUpPoint>
    {
        public void Configure(EntityTypeBuilder<PickUpPoint> builder)
        {
            builder.ToTable("PickUpPoints");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PostCode)
                .IsRequired();

            builder.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Street)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.House)
                .IsRequired();

            // Составной уникальный индекс на адрес (чтобы избежать дубликатов)
            builder.HasIndex(p => new { p.PostCode, p.City, p.Street, p.House })
                .IsUnique();
        }
    }
}