namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class TiposVivienda
{
    public int TipoViviendaId { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }
}