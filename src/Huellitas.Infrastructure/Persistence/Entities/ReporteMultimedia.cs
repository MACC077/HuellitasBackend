namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class ReporteMultimedia
{
    public int Id { get; set; }

    public int ReporteId { get; set; }

    public string RutaArchivo { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ReportesCiudadano Reporte { get; set; } = null!;
}
