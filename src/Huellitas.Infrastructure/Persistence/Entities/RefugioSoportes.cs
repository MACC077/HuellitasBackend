namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class RefugioSoportes
{
    public int SoporteId { get; set; }

    public int RefugioId { get; set; }

    /// <summary>PDF o Imagen.</summary>
    public string Tipo { get; set; } = null!;

    public string Archivo { get; set; } = null!;

    public DateTime FechaCarga { get; set; }

    public bool Activo { get; set; }

    public virtual Refugios Refugio { get; set; } = null!;
}
