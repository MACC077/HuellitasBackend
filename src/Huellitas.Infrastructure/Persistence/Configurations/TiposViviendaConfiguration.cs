using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huellitas.Infrastructure.Persistence.Configurations;

public class TiposViviendaConfiguration : IEntityTypeConfiguration<TiposVivienda>
{
    public void Configure(EntityTypeBuilder<TiposVivienda> builder)
    {
        builder.HasKey(t => t.TipoViviendaId);
        builder.Property(t => t.Nombre).HasMaxLength(50).IsRequired();
    }
}