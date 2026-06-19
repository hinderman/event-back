using EventosVivos.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
{
    public void Configure(EntityTypeBuilder<UserType> builder)
    {
        builder.ToTable("user_types");

        builder.HasKey(userType => userType.Id)
            .HasName("pk_user_types");

        builder.Property(userType => userType.Id)
            .HasColumnName("user_type_id")
            .HasConversion(ValueObjectConverters.UserTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(userType => userType.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .HasConversion(ValueObjectConverters.CatalogCode)
            .IsRequired();

        builder.Property(userType => userType.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(userType => userType.Code)
            .IsUnique()
            .HasDatabaseName("uq_user_types_code");

        builder.HasIndex(userType => userType.Name)
            .IsUnique()
            .HasDatabaseName("uq_user_types_name");
    }
}
