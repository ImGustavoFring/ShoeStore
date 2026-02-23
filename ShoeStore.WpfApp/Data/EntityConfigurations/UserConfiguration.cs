using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Data.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Login)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Login)
                .IsUnique(); // Логин должен быть уникальным

            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(100); // для хеша пароля

            // Связь с Role
            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}