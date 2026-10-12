using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huellitas.Infrastructure.Persistence.Configurations;

public class RefreshTokensConfiguration : IEntityTypeConfiguration<RefreshTokens>
{
    public void Configure(EntityTypeBuilder<RefreshTokens> builder)
    {
        builder.HasKey(r => r.RefreshTokenId);

        builder.Property(r => r.TokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.ReemplazadoPor)
            .HasMaxLength(500);

        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}