using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class PlanesSeguimiento
{
    public int PlanId { get; set; }

    public int AdopcionId { get; set; }

    public byte CantidadSeguimientos { get; set; }

    public string Periodicidad { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Adopciones Adopcion { get; set; } = null!;

    public virtual ICollection<Seguimientos> Seguimientos { get; set; } = new List<Seguimientos>();
}
