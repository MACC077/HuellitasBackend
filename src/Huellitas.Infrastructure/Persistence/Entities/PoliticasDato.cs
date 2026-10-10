using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class PoliticasDato
{
    public int PoliticaId { get; set; }

    public string Version { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public DateTime FechaPublicacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ConsentimientosDatos> ConsentimientosDatos { get; set; } = new List<ConsentimientosDatos>();
}
