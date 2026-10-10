using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class VisitasEntrevista
{
    public int VisitaId { get; set; }

    public int SolicitudId { get; set; }

    public DateTime Fecha { get; set; }

    public byte Modalidad { get; set; }

    public byte Estado { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual SolicitudesAdopcion Solicitud { get; set; } = null!;
}
