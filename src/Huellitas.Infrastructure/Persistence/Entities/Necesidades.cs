using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Necesidades
{
    public int NecesidadId { get; set; }

    public int RefugioId { get; set; }

    public byte Tipo { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? MedioDonacion { get; set; }

    public byte Estado { get; set; }

    public DateTime FechaPublicacion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Refugios Refugio { get; set; } = null!;
}
