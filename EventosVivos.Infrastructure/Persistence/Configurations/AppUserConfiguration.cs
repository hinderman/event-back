using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");

        builder.Ignore(user => user.DomainEvents);

        builder.HasKey(user => user.Id)
            .HasName("pk_users");

        builder.Property(user => user.Id)
            .HasColumnName("user_id")
            .HasConversion(ValueObjectConverters.UserId)
            .ValueGeneratedOnAdd();

        builder.Property(user => user.UserTypeId)
            .HasColumnName("user_type_id")
            .HasConversion(ValueObjectConverters.UserTypeId)
            .IsRequired();

        builder.Property(user => user.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .UseCollation("Latin1_General_100_CI_AS_SC")
            .HasConversion(ValueObjectConverters.Email)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .HasConversion(ValueObjectConverters.PasswordHash)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<UserType>()
            .WithMany()
            .HasForeignKey(user => user.UserTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_users_user_types");

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("uq_users_email");
    }
}
