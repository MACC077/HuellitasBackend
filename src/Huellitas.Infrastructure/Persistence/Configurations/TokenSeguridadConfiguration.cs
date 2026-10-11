using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huellitas.Infrastructure.Persistence.Configurations;

public class TokensSeguridadConfiguration : IEntityTypeConfiguration<TokensSeguridad>
{
    public void Configure(EntityTypeBuilder<TokensSeguridad> builder)
    {
        builder.HasKey(t => t.TokenId);

        builder.Property(t => t.Tipo)
            .HasConversion<byte>()
            .HasMaxLength(500)
            .IsRequired();
    }
}