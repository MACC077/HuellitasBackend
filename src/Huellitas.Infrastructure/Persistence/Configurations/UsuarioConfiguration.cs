using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huellitas.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuarios>
{
    public void Configure(EntityTypeBuilder<Usuarios> builder)
    {
        builder.Property(u => u.Estado)
            .HasConversion<byte>()
            .IsRequired();

        builder.HasIndex(u => u.Correo)
            .IsUnique();
    }
}