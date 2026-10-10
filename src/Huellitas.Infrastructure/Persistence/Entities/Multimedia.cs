using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Multimedia
{
    public int MultimediaId { get; set; }

    public int? MascotaId { get; set; }

    public int? ReporteId { get; set; }

    public int? SeguimientoId { get; set; }

    public byte Tipo { get; set; }

    public string Archivo { get; set; } = null!;

    public bool EsPrincipal { get; set; }

    public DateTime FechaCarga { get; set; }

    public bool Activo { get; set; }

    public virtual Mascotas? Mascota { get; set; }

    public virtual ReportesCiudadano? Reporte { get; set; }

    public virtual Seguimientos? Seguimiento { get; set; }
}
