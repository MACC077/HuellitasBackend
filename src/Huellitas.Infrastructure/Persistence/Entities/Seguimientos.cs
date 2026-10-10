using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Seguimientos
{
    public int SeguimientoId { get; set; }

    public int PlanId { get; set; }

    public byte Numero { get; set; }

    public DateOnly FechaProgramada { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? EstadoSalud { get; set; }

    public string? Adaptacion { get; set; }

    public string? Comentarios { get; set; }

    public byte? Resultado { get; set; }

    public byte Estado { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Multimedia> Multimedia { get; set; } = new List<Multimedia>();

    public virtual PlanesSeguimiento Plan { get; set; } = null!;
}
