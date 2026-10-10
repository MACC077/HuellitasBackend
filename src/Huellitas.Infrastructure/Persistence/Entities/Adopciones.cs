using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Adopciones
{
    public int AdopcionId { get; set; }

    public int SolicitudId { get; set; }

    public DateTime FechaFinalizacion { get; set; }

    public string CodigoConstancia { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Calificaciones> Calificaciones { get; set; } = new List<Calificaciones>();

    public virtual ICollection<PlanesSeguimiento> PlanesSeguimientos { get; set; } = new List<PlanesSeguimiento>();

    public virtual SolicitudesAdopcion Solicitud { get; set; } = null!;
}
