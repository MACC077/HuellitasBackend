using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class RequisitosAdopcion
{
    public int RequisitoId { get; set; }

    public int RefugioId { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool EsObligatorio { get; set; }

    public byte Orden { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Refugios Refugio { get; set; } = null!;
}
